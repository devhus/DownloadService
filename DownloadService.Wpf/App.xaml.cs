using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Devhus.DownloadService.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        internal static MainWindow GUI;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            GUI = new MainWindow();
            GUI.Show();
        }
    }
}
