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

#pragma warning disable 649

namespace WinDbgX.Commands {
	[Export(typeof(ICommandCollection))]
	class OptionsCommands : ICommandCollection {
		[Import]
		UIManager UIManager;

		public DelegateCommandBase AlwaysOnTop { get; }
		public DelegateCommandBase FontsOptions { get; }
		public DelegateCommandBase ColorsOptions { get; }

		private OptionsCommands() {
			AlwaysOnTop = new DelegateCommand(() => {
				var window = UIManager.CurrentWindow.Window;
				bool ontop = !window.Topmost;
				window.Topmost = ontop;
				UIManager.CurrentWindow.Menu[nameof(AlwaysOnTop)].IsChecked = ontop;
			});
		}

		public IDictionary<string, ICommand> GetCommands() {
			return new Dictionary<string, ICommand> {
				{ nameof(AlwaysOnTop), AlwaysOnTop },
			};
		}
	}
}
