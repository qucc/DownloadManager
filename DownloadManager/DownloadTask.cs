using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DownloadManager
{
    public class DownloadTask
    {
        private DownloadStatus _downloadState = DownloadStatus.NotDownloading;
        string _url;

        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }
        string _outFilename;

        public string OutFilename
        {
            get { return _outFilename; }
            set { _outFilename = value; }
        }

        string _tempOutputFilename;

        public string TempOutputFilename
        {
            get { return _tempOutputFilename; }
            set { _tempOutputFilename = value; }
        }
        private long _totalSize;
        private long _downloadSize = 0L;
        public static int DOWNLOAD_BLOCK = 50;
       
        public DownloadTask(string url, string outFilename)
        {
            _url = url;
            _outFilename = outFilename;
            _tempOutputFilename = Path.GetDirectoryName(_outFilename) + Path.DirectorySeparatorChar + "edu" + Path.GetRandomFileName() +".crdownload";
        }

        public void StartDownload()
        {
            lock (this)
            {
                if (_downloadState == DownloadStatus.NotDownloading)
                {
                    // Create a delegate to the calculation method.
                    DownloadDelegate download = new DownloadDelegate(Download);

                    // Start the calculation.
                    download.BeginInvoke(new AsyncCallback(EndDownload), download);

                    // Update the calculation status.
                    _downloadState = DownloadStatus.Downloading;

                    // Fire a status changed event.
                    FireStatusChangedEvent(_downloadState);
                }
            }
        }
        public void StopDownload()
        {
            lock (this)
            {
                if (_downloadState == DownloadStatus.Downloading)
                {
                    // Update the calculation status.
                    _downloadState = DownloadStatus.NotDownloading;

                    // Fire a status changed event.
                    FireStatusChangedEvent(_downloadState);
                }
            }
        }

        private void FireStatusChangedEvent(DownloadStatus status)
        {
            if (DownloadStatusChanged != null)
            {
                DownloadEventArgs args =
                    new DownloadEventArgs(status);
                if (DownloadStatusChanged.Target is
                        System.Windows.Forms.Control)
                {
                    System.Windows.Forms.Control targetForm = DownloadStatusChanged.Target
                            as System.Windows.Forms.Control;
                    targetForm.Invoke(DownloadStatusChanged,
                            new object[] { this, args });
                }
                else
                {
                    DownloadStatusChanged(this, args);
                }
            }
        }
        private void FireProgressChangedEvent(int progress)
        {
            if (DownloadProgressChanged != null)
            {
                DownloadEventArgs args =
                    new DownloadEventArgs(progress);
                if (DownloadProgressChanged.Target is
                        System.Windows.Forms.Control)
                {
                    Control targetForm = DownloadProgressChanged.Target
                            as System.Windows.Forms.Control;
                    targetForm.Invoke(DownloadProgressChanged,
                            new object[] { this, args });
                }
                else
                {
                    DownloadProgressChanged(this, args);
                }
            }
        }

        private void Download()
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_url);
            req.Method = "HEAD";
            try
            {
                HttpWebResponse resp = (HttpWebResponse)(req.GetResponse());
                long len = resp.ContentLength;
                _totalSize = len;
                Console.WriteLine("totalSize : " + _totalSize);

                req = (HttpWebRequest)WebRequest.Create(_url);
                req.AddRange(_downloadSize, _totalSize - 1);
                Console.Write("Resulting Request Headers: ");
                Console.WriteLine(req.Headers.ToString());

                // Assign the response object of 'HttpWebRequest' to a 'HttpWebResponse' variable.
                HttpWebResponse myHttpWebResponse = (HttpWebResponse)req.GetResponse();

                // Displays the headers in the response received
                Console.Write("Resulting Response Headers: ");
                Console.WriteLine(myHttpWebResponse.Headers.ToString());

                // Display the contents of the page to the console.
                Stream streamResponse = myHttpWebResponse.GetResponseStream();
                byte[] readBuffer = new byte[DOWNLOAD_BLOCK];
                int count = streamResponse.Read(readBuffer, 0, readBuffer.Length);
                using (Stream stream = new FileStream(_tempOutputFilename, FileMode.OpenOrCreate))
                {
                    stream.Seek(_downloadSize, SeekOrigin.Begin);
                    int p_progress = 0;
                    while (count > 0)
                    {
                        if (_downloadState == DownloadStatus.NotDownloading) break;
                        stream.Write(readBuffer, 0, count);

                        count = streamResponse.Read(readBuffer, 0, readBuffer.Length);
                        _downloadSize += count;
                        int progress = (int)(_downloadSize * 100 / _totalSize);
                        if (progress != p_progress)
                        {
                            p_progress = progress;
                            FireProgressChangedEvent(progress);
                        }
                    }
                    if (_downloadState == DownloadStatus.Downloading)
                    {
                        FireProgressChangedEvent(100);
                    }
                }
                // Release the response object resources.
                streamResponse.Close();
                myHttpWebResponse.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                _downloadState = DownloadStatus.Error;
                CleanUp();
            }
            if (_downloadState == DownloadStatus.Downloading)
            {
                File.Move(_tempOutputFilename, _outFilename);
                _downloadState = DownloadStatus.Completed;
            }
        }

        public void CleanUp()
        {
            File.Delete(_tempOutputFilename);
        }


        private void EndDownload(IAsyncResult ar)
        {
            DownloadDelegate del = (DownloadDelegate)ar.AsyncState;

            lock (this)
            {
                
                FireStatusChangedEvent(_downloadState);
            }
        }


        private delegate void DownloadDelegate();

        public delegate void DownloadStatusEventHandler(
                        object sender, DownloadEventArgs e);

        public delegate void DownloadProgressEventHandler(
                        object sender, DownloadEventArgs e);

        public event DownloadStatusEventHandler DownloadStatusChanged;
        public event DownloadProgressEventHandler DownloadProgressChanged;
    }

    public enum DownloadStatus
    {
        NotDownloading,
        Downloading,
        Completed,
        Error
    }

    public class DownloadEventArgs : EventArgs
    {
        public string Result;
        public int Progress;
        public DownloadStatus Status;

        public DownloadEventArgs(int progress)
        {
            this.Progress = progress;
            this.Status = DownloadStatus.Downloading;
        }

        public DownloadEventArgs(DownloadStatus status)
        {
            this.Status = status;
        }
    }    
}
