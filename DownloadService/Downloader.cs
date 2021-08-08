using System;
using Devhus.DownloadService.Core;
using Devhus.DownloadService.Enums;
using Devhus.DownloadService.Events;

namespace Devhus.DownloadService
{
    public class Downloader : IDisposable
    {

        public static Downloader Instnce { get; set; }

        /// <summary>
        /// A DownloderAgent that store and handl the downloader tasks
        /// </summary>
        internal DownloaderAgent Agent;

        /// <summary>
        /// Stores the current state of the downloader
        /// </summary>
        public DownloaderState State
        {
            get { return _State; }
            set
            {
                if(_State != value)
                {
                    _State = value;
                    StateNotify();

                    if(OnDownloaderStateChanged != null)
                        OnDownloaderStateChanged(this, value);

                }
            }
        }
        private DownloaderState _State = DownloaderState.Stopped;
        private void StateNotify()
        {
            switch (State)
            {
                case DownloaderState.Started:
                    foreach (var package in Agent.PackageList)
                        if(package.State !=  PackageState.Completed)
                            package.State = PackageState.OnQueue;
                    break;
                case DownloaderState.Stopped:
                    foreach (var package in Agent.PackageList)
                        if (package.State < PackageState.Completed)
                            package.State = PackageState.Canceled;
                    break;
            }
        }

        /// <summary>
        /// Define the downloads path that files without local file path will be downloaded in
        /// </summary>
        internal string DownloadsPath { get; set; }

        /// <summary>
        /// Set up new downloder object
        /// </summary>
        public Downloader(string downloadsFolder = null)
        {
            if (downloadsFolder == null)
                DownloadsPath = DownloaderOptions.DownloadsPath;
            else
                DownloadsPath = downloadsFolder;

            Instnce = this;

            Agent = new DownloaderAgent(this);

            Console.WriteLine("New Devhus.Downloader is created");
            
        }

        /// <summary>
        /// Update the downloader main download path where files with no LocalFilePath will be downloaded
        /// </summary>
        /// <param name="downloadsFolder"></param>
        public void SetDownloadsFolderPath(string downloadsFolder)
        {
            DownloadsPath = downloadsFolder;

            Console.WriteLine("Devhus.Downloader download path set to: {0}", DownloadsPath);
        }


        void IDisposable.Dispose()
        {
            Instnce = null;
            Agent = null;

            // clearing events
            OnDownloadAllComplete = null;
            OnDownloadFileAdded = null;
            OnDownloadFileComplete = null;
            OnDownloadFileProgress = null;
            OnDownloadFileStarted = null;
            OnDownloadPause = null;
            OnDownloadResume = null;
            OnDownloadStop = null;
        }

        /// <summary>
        /// Using a url and basic file info will create a download package and add it to the downloder
        /// </summary>
        /// <param name="url"></param>
        /// <param name="fileLocalPath"></param>
        /// <returns></returns>
        public async void AddFile(string url, string fileLocalPath = null, bool requestRemoteSize = true)
        {
            if (fileLocalPath == string.Empty)
                fileLocalPath = null;

            var package = await Agent.AddFile(url, fileLocalPath, requestRemoteSize);

            
            CallDownloadFileAdded(package);
        }

        public async System.Threading.Tasks.Task<DownloadPackage> AddFileAsync(string url, string fileLocalPath = null, bool requestRemoteSize = true)
        {
            if (fileLocalPath == string.Empty)
                fileLocalPath = null;

            var package = await Agent.AddFile(url, fileLocalPath, requestRemoteSize);

            CallDownloadFileAdded(package);

            return package;
        }

        /// <summary>
        /// Proceed to next download package after a package is done downloading, this method only works when Options.AutoQueueHandling is disabled
        /// </summary>
        public void QueueNext()
        {
            if (DownloaderOptions.AutoQueueHandling == true)
                return;

            if (Agent.State == AgentState.Busy && Agent.CurrentPacakge != null)
                Agent.QueueProceedPermission = true;

        }


        /// <summary>
        /// Stop files downloading/building process
        /// </summary>
        public void Stop(bool forceStop = false)
        {
            if (Agent.State == AgentState.Busy && Agent.CurrentPacakge != null)
            {
                if (Agent.CurrentPacakge.State == Enums.PackageState.Downloading || 
                    Agent.CurrentPacakge.State == Enums.PackageState.Paused || forceStop == true)
                {
                    Agent.Stop(forceStop);
                    Console.WriteLine("Devhus.Downloader has stopped, agent state was {0} - package state {1}", Agent.State, Agent.CurrentPacakge.State);
                    return;
                }
                    
                 Console.WriteLine("Devhus.Downloader cannot be stopped while package is {0} state.", Agent.CurrentPacakge.State);
            }
        }

        /// <summary>
        /// Pause files downloading/building process
        /// </summary>
        public void Pause()
        {
            if (Agent.State == AgentState.Busy && Agent.CurrentPacakge != null)
            {
                if (Agent.CurrentPacakge.State == Enums.PackageState.Downloading)
                    Agent.Pause();
                else
                    Console.WriteLine("Downloader cannot be paused while package is " + Agent.CurrentPacakge.State + " state.");
            }
        }

        /// <summary>
        /// Resume files downloading/building process
        /// </summary>
        public void Resume()
        {
            if (Agent.State == AgentState.Busy && Agent.CurrentPacakge != null)
            {
                if (Agent.CurrentPacakge.State == Enums.PackageState.Paused)
                    Agent.Resume();
                else
                    Console.WriteLine("Downloader cannot be resumed while package is " + Agent.CurrentPacakge.State + " state.");
            }
        }

        /// <summary>
        /// Begin files downloading/building process
        /// </summary>
        public void Begin()
        {
            if (Agent.State != AgentState.Busy)
                Agent.Begin();
            else
                Console.WriteLine("Downloader cannot be started at "+ Agent.State + " state.");
        }

        /// <summary>
        /// Returns download package using the package id 
        /// </summary>
        /// <param name="packageID"></param>
        /// <returns></returns>
        public DownloadPackage GetPackage(string packageID)
        {
            return Agent.GetPackageById(packageID);
        }

        #region events calling methods
        internal void CallDownloadFileAdded(DownloadPackage package)
        {
            if (OnDownloadFileAdded == null)
                return;

            OnDownloadFileAdded(this, package);
        }
        internal void CallDownloadFileStarted(DownloadPackage package)
        {
            if (OnDownloadFileStarted == null)
                return;

            OnDownloadFileStarted(this, package);
        }
        internal void CallDownloadFileComplete(DownloadPackage package)
        {
            if (OnDownloadFileComplete == null)
                return;

            OnDownloadFileComplete(this, package);
        }
        internal void CallDownloadFileFail(DownloadPackage package)
        {
            if (OnDownloadFileFail == null)
                return;

            OnDownloadFileFail(this, package);
        }
        internal void CallDownloadResume(DownloadPackage package)
        {
            if (OnDownloadResume == null)
                return;

            OnDownloadResume(this, package);
        }
        internal void CallDownloadFileProgress(DownloaderProgressArgs args)
        {
            if (OnDownloadFileProgress == null)
                return;

            OnDownloadFileProgress(this, args);
        }
        internal void CallBuildingFileProgress(DownloaderProgressArgs args)
        {
            if (OnBuildingFileProgress == null)
                return;

            OnBuildingFileProgress(this, args);
        }
        internal void CallSpeedUpdate(DownloaderProgressArgs args)
        {
            if (OnSpeedUpdate == null)
                return;

            OnSpeedUpdate(this, args);
        }
        internal void CallDownloadAllComplete()
        {
            State = DownloaderState.Stopped;

            if (OnDownloadAllComplete == null)
                return;

            OnDownloadAllComplete(this, null);
        }
        internal void CallDownloadStop()
        {
            if (OnDownloadStop == null)
                return;

            OnDownloadStop(this, null);
        }
        internal void CallDownloadBegin()
        {
            if (OnDownloadBegin == null)
                return;

            OnDownloadBegin(this, null);
        }
        internal void CallDownloadPause()
        {
            if (OnDownloadPause == null)
                return;

            OnDownloadPause(this, null);
        }


        #endregion

        #region events
        /// <summary>
        /// This event is called when all download tasks are being completed
        /// </summary>
        public event EventHandler OnDownloadAllComplete;

        /// <summary>
        /// This event is called when all download tasks are canacled and downloader stopped
        /// </summary>
        public event EventHandler OnDownloadStop;

        /// <summary>
        /// This event is called when all download tasks are paused
        /// </summary>
        public event EventHandler OnDownloadPause;


        /// <summary>
        /// This event is called when downloader start
        /// </summary>
        public event EventHandler OnDownloadBegin;


        /// <summary>
        /// This event is called when a new file is being added to the downloader
        /// 
        /// <para>Parameters:</para> 
        /// <para>    Sender: downloader object</para> 
        /// <para>    Package: the download package of the added file</para> 
        /// 
        /// </summary>
        public event EventHandler<DownloadPackage> OnDownloadFileAdded;

        /// <summary>
        /// This event is called when a file download is being started
        /// 
        /// <para>Parameters:</para> 
        /// <para>    Sender: downloader object</para> 
        /// <para>    Package: the download package of the started file</para> 
        /// </summary>
        public event EventHandler<DownloadPackage> OnDownloadFileStarted;

        /// <summary>
        /// This event is called when a file download is being completed
        /// 
        /// <para>Parameters:</para>
        /// <para>    Sender: downloader object</para>
        /// <para>    Package: the download package of the completed file</para>
        /// </summary>
        public event EventHandler<DownloadPackage> OnDownloadFileComplete;

        /// <summary>
        /// This event is called when a file download is failed
        /// 
        /// <para>Parameters:</para>
        /// <para>    Sender: downloader object</para>
        /// <para>    Package: the download package of the faild file</para>
        /// </summary>
        public event EventHandler<DownloadPackage> OnDownloadFileFail;

        /// <summary>
        /// This event is called when the current download file is being resumed
        /// 
        /// <para>Parameters:</para>
        /// <para>    Sender: downloader object</para>
        /// <para>    Package: the download package of the current file</para>
        /// </summary>
        public event EventHandler<DownloadPackage> OnDownloadResume;

        /// <summary>
        /// this event will be called while a file is being downloaded
        /// 
        /// <para>Parameters:</para>
        /// <para>    Sender: downloader object</para>
        /// <para>    ProgressArgs: the current progress of the file that being downloaded</para>
        /// </summary>
        public event EventHandler<DownloaderProgressArgs> OnDownloadFileProgress;


        /// <summary>
        /// this event will be called while a file is being building
        /// 
        /// <para>Parameters:</para>
        /// <para>    Sender: downloader object</para>
        /// <para>    ProgressArgs: the current progress of the file that being building</para>
        /// </summary>
        public event EventHandler<DownloaderProgressArgs> OnBuildingFileProgress;

        /// <summary>
        /// this event will be called while a file is being downloaded to update the speed
        /// 
        /// <para>Parameters:</para>
        /// <para>    Sender: downloader object</para>
        /// <para>    ProgressArgs: the current progress of the file that being downloaded including the speed parameters</para>
        /// </summary>
        public event EventHandler<DownloaderProgressArgs> OnSpeedUpdate;


        /// <summary>
        /// This event is called when downloader state has been changed
        /// 
        /// <para>Parameters:</para> 
        /// <para>    Sender: downloader object</para> 
        /// <para>    Package: the download package of the started file</para> 
        /// </summary>
        public event EventHandler<DownloaderState> OnDownloaderStateChanged;
        #endregion

    }
}
