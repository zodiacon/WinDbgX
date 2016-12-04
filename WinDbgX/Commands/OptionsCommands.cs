using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WinDbgX.Models;

namespace WinDbgX.Commands {
	class OptionsCommands {
		public static DelegateCommandBase AlwaysOnTop { get; } 
			= new DelegateCommand<AppManager>(app => {
				bool ontop = !app.UI.CurrentWindow.Window.Topmost;
				app.UI.CurrentWindow.Window.Topmost = ontop;
				app.UI.CurrentWindow.Menu["AlwaysOnTop"].IsChecked = ontop; 
			});
	}
}
