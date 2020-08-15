﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using CmlLib;
using CmlLib.Core;
using System.Diagnostics;
using CmlLib.Core.Downloader;
using CmlLib.Core.Auth;
using CmlLib.Core.Version;

namespace CmlLibWinFormSample
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        bool useMJava = true;
        string javaPath = "java.exe";

        MinecraftPath MinecraftPath;
        MVersionCollection Versions;
        MSession Session = MSession.GetOfflineSession("test_user");

        GameLog logForm;

        private void InitializeLauncher(MinecraftPath path)
        {
            txtPath.Text = path.BasePath;
            MinecraftPath = path;

            var th = new Thread(new ThreadStart(delegate
            {
                Versions = MVersionLoader.GetVersionMetadatas(MinecraftPath);
                Invoke(new Action(() =>
                {
                    cbVersion.Items.Clear();
                    foreach (var item in Versions)
                    {
                        cbVersion.Items.Add(item.Name);
                    }

                    btnSetLastVersion_Click(null, null);
                }));
            }));
            th.Start();
        }

        private void SetSession(MSession session)
        {
            lbUsername.Text = session.Username;
            this.Session = session;
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            // Initialize launcher

            var defaultPath = new MinecraftPath(MinecraftPath.GetOSDefaultPath());
            InitializeLauncher(defaultPath);
        }

        private void btnChangePath_Click(object sender, EventArgs e)
        {
            var form = new PathForm(MinecraftPath);
            form.ShowDialog();
            InitializeLauncher(form.MinecraftPath);

            if (useMJava)
                lbJavaPath.Text = MinecraftPath.Runtime;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            var form = new LoginForm(Session);
            form.ShowDialog();
            SetSession(form.Session);
        }

        private void btnChangeJava_Click(object sender, EventArgs e)
        {
            var form = new JavaForm(useMJava, MinecraftPath.Runtime, javaPath);
            form.ShowDialog();

            useMJava = form.UseMJava;
            MinecraftPath.Runtime = form.MJavaDirectory;
            javaPath = form.JavaBinaryPath;

            if (useMJava)
                lbJavaPath.Text = form.MJavaDirectory;
            else
                lbJavaPath.Text = form.JavaBinaryPath;
        }

        private void btnSetLastVersion_Click(object sender, EventArgs e)
        {
            cbVersion.Text = Versions.LatestReleaseVersion?.Name;
        }

        private void btnAutoRamSet_Click(object sender, EventArgs e)
        {
            var computerMemory = Util.GetMemoryMb();
            if (computerMemory == null)
            {
                MessageBox.Show("Failed to get computer memory");
                return;
            }

            var max = computerMemory / 2;
            if (max < 1024)
                max = 1024;
            else if (max > 8192)
                max = 8192;

            var min = max / 10;

            TxtXmx.Text = max.ToString();
            txtXms.Text = min.ToString();
        }

        private MLaunchOption createLaunchOption()
        {
            try
            {
                var launchOption = new MLaunchOption()
                {
                    Path = MinecraftPath,

                    MaximumRamMb = int.Parse(TxtXmx.Text),
                    Session = this.Session,

                    VersionType = Txt_VersionType.Text,
                    GameLauncherName = Txt_GLauncherName.Text,
                    GameLauncherVersion = Txt_GLauncherVersion.Text,

                    ServerIp = Txt_ServerIp.Text,

                    DockName = Txt_DockName.Text,
                    DockIcon = Txt_DockIcon.Text
                };

                if (!useMJava)
                    launchOption.JavaPath = javaPath;

                if (!string.IsNullOrEmpty(txtXms.Text))
                    launchOption.MinimumRamMb = int.Parse(txtXms.Text);

                if (!string.IsNullOrEmpty(Txt_ServerPort.Text))
                    launchOption.ServerPort = int.Parse(Txt_ServerPort.Text);

                if (!string.IsNullOrEmpty(Txt_ScWd.Text) && !string.IsNullOrEmpty(Txt_ScHt.Text))
                {
                    launchOption.ScreenHeight = int.Parse(Txt_ScHt.Text);
                    launchOption.ScreenWidth = int.Parse(Txt_ScWd.Text);
                }

                if (!string.IsNullOrEmpty(Txt_JavaArgs.Text))
                    launchOption.JVMArguments = Txt_JavaArgs.Text.Split(' ');

                return launchOption;
            }
            catch (Exception ex) // exceptions. like FormatException in int.Parse
            {
                MessageBox.Show("Failed to create MLaunchOption\n\n" + ex.ToString());
                return null;
            }
        }

        private void Btn_Launch_Click(object sender, EventArgs e)
        {
            // Launch

            if (Session == null)
            {
                MessageBox.Show("Login First");
                return;
            }

            if (cbVersion.Text == "")
            {
                MessageBox.Show("Select Version");
                return;
            }

            // disable ui
            groupBox1.Enabled = false;
            groupBox2.Enabled = false;
            groupBox3.Enabled = false;
            groupBox4.Enabled = false;

            // create LaunchOption
            var launchOption = createLaunchOption();
            if (launchOption == null)
                return;

            var version = cbVersion.Text;
            var useParallel = rbParallelDownload.Checked;
            var checkHash = cbCheckFileHash.Checked;
            var downloadAssets = !cbSkipAssetsDownload.Checked;

            var th = new Thread(() =>
            {
                try
                {
                    if (useMJava) // Download Java
                    {
                        var mjava = new MJava(MinecraftPath.Runtime);
                        mjava.ProgressChanged += Launcher_ProgressChanged;

                        var javapath = mjava.CheckJava();
                        launchOption.JavaPath = javapath;
                    }

                    MVersion versionInfo = Versions.GetVersion(version); // Get Version Info
                    launchOption.StartVersion = versionInfo;

                    MDownloader downloader; // Create Downloader
                    if (useParallel)
                        downloader = new MParallelDownloader(MinecraftPath, versionInfo, 10, true);
                    else
                        downloader = new MDownloader(MinecraftPath, versionInfo);

                    downloader.ChangeFile += Launcher_FileChanged;
                    downloader.ChangeProgress += Launcher_ProgressChanged;
                    downloader.CheckHash = checkHash;
                    downloader.DownloadAll(downloadAssets);

                    var launch = new MLaunch(launchOption); // Create Arguments and Process
                    var process = launch.GetProcess();

                    StartProcess(process); // Start Process with debug options

                    // or just start process
                    // process.Start();
                }
                catch (MDownloadFileException mex) // download exception
                {
                    MessageBox.Show(
                        $"FileName : {mex.ExceptionFile.Name}\n" +
                        $"FilePath : {mex.ExceptionFile.Path}\n" +
                        $"FileUrl : {mex.ExceptionFile.Url}\n" +
                        $"FileType : {mex.ExceptionFile.Type}\n\n" +
                        mex.ToString());
                }
                catch (Win32Exception wex) // java exception
                {
                    MessageBox.Show(wex.ToString() + "\n\nIt seems your java setting has problem");
                }
                catch (Exception ex) // all exception
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    Invoke(new Action(() =>
                    {
                        // re open log form
                        if (logForm != null)
                            logForm.Close();

                        logForm = new GameLog();
                        logForm.Show();

                        // enable ui
                        groupBox1.Enabled = true;
                        groupBox2.Enabled = true;
                        groupBox3.Enabled = true;
                        groupBox4.Enabled = true;
                    }));
                }
            });
            th.Start();
        }

        private void Launcher_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Invoke(new Action(() =>
            {
                Pb_Progress.Value = e.ProgressPercentage;
            }));
        }

        private void Launcher_FileChanged(DownloadFileChangedEventArgs e)
        {
            Invoke(new Action(() =>
            {
                Pb_File.Maximum = e.TotalFileCount;
                Pb_File.Value = e.ProgressedFileCount;
                Lv_Status.Text = $"{e.FileKind.ToString()} : {e.FileName} ({e.ProgressedFileCount}/{e.TotalFileCount})";
            }));
        }

        private void StartProcess(Process process)
        {
            File.WriteAllText("launcher.txt", process.StartInfo.Arguments);
            output(process.StartInfo.Arguments);

            // process options to display game log

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.EnableRaisingEvents = true;
            process.ErrorDataReceived += Process_ErrorDataReceived;
            process.OutputDataReceived += Process_OutputDataReceived;

            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            output(e.Data);
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            output(e.Data);
        }

        void output(string msg)
        {
            GameLog.AddLog(msg);
        }

        private void btnChangelog_Click(object sender, EventArgs e)
        {
            // Game Changelog
            var f = new ChangeLog();
            f.Show();
        }

        private void btnMojangServer_Click(object sender, EventArgs e)
        {
            // Mojang Server
            var f = new MojangServerForm();
            f.Show();
        }

        private void btnOptions_Click(object sender, EventArgs e)
        {
            // options.txt
            var path = System.IO.Path.Combine(MinecraftPath.BasePath, "options.txt");
            var f = new GameOptions(path);
            f.Show();
        }

        private void btnGithub_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://github.com/AlphaBs/CmlLib.Core");
            }
            catch
            {

            }
        }

        private void btnWiki_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://github.com/AlphaBs/CmlLib.Core/wiki/MLaunchOption");
            }
            catch
            {

            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void btnForgeInstall_Click(object sender, EventArgs e)
        {
            MessageBox.Show("not implemented");
        }
    }
}
