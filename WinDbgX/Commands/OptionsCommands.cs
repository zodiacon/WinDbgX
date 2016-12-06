using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WinDbgX.Models;
using WinDbgX.UICore;
using System.Windows.Input;

namespace WinDbgX.Commands {
	[Export(typeof(ICommandCollection))]
	class OptionsCommands : ICommandCollection {
		public static DelegateCommandBase AlwaysOnTop { get; } 
			= new DelegateCommand<AppManager>(app => {
				bool ontop = !app.UI.CurrentWindow.Window.Topmost;
				app.UI.CurrentWindow.Window.Topmost = ontop;
				app.UI.CurrentWindow.Menu["AlwaysOnTop"].IsChecked = ontop; 
			});

		public IDictionary<string, ICommand> GetCommands() {
			return new Dictionary<string, ICommand> {
				{ nameof(AlwaysOnTop), AlwaysOnTop },
			};
		}
	}
}
