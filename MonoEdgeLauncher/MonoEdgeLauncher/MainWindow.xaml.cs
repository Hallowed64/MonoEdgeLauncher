using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;

namespace MonoEdgeLauncher
{
    enum LauncherStatus
    { 
        ready,
        failed,
        downloadingGame,
        downloadingUpdate
    }

    public partial class MainWindow : Window
    {
        private string rootPath;
        private string versionFile;
        private string gameZip;
        private string gameExe;

        private LauncherStatus _status;
        internal LauncherStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                switch (_status)
                {
                    case LauncherStatus.ready:
                        PlayButton.Visibility = Visibility.Visible;
                        DownloadingButton.Visibility = Visibility.Hidden;
                        ErrorButton.Visibility = Visibility.Hidden;
                        break;
                    case LauncherStatus.failed:
                        PlayButton.Visibility = Visibility.Hidden;
                        DownloadingButton.Visibility = Visibility.Hidden;
                        ErrorButton.Visibility = Visibility.Visible;
                        break;
                    case LauncherStatus.downloadingGame:
                        PlayButton.Visibility = Visibility.Hidden;
                        DownloadingButton.Visibility = Visibility.Visible;
                        ErrorButton.Visibility = Visibility.Hidden;
                        break;
                    case LauncherStatus.downloadingUpdate:
                        PlayButton.Visibility = Visibility.Hidden;
                        DownloadingButton.Visibility = Visibility.Visible;
                        ErrorButton.Visibility = Visibility.Hidden;
                        break;
                    default:
                        break;
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            rootPath = Directory.GetCurrentDirectory();
            versionFile = Path.Combine(rootPath, "Version.txt");
            gameZip = Path.Combine(rootPath, "Build.zip");
            gameExe = Path.Combine(rootPath, "Monoedge", "Monoedge.exe");
        }

        private void CheckForUpdates()
        {
            if (File.Exists(versionFile))
            {
                Version localVersion = new Version(File.ReadAllText(versionFile));
                VersionText.Text = localVersion.ToString();

                try
                {
                    WebClient webClient = new WebClient();
                    Version onlineVersion = new Version(webClient.DownloadString("https://www.dropbox.com/s/gdlyo09fhza6ki8/Version.txt?dl=1"));

                    if (onlineVersion.IsDifferent(localVersion))
                    {
                        InstallGameFiles(true, onlineVersion);
                    }
                    else
                    {
                        Status = LauncherStatus.ready;
                    }
                }
                catch (Exception ex)
                {
                    Status = LauncherStatus.failed;
                    MessageBox.Show($"Error checking for game updates: {ex}");
                }
            }
            else
            {
                InstallGameFiles(false, Version.zero);
            }
        }

        private void InstallGameFiles(bool _isUpdate, Version _onlineVersion)
        {
            try
            {
                WebClient webClient = new WebClient();
                if (_isUpdate)
                {
                    Status = LauncherStatus.downloadingUpdate;
                }
                else
                {
                    Status = LauncherStatus.downloadingGame;
                    _onlineVersion = new Version(webClient.DownloadString("https://www.dropbox.com/s/gdlyo09fhza6ki8/Version.txt?dl=1"));
                }

                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadGameCompletedCallback);
                webClient.DownloadFileAsync(new Uri("https://www.dropbox.com/s/r21qx6nmjwgn19r/Build.zip?dl=1"), gameZip, _onlineVersion);
            }
            catch (Exception ex)
            {
                Status = LauncherStatus.failed;
                MessageBox.Show($"Error installing game files: {ex}");
            }
        }

        private void DownloadGameCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                string onlineVersion = ((Version)e.UserState).ToString();
                string pathString = Path.Combine(rootPath, "Monoedge");
                bool directoryExists = Directory.Exists(pathString);
                if (directoryExists)
                {
                    Directory.Delete(pathString, true);
                    Directory.CreateDirectory(pathString);
                }
                else if (!directoryExists)
                {
                    Directory.CreateDirectory(pathString);
                }
                ZipFile.ExtractToDirectory(gameZip, pathString);
                File.Delete(gameZip);
                File.WriteAllText(versionFile, onlineVersion);

                VersionText.Text = onlineVersion;
                Status = LauncherStatus.ready;
            }
            catch (Exception ex)
            {
                Status = LauncherStatus.failed;
                MessageBox.Show($"Error finishing download: {ex}");
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            CheckForUpdates();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(gameExe) && Status == LauncherStatus.ready)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(gameExe);
                startInfo.WorkingDirectory = Path.Combine(rootPath, "Monoedge");
                Process.Start(startInfo);

                Close();
            }
            else if (Status == LauncherStatus.failed)
            {
                CheckForUpdates();
            }
        }

        private void AccountButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DataButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }

    struct Version
    {
        internal static Version zero = new Version(0, 0, 0);

        private short major;
        private short minor;
        private short subMinor;

        internal Version(short _major, short _minor, short _subMinor)
        {
            major = _major;
            minor = _minor;
            subMinor = _subMinor;
        }
        internal Version(string _version)
        {
            string[] _versionStrings = _version.Split('.');
            if(_versionStrings.Length != 3)
            {
                major = 0;
                minor = 0;
                subMinor = 0;
                return;
            }

            major = short.Parse(_versionStrings[0]);
            minor = short.Parse(_versionStrings[1]);
            subMinor = short.Parse(_versionStrings[2]);
        }

        internal bool IsDifferent(Version _otherVersion)
        {
            if(major != _otherVersion.major)
            {
                return true;
            }
            else
            {
                {
                    if(minor != _otherVersion.minor)
                    {
                        return true;
                    }
                    else
                    {
                        if(subMinor != _otherVersion.subMinor)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public override string ToString()
        {
            return $"{major}.{minor}.{subMinor}";
        }
    }
}
