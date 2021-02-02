using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Youtube_DL_Wrapper {
    public class AudioDownloader {

        //private readonly string binaryFolder = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), @"../Binaries");
        private readonly Process process;
        private double dlPercentage = 0.0;
        private string outputMessage = String.Empty;
        private int lineCount = 0;

        public delegate void DownloadProgressEventHandler(object sender, DownloadProgressEventArgs e);
        public event DownloadProgressEventHandler DownloadProgress;

        public AudioDownloader(string url, string outputFolder, string outputName) {
            string binaryFolder = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), @"../Binaries");
            string destinationPath = System.IO.Path.Combine(outputFolder, outputName);

            string arguments = $"--continue  --no-overwrites --restrict-filenames --extract-audio --audio-format mp3 {url} -o \"{destinationPath}\"";

            process = new Process();
            process.StartInfo.FileName = System.IO.Path.Combine(binaryFolder, "youtube-dl.exe");
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
        }

        public AudioDownloader(string url, string outputFolder) : this(url, outputFolder, "%(title)s.%(ext)s") { }

        public void StartDownload() {
            process.Start();

            process.BeginOutputReadLine();
            process.WaitForExit();

            process.WaitForExit();
            process.Close();
        }

        private void OutputHandler(object sender, DataReceivedEventArgs e) {
            if (!String.IsNullOrEmpty(e.Data)) {
                if (e.Data.StartsWith("[download]")) {
                    Regex pattern = new Regex(@"(\d+[.,]\d{1,2}|\d+)(?=\s*%)", RegexOptions.None);
                    if (pattern.IsMatch(e.Data)) {
                        // percentage updated, send to progress bar
                        lineCount++;
                        dlPercentage = Convert.ToDouble(pattern.Match(e.Data).Value);
                        DownloadProgress?.Invoke(this, new DownloadProgressEventArgs(dlPercentage));
                        outputMessage += $"[{lineCount}] {dlPercentage}\n";
                    } else {
                        lineCount++;
                        outputMessage += $"[{lineCount}] {e.Data}\n";
                    }
                }
            }
        }
    }
}
