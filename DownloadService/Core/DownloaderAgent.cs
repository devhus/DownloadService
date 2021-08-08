using Devhus.DownloadService.Enums;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Timers;

namespace Devhus.DownloadService.Core
{
    public class DownloaderAgent
    {
        /// <summary>
        /// Store a reference to the parent downloder object
        /// </summary>
        internal Downloader ParentDownloder { get; set; }

        /// <summary>
        /// Holds the current state of the downloader agent
        /// </summary>
        internal AgentState State { get; set; } = AgentState.Free;

        /// <summary>
        /// Store a list of the full downloader files
        /// </summary>
        internal List<DownloadPackage> PackageList { get; set; }

        /// <summary>
        /// Store the current package that begin downloaded
        /// </summary>
        internal DownloadPackage CurrentPacakge { get; set; }

        /// <summary>
        /// Stores the count of total files that need to be downloaded
        /// </summary>
        internal int TotalDownloadFilesCount { get; set; } = 0;


        /// <summary>
        /// Timer to handle speed reporting per second
        /// </summary>
        private Timer DownloadSpeedTracker { get; set; }


        /// <summary>
        /// QueueProceedPermission is required to be TRUE after every file download is complete so the downloader start working on the next file
        /// </summary>
        public bool QueueProceedPermission { get; set; } = true;


        /// <summary>
        /// Setup a new agent object and store a reference for the parent downloader
        /// </summary>
        /// <param name="_parentDownloader"></param>
        public DownloaderAgent(Downloader _parentDownloader)
        {
            ParentDownloder = _parentDownloader;
            PackageList = new List<DownloadPackage>();

            DownloadSpeedTracker = new Timer(1000);
            DownloadSpeedTracker.Elapsed += new ElapsedEventHandler(DownloadSpeedTrackerHandler);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void DownloadSpeedTrackerHandler(object source, ElapsedEventArgs e)
        {
            if (CurrentPacakge != null)
            {
                var progress = CurrentPacakge.Progress;

                if (CurrentPacakge.State != PackageState.Downloading || progress.SpeedBytesMs < 1)
                    return;

                progress.SecondsRemaining = progress.RemainingBytes / progress.SpeedBytesMs;

                ParentDownloder.CallSpeedUpdate(progress);

                progress.SpeedBytesMs = 0;
            }

        }


        /// <summary>
        /// Stops all agent tasks safely.
        /// </summary>
        internal void Stop(bool forceStop = false)
        {
            if (forceStop == false)
                if (CurrentPacakge == null ||
                    (CurrentPacakge.State != PackageState.Downloading && CurrentPacakge.State != PackageState.Paused))
                    return;

            CurrentPacakge.State = PackageState.Canceled;
            ParentDownloder.State = DownloaderState.Stopped;
            State = AgentState.Canceled;

            DownloadSpeedTracker.Stop();

            ParentDownloder.CallDownloadStop();
        }

        /// <summary>
        /// Pause all agent tasks safely.
        /// </summary>
        internal void Pause()
        {
            if (CurrentPacakge == null || CurrentPacakge.State != PackageState.Downloading)
                return;

            CurrentPacakge.State = PackageState.Paused;
            ParentDownloder.State = DownloaderState.Paused;

            DownloadSpeedTracker.Stop();

            ParentDownloder.CallDownloadPause();
        }

        /// <summary>
        /// Resume all agent tasks safely.
        /// </summary>
        internal void Resume()
        {
            if (CurrentPacakge == null || CurrentPacakge.State != PackageState.Paused)
                return;

            CurrentPacakge.State = PackageState.Downloading;
            ParentDownloder.State = DownloaderState.Resumed;

            DownloadSpeedTracker.Start();

            ParentDownloder.CallDownloadResume(CurrentPacakge);
        }

        /// <summary>
        /// Begin all agent tasks safely.
        /// </summary>
        internal async void Begin()
        {
            if (PackageList.Count == 0)
                return;

            System.Console.WriteLine("Devhus.Downloader is beginning...");
            State = AgentState.Busy;
            ParentDownloder.State = DownloaderState.Started;
            ParentDownloder.CallDownloadBegin();
            DownloadSpeedTracker.Start();

            await StartDownloading();
        }

        /// <summary>
        /// Starts the downloader Task to create chunks and download data into them, package by package.
        /// </summary>
        /// <returns></returns>
        internal Task StartDownloading()
        {
            int currentPackageIndex = 0;

            System.Console.WriteLine("Devhus.Downloader is downloading...");

            while (currentPackageIndex < TotalDownloadFilesCount)
            {
                if (State != AgentState.Busy)
                    break;

                System.Console.WriteLine("Devhus.Downloader checking queue permission...");
                while (QueueProceedPermission == false) { /* waits for confirm to start downloading the next file */ }
                System.Console.WriteLine("Devhus.Downloader queue permission is granted!");


                if (!DownloaderOptions.AutoQueueHandling)
                {
                    QueueProceedPermission = false;
                }

                CurrentPacakge = PackageList[currentPackageIndex] ?? null;

                if (CurrentPacakge == null || CurrentPacakge.State == PackageState.Completed)
                {
                    System.Console.WriteLine("Devhus.Downloader packge {0} was not found or was completed on download starting!", currentPackageIndex);

                    currentPackageIndex++;
                    continue;
                }


                System.Console.WriteLine("Devhus.Downloader {0} file in {1} package started", CurrentPacakge.MyFile.Name, CurrentPacakge.PackageID);

                ParentDownloder.CallDownloadFileStarted(CurrentPacakge);

                CurrentPacakge.CreateChunks();

                CurrentPacakge.StartDownload();

                currentPackageIndex++;

            }

            if (State != AgentState.Canceled)
            {
                State = AgentState.Completed;
                ParentDownloder.CallDownloadAllComplete();
            }

            return Task.FromResult(0);
        }

        /// <summary>
        /// Returns download package by its id if the package was exists and if not it will return NULL
        /// </summary>
        /// <param name="packageID"></param>
        /// <returns></returns>
        internal DownloadPackage GetPackageById(string packageID)
        {
            if (packageID == string.Empty || PackageList.Count == 0)
                return null;

            foreach (var package in PackageList)
                if (package.PackageID == packageID)
                    return package;

            return null;
        }


        internal HttpWebResponse GetRequestRespone(string address, string method = null, long? rangeStart = null, long? rangeEnd = null, int? failTrys = null)
        {


            try
            {
                System.Console.WriteLine("Devhus.Downloader creating new http reponse...");
                var httpRequest = HttpWebRequest.CreateHttp(address);

                if (method != null)
                {
                    httpRequest.Method = method;
                    System.Console.WriteLine("Devhus.Downloader http response method is {0}", method);
                }

                httpRequest.Timeout = -1;
                httpRequest.ReadWriteTimeout = -1;

                httpRequest.Accept = DownloaderOptions.RequestConfiguration.Accept;
                httpRequest.KeepAlive = DownloaderOptions.RequestConfiguration.KeepAlive;
                httpRequest.AllowAutoRedirect = DownloaderOptions.RequestConfiguration.AllowAutoRedirect;
                httpRequest.AutomaticDecompression = DownloaderOptions.RequestConfiguration.AutomaticDecompression;
                httpRequest.UserAgent = DownloaderOptions.RequestConfiguration.UserAgent;
                httpRequest.ProtocolVersion = DownloaderOptions.RequestConfiguration.ProtocolVersion;
                httpRequest.UseDefaultCredentials = DownloaderOptions.RequestConfiguration.UseDefaultCredentials;
                httpRequest.SendChunked = DownloaderOptions.RequestConfiguration.SendChunked;
                httpRequest.TransferEncoding = DownloaderOptions.RequestConfiguration.TransferEncoding;
                httpRequest.Expect = DownloaderOptions.RequestConfiguration.Expect;
                httpRequest.MaximumAutomaticRedirections = DownloaderOptions.RequestConfiguration.MaximumAutomaticRedirections;
                httpRequest.MediaType = DownloaderOptions.RequestConfiguration.MediaType;
                httpRequest.PreAuthenticate = DownloaderOptions.RequestConfiguration.PreAuthenticate;
                httpRequest.Credentials = DownloaderOptions.RequestConfiguration.Credentials;
                httpRequest.ClientCertificates = DownloaderOptions.RequestConfiguration.ClientCertificates;
                httpRequest.Referer = DownloaderOptions.RequestConfiguration.Referer;
                httpRequest.Pipelined = DownloaderOptions.RequestConfiguration.Pipelined;
                httpRequest.Proxy = DownloaderOptions.RequestConfiguration.Proxy;

                //if(DownloaderOptions.RequestConfiguration.Headers.Count > 0)
                //{
                //    foreach(var header in DownloaderOptions.RequestConfiguration.Headers)
                //    {
                //        httpRequest.Headers.Add(header.Key, header.Value);
                //    }
                //}

                if (rangeStart != null)
                {
                    if (rangeEnd == null)
                        httpRequest.AddRange((long)rangeStart);
                    else
                        httpRequest.AddRange((long)rangeStart, (long)rangeEnd);
                }

                System.Console.WriteLine("Devhus.Downloader http response requesting...");
                var response = (HttpWebResponse)httpRequest.GetResponse();

                System.Console.WriteLine("Devhus.Downloader http response ready to return...");
                System.Console.WriteLine("Devhus.Downloader http response ready {0}", response);
                return response;
            }
            catch (WebException e)
            {
                System.Console.WriteLine("WebException throwed with Status: {0} and Message: {1}", e.Status, e.Message);

                //if (failTrys != null && failTrys++ <= DownloaderOptions.MaxTryAgainOnFailover)
                //{
                //    return GetRequestRespone(address, method, rangeStart, rangeEnd, failTrys);
                //}

                switch (e.Status)
                {
                    case WebExceptionStatus.ProtocolError:
                        {
                            switch (((HttpWebResponse)e.Response).StatusCode)
                            {
                                case HttpStatusCode.NotFound:
                                    throw new DownloaderException("Invalid Url. File was not found!", ErrorCode.DHDE003);
                            }
                            break;
                        }
                    case WebExceptionStatus.SecureChannelFailure:
                        {
                            if (ServicePointManager.SecurityProtocol != SecurityProtocolType.Tls &&
                                ServicePointManager.SecurityProtocol != SecurityProtocolType.Tls11 &&
                                ServicePointManager.SecurityProtocol != SecurityProtocolType.Tls12 &&
                                ServicePointManager.SecurityProtocol != SecurityProtocolType.Ssl3)
                            {
                                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                            }
                            else
                            {
                                if (ServicePointManager.SecurityProtocol == SecurityProtocolType.Tls)
                                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
                                else if (ServicePointManager.SecurityProtocol == SecurityProtocolType.Tls11)
                                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                                else if (ServicePointManager.SecurityProtocol == SecurityProtocolType.Tls12)
                                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
                                else
                                    throw new DownloaderException(e.Message, ErrorCode.DHDE003);
                            }


                            return GetRequestRespone(address, method);
                        }

                    default:
                        {
                            throw new DownloaderException(e.Message + e.Status, ErrorCode.DHDE003);
                        }
                }

            }
            catch
            {
                System.Console.WriteLine("Unknown error with getting response for http request!");
                throw new DownloaderException("Unknown error with getting response for http request!");
            }

            return null;
        }


        /// <summary>
        /// Using a url and basic file info will create a download package and add it to the downloder
        /// </summary>
        /// <param name="url"></param>
        /// <param name="fileLocalPath"></param>
        /// <returns></returns>
        internal Task<DownloadPackage> AddFile(string url, string fileLocalPath = null, bool requestRemoteSize = true)
        {
            DownloadPackage package;

            package = new DownloadPackage(this, url, fileLocalPath, requestRemoteSize);

            if (State == AgentState.Busy)
                package.State = PackageState.OnQueue;

            PackageList.Add(package);

            TotalDownloadFilesCount++;

            //ParentDownloder.CallDownloadFileAdded(package);
            System.Console.WriteLine("Devhus.Downloader file has been added: {0}", package.MyFile.FullName);

            return Task.FromResult(package);
        }

    }
}
