using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDbgEx.Models;
using WinDbgEx.UICore;
using WinDbgEx.ViewModels;

namespace WinDbgEx.Commands {
	static class ViewCommands {
		public static DelegateCommandBase Modules { get; } = new DelegateCommand<DebugContext>(context => ViewTab<ModulesViewModel>(context));
		public static DelegateCommandBase Command { get; } = new DelegateCommand<DebugContext>(context => ViewTab<CommandViewModel>(context));
		public static DelegateCommandBase Registers { get; } = new DelegateCommand<DebugContext>(context => ViewTab<RegistersViewModel>(context));

		static void ViewTab<T>(DebugContext context) where T : TabViewModelBase, new() {
			MainViewModel vm;
			var tab = context.UI.FindTab<T>(out vm);
			if (tab == null) {
				tab = new T();
				context.UI.Current.AddItem(tab);
			}
			else {
				vm.Window.Activate();
				vm.SelectedTab = tab;
			}
		}
	}
}
