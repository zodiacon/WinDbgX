using DebuggerEngine.Interop;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDbgEx.Models;

namespace WinDbgEx.Commands {
    static class DebugCommands {
		public static DelegateCommandBase Go { get; } = new DelegateCommand<DebugContext>(context => context.Debugger.Execute("g"), context => context.Status == DEBUG_STATUS.BREAK);
		public static DelegateCommandBase StepOver { get; } = new DelegateCommand<DebugContext>(context => context.Debugger.Execute("p"), context => context.Status == DEBUG_STATUS.BREAK);
		public static DelegateCommandBase StepInto { get; } = new DelegateCommand<DebugContext>(context => context.Debugger.Execute("t"), context => context.Status == DEBUG_STATUS.BREAK);
		public static DelegateCommandBase StepOut { get; } = new DelegateCommand<DebugContext>(context => context.Debugger.Execute("gu"), context => context.Status == DEBUG_STATUS.BREAK);
		public static DelegateCommandBase Restart { get; } = new DelegateCommand<DebugContext>(context => context.Debugger.Execute(".restart"), context => context.Status == DEBUG_STATUS.BREAK);
		public static DelegateCommandBase StepToCall { get; } = new DelegateCommand<DebugContext>(context => context.Debugger.Execute("gc"), context => context.Status == DEBUG_STATUS.BREAK);
		public static DelegateCommandBase StepToBranch { get; } = new DelegateCommand<DebugContext>(context => context.Debugger.Execute("ph"), context => context.Status == DEBUG_STATUS.BREAK);
		public static DelegateCommandBase StepToReturn { get; } = new DelegateCommand<DebugContext>(context => context.Debugger.Execute("pt"), context => context.Status == DEBUG_STATUS.BREAK);
		public static DelegateCommandBase Break { get; } = new DelegateCommand<DebugContext>(context => context.Debugger.Break(), context => context.Status == DEBUG_STATUS.GO);

	}
}
