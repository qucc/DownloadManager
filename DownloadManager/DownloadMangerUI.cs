using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;


namespace DownloadManager
{
    public partial class DownloadMangerUI : Form
    {
        List<DownloadTask> downloadTasks = new List<DownloadTask>();
        string downloadDirectory = null;
        public DownloadMangerUI()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            downloadDirectory = folderBrowserDialog1.SelectedPath;
            Console.WriteLine("init download_directory " + downloadDirectory);
        }

        private void addBtn_Click(object sender, EventArgs e)
        {
            string uriName = urlInput.Text;
            Uri uriResult;
            bool result = Uri.TryCreate(uriName, UriKind.Absolute, out uriResult) && uriResult.Scheme == Uri.UriSchemeHttp;
            if (result)
            {
                if(!Directory.Exists(downloadDirectory))
                {
                    Directory.CreateDirectory(downloadDirectory);
                }
                string outfileName = GetAvailableOutputName(downloadDirectory + Path.DirectorySeparatorChar + Path.GetFileName(uriResult.LocalPath));
                Console.WriteLine("outfile " + outfileName);

                DownloadTask downloadTask = new DownloadTask(uriName, outfileName);
                downloadTasks.Add(downloadTask);

                
                DownloadListItem downloadListItem = new DownloadListItem(downloadTask);
                downloadListItem.Text = Path.GetFileName(outfileName);
                downloadListItem.DownloadItemRemoved += downloadListItem_DownloadItemRemoved;
                flowLayoutPanel1.Controls.Add(downloadListItem);
                downloadTask.StartDownload();
            }
            else
            {
                MessageBox.Show("Invalid Url");
            }
            urlInput.Text = "";
        }

        private string GetAvailableOutputName(string outputFilename)
        {
            string directoryName = Path.GetDirectoryName(outputFilename);
            string extension = Path.GetExtension(outputFilename);
            string fileNameWithoudExtension = Path.GetFileNameWithoutExtension(outputFilename);
            Regex reg = new Regex(fileNameWithoudExtension + @"( \(\d\))*" + extension + "$");
            List<string> usedNames = new List<string>();
            int countFile = 0;
            if(downloadTasks.Any(downloadTask => downloadTask.OutFilename.Equals(outputFilename)) || new FileInfo(outputFilename).Exists)
            {
                countFile = 1;
                string newOutputFilename = directoryName + Path.DirectorySeparatorChar + fileNameWithoudExtension + " (" + countFile + ")" + extension;
                while (downloadTasks.Any(downloadTask => downloadTask.OutFilename.Equals(newOutputFilename)) || new FileInfo(newOutputFilename).Exists)
                {
                    newOutputFilename = directoryName + Path.DirectorySeparatorChar + fileNameWithoudExtension + " (" + (++countFile) + ")" + extension;
                    Console.WriteLine("Generate new OutputFilename " + newOutputFilename);
                }
            }
            if(countFile > 0)
            {
                outputFilename = directoryName + Path.DirectorySeparatorChar + fileNameWithoudExtension + " (" + countFile + ")" + extension;
            }  
            return outputFilename;
        }

        private void downloadListItem_DownloadItemRemoved(object sender, EventArgs e)
        {
            if (sender is DownloadListItem)
            {
                int removeIndex = flowLayoutPanel1.Controls.IndexOf(sender as DownloadListItem);
                flowLayoutPanel1.Controls.RemoveAt(removeIndex);
            }
        }

        private void DownloadStatusChanged(object sender, DownloadEventArgs e)
        {
            if (sender is DownloadTask)
            {
                DownloadTask currentDownloadTask = sender as DownloadTask;
                int index = downloadTasks.FindIndex(downloadTask => downloadTask.Url.Equals(currentDownloadTask.OutFilename, StringComparison.Ordinal));
                DownloadListItem downloadListItem = flowLayoutPanel1.Controls[index] as DownloadListItem;
                if (e.Status == DownloadStatus.Completed)
                {
                    downloadListItem.Done();
                }
            }
           
        }

        private void DownloadProgressChanged(object sender, DownloadEventArgs e)
        {
            if (sender is DownloadTask)
            {
                DownloadTask currentDownloadTask = sender as DownloadTask;
                int index = downloadTasks.FindIndex(downloadTask => downloadTask.Url.Equals(currentDownloadTask.OutFilename, StringComparison.Ordinal));
                DownloadListItem downloadListItem = flowLayoutPanel1.Controls[index] as DownloadListItem;
                downloadListItem.SetProgress(e.Progress);
            }
        }


        private void clearBtn_Click(object sender, EventArgs e)
        {
            flowLayoutPanel1.Controls.Clear();

            downloadTasks.Clear();
        }

        private void settingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                Console.WriteLine(folderBrowserDialog1.SelectedPath);
                downloadDirectory = folderBrowserDialog1.SelectedPath;
                Properties.Settings.Default.download_directory = downloadDirectory;
            }
        }

       


    }

  
}
