using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Youtube_DL_Wrapper {
    public class DownloadProgressEventArgs : EventArgs {
        private readonly double progress;
        private readonly string progressText;

        public double Progress { get => progress; }
        public string ProgressText { get => progressText; }

        public DownloadProgressEventArgs(double progressIn, string progressTextIn) {
            progress = progressIn;
            progressText = progressTextIn;
        }
    }
}
