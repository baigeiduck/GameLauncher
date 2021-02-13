﻿using CmlLib.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CmlLib.Core.Downloader
{
    public class ParallelDownloader : IDownloader
    {
        public event DownloadFileChangedHandler ChangeFile;
        public event ProgressChangedEventHandler ChangeProgress;

        public int MaxThread { get; set; } = 10;
        public bool IgnoreInvalidFiles { get; set; } = true;

        int total = 0;
        int progressed = 0;

        bool isRunning = false;

        public Task DownloadFiles(DownloadFile[] files)
        {
            if (isRunning)
                throw new InvalidOperationException("already downloading");

            total = files.Length;
            progressed = 0;

            return Task.Run(() =>
            {
                Parallel.ForEach(
                    files,
                    new ParallelOptions { MaxDegreeOfParallelism = MaxThread },
                    doDownload);

                isRunning = false;
            });
        }

        private void doDownload(DownloadFile file)
        {
            //doDownload(file, 0);

            try
            {
                var downloader = new WebDownload();
                Console.WriteLine("start " + file.Name);
                downloader.DownloadFileLimit(file.Url, file.Path);
                Console.WriteLine("end " + file.Name);

                Interlocked.Increment(ref progressed);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            finally
            {
                Task.Run(() =>
                {
                    fireDownloadFileChangedEvent(file.Type, file.Name, total, progressed);
                });
            }
        }

        private bool doDownload(DownloadFile file, int failedCount)
        {
            try
            {
                if (failedCount > 2)
                    return false;

                var downloader = new WebDownload();
                Console.WriteLine("start " + file.Name);
                downloader.DownloadFileLimit(file.Url, file.Path);
                Console.WriteLine("end " + file.Name);

                Interlocked.Increment(ref progressed);

                var ev = Task.Run(() =>
                {
                    fireDownloadFileChangedEvent(file.Type, file.Name, total, progressed);
                });
                return true;
            }
            catch (Exception ex)
            {
                //System.Diagnostics.Debug.WriteLine(ex);
                failedCount++;

                return doDownload(file, failedCount);
            }
        }

        private void fireDownloadFileChangedEvent(MFile file, string name, int totalFiles, int progressedFiles)
        {
            var e = new DownloadFileChangedEventArgs(file, name, totalFiles, progressedFiles);
            fireDownloadFileChangedEvent(e);
        }

        private void fireDownloadFileChangedEvent(DownloadFile file, int totalFiles, int progressedFiles)
        {
            fireDownloadFileChangedEvent(file.Type, file.Name, totalFiles, progressedFiles);
        }

        private void fireDownloadFileChangedEvent(DownloadFileChangedEventArgs e)
        {
            ChangeFile?.Invoke(e);
        }

        private void fireDownloadProgressChangedEvent(object sender, ProgressChangedEventArgs e)
        {
            ChangeProgress?.Invoke(this, e);
        }
    }
}