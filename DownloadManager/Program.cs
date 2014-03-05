using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DownloadManager
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ApplicationExit += Application_ApplicationExit;
            Application.Run(new DownloadMangerUI());
        }

        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            Console.WriteLine(Properties.Settings.Default.download_directory);
            Properties.Settings.Default.Save();
        }
    }
}
