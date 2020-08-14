namespace Devhus.DownloadService.Events
{
    public class DownloaderProgressArgs : System.EventArgs
    {
        /// <summary>
        /// Declares the id of the package that owns the progress info
        /// </summary>
        public string PackageID { get; set; } = string.Empty;

        /// <summary>
        /// If true the progress will be considered as building phase
        /// </summary>
        public bool IsBuilding { get; set; } = false;

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

        /// <summary>
        /// Recording the bytes that has been downloading in every second to calculate the download speed per second
        /// </summary>
        public long SpeedBytesMs { get; set; } = 0;

        /// <summary>
        /// Human-readable string of bytes that has been downloading in every second to calculate the download speed per second
        /// </summary>
        public string SpeedBytesMsText { get { return Core.Utilities.BytesToString(SpeedBytesMs); } }

        /// <summary>
        /// Stores the amount of remaining seconds until download is complete
        /// </summary>
        public long SecondsRemaining { get; set; } = 0;

        public DownloadChunkProgress[] Chunks { get; set; }
    }

}
