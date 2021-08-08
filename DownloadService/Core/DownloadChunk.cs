using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Devhus.DownloadService.Core
{
    internal class DownloadChunk : IDisposable
    {
        /// <summary>
        /// Stores the chunk index on the package chunk list
        /// </summary>
        internal int ChunkIndex { get; set; }

        /// <summary>
        /// Define the chunk temp file path based on the packageId and chunkIndex
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Declares the total size of the chunk file
        /// </summary>
        public long Length => (End - Start) + 1;

        /// <summary>
        /// Declares range start of the chunk on the remote file stream
        /// </summary>
        public long Start { get; set; }

        /// <summary>
        /// Declares range end of the chunk on the remote file stream
        /// </summary>
        public long End { get; set; }

        /// <summary>
        /// Declares current bytes size of the local chunk file
        /// </summary>
        public long LocalBytes { get; set; }


        /// <summary>
        /// Stores the parent package object that owns this chunk
        /// </summary>
        private DownloadPackage ParentPackage { get; set; }

        /// <summary>
        /// Stores the download url string converted as Uri
        /// </summary>
        private string DownloadUri { get; set; }


        internal int FailoverCount { get; set; } = 0;


        /// <summary>
        /// Setup chunk object
        /// </summary>
        /// <param name="chunkIndex"></param>
        /// <param name="rangeStart"></param>
        /// <param name="rangeEnd"></param>
        /// <param name="filePath"></param>
        internal DownloadChunk(DownloadPackage ownerPackage, int chunkIndex, long rangeStart, long rangeEnd)
        {

            ParentPackage = ownerPackage;

            ChunkIndex = chunkIndex;

            FilePath = string.Format(ParentPackage.MyFile.LocalTempPath + ".{0:000}", ChunkIndex);

            Start = rangeStart;

            End = rangeEnd;

            LocalBytes = ReadChunkFileBytes();

            ParentPackage.ReportChunkProgress(ChunkIndex, LocalBytes);
            //ParentPackage.Progress.LocalBytes += LocalBytes;

            DownloadUri = ParentPackage.MyFile.DownloadUrl;

            System.Console.WriteLine("Devhus.Downloader {0} package #{1} chunk set download range from {2} to {3}",
                   ParentPackage.PackageID, ChunkIndex, Start, End);

        }

        public void Dispose()
        {
            ParentPackage = null;
        }

        /// <summary>
        /// A task to handle chunk file download and stream write
        /// </summary>
        /// <returns></returns>
        internal Task Transferring()
        {
            int failTrys = 0;

            while (failTrys <= DownloaderOptions.MaxTryAgainOnFailover)
            {
                try
                {
                    var rangeStart = Start + LocalBytes;

                    if (rangeStart >= End)
                    {
                        return Task.FromResult(0);
                    }


                    System.Console.WriteLine("Devhus.Downloader package {0} chunk #{1} with {2} bytes is transfering.... (range: {3} to {4})",
                        this.ParentPackage.PackageID, this.ChunkIndex, this.Length, rangeStart, End);

                    using (var httpResponse = Downloader.Instnce.Agent.GetRequestRespone(DownloadUri, "GET", rangeStart, End, this.FailoverCount))
                    {

                        if (httpResponse == null)
                            return Task.FromResult(1);

                        using (var remoteFileStream = httpResponse.GetResponseStream())
                        {
                            failTrys = 0;

                            if (remoteFileStream == null || string.IsNullOrWhiteSpace(FilePath))
                                return Task.FromResult(2);

                            if (File.Exists(FilePath) == false)
                                File.Create(FilePath).Dispose();

                            long bytesToReceiveCount = (Length - LocalBytes);
                            long bytesToReceiveCountLog = (Length - LocalBytes);

                            using (var chunkFileStream = new FileStream(FilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                            {

                                long bufferSize = bytesToReceiveCount > DownloaderOptions.BufferBlockSize ?
                                    DownloaderOptions.BufferBlockSize :
                                    bytesToReceiveCount;

                                int readSize = 0;
                                byte[] downBuffer = new byte[bufferSize];


                                while (bytesToReceiveCount > 0)
                                {
                                    while (ParentPackage.State == Enums.PackageState.Paused) { /* pause trasfering and wait */ }

                                    if (ParentPackage.State != Enums.PackageState.Downloading)
                                        break;


                                    readSize = remoteFileStream.Read(downBuffer, 0, downBuffer.Length);
                                    chunkFileStream.Write(downBuffer, 0, readSize);

                                    LocalBytes += readSize;
                                    bytesToReceiveCount -= readSize;

                                    ParentPackage.ReportChunkProgress(ChunkIndex, readSize);

                                }

                            }
                        }

                    }

                }
                //catch (TaskCanceledException e) // when stream reader timeout occured 
                //{
                //    Console.WriteLine("DownloadChunk.class TaskCanceledException error {0}", e.Message);
                //    throw new DownloaderException("TaskCanceledException! " + e.Message, Enums.ErrorCode.DHDE002);
                //}
                //catch (WebException e)
                //{
                //    Console.WriteLine("DownloadChunk.class WebException error {0}", e.Message);
                //    throw new DownloaderException("WebException! " + e.Message, Enums.ErrorCode.DHDE002);
                //}
                //catch (Exception e) when (e.Source == "System.Net.Http" ||
                //                          e.Source == "System.Net.Sockets" ||
                //                          e.Source == "System.Net.Security" ||
                //                          e.InnerException is System.Net.Sockets.SocketException)
                //{
                //    Console.WriteLine("DownloadChunk.class {0} Exception error {1}", e.Source, e.Message);
                //    throw new DownloaderException("Exception! " + e.Message, Enums.ErrorCode.DHDE002);
                //}
                catch (Exception e) // Maybe no internet!
                {
                    Console.WriteLine("DownloadChunk.class Exception error {0}", e.Message);
                    failTrys++;

                    if (failTrys == DownloaderOptions.MaxTryAgainOnFailover)
                    {
                        throw new DownloaderException("UknownException! " + e.Message, Enums.ErrorCode.DHDE002);
                    }
                    else
                    {
                        Console.WriteLine("DownloadChunk.class Chunk[{0}] Is retrying to resume downloading", this.ChunkIndex);
                    }
                }

            }


            return Task.FromResult(15);
        }


        /// <summary>
        /// Reads and return the current size of the chunk local file or create it if not exists with returning 0 as file size
        /// </summary>
        /// <returns></returns>
        private long ReadChunkFileBytes()
        {
            long fileSize = 0;

            if (File.Exists(FilePath))
            {

                FileInfo localFileInfo = new FileInfo(FilePath);
                fileSize = localFileInfo.Length;

                System.Console.WriteLine("Devhus.Downloader {0} package found #{1} chunk with size {2} bytes",
                    ParentPackage.PackageID, ChunkIndex, fileSize);
            }
            else
            {
                File.Create(FilePath).Dispose();

                System.Console.WriteLine("Devhus.Downloader {0} package created #{1} chunk",
                    ParentPackage.PackageID, ChunkIndex, fileSize);
            }

            return fileSize;
        }
    }
}
