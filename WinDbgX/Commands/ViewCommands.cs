using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDbgX.Models;
using WinDbgX.UICore;
using WinDbgX.ViewModels;

namespace WinDbgX.Commands {
	static class ViewCommands {
		public static DelegateCommandBase Modules { get; } = new DelegateCommand<AppManager>(context => ViewTab<ModulesViewModel>(context));
		public static DelegateCommandBase Command { get; } = new DelegateCommand<AppManager>(context => ViewTab<CommandViewModel>(context));
		public static DelegateCommandBase Registers { get; } = new DelegateCommand<AppManager>(context => ViewTab<RegistersViewModel>(context));
		public static DelegateCommandBase Threads { get; } = new DelegateCommand<AppManager>(context => ViewTab<ThreadsViewModel>(context));
		public static DelegateCommandBase EventLog { get; } = new DelegateCommand<AppManager>(context => ViewTab<EventLogViewModel>(context));
		public static DelegateCommandBase Breakpoints { get; } = new DelegateCommand<AppManager>(context => ViewTab<BreakpointsViewModel>(context));

		static void ViewTab<T>(AppManager context) where T : TabItemViewModelBase {
			MainViewModel vm;
			var tab = context.UI.FindTab<T>(out vm);
			if (tab == null) {
				tab = App.Container.GetExportedValue<T>();
				context.UI.CurrentWindow.AddItem(tab);
			}
			else {
				vm.Window.Activate();
				vm.SelectedTab = tab;
			}
		}
	}
}
