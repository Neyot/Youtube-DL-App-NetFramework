using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.ComponentModel;
using System.Diagnostics;
using Serilog;
using Youtube_DL_Wrapper;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;
using DialogResult = System.Windows.Forms.DialogResult;

namespace Youtube_DL_App {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private readonly ConsoleLogWindow consoleLogWindow = new ConsoleLogWindow();
        private readonly Task verifyBinaryPathTask;
        private string binaryPath;
        private string outputFolder;
        private string youtubeUrl;
        private bool downloadInProgress;
        private bool downloadFinishedTextVisible;

        public MainWindow(bool showConsoleLog) {
            InitializeComponent();
            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            //DateTime buildDate = new DateTime(2000, 1, 1).AddDays(version.Build).AddSeconds(version.Revision * 2);
            //ConsoleLog($"Version: {version} ({buildDate})");
            //Log.Debug($"Version: {version} ({buildDate})");
            ConsoleLog($"Version: {version}");
            Log.Debug($"Version: {version}");
            if (showConsoleLog)
                consoleLogWindow.Show();

            outputFolder = Properties.Settings.Default["OutputFolder"].ToString();
            if (!Path.IsPathRooted(outputFolder)) {
                if (outputFolder.StartsWith(".")) {
                    outputFolder = Path.GetFullPath(Directory.GetCurrentDirectory() + outputFolder);
                } else {
                    outputFolder = Path.Combine(Directory.GetCurrentDirectory(), outputFolder);
                }
                
            }
            OutputFolderTextBox.Text = outputFolder;
            youtubeUrl = Properties.Settings.Default["YoutubeUrl"].ToString();
            YoutubeUrlTextBox.Text = youtubeUrl;

            verifyBinaryPathTask = Task.Run(() => VerifyBinaryPath());
        }

        public MainWindow() : this(false) { }

        protected override void OnMouseDown(MouseButtonEventArgs e) {
            base.OnMouseDown(e);
            Keyboard.ClearFocus();
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }

        private void YoutubeUrlButton_Click(object sender, RoutedEventArgs e) {
            if (!downloadInProgress) {
                verifyBinaryPathTask?.Wait();

                if (!this.IsBinaryPathValid) {
                    MessageBox.Show($"ERROR: Cannot find file: \n\"{binaryPath}\". \n\nWill not be able to download until a valid .exe is found.", $"{this.Title}", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    return;
                }

                foreach (var prerequisite in new string[] { "ffmpeg.exe", "ffplay.exe", "ffprobe.exe" }) {
                    if (!File.Exists(Path.Combine(Path.GetDirectoryName(binaryPath), prerequisite))) {
                        MessageBoxResult result = MessageBox.Show($"WARNING: Cannot find youtube-dl.exe prequisite \"{prerequisite}\" in folder: \n\"{Path.GetDirectoryName(binaryPath)}\". \n\nDownloading may not work as intended. Please see <Youtube-DL-App GitHub installation instructions> for instructions on how to download these prequisites. \n\nContinue with download?", $"{this.Title}", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel);
                        switch (result) {
                            case MessageBoxResult.Yes:
                                continue;
                            case MessageBoxResult.No:
                            case MessageBoxResult.Cancel:
                                MessageBox.Show("Cancelling download.", $"{this.Title}", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                                return;
                        }
                    }
                }

                if (!Directory.Exists(outputFolder)) {
                    MessageBoxResult result = MessageBox.Show("Output Folder does not exist. Create it?", $"{this.Title}", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                    switch (result) {
                        case MessageBoxResult.Yes:
                            Directory.CreateDirectory(outputFolder);
                            break;
                        case MessageBoxResult.No:
                            MessageBox.Show("Not creating folder and stopping download.", $"{this.Title}", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                            return;
                    }
                }

                downloadFinishedTextVisible = false;
                downloadInProgress = true;
                this.Dispatcher.Invoke(() => {
                    DownloadProgressBarText.Text = "Starting download...";
                    DownloadProgressBar.Visibility = Visibility.Visible;
                    DownloadProgressBarText.Visibility = Visibility.Visible;
                });
                AudioDownloader AudioDL = new AudioDownloader(youtubeUrl, binaryPath, outputFolder);
                AudioDL.ConsoleLog += AudioDL_ConsoleLog;
                AudioDL.DownloadProgress += AudioDL_DownloadProgress;
                AudioDL.ConvertingProgress += AudioDL_ConvertingProgress;
                Task t = Task.Run(() => AudioDL.StartDownload());
            }
        }

        private void OutputFolderButton_Click(object sender, RoutedEventArgs e) {
            ConsoleLog($"outputFolder: {outputFolder}");
            Log.Debug($"outputFolder: {outputFolder}");
            if (!downloadInProgress) {
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog {
                    Description = "Select the directory that you want to output the downloaded file(s) to.",
                    ShowNewFolderButton = true
                };
                if (Directory.Exists(outputFolder)) {
                    folderBrowserDialog.SelectedPath = outputFolder;
                } else {
                    folderBrowserDialog.SelectedPath = Directory.GetCurrentDirectory();
                }
                DialogResult result = folderBrowserDialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK) {
                    outputFolder = folderBrowserDialog.SelectedPath;
                    OutputFolderTextBox.Text = outputFolder;
                    Properties.Settings.Default["OutputFolder"] = outputFolder;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void OpenFolderButton_Click(object sender, RoutedEventArgs e) {
            if (Directory.Exists(outputFolder)) {
                Process.Start("explorer.exe", outputFolder);
            }
        }

        private void ChooseBinaryFolderButton_Click(object sender, RoutedEventArgs e) {
            ConsoleLog($"binaryPath: {binaryPath}");
            Log.Debug($"binaryPath: {binaryPath}");
            if (!downloadInProgress) {
                string binaryDirectory;
                if (Path.GetExtension(binaryPath) == ".exe")
                    binaryDirectory = Path.GetDirectoryName(binaryPath);
                else
                    binaryDirectory = binaryPath;
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog {
                    Description = "Select the directory that contains the 4 youtube-dl and ffmpeg exe's.",
                    ShowNewFolderButton = false,
                    SelectedPath = binaryDirectory
                };
                DialogResult result = folderBrowserDialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK) {
                    var combinedPath = Path.Combine(folderBrowserDialog.SelectedPath, "youtube-dl.exe");
                    binaryPath = combinedPath;
                    if (File.Exists(combinedPath)) {
                        Properties.Settings.Default["BinaryPath"] = binaryPath;
                        Properties.Settings.Default.Save();
                    } else {
                        MessageBox.Show($"ERROR: Cannot find file \"youtube-dl.exe\" in folder: \"{folderBrowserDialog.SelectedPath}\". \n\nWill not be able to download until a valid .exe is found.", $"{this.Title}", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    }
                }
            }
        }

        private void AudioDL_ConsoleLog(object sender, ConsoleLogEventArgs e) {
            ConsoleLog(e.Data);
            Log.Debug(e.Data);
        }

        private void AudioDL_DownloadProgress(object sender, DownloadProgressEventArgs e) {
            this.Dispatcher.Invoke(() => {
                DownloadProgressBar.Value = e.Progress;
                DownloadProgressBarText.Text = e.ProgressText;
            });
        }

        private void AudioDL_ConvertingProgress(object sender, ConvertingProgressEventArgs e) {
            if (!e.Finished) {
                this.Dispatcher.Invoke(() => {
                    Storyboard sb = this.FindResource("ProgressBarLoadingStoryboard") as Storyboard;
                    sb.Begin();
                });
            } else if (e.Finished) {
                this.Dispatcher.Invoke(() => {
                    Storyboard sb = this.FindResource("ProgressBarLoadingStoryboard") as Storyboard;
                    sb.Stop();
                    DownloadProgressBar.Visibility = Visibility.Hidden;
                    DownloadProgressBar.Value = 0;
                    DownloadProgressBarText.Text = "Download finished.";
                    downloadInProgress = false;
                    downloadFinishedTextVisible = true;
                });
            }
        }

        private void YoutubeUrlTextBox_LostFocus(object sender, RoutedEventArgs e) {
            if (YoutubeUrlTextBox.Text != youtubeUrl) {
                youtubeUrl = YoutubeUrlTextBox.Text;
                Properties.Settings.Default["YoutubeUrl"] = youtubeUrl;
                Properties.Settings.Default.Save();
                if (downloadFinishedTextVisible) {
                    downloadFinishedTextVisible = false;
                    this.Dispatcher.Invoke(() => {
                        DownloadProgressBarText.Visibility = Visibility.Hidden;
                    });
                }
            }
        }

        private void OutputFolderTextBox_LostFocus(object sender, RoutedEventArgs e) {
            if (OutputFolderTextBox.Text != outputFolder) {
                outputFolder = OutputFolderTextBox.Text;
                Properties.Settings.Default["OutputFolder"] = outputFolder;
                Properties.Settings.Default.Save();
            }
        }

        private bool IsBinaryPathValid => Path.GetFileName(binaryPath) == "youtube-dl.exe" && File.Exists(binaryPath);

        private void VerifyBinaryPath() {
            string binaryPathSetting = Properties.Settings.Default["BinaryPath"].ToString();

            if (File.Exists(binaryPathSetting)) {
                binaryPath = Path.GetFullPath(binaryPathSetting);
                Properties.Settings.Default["BinaryPath"] = binaryPath;
                Properties.Settings.Default.Save();
                return;
            }

            string binary = "youtube-dl.exe";

            if (File.Exists(binary)) {
                binaryPath = Path.GetFullPath(binary);
                Properties.Settings.Default["BinaryPath"] = binaryPath;
                Properties.Settings.Default.Save();
                return;
            }

            var values = Environment.GetEnvironmentVariable("PATH");
            foreach (var path in values.Split(Path.PathSeparator)) {
                var fullPath = Path.Combine(path, binary);
                if (File.Exists(fullPath)) {
                    binaryPath = fullPath;
                    Properties.Settings.Default["BinaryPath"] = binaryPath;
                    Properties.Settings.Default.Save();
                    return;
                }
            }

            var folders = Directory.GetDirectories(Directory.GetCurrentDirectory(), "*", SearchOption.TopDirectoryOnly);
            foreach (var path in folders) {
                var fullPath = Path.Combine(path, binary);
                if (File.Exists(fullPath)) {
                    binaryPath = fullPath;
                    Properties.Settings.Default["BinaryPath"] = binaryPath;
                    Properties.Settings.Default.Save();
                    return;
                }
            }

            this.Dispatcher.Invoke(() => {
                MessageBox.Show($"ERROR: Cannot locate a \"youtube-dl.exe\" executable in current directory or PATH. \n\nClick the button \"{ChooseBinaryFolderButton.Content}\" to choose a folder containing the executable.", $"{this.Title}", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            });
        }

        public void ConsoleLog(string text) {
            consoleLogWindow.Dispatcher.Invoke(() => {
                consoleLogWindow.ConsoleLogTextBox.AppendText(text + Environment.NewLine);
                consoleLogWindow.ConsoleLogTextBox.ScrollToEnd();
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null) {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
