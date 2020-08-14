using Devhus.DownloadService.Wpf.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Devhus.DownloadService.Wpf
{


    public partial class DownloadInformation : Window
    {
        internal Core.DownloadPackage BindPackage;

        public DownloadInformation()
        {
            InitializeComponent();

            DataContext = this;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }


        internal void OnSpeedUpdate(DownloadService.Events.DownloaderProgressArgs progress)
        {
            labelSpeed.Text = string.Format("({0}/second)", progress.SpeedBytesMsText);
            labelTimeRemaining.Text = Utilities.SecondsToString(progress.SecondsRemaining);
        }

        internal void OnProgressUpdate(DownloadService.Events.DownloaderProgressArgs progress)
        {
            labelDownloadedSize.Text = progress.LocalBytesText;

            progressDownload.Value = progress.CompletePercent;
            labelDownloadPercent.Content = progress.CompletePercent + "%";

            if(chunksInfoDatagrid.ItemsSource == null)
                chunksInfoDatagrid.ItemsSource = progress.Chunks;

            chunksInfoDatagrid.Items.Refresh();

        }
        internal void OnBuildProgressUpdate(DownloadService.Events.DownloaderProgressArgs progress)
        {
            progressBuilding.Value = progress.CompletePercent;
            labelBuildPercent.Content = progress.CompletePercent + "%";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if(BindPackage != null)
            {
                this.DataContext = BindPackage;

                labelUrl.Text = BindPackage.MyFile.DownloadUrl;
                //labelState.Text = BindPackage.State.ToString();
                labelFullSize.Text = BindPackage.MyFile.RemoteSizeText;

                labelDownloadedSize.Text = BindPackage.Progress.LocalBytesText;
                labelSpeed.Text = string.Format("({0}/second)", BindPackage.Progress.SpeedBytesMsText);
                labelTimeRemaining.Text = Utilities.SecondsToString(BindPackage.Progress.SecondsRemaining);

            }
        }

    }
}
