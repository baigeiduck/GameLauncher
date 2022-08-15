using CmlLib.Utils;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using TLSP.GameLauncher.Core.Downloader;

namespace CmlLib.Core.Downloader
{
    public class SequenceDownloader : IDownloader
    {
        public bool IgnoreInvalidFiles { get; set; } = true;
        private IProgress<ProgressChangedEventArgs>? pChangeProgress;

        public async Task DownloadFiles(DownloadFile[] files, 
            IProgress<DownloadFileChangedEventArgs>? fileProgress,
            IProgress<ProgressChangedEventArgs>? downloadProgress)
        {
            if (files.Length == 0)
                return;

            pChangeProgress = downloadProgress;

            WebDownload downloader = new WebDownload();
            downloader.FileDownloadProgressChanged += Downloader_FileDownloadProgressChanged;

            fileProgress?.Report(
                new DownloadFileChangedEventArgs(files[0].Type, this, null, files.Length, 0));

            for (int i = 0; i < files.Length; i++)
            {
                DownloadFile file = files[i];

                string[] urls = DownloadCDN.AppleCDN(file.Url);
                Exception exception = null;
                foreach (string url in urls)
                {
                    try
                    {
                        file.Url = url;

                        var directoryPath = Path.GetDirectoryName(file.Path);
                        if (!string.IsNullOrEmpty(directoryPath))
                            Directory.CreateDirectory(directoryPath);

                        await downloader.DownloadFileAsync(file).ConfigureAwait(false);

                        if (file.AfterDownload != null)
                        {
                            foreach (var item in file.AfterDownload)
                            {
                                await item().ConfigureAwait(false);
                            }
                        }

                        fileProgress?.Report(
                            new DownloadFileChangedEventArgs(file.Type, this, file.Name, files.Length, i));

                        exception = null;
                        break;
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                    }
                }
                if (exception != null)
                {
                    if (!IgnoreInvalidFiles)
                        throw new MDownloadFileException(exception.Message, exception, files[i]);
                }
            }
        }

        private void Downloader_FileDownloadProgressChanged(object? sender, DownloadFileProgress e)
        {
            pChangeProgress?.Report(new ProgressChangedEventArgs(e.ProgressPercentage, null));
        }
    }
}
