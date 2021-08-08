using Devhus.DownloadService.Enums;
using Devhus.DownloadService.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Devhus.DownloadService.Core
{
    public class DownloadPackage : INotifyPropertyChanged
    {
        /// <summary>
        /// A unique Guid to define the package
        /// </summary>
        public string PackageID { get; set; }

        /// <summary>
        /// Downloder file object that holds the file info for both remote and local/temp files.
        /// </summary>
        /// 
        public DownloadFileInfo MyFile
        {
            get { return _MyFile; }
            set
            {
                if (_MyFile != value)
                {
                    _MyFile = value;
                    NotifyPropertyChanged("MyFile");
                }
            }
        }
        private DownloadFileInfo _MyFile;

        /// <summary>
        /// The current state of the package
        /// </summary>
        public PackageState State
        {
            get { return _State; }
            set
            {
                if (_State != value)
                {
                    _State = value;
                    NotifyPropertyChanged("State");
                }
            }
        }
        private PackageState _State = PackageState.Fetching;

        /// <summary>
        /// Stores the owner agent of this package
        /// </summary>
        internal DownloaderAgent ParentAgent;


        /// <summary>
        /// Define the number of chunks used to download this package
        /// </summary>
        public int ChunkParts
        {
            get { return _ChunkParts; }
            set
            {
                if (_ChunkParts != value)
                {
                    _ChunkParts = value;
                    NotifyPropertyChanged("ChunkParts");
                }
            }
        }
        private int _ChunkParts = 1;

        /// <summary>
        /// Stores a list with the chunks that belong to this package
        /// </summary>
        internal List<DownloadChunk> Chunks { get; set; }

        /// <summary>
        /// Stores the package progress arguments, it will be used on reporting progress using the progress tracker timer on Agent class
        /// </summary>
        public DownloaderProgressArgs Progress { get; set; }

        /// <summary>
        /// If disabled the package will not request file size through http request, user need to add the file size manually
        /// Only enable it if you can provide the remote file size after adding the package
        /// </summary>
        internal bool ReadRemoteSize;


        /// <summary>
        /// Set up a package object
        /// </summary>
        public DownloadPackage(DownloaderAgent ownweAgent, string url, string fileLocalPath = null, bool requestRemoteSize = true)
        {
            System.Console.WriteLine("Devhus.Downloader is creating new package..");
            ParentAgent = ownweAgent;

            MyFile = new DownloadFileInfo()
            {
                DownloadUrl = url,
                LocalPath = fileLocalPath
            };

            ReadRemoteSize = requestRemoteSize;

            System.Console.WriteLine("Devhus.Downloader fetching package info..");
            FetchFileInfo();

            PackageID = CreatePackageID();
            System.Console.WriteLine("Devhus.Downloader defined package with {0} as id", PackageID);

            MyFile.LocalTempPath = Path.Combine(MyFile.LocalFolder, PackageID);

            Progress = new DownloaderProgressArgs() { PackageID = PackageID, TotalBytes = MyFile.RemoteSize };
        }

        /// <summary>
        /// Returns md5 hash based on the file full path
        /// </summary>
        /// <returns></returns>
        private string CreatePackageID()
        {
            StringBuilder hash = new StringBuilder();
            MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
            byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(MyFile.LocalPath));

            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("x2"));
            }

            return hash.ToString();
        }

        /// <summary>
        /// Fetch and setup the download file info for both remote file and local file if exists
        /// </summary>
        private void FetchFileInfo()
        {

            Uri uriResult;
            bool result = Uri.TryCreate(MyFile.DownloadUrl, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (result == false)
                throw new DownloaderException("Invalid Url. Url format is not correct.", ErrorCode.DHDE001);

            MyFile.FullName = Path.GetFileName(uriResult.LocalPath);
            MyFile.Name = Path.GetFileNameWithoutExtension(MyFile.FullName);
            MyFile.Extension = Path.GetExtension(MyFile.FullName);

            if (MyFile.LocalPath == null)
            {
                MyFile.LocalFolder = Downloader.Instnce.DownloadsPath;
                MyFile.LocalPath = Path.Combine(MyFile.LocalFolder, MyFile.FullName);
            }
            else
            {
                MyFile.LocalFolder = Path.GetDirectoryName(MyFile.LocalPath);
            }


            if (File.Exists(MyFile.LocalPath))
            {
                System.Console.WriteLine("Devhus.Downloader getting package local size...");
                FileInfo localFileInfo = new FileInfo(MyFile.LocalPath);
                MyFile.LocalSize = localFileInfo.Length;
                MyFile.LocalSignature = Utilities.GetFileMd5Hash(MyFile.LocalPath);
                System.Console.WriteLine($"Devhus.Downloader local size defined {MyFile.LocalSize} bytes");

            }

            if (ReadRemoteSize)
            {
                System.Console.WriteLine("Devhus.Downloader getting package remote size...");
                using (var webResponse = ParentAgent.GetRequestRespone(MyFile.DownloadUrl, "HEAD"))
                    MyFile.RemoteSize = webResponse.ContentLength;

                System.Console.WriteLine($"Devhus.Downloader remote size defined {MyFile.RemoteSize} bytes");
            }

            State = PackageState.Ready;


        }

        /// <summary>
        /// Reports progress of each chunk to calculate into total progress of the package
        /// </summary>
        /// <param name="chunkIndex"></param>
        /// <param name="receivedBytes"></param>
        internal void ReportChunkProgress(int chunkIndex, long receivedBytes)
        {
            if (receivedBytes == 0)
                return;

            Progress.ReceivedBytes = receivedBytes;
            Progress.LocalBytes += receivedBytes;
            Progress.SpeedBytesMs += receivedBytes;

            var ChunkProgress = Progress.Chunks[chunkIndex];

            ChunkProgress.ReceivedBytes = receivedBytes;
            ChunkProgress.LocalBytes += receivedBytes;

            ParentAgent.ParentDownloder.CallDownloadFileProgress(Progress);
        }

        /// <summary>
        /// Reports progress of building the package by mergine chunks
        /// </summary>
        /// <param name="chunkIndex"></param>
        /// <param name="receivedBytes"></param>
        internal void ReportBuildingProgress(int chunkIndex, long receivedBytes)
        {
            Progress.ReceivedBytes = receivedBytes;
            Progress.LocalBytes += receivedBytes;
            //Progress.SpeedBytesMs += receivedBytes;

            ParentAgent.ParentDownloder.CallBuildingFileProgress(Progress);
        }


        /// <summary>
        /// Update the package status and the last events required.
        /// </summary>
        /// <param name="forceComplete"></param>
        internal void CompleteDownload(bool forceComplete = false)
        {
            if (forceComplete)
                Progress.ReceivedBytes = Progress.TotalBytes;
            else
                Progress.ReceivedBytes = 0;

            Progress.LocalBytes = Progress.TotalBytes;

            ParentAgent.ParentDownloder.CallDownloadFileProgress(Progress);

            if (State < PackageState.Completed)
                State = PackageState.Completed;

            ParentAgent.ParentDownloder.CallDownloadFileComplete(this);

        }


        /// <summary>
        /// Handles the download stage by starting chunks transferring then merging and cleaning the package
        /// </summary>
        internal void StartDownload()
        {
            if (State == PackageState.Completed)
            {
                System.Console.WriteLine("Devhus.Downloader package {0} was completed when tried to start downloading it !",
                PackageID);
                return;
            }

            State = PackageState.Downloading;

            if (Chunks.Count == 0)
            {
                State = PackageState.Building;
                return;
            }

            System.Net.ServicePointManager.DefaultConnectionLimit = ChunkParts;

            System.Console.WriteLine("Devhus.Downloader package {0} connected with {1} connection limit",
                PackageID, ChunkParts);

            var tasks = new List<Task>();

            foreach (var chunk in Chunks)
            {

                var task = Task.Run(chunk.Transferring);
                System.Console.WriteLine("Devhus.Downloader package {0} chunk #{1} with {2} bytes set to be transfering....",
                    PackageID, chunk.ChunkIndex, chunk.Length);
                tasks.Add(task);
            }

            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (AggregateException e)
            {
                System.Console.WriteLine("Devhus.Downloader package {0} download faild! error: {1}",
                   PackageID, e.Message);

                State = PackageState.Failed;
                Downloader.Instnce.CallDownloadFileFail(this);
            }

            if (State == PackageState.Canceled || State == PackageState.Failed)
            {
                System.Console.WriteLine("Devhus.Downloader package {0} was Canceled or Failed while downloading its chunks!",
                   PackageID);
                return;
            }

            // build the file
            BuildPackage();

            //remove temps
            RemoveChunks();

            //check if file is corrupted
            CorruptionScan();

            //..
            CompleteDownload();

        }

        /// <summary>
        /// Matches the signature between remote and local file to check if file was corrupted after download
        /// </summary>
        internal void CorruptionScan()
        {
            if (MyFile.RemoteSignature == "")
            {
                System.Console.WriteLine("Devhus.Downloader package {0} has no Remote Signature to be used in matching...",
               PackageID);
                return;
            }

            System.Console.WriteLine("Devhus.Downloader package {0} scanning for corruption.. (local: {1} - remote:{2} )",
               PackageID, MyFile.LocalSignature, MyFile.RemoteSignature);

            if (MyFile.RemoteSignature == MyFile.LocalSignature)
                return;

            System.Console.WriteLine("Devhus.Downloader package {0} is corrupted..",
               PackageID);

            State = PackageState.Corrupted;

        }

        /// <summary>
        /// Merge chunks files using streams
        /// </summary>
        internal void BuildPackage()
        {
            if (State != PackageState.Downloading)
            {
                System.Console.WriteLine("Devhus.Downloader package {0} is not ready for building.... State {1}",
                   PackageID, State);
                return;
            }

            System.Console.WriteLine("Devhus.Downloader package {0} is building....",
                   PackageID);
            State = PackageState.Building;

            Progress = new DownloaderProgressArgs()
            {
                TotalBytes = MyFile.RemoteSize,
                PackageID = this.PackageID,
                IsBuilding = true
            };

            if (Directory.Exists(MyFile.LocalFolder) == false)
                Directory.CreateDirectory(MyFile.LocalFolder);

            using (var destinationStream = new FileStream(MyFile.LocalPath, FileMode.Create, FileAccess.Write))
            {
                for (int chunkIndex = 0; chunkIndex < ChunkParts; chunkIndex++)
                {
                    System.Console.WriteLine("Devhus.Downloader package {0} is merging with {1} chunk....",
                   PackageID, chunkIndex);

                    var chunk = Chunks[chunkIndex] ?? null;

                    if (chunk == null || !File.Exists(chunk.FilePath))
                    {
                        System.Console.WriteLine("Devhus.Downloader {0} chunk file was not found!",
                     chunkIndex);
                        continue;
                    }

                    using (var chunkStream = new FileStream(chunk.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        long bytesToWrite = chunk.LocalBytes;
                        var bufferSize = bytesToWrite > DownloaderOptions.BufferBlockSize ?
                                DownloaderOptions.BufferBlockSize :
                                bytesToWrite;
                        int readBytes = 0;
                        byte[] readBuffer = new byte[bufferSize];

                        //while ((readBytes = chunkStream.Read(readBuffer, 0, readBuffer.Length)) > 0)
                        while(bytesToWrite > 0)
                        {
                            //if((bytesToWrite - readBytes) <= 0 && chunkIndex == (Chunks.Count - 1))
                            //{
                            //    readBytes -= 1;
                            //}

                            readBytes = chunkStream.Read(readBuffer, 0, readBuffer.Length);
                            destinationStream.Write(readBuffer, 0, readBytes);

                            bytesToWrite -= readBytes;
                            ReportBuildingProgress(chunkIndex, readBytes);
                        }

                    }

                }

            }

            Progress.LocalBytes = 0;
            Progress.ReceivedBytes = 0;
            ParentAgent.ParentDownloder.CallBuildingFileProgress(Progress);

        }

        /// <summary>
        /// Delete the package chunks, can be called after all chunks are merged into package file
        /// </summary>
        internal void RemoveChunks()
        {
            if (State != PackageState.Building)
            {
                System.Console.WriteLine("Devhus.Downloader package {0} is not ready for cleaning.... State {1}",
                   PackageID, State);
                return;
            }

            System.Console.WriteLine("Devhus.Downloader package {0} is removing chunks ",
                  PackageID);

            State = PackageState.Cleaning;

            if (DownloaderOptions.DeleteChunksAfterBuild == true)
            {

                for (int chunkIndex = 0; chunkIndex < ChunkParts; chunkIndex++)
                {
                    var chunk = Chunks[chunkIndex];

                    System.Console.WriteLine("Devhus.Downloader package {0} deleting {1} chunk..",
                      PackageID, chunkIndex);

                    if (File.Exists(chunk.FilePath))
                        File.Delete(chunk.FilePath);
                }

                System.Console.WriteLine("Devhus.Downloader package {0} chunks removed",
                     PackageID);
            }

            System.Console.WriteLine("Devhus.Downloader package {0} creating signutre...",
                   PackageID);

            MyFile.LocalSignature = Utilities.GetFileMd5Hash(MyFile.LocalPath);

            System.Console.WriteLine("Devhus.Downloader package {0} is builded with signutre: {1}",
                   PackageID, MyFile.LocalSignature);
        }

        /// <summary>
        /// Set up the chunks list for the owner package
        /// </summary>
        internal void CreateChunks()
        {
            if (MyFile.LocalSignature != null)
            {
                if (MyFile.LocalSignature == MyFile.RemoteSignature)
                {
                    System.Console.WriteLine("Devhus.Downloader {0} package file is found and checksum is matched", PackageID);
                    CompleteDownload(true);
                    return;
                }
            }

            System.Console.WriteLine("Devhus.Downloader {0} package creating chunks...", PackageID);

            Progress = new DownloaderProgressArgs()
            {
                TotalBytes = MyFile.RemoteSize,
                PackageID = this.PackageID,
            };

            State = PackageState.Preparing;

            //var neededChunks = (int)Math.Ceiling((double)MyFile.RemoteSize / DownloaderOptions.RequiredSizeForChunks);
            ChunkParts = DownloaderOptions.MaxCoresUsage;

            if (ChunkParts < 1)
                ChunkParts = 1;

            long chunkSize = MyFile.RemoteSize / ChunkParts;
            //var chunkRemainSize = MyFile.RemoteSize % ChunkParts;

            if (chunkSize < 1){
                chunkSize = MyFile.RemoteSize;
                ChunkParts = 1;
            }

            Chunks = new List<DownloadChunk>();
            Progress.Chunks = new DownloadChunkProgress[ChunkParts];

            if (!Directory.Exists(MyFile.LocalFolder))
                Directory.CreateDirectory(MyFile.LocalFolder);

            long chunkRangeStart = 0;
            long chunkRangeEnd = 0;

            for (var chunkIndex = 0; chunkIndex < ChunkParts; chunkIndex++)
            {
                System.Console.WriteLine("Devhus.Downloader {0} chunk is creating...", chunkIndex);

                chunkRangeStart += chunkIndex == 0 ? 0 : chunkSize;
                chunkRangeEnd += chunkIndex == 0 ? chunkSize - 1 : chunkSize;

                //long ChunkRangeStart = chunkIndex * chunkSize;
                //long ChunkRangeEnd = ((chunkIndex + 1) * chunkSize) - 1;

                //if (chunkIndex == (ChunkParts - 1) && chunkRemainSize != 0)
                //    ChunkRangeEnd += chunkRemainSize;

                if(ChunkParts > 1 && chunkIndex == (ChunkParts - 1))
                {
                    //chunkRangeStart += 1;
                    chunkRangeEnd = MyFile.RemoteSize - 1;
                }

                //if (chunkIndex != (ChunkParts - 1))
                //    ChunkRangeEnd -= 1;

                Progress.Chunks[chunkIndex] = new DownloadChunkProgress()
                {
                    ChunkIndex = chunkIndex
                };

                var chunk = new DownloadChunk(this, chunkIndex, chunkRangeStart, chunkRangeEnd);
                Progress.Chunks[chunkIndex].TotalBytes = chunkIndex != (ChunkParts - 1) ? chunk.Length + 1 : chunk.Length;

                Chunks.Add(chunk);
            }


        }

        /// <summary>
        /// Property Changed Event Handler
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notify Property Changed hanlder
        /// </summary>
        /// <param name="propertyName"></param>
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
