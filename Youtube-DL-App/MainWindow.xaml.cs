using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace Youtube_DL_App {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public DownloadProgressBarValue DownloadProgressValue { get; set; }

        private string outputFolder;

        public MainWindow() {
            InitializeComponent();
            outputFolder = Properties.Settings.Default["OutputFolder"].ToString();
            if (!System.IO.Path.IsPathRooted(outputFolder)) {
                outputFolder = System.IO.Path.GetFullPath(System.IO.Directory.GetCurrentDirectory() + outputFolder);
            }
            OutputFolderTextBox.Text = outputFolder;

            Binding binding = new Binding("Data");
            binding.Source = DownloadProgressValue;
            downloadProgressBar.SetBinding(ProgressBar.ValueProperty, binding);
        }

        private void OutputFolderButton_Click(object sender, RoutedEventArgs e) {
            Properties.Settings.Default["OutputFolder"] = OutputFolderTextBox.Text;
            Properties.Settings.Default.Save();
        }

        public void AudioDL_DownloadProgress(object sender, DownloadProgressEventArgs e) {
            this.Dispatcher.Invoke(() => {
                downloadProgressBar.Value = e.Progress;
                downloadProgressBarText.Text = e.ProgressText;
            });
        }

        private void YoutubeUrlButton_Click(object sender, RoutedEventArgs e) {
            downloadProgressBar.Visibility = Visibility.Visible;
            downloadProgressBarText.Visibility = Visibility.Visible;
            AudioDownloader AudioDL = new AudioDownloader("https://www.youtube.com/watch?v=dvgZkm1xWPE", @"..\test downloads");
            AudioDL.DownloadProgress += AudioDL_DownloadProgress;
            ThreadStart ths = new ThreadStart(() => AudioDL.StartDownload());
            Thread th = new Thread(ths);
            th.Start();
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
