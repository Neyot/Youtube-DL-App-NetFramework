using System;

namespace Youtube_DL_Wrapper {
    public class DownloadProgressEventArgs : EventArgs {

        public double Progress { get; private set; }
        public string ProgressText { get; private set; }

        public DownloadProgressEventArgs(double progressIn, string progressTextIn) {
            Progress = progressIn;
            ProgressText = progressTextIn;
        }
    }
}
