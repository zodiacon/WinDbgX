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

#pragma warning disable 649

namespace WinDbgX.Commands {
	[Export(typeof(ICommandCollection))]
	class ViewCommands : ICommandCollection {
		[Import]
		UIManager UIManager;

		[Import]
		AppManager AppManager;

		[Import]
		DebugManager DebugManager;

		public DelegateCommandBase ViewModules { get; } 
		public DelegateCommandBase ViewCommand { get; } 
		public DelegateCommandBase ViewRegisters { get; } 
		public DelegateCommandBase ViewThreads { get; } 
		public DelegateCommandBase ViewEventLog { get; }
		public DelegateCommandBase ViewBreakpoints { get; } 
		public DelegateCommandBase ViewCallStack { get; } 

		void ViewTab<T>() where T : TabItemViewModelBase {
			MainViewModel vm;
			var tab = UIManager.FindTab<T>(out vm);
			if (tab == null) {
				tab = AppManager.Container.GetExportedValue<T>();
				UIManager.CurrentWindow.AddItem(tab);
			}
			else {
				vm.Window.Activate();
				vm.SelectedTab = tab;
			}
		}

		private ViewCommands() {
			ViewModules = new DelegateCommand(() => ViewTab<ModulesViewModel>());
			ViewRegisters = new DelegateCommand(() => ViewTab<RegistersViewModel>());
			ViewCommand = new DelegateCommand(() => ViewTab<CommandViewModel>());
			ViewCallStack = new DelegateCommand(() => ViewTab<CallStackViewModel>());
			ViewEventLog = new DelegateCommand(() => ViewTab<EventLogViewModel>());
			ViewBreakpoints = new DelegateCommand(() => ViewTab<BreakpointsViewModel>(), () => !DebugManager.IsDumpFile);
			ViewThreads = new DelegateCommand(() => ViewTab<ThreadsViewModel>());

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
