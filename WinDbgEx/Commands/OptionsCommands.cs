using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WinDbgEx.Models;

namespace WinDbgEx.Commands {
	class OptionsCommands {
		public static DelegateCommandBase AlwaysOnTop { get; } 
			= new DelegateCommand<DebugContext>(context => {
				bool ontop = !context.UI.Current.Window.Topmost;
				context.UI.Current.Window.Topmost = ontop;
				context.UI.Current.Menu["AlwaysOnTop"].IsChecked = ontop; 
			});
	}
}
