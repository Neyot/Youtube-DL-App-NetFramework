using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Diagnostics;
using Youtube_DL_Wrapper;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;
using DialogResult = System.Windows.Forms.DialogResult;

namespace Youtube_DL_App {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public DownloadProgressBarValue DownloadProgressValue { get; private set; }

        private readonly ConsoleLogWindow consoleLogWindow = new ConsoleLogWindow();
        private string outputFolder;
        private string youtubeUrl;
        private bool downloadInProgress;
        private bool downloadFinishedTextVisible;

        public MainWindow(bool showConsoleLog) {
            InitializeComponent();
            if (showConsoleLog)
                consoleLogWindow.Show();

            outputFolder = Properties.Settings.Default["OutputFolder"].ToString();
            if (!System.IO.Path.IsPathRooted(outputFolder)) {
                if (outputFolder.StartsWith(".")) {
                    outputFolder = System.IO.Path.GetFullPath(System.IO.Directory.GetCurrentDirectory() + outputFolder);
                } else {
                    outputFolder = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), outputFolder);
                }
                
            }
            OutputFolderTextBox.Text = outputFolder;
            youtubeUrl = Properties.Settings.Default["YoutubeUrl"].ToString();
            YoutubeUrlTextBox.Text = youtubeUrl;
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
                if (!System.IO.Directory.Exists(outputFolder)) {
                    MessageBoxResult result = MessageBox.Show("Output Folder does not exist. Create it?", "Youtube DL Wrapper", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                    switch (result) {
                        case MessageBoxResult.Yes:
                            Directory.CreateDirectory(outputFolder);
                            break;
                        case MessageBoxResult.No:
                            MessageBox.Show("Not creating folder and stopping download.", "Youtube DL Wrapper", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
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
                AudioDownloader AudioDL = new AudioDownloader(youtubeUrl, outputFolder);
                AudioDL.ConsoleLog += AudioDL_ConsoleLog;
                AudioDL.DownloadProgress += AudioDL_DownloadProgress;
                AudioDL.ConvertingProgress += AudioDL_ConvertingProgress;
                Task t = Task.Run(() => AudioDL.StartDownload());
            }
        }

        private void OutputFolderButton_Click(object sender, RoutedEventArgs e) {
            if (!downloadInProgress) {
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                folderBrowserDialog.Description = "Select the directory that you want to output the downloaded file(s) to.";
                folderBrowserDialog.ShowNewFolderButton = true;
                if (System.IO.Directory.Exists(outputFolder)) {
                    folderBrowserDialog.SelectedPath = outputFolder;
                } else {
                    folderBrowserDialog.SelectedPath = System.IO.Directory.GetCurrentDirectory();
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
            if (System.IO.Directory.Exists(outputFolder)) {
                Process.Start("explorer.exe", outputFolder);
            }
        }

        private void AudioDL_ConsoleLog(object sender, ConsoleLogEventArgs e) {
            consoleLogWindow.Dispatcher.Invoke(() => {
                consoleLogWindow.ConsoleLog.AppendText(e.LatestLine + Environment.NewLine);
                consoleLogWindow.ConsoleLog.ScrollToEnd();
            });
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
    }

    public class DownloadProgressBarValue : INotifyPropertyChanged {
        private double data;
        
        public double Data {
            get => data;
            set {
                if (value != data) {
                    data = value;
                    NotifyPropertyChanged("Data");
                }
            }
        }

        protected void NotifyPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
