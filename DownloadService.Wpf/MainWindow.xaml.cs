using Devhus.DownloadService.Enums;
using Devhus.DownloadService.Wpf.Logic;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Devhus.DownloadService.Core;
using System.Threading.Tasks;

namespace Devhus.DownloadService.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal Downloader MyDownloder;

        private OptionsWindow OptionsWnd;

        private FileAdd FileAddWnd;

        private List<DownloadInformation> InformationWindows;


        public MainWindow()
        {
            InitializeComponent();

            InformationWindows = new List<DownloadInformation>();

            MyDownloder = new Downloader();

            MyDownloder.OnDownloadBegin     += OnDownloadBegin;
            MyDownloder.OnDownloadPause     += OnDownloadPause;
            MyDownloder.OnDownloadResume    += OnDownloadResume;
            MyDownloder.OnDownloadStop      += OnDownloadStop;
            MyDownloder.OnSpeedUpdate       += OnSpeedUpdate;

            MyDownloder.OnDownloadFileAdded         += OnFileAdded;
            MyDownloder.OnDownloadFileStarted       += OnDownloadFileStarted;
            MyDownloder.OnDownloaderStateChanged    += OnDownloaderStateChanged;
            MyDownloder.OnDownloadFileFail          += OnDownloadFileFail;
            MyDownloder.OnDownloadFileProgress      += OnDownloadFileProgress;
            MyDownloder.OnBuildingFileProgress      += OnBuildingFileProgress;
            MyDownloder.OnDownloadFileComplete      += OnDownloadFileComplete;

        }

        private void OnDownloadFileFail(object sender, DownloadPackage e)
        {
          
        }

        private void OnDownloadFileStarted(object sender, DownloadPackage e)
        {
           
        }

        private void OnDownloadStop(object sender, EventArgs e)
        {
           
        }

        private void OnDownloadResume(object sender, DownloadPackage e)
        {
         
        }

        private void OnDownloadPause(object sender, EventArgs e)
        {
            
        }

        private void OnDownloadBegin(object sender, EventArgs e)
        {
           
        }

        private void OnDownloadFileComplete(object sender, DownloadPackage e)
        {
            if (!DownloaderOptions.AutoQueueHandling)
                MyDownloder.QueueNext();
        }

        private void OnSpeedUpdate(object sender, DownloadService.Events.DownloaderProgressArgs progress)
        {
            if (!App.Current.Dispatcher.CheckAccess())
            {
                App.Current.Dispatcher.Invoke(() => {
                    OnSpeedUpdate(this, progress);
                });
                return;
            }

            GetInfoWindowByPackageId(progress.PackageID).OnSpeedUpdate(progress);
            return;

        }

        private void OnDownloadFileProgress(object sender, DownloadService.Events.DownloaderProgressArgs progress)
        {
            if (!App.Current.Dispatcher.CheckAccess())
            {
                App.Current.Dispatcher.Invoke(() => {
                    OnDownloadFileProgress(this, progress);
                });
                return;
            }

            GetInfoWindowByPackageId(progress.PackageID).OnProgressUpdate(progress);

            return;
        }

         private void OnBuildingFileProgress(object sender, DownloadService.Events.DownloaderProgressArgs progress)
        {
            if (!App.Current.Dispatcher.CheckAccess())
            {
                App.Current.Dispatcher.Invoke(() => {
                    OnBuildingFileProgress(this, progress);
                });
                return;
            }

            GetInfoWindowByPackageId(progress.PackageID)?.OnBuildProgressUpdate(progress);
            return;
        }

        private void OnDownloaderStateChanged(object sender, DownloaderState state)
        {
            if (!App.Current.Dispatcher.CheckAccess())
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    OnDownloaderStateChanged(sender, state);
                });
                return;
            }

            switch (state)
            {
                case DownloaderState.Paused:
                    btnDownlodPauseResume.Content = "Resume";
                    break;
                case DownloaderState.Resumed:
                    btnDownlodPauseResume.Content = "Pause";
                    break;
                case DownloaderState.Stopped:
                    btnDownlodStartStop.Content = "Start";
                    break;
                case DownloaderState.Started:
                    btnDownlodStartStop.Content = "Stop";
                    btnDownlodPauseResume.Content = "Pause";
                    break;
            }
        }

        private void OnFileAdded(object sender, Core.DownloadPackage package)
        {
            if (!App.Current.Dispatcher.CheckAccess())
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    OnFileAdded(sender, package);
                });
                return;
            }

            //inputFilePath.Text = package.MyFile.LocalPath;
            downloadFileList.Items.Add(package);


            if(GetInfoWindowByPackageId(package.PackageID) == null)
                InformationWindows.Add(new DownloadInformation() { BindPackage = package });
        }

        private DownloadInformation GetInfoWindowByPackageId(string packageID)
        {
            foreach (var wnd in InformationWindows)
                if (wnd.BindPackage.PackageID == packageID)
                    return wnd;

            return null;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }


        private void btnDownlodStartStop_Click(object sender, RoutedEventArgs e)
        {
            switch (MyDownloder.State)
            {
                case DownloaderState.Stopped:
                    Task.Run(() =>
                    {
                        MyDownloder.Begin();
                    });
                    break;
                case DownloaderState.Started:
                case DownloaderState.Resumed:
                case DownloaderState.Paused:
                    MyDownloder.Stop();
                    break;
            }

        }

        private void btnDownlodPauseResume_Click(object sender, RoutedEventArgs e)
        {
            switch (MyDownloder.State)
            {
                case DownloaderState.Paused:
                    MyDownloder.Resume();
                    break;
                case DownloaderState.Resumed:
                case DownloaderState.Started:
                    MyDownloder.Pause();
                    break;
            }
        }

        private void btnDownloaderOptions_Click(object sender, RoutedEventArgs e)
        {
            if (MyDownloder.State != DownloaderState.Stopped)
                MessageBox.Show("Stop downloading first to open options window.");

            if (OptionsWnd == null)
                OptionsWnd = new OptionsWindow();

            OptionsWnd.Owner = this;
            OptionsWnd.ShowDialog();
        }

        private void downloadFileList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var row = ItemsControl.ContainerFromElement((DataGrid)sender,
                                       e.OriginalSource as DependencyObject) as DataGridRow;
            if (row == null)
                return;

            var wnd = GetInfoWindowByPackageId(((Core.DownloadPackage)row.DataContext).PackageID);
            wnd.Show();
        }

        private void btnFileAdd_Click(object sender, RoutedEventArgs e)
        {
            if (FileAddWnd == null)
            {
                FileAddWnd = new FileAdd();
                FileAddWnd.Owner = this;
            }

            FileAddWnd.Show();
        }
    }
}
