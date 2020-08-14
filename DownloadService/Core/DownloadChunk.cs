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
        public long Length => (End - Start);

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
            try
            {

                var rangeStart = Start + LocalBytes;

                if (rangeStart >= End)
                {
                    return Task.FromResult(0);
                }

                using (var httpResponse = Downloader.Instnce.Agent.GetRequestRespone(DownloadUri, "GET", rangeStart, End))
                {

                    if (httpResponse == null)
                        return Task.FromResult(1);

                    using (var remoteFileStream = httpResponse.GetResponseStream())
                    {
                        if (remoteFileStream == null || string.IsNullOrWhiteSpace(FilePath))
                            return Task.FromResult(2);

                        if (File.Exists(FilePath) == false)
                            File.Create(FilePath).Dispose();

                        long bytesToReceiveCount = (Length - LocalBytes);

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
                                bytesToReceiveCount = Length - LocalBytes;

                                ParentPackage.ReportChunkProgress(ChunkIndex, readSize);

                            }

                        }
                    }

                }

            }
            catch (TaskCanceledException e) // when stream reader timeout occured 
            {
                Console.WriteLine("DownloadChunk.class TaskCanceledException error {0}", e.Message);
                throw new DownloaderException("TaskCanceledException! " + e.Message, Enums.ErrorCode.DHDE002);
            }
            catch (WebException e)
            {
                Console.WriteLine("DownloadChunk.class WebException error {0}", e.Message);
                throw new DownloaderException("WebException! " + e.Message, Enums.ErrorCode.DHDE002);
            }
            catch (Exception e) when (e.Source == "System.Net.Http" ||
                                      e.Source == "System.Net.Sockets" ||
                                      e.Source == "System.Net.Security" ||
                                      e.InnerException is System.Net.Sockets.SocketException)
            {
                Console.WriteLine("DownloadChunk.class Net/Socket/Security Exception error {0}", e.Message);
                throw new DownloaderException("Exception! " + e.Message, Enums.ErrorCode.DHDE002);
            }
            catch (Exception e) // Maybe no internet!
            {
                Console.WriteLine("DownloadChunk.class Exception error {0}", e.Message);
                throw new DownloaderException("UknownException! " + e.Message, Enums.ErrorCode.DHDE002);

            }

            return Task.FromResult(15);
        }

        /// <summary>
        /// Returns Http requet of the given address using the given method
        /// </summary>
        /// <param name="address"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        protected HttpWebRequest GetRequest(Uri address, string method = null)
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create(address);

            if (method != null)
                httpRequest.Method = method;

            httpRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
            httpRequest.AllowWriteStreamBuffering = false;

            return httpRequest;
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
