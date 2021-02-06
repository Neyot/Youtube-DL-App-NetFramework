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
using Microsoft.Win32;
using Youtube_DL_Wrapper;

namespace Youtube_DL_App {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public DownloadProgressBarValue DownloadProgressValue { get; set; }

        private readonly ConsoleLogWindow consoleLogWindow = new ConsoleLogWindow();
        private string outputFolder;

        public MainWindow() {
            InitializeComponent();
            consoleLogWindow.Show();
            outputFolder = Properties.Settings.Default["OutputFolder"].ToString();
            if (!System.IO.Path.IsPathRooted(outputFolder)) {
                outputFolder = System.IO.Path.GetFullPath(System.IO.Directory.GetCurrentDirectory() + outputFolder);
            }
            OutputFolderTextBox.Text = outputFolder;

            Binding binding = new Binding("Data");
            binding.Source = DownloadProgressValue;
            downloadProgressBar.SetBinding(ProgressBar.ValueProperty, binding);
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }

        private void OutputFolderButton_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
            outputFolder = OutputFolderTextBox.Text;
            Properties.Settings.Default["OutputFolder"] = outputFolder;
            Properties.Settings.Default.Save();
        }

        private void YoutubeUrlButton_Click(object sender, RoutedEventArgs e) {
            downloadProgressBar.Visibility = Visibility.Visible;
            downloadProgressBarText.Visibility = Visibility.Visible;
            AudioDownloader AudioDL = new AudioDownloader("https://www.youtube.com/watch?v=dvgZkm1xWPE", @"..\test downloads");
            AudioDL.ConsoleLog += AudioDL_ConsoleLog;
            AudioDL.DownloadProgress += AudioDL_DownloadProgress;
            Task t = Task.Run(() => AudioDL.StartDownload());
        }

        private void AudioDL_ConsoleLog(object sender, ConsoleLogEventArgs e) {
            consoleLogWindow.Dispatcher.Invoke(() => {
                consoleLogWindow.ConsoleLog.AppendText(e.LatestLine + Environment.NewLine);
            });
        }

        private void AudioDL_DownloadProgress(object sender, DownloadProgressEventArgs e) {
            this.Dispatcher.Invoke(() => {
                downloadProgressBar.Value = e.Progress;
                downloadProgressBarText.Text = e.ProgressText;
            });
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
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
