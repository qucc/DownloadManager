using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace DownloadManager
{
    public partial class DownloadListItem : UserControl 
    {

        DownloadTask downloadTask;

        public override string Text
        {
	          get 
	        { 
		         return base.Text;
	        }
	          set 
	        { 
		        base.Text = value;
                fileNameText.Text = value;
	        }
        }

        public void SetProgress(int progress)
        {
            progressBar1.Value = progress;
        }

        public void Done()
        {
            openBtn.Visible = true;
            clearBtn.Visible = true;
            cancelBtn.Visible = false;
        }

        public DownloadListItem()
        {
            InitializeComponent();
        }

        public DownloadListItem(DownloadTask downloadTask):this()
        {
            this.downloadTask = downloadTask;
            downloadTask.DownloadProgressChanged += DownloadProgressChanged;
            downloadTask.DownloadStatusChanged += DownloadStatusChanged;
        }

        private void DownloadStatusChanged(object sender, DownloadEventArgs e)
        {
            if (e.Status == DownloadStatus.Completed)
            {
                Done();
            }
            else if(e.Status == DownloadStatus.Error)
            {
                statusLabel.Visible =true;
            }
        }

        private void DownloadProgressChanged(object sender, DownloadEventArgs e)
        {
            progressBar1.Value = e.Progress;
        }


        private void deleteBtn_Click(object sender, EventArgs e)
        {
            FireItemRemovedEvent();
        }

        private void openBtn_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(downloadTask.OutFilename);
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            downloadTask.StopDownload();
            if (MessageBox.Show("Do you want cancel the Download?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                downloadTask.CleanUp();
                FireItemRemovedEvent();
            }
            else
            {
                downloadTask.StartDownload();
            }

        }

        private void FireItemRemovedEvent()
        {
            if (DownloadItemRemoved != null)
            {
                if (DownloadItemRemoved.Target is Control)
                {
                    Control control = DownloadItemRemoved.Target as Control;
                    control.Invoke(DownloadItemRemoved, new object []{this, EventArgs.Empty});
                }
                else
                {
                    DownloadItemRemoved(this, EventArgs.Empty);
 
                }
            }
        }

        

        public delegate void DownloadItemRemoveEventHandler(object sender, EventArgs e);
        public event DownloadItemRemoveEventHandler DownloadItemRemoved;

      
    }
}
