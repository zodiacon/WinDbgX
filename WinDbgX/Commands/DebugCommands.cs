using DebuggerEngine.Interop;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDbgX.Models;
using WinDbgX.UICore;
using System.Windows.Input;

#pragma warning disable 649

namespace WinDbgX.Commands {
	[Export(typeof(ICommandCollection))]
    class DebugCommands : ICommandCollection {
		[Import]
		DebugManager DebugManager;

		public DelegateCommandBase Go { get; } 
		public DelegateCommandBase StepOver { get; } 
		public DelegateCommandBase StepInto { get; } 
		public DelegateCommandBase StepOut { get; } 
		public DelegateCommandBase Restart { get; } 
		public DelegateCommandBase StepToCall { get; } 
		public DelegateCommandBase StepToBranch { get; }
		public DelegateCommandBase StepToReturn { get; } 
		public DelegateCommandBase Break { get; } 
		public DelegateCommandBase Stop { get; } 

		public DelegateCommandBase Detach { get; } 
		public DelegateCommandBase DetachAll { get; } 

		public DelegateCommandBase DeleteAllBreakpoints { get; } 

		private DebugCommands() {
			Go = new DelegateCommand(() => DebugManager.Debugger.Execute("g"), () => DebugManager.Status == DEBUG_STATUS.BREAK);
			StepOver = new DelegateCommand(() => DebugManager.Debugger.Execute("p"), () => DebugManager.Status == DEBUG_STATUS.BREAK);
			StepInto = new DelegateCommand(() => DebugManager.Debugger.Execute("t"), () => DebugManager.Status == DEBUG_STATUS.BREAK);
			StepOver = new DelegateCommand(() => DebugManager.Debugger.Execute("gu"), () => DebugManager.Status == DEBUG_STATUS.BREAK);
			Restart = new DelegateCommand(() => DebugManager.Debugger.Execute(".restart"), () => DebugManager.Status == DEBUG_STATUS.BREAK);
			StepToCall = new DelegateCommand(() => DebugManager.Debugger.Execute("gc"), () => DebugManager.Status == DEBUG_STATUS.BREAK);
			StepToBranch = new DelegateCommand(() => DebugManager.Debugger.Execute("ph"), () => DebugManager.Status == DEBUG_STATUS.BREAK);
			StepToReturn = new DelegateCommand(() => DebugManager.Debugger.Execute("pt"), () => DebugManager.Status == DEBUG_STATUS.BREAK);
			Break = new DelegateCommand(() => DebugManager.Debugger.Break(), () => DebugManager.Status == DEBUG_STATUS.GO);
			Stop = new DelegateCommand(() => DebugManager.Debugger.Stop(), () => DebugManager.Status != DEBUG_STATUS.NO_DEBUGGEE);
			Detach = new DelegateCommand(() => DebugManager.Debugger.Detach(), () => DebugManager.Status == DEBUG_STATUS.BREAK);
			DetachAll = new DelegateCommand(() => DebugManager.Debugger.DetachAll(), () => DebugManager.Status == DEBUG_STATUS.BREAK);
			DeleteAllBreakpoints = new DelegateCommand(() => DebugManager.Debugger.DeleteAllBreakpoints(), () => DebugManager.Status != DEBUG_STATUS.NO_DEBUGGEE);
		}

		public IDictionary<string, ICommand> GetCommands() {
			return new Dictionary<string, ICommand> {
				{ nameof(Go), Go },
				{ nameof(Break), Break },
				{ nameof(StepInto), StepInto },
				{ nameof(StepOut), StepOut },
				{ nameof(StepOver), StepOver },
				{ nameof(Stop), Stop },
				{ nameof(StepToCall), StepToCall },
				{ nameof(StepToReturn), StepToReturn },
				{ nameof(StepToBranch), StepToBranch },
				{ nameof(Restart), Restart },
				{ nameof(Detach), Detach },
				{ nameof(DetachAll), DetachAll },
				{ nameof(DeleteAllBreakpoints), DeleteAllBreakpoints },
			};
		}
	}
}
