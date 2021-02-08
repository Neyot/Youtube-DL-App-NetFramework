using System;

namespace Youtube_DL_Wrapper {
    public class ConsoleLogEventArgs : EventArgs {
        public string LatestLine { get; private set; }

        public ConsoleLogEventArgs(string latestLineIn) {
            LatestLine = latestLineIn;
        }
    }
}
