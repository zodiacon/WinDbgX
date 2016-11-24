using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDbgEx.Models;

namespace WinDbgEx.Commands {
    static class DebugCommands {
		public static DelegateCommandBase Go { get; } = new DelegateCommand<DebugContext>(context => context.Debugger.Execute("g"));
		public static DelegateCommandBase StepOver { get; } = new DelegateCommand<DebugContext>(context => context.Debugger.Execute("p"));
		public static DelegateCommandBase StepInto { get; } = new DelegateCommand<DebugContext>(context => context.Debugger.Execute("t"));
		public static DelegateCommandBase StepOut { get; } = new DelegateCommand<DebugContext>(context => context.Debugger.Execute("gu"));
		public static DelegateCommandBase Restart { get; } = new DelegateCommand<DebugContext>(context => context.Debugger.Execute(".restart"));
		public static DelegateCommandBase StepToCall { get; } = new DelegateCommand<DebugContext>(context => context.Debugger.Execute("gc"));
		public static DelegateCommandBase StepToBranch { get; } = new DelegateCommand<DebugContext>(context => context.Debugger.Execute("ph"));
		public static DelegateCommandBase StepToReturn { get; } = new DelegateCommand<DebugContext>(context => context.Debugger.Execute("pt"));
		public static DelegateCommandBase Break { get; } = new DelegateCommand<DebugContext>(context => context.Debugger.Break());

	}
}
