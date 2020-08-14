using System;
using System.IO;


namespace Devhus.DownloadService.Core
{
    public class DownloaderOptions
    {
        /// <summary>
        /// Declares the max core count that can be used from the processor
        /// </summary>
        public static int MaxCoresUsage
        {
            get { return _MaxCoresUsage; }
            set
            {
                if (_MaxCoresUsage == value)
                    return;

                _MaxCoresUsage = value;
                CHUNK_MAX = Environment.ProcessorCount > MaxCoresUsage ? MaxCoresUsage : Environment.ProcessorCount;
                if (ChunkCount > CHUNK_MAX)
                     ChunkCount = CHUNK_MAX;
            }
        }
        private static int _MaxCoresUsage = 8;


        /// <summary>
        /// Define the maximum cores that allowed to be used for the downloader
        /// </summary>
        public static int CHUNK_MAX = Environment.ProcessorCount > MaxCoresUsage ? MaxCoresUsage : Environment.ProcessorCount;

        /// <summary>
        /// declares the default download path that will be used on files with no local file path and be default path for downloaders that doesn't have downloads path
        /// </summary>
        public static string DownloadsPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Downloads");

        /// <summary>
        /// declares the number of download threads and file parts, automatically will use CHUNK_MAX if not set or the parameter value was higher than CHUNK_MAX
        /// </summary>
        public static int ChunkCount
        {
            get { return _ChunkCount; }
            set
            {
                if (value > CHUNK_MAX)
                    _ChunkCount = CHUNK_MAX;
                else
                    _ChunkCount = value;
            }

        }
        private static int _ChunkCount = CHUNK_MAX;

        /// <summary>
        /// Stream buffer size which is used for size of blocks
        /// </summary>
        public static int BufferBlockSize { get; set; } = 1024 * 64;


        /// <summary>
        /// Declares the needed file size to use chunks system,
        /// example if was set as 52428800 the Chunks system will be used on files with size of 50mb or higher.
        /// The default size is 52428800(50mb)
        /// </summary>
        public static long RequiredSizeForChunks { get; set; } = 52428800;

        /// <summary>
        /// How many time try again to download on failed
        /// </summary>
        //public int MaxTryAgainOnFailover { get; set; } 

        /// <summary>
        /// Disabling this will cause a need for confirming the downloader to proceed between files by calling Downloader.QueueNext() 
        /// </summary>
        public static bool AutoQueueHandling { get; set; } = true;


        public static DownloaderRequestConfig RequestConfiguration { get; set; } 
            = new DownloaderRequestConfig();
    }
}
