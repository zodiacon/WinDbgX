using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WinDbgEx.Commands {
	class OptionsCommands {
		public static DelegateCommandBase AlwaysOnTop { get; } 
			= new DelegateCommand(() => Application.Current.MainWindow.Topmost = !Application.Current.MainWindow.Topmost);
	}
}
