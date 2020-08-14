using Devhus.DownloadService.Core;
using System;
using System.Windows;

namespace Devhus.DownloadService.Wpf
{
    /// <summary>
    /// Interaction logic for FileAdd.xaml
    /// </summary>
    public partial class FileAdd : Window
    {
        private bool Failed = false;


        public FileAdd()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            e.Cancel = true;
            this.Hide();
        }

        private async void BtnAddFile_Click(object sender, RoutedEventArgs e)
        {
            BtnAddFile.IsEnabled = false;
            StatusMessage.Text = "Adding the file...";

            string fileUrl = InputFileUrl.Text;
            try
            {
                var package = await App.GUI.MyDownloder.AddFileAsync(fileUrl);
            }
            catch(DownloaderException ex)
            {
                Failed = true;
                MessageBox.Show(ex.Message);
                StatusMessage.Text = "Failed to add the file, Error Code: " + ex.Code;
            }


            if (!Failed)
                StatusMessage.Text = "File was successfully added.";

            BtnAddFile.IsEnabled = true;
        }
    }
}
