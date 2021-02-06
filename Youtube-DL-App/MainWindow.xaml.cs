using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using Youtube_DL_Wrapper;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;
using DialogResult = System.Windows.Forms.DialogResult;

namespace Youtube_DL_App {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public DownloadProgressBarValue DownloadProgressValue { get; set; }

        private readonly ConsoleLogWindow consoleLogWindow = new ConsoleLogWindow();
        private string outputFolder;
        private string youtubeUrl;

        public MainWindow() {
            InitializeComponent();
            consoleLogWindow.Show();

            outputFolder = Properties.Settings.Default["OutputFolder"].ToString();
            if (!System.IO.Path.IsPathRooted(outputFolder)) {
                outputFolder = System.IO.Path.GetFullPath(System.IO.Directory.GetCurrentDirectory() + outputFolder);
            }
            OutputFolderTextBox.Text = outputFolder;
            youtubeUrl = Properties.Settings.Default["YoutubeUrl"].ToString();
            YoutubeUrlTextBox.Text = youtubeUrl;

            Binding binding = new Binding("Data");
            binding.Source = DownloadProgressValue;
            DownloadProgressBar.SetBinding(ProgressBar.ValueProperty, binding);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e) {
            base.OnMouseDown(e);
            Keyboard.ClearFocus();
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }

        private void OutputFolderButton_Click(object sender, RoutedEventArgs e) {
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

        private void YoutubeUrlButton_Click(object sender, RoutedEventArgs e) {
            DownloadProgressBar.Visibility = Visibility.Visible;
            DownloadProgressBarText.Visibility = Visibility.Visible;
            AudioDownloader AudioDL = new AudioDownloader(youtubeUrl, outputFolder);
            AudioDL.ConsoleLog += AudioDL_ConsoleLog;
            AudioDL.DownloadProgress += AudioDL_DownloadProgress;
            Task t = Task.Run(() => AudioDL.StartDownload());
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

        private void YoutubeUrlTextBox_LostFocus(object sender, RoutedEventArgs e) {
            if (YoutubeUrlTextBox.Text != youtubeUrl) {
                youtubeUrl = YoutubeUrlTextBox.Text;
                Properties.Settings.Default["YoutubeUrl"] = youtubeUrl;
                Properties.Settings.Default.Save();
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
