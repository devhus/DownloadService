namespace Devhus.DownloadService.Enums
{
    public enum ErrorCode
    {
        /// <summary>
        /// failed to fetch file info on adding because of an invalid url
        /// </summary>
        DHDE001,

        /// <summary>
        /// faild while trying to download chunk file 
        /// </summary>
        DHDE002,

        /// <summary>
        /// faild to create a response from HttpWebRequest
        /// </summary>
        DHDE003,

    }

}
