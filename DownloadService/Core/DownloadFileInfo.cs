using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Devhus.DownloadService.Core
{
    public class DownloadFileInfo : INotifyPropertyChanged
    {
        /// <summary>
        /// Stores the file name without extension
        /// </summary>
        public string Name
        {
            get { return _Name; }
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }
        private string _Name = string.Empty;

        /// <summary>
        /// Stores the file name with extension
        /// </summary>
        public string FullName
        {
            get { return _FullName; }
            set
            {
                if (_FullName != value)
                {
                    _FullName = value;
                    NotifyPropertyChanged("FullName");
                }
            }
        }
        private string _FullName = string.Empty;

        /// <summary>
        /// Stores the file extension only
        /// </summary>
        public string Extension
        {
            get { return _Extension; }
            set
            {
                if (_Extension != value)
                {
                    _Extension = value;
                    NotifyPropertyChanged("Extension");
                }
            }
        }
        private string _Extension = string.Empty;

        /// <summary>
        /// Stores the file download url
        /// </summary>
        public string DownloadUrl { get; set; } = string.Empty;

        /// <summary>
        /// Stores the target folder on the client drive where the file will be downloaded
        /// </summary>
        public string LocalFolder { get; set; }

        /// <summary>
        /// Stores the target file full path on the client drive 
        /// </summary>
        public string LocalPath { get; set; } = string.Empty;

        /// <summary>
        /// Stores the target temp file full path on the client drive that used to define the Chunks files based on the package id
        /// </summary>
        public string LocalTempPath { get; set; } = string.Empty;


        /// <summary>
        /// Stores a Md5 Signature of the remote file that will be used in file checksum for corruption.
        /// If not set will not check if the file was corrupted.
        /// </summary>
        public string RemoteSignature { get; set; } = string.Empty;

        /// <summary>
        /// Stores a Md5 Signature of the local file that will be used in file checksum for corruption.
        /// Downloader handle this parameter automatically.
        /// </summary>
        public string LocalSignature { get; set; } = null;


        /// <summary>
        /// Stores the remote file size as human readable size
        /// </summary>
        public string RemoteSizeText
        {
            get { return Utilities.BytesToString(_RemoteSize); }
        }
        /// <summary>
        /// Stores the remote file size in bytes
        /// </summary>
        public long RemoteSize
        {
            get { return _RemoteSize; }
            set
            {
                if (_RemoteSize != value)
                {
                    _RemoteSize = value;
                    NotifyPropertyChanged("RemoteSize");
                }
            }
        }
        private long _RemoteSize = 0;

        /// <summary>
        /// Stores the local file size as human readable size, Downloader handle this parameter automatically.
        /// </summary>
        public string LocalSizeText
        {
            get { return Utilities.BytesToString(_LocalSize); }
        }
        /// <summary>
        /// Stores the local file size in bytes, Downloader handle this parameter automatically.
        /// </summary>
        public long LocalSize
        {
            get { return _LocalSize; }
            set
            {
                if (_LocalSize != value)
                {
                    _LocalSize = value;
                    NotifyPropertyChanged("LocalSize");
                }
            }
        }
        private long _LocalSize = 0;

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
