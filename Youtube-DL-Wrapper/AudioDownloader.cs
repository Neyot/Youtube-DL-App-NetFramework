using System;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Youtube_DL_Wrapper {
    public class AudioDownloader {

        public delegate void DownloadProgressEventHandler(object sender, DownloadProgressEventArgs e);
        public event DownloadProgressEventHandler DownloadProgress;
        public delegate void ConvertingProgressEventHandler(object sender, ConvertingProgressEventArgs e);
        public event ConvertingProgressEventHandler ConvertingProgress;
        public delegate void ConsoleLogEventHandler(object sender, ConsoleLogEventArgs e);
        public event ConsoleLogEventHandler ConsoleLog;

        private readonly Process process;
        private readonly string fileExtension;
        private bool convertingInProgress;

        public AudioDownloader(string url, string binaryPath, string outputFolder, string outputName) {
            if (binaryPath == null || binaryPath == string.Empty || Path.GetFileName(binaryPath) != "youtube-dl.exe" || !File.Exists(binaryPath))
                throw new ArgumentException($"String passed to binaryPath: <{binaryPath}> not a valid youtube-dl.exe binary.");

            string destinationPath = Path.Combine(outputFolder, outputName);
            fileExtension = "mp3";

            string arguments = $"--continue  --no-overwrites --restrict-filenames --extract-audio --audio-format {fileExtension} {url} -o \"{destinationPath}\"";

            process = new Process();
            process.StartInfo.FileName = binaryPath;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
        }

        public AudioDownloader(string url, string binaryPath, string outputFolder) : this(url, binaryPath, outputFolder, "%(title)s.%(ext)s") { }

        public void StartDownload() {
            process.Start();

            process.BeginOutputReadLine();
            process.WaitForExit();

            process.Close();
        }

        private void OutputHandler(object sender, DataReceivedEventArgs e) {
            if (!String.IsNullOrEmpty(e.Data)) {
                ConsoleLog?.Invoke(this, new ConsoleLogEventArgs(e.Data));
                if (e.Data.StartsWith("[download]")) {
                    Regex pattern = new Regex(@"(\d+[.,]\d{1,2}|\d+)(?=\s*%)", RegexOptions.None);
                    if (pattern.IsMatch(e.Data)) {
                        // percentage updated, send to progress bar
                        DownloadProgress?.Invoke(this, new DownloadProgressEventArgs(Convert.ToDouble(pattern.Match(e.Data).Value), $"{e.Data}"));
                    }
                } else if (e.Data.StartsWith("[ffmpeg]") && e.Data.EndsWith(fileExtension)) {
                    convertingInProgress = true;
                    ConvertingProgress?.Invoke(this, new ConvertingProgressEventArgs(false));
                } else if (convertingInProgress && e.Data.StartsWith("Deleting original file")) {
                    convertingInProgress = false;
                    ConvertingProgress?.Invoke(this, new ConvertingProgressEventArgs(true));
                }
            }
        }
    }
}
