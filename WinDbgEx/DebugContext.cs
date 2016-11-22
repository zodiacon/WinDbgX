using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DebuggerEngine;
using Zodiacon.WPF;

namespace WinDbgEx {
    sealed class DebugContext {
        public readonly DebugClient Debugger;

        internal DebugContext(DebugClient debugger) {
            Debugger = debugger;
        }

        public IFileDialogService FileDialogService { get; internal set; }
        public IDialogService DialogService { get; internal set; }
        public IMessageBoxService MessageBoxService { get; internal set; }
    }
}
