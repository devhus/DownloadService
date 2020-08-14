using System;

namespace Devhus.DownloadService.Core
{
    public class DownloaderException : Exception
    {
        public Enums.ErrorCode Code;

        public DownloaderException() { }

        public DownloaderException(string message) 
            : base(String.Format("Devhus.DownloadService Exception: {0}", message)) { }

        public DownloaderException(string message, Enums.ErrorCode errorCode) 
            : base(String.Format("Devhus.DownloadService Exception: {0}\nErro Code: {1}", message, errorCode.ToString())) 
        {
            Code = errorCode;
        }
    }
}
