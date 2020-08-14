namespace Devhus.DownloadService.Enums
{
    public enum PackageState
    {
        Fetching,
        Ready,
        Preparing,
        Downloading,
        Building,
        Cleaning,
        Paused,
        Failed,
        OnQueue,
        Canceled,
        Completed,
        Corrupted,
    }

}
