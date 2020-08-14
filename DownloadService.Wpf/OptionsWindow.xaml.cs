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
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        private bool IsReady = false;

        public OptionsWindow()
        {
            InitializeComponent();
        }



        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }


        private int[] GetCoresList()
        {
            int listCount = Core.DownloaderOptions.ChunkCount / 2;
            int[] list = new int[listCount];

            for (int c = 1; c <= listCount; c++)
                list[(c - 1)] = c * 2;

            return list;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var coresList = GetCoresList();
            for (int c = 0; c < coresList.Length; c++)
            {
                var cores = coresList[c];
                inputMaxCoresUsage.Items.Add(new ComboBoxItem() { Content = cores.ToString() });

                if (cores == Core.DownloaderOptions.ChunkCount)
                    inputMaxCoresUsage.SelectedIndex = c;
            }

            inputAutoQueueHandling.SelectedIndex = Core.DownloaderOptions.AutoQueueHandling ? 1 : 0;
            inputDownloadsPath.Text = Core.DownloaderOptions.DownloadsPath;
            inputRequiredSizeForChunks.Text = Core.DownloaderOptions.RequiredSizeForChunks.ToString();

            IsReady = true;
        }


        private void inputMaxCoresUsage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsReady == false)
                return;

            ComboBoxItem item = (ComboBoxItem)inputMaxCoresUsage.SelectedItem;
            int cores = int.Parse(item.Content.ToString());
            if (cores < Core.DownloaderOptions.CHUNK_MAX)
                Core.DownloaderOptions.ChunkCount = cores;
        }

        private void inputDownloadsPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsReady == false)
                return;

            string newPath = inputDownloadsPath.Text;

            if (newPath != Core.DownloaderOptions.DownloadsPath)
                Core.DownloaderOptions.DownloadsPath = newPath;
        }

        private void inputAutoQueueHandling_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsReady == false)
                return;

            Core.DownloaderOptions.AutoQueueHandling = inputAutoQueueHandling.SelectedIndex == 1;
        }

        private void inputRequiredSizeForChunks_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsReady == false)
                return;

            long newValue = long.Parse(inputRequiredSizeForChunks.Text);

            if (newValue != Core.DownloaderOptions.RequiredSizeForChunks)
                Core.DownloaderOptions.RequiredSizeForChunks = newValue;
        }

    }
}
