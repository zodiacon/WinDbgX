using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebuggerEngine {
    public class SymbolInfo {
        public ulong Offset { get; internal set; }
        public string Name { get; internal set; }
    }
}
