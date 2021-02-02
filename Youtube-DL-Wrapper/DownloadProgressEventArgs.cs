using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Youtube_DL_Wrapper {
    public class DownloadProgressEventArgs : EventArgs {
        private readonly double data;
        public double Data { get => data; }

        public DownloadProgressEventArgs(double progress) {
            data = progress;
        }
    }
}
