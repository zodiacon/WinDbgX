using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DebuggerEngine;

namespace WinDbgEx {
    sealed class DebugContext {
        public readonly DebugClient Debugger;

        internal DebugContext(DebugClient debugger) {
            Debugger = debugger;
        }
    }
}
