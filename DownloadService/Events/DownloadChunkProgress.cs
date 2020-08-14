
namespace Devhus.DownloadService.Events
{
    public class DownloadChunkProgress
    {
        /// <summary>
        /// Declares the id of the chunk that owns the progress info
        /// </summary>
        public int ChunkIndex { get; set; }


        /// <summary>
        /// Total bytes of the remote file size
        /// </summary>
        public long TotalBytes { get; set; } = 0;

        /// <summary>
        /// Human-readable of total bytes of the remote file size
        /// </summary>
        public string TotalBytesText { get { return Core.Utilities.BytesToString(TotalBytes);  } }

        /// <summary>
        /// Total received and written bytes to the local file
        /// </summary>
        public long LocalBytes { get; set; } = 0;

        /// <summary>
        /// Human-readable of total received and written bytes to the local file
        /// </summary>
        public string LocalBytesText { get { return Core.Utilities.BytesToString(LocalBytes); } }

        /// <summary>
        /// The remaining amount of bytes that need to be downloaded
        /// </summary>
        public long RemainingBytes { get { return (TotalBytes - LocalBytes); } }

        /// <summary>
        /// The amount of bytes last time bytes was received and written to local file
        /// </summary>
        public long ReceivedBytes { get; set; } = 0;

        /// <summary>
        /// Sum the progress bytes to give your a percent of how much download complete 
        /// </summary>
        public double CompletePercent { get { return ((double)LocalBytes / (double)TotalBytes) * 100; } }

    }

}
