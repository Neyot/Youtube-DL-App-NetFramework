using System;

namespace Youtube_DL_Wrapper {
    public class ConsoleLogEventArgs : EventArgs {
        public string Data { get; private set; }

        public ConsoleLogEventArgs(string data) {
            Data = data;
        }
    }
}
