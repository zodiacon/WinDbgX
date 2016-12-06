using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDbgX.Models;
using WinDbgX.UICore;
using WinDbgX.ViewModels;
using System.Windows.Input;

namespace WinDbgX.Commands {
	[Export(typeof(ICommandCollection))]
	class ViewCommands : ICommandCollection {
		public DelegateCommandBase ViewModules { get; } = new DelegateCommand<AppManager>(context => ViewTab<ModulesViewModel>(context));
		public DelegateCommandBase ViewCommand { get; } = new DelegateCommand<AppManager>(context => ViewTab<CommandViewModel>(context));
		public DelegateCommandBase ViewRegisters { get; } = new DelegateCommand<AppManager>(context => ViewTab<RegistersViewModel>(context));
		public DelegateCommandBase ViewThreads { get; } = new DelegateCommand<AppManager>(context => ViewTab<ThreadsViewModel>(context));
		public DelegateCommandBase ViewEventLog { get; } = new DelegateCommand<AppManager>(context => ViewTab<EventLogViewModel>(context));
		public DelegateCommandBase ViewBreakpoints { get; } = new DelegateCommand<AppManager>(context => ViewTab<BreakpointsViewModel>(context));
		public DelegateCommandBase ViewCallStack { get; } = new DelegateCommand<AppManager>(context => ViewTab<CallStackViewModel>(context));

		static void ViewTab<T>(AppManager context) where T : TabItemViewModelBase {
			MainViewModel vm;
			var tab = context.UI.FindTab<T>(out vm);
			if (tab == null) {
				tab = context.Container.GetExportedValue<T>();
				context.UI.CurrentWindow.AddItem(tab);
			}
			else {
				vm.Window.Activate();
				vm.SelectedTab = tab;
			}
		}

		public IDictionary<string, ICommand> GetCommands() {
			return new Dictionary<string, ICommand> {
				{ nameof(ViewModules), ViewModules },
				{ nameof(ViewCommand), ViewCommand },
				{ nameof(ViewRegisters), ViewRegisters },
				{ nameof(ViewThreads), ViewThreads},
				{ nameof(ViewEventLog), ViewEventLog },
				{ nameof(ViewBreakpoints), ViewBreakpoints},
				{ nameof(ViewCallStack), ViewCallStack},
			};
		}
	}
}
