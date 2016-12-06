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

namespace WinDbgX.Commands {
	[Export(typeof(ICommandCollection))]
    class DebugCommands : ICommandCollection {
		public DelegateCommandBase Go { get; } = new DelegateCommand<AppManager>(context => context.Debug.Debugger.Execute("g"), context => context?.Debug.Status == DEBUG_STATUS.BREAK);
		public DelegateCommandBase StepOver { get; } = new DelegateCommand<AppManager>(context => context.Debug.Debugger.Execute("p"), context => context?.Debug.Status == DEBUG_STATUS.BREAK);
		public DelegateCommandBase StepInto { get; } = new DelegateCommand<AppManager>(context => context.Debug.Debugger.Execute("t"), context => context?.Debug.Status == DEBUG_STATUS.BREAK);
		public DelegateCommandBase StepOut { get; } = new DelegateCommand<AppManager>(context => context.Debug.Debugger.Execute("gu"), context => context?.Debug.Status == DEBUG_STATUS.BREAK);
		public DelegateCommandBase Restart { get; } = new DelegateCommand<AppManager>(context => context.Debug.Debugger.Execute(".restart"), context => context?.Debug.Status == DEBUG_STATUS.BREAK);
		public DelegateCommandBase StepToCall { get; } = new DelegateCommand<AppManager>(context => context.Debug.Debugger.Execute("gc"), context => context?.Debug.Status == DEBUG_STATUS.BREAK);
		public DelegateCommandBase StepToBranch { get; } = new DelegateCommand<AppManager>(context => context.Debug.Debugger.Execute("ph"), context => context?.Debug.Status == DEBUG_STATUS.BREAK);
		public DelegateCommandBase StepToReturn { get; } = new DelegateCommand<AppManager>(context => context.Debug.Debugger.Execute("pt"), context => context?.Debug.Status == DEBUG_STATUS.BREAK);
		public DelegateCommandBase Break { get; } = new DelegateCommand<AppManager>(context => context.Debug.Debugger.Break(), 
			context => context?.Debug.Status == DEBUG_STATUS.GO);
		public DelegateCommandBase Stop { get; } = new DelegateCommand<AppManager>(context => context.Debug.Debugger.Stop(), 
			context => context != null && context.Debug.Status != DEBUG_STATUS.NO_DEBUGGEE);

		public DelegateCommandBase Detach { get; } = new DelegateCommand<AppManager>(context => context.Debug.Debugger.Detach(), context => context?.Debug.Status == DEBUG_STATUS.BREAK);
		public DelegateCommandBase DetachAll { get; } = new DelegateCommand<AppManager>(context => context.Debug.Debugger.DetachAll(), context => context?.Debug.Status == DEBUG_STATUS.BREAK);

		public DelegateCommandBase DeleteAllBreakpoints { get; } = new DelegateCommand<AppManager>(context => context.Debug.Debugger.DeleteAllBreakpoints(),
			context => (context ?? AppManager.Instance).Debug.Status != DEBUG_STATUS.NO_DEBUGGEE);

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
