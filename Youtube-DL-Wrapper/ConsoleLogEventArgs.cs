using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Youtube_DL_Wrapper {
    public class ConsoleLogEventArgs : EventArgs {
        private readonly string latestLine;

        public string LatestLine { get => latestLine; }

        public ConsoleLogEventArgs(string latestLineIn) {
            latestLine = latestLineIn;
        }
    }
}
