using System;

namespace Youtube_DL_Wrapper {
    public class ConvertingProgressEventArgs : EventArgs {
        public bool Finished { get; private set; }

        public ConvertingProgressEventArgs(bool finishedIn) {
            Finished = finishedIn;
        }
    }
}
