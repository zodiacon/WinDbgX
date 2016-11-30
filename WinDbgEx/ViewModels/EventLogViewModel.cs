using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WinDbgEx.Converters;
using WinDbgEx.Models;
using WinDbgEx.UICore;

namespace WinDbgEx.ViewModels {
	[TabItem("Event Log", Icon = "/icons/flash.ico")]
	[Export]
	class EventLogViewModel : TabViewModelBase {
		public IEnumerable<EventLogItem> Log => _appManager.Debug.Log;

		AppManager _appManager;

		[ImportingConstructor]
		public EventLogViewModel(AppManager appManager) {
			_appManager = appManager;
		}

		public ICommand ClearCommand => new DelegateCommand(() => _appManager.Debug.ClearLog());

		public ICommand SaveLogCommand => new DelegateCommand(() => {
			if (Log.Count() == 0)
				return;

			var filename = _appManager.UI.FileDialogService.GetFileForSave();
			if (filename == null)
				return;

			var converter = new EventLogItemToStringConverter();
			File.WriteAllLines(filename, Log.Select(item => $"{item.Time}\t{item.Type}\t{converter.Convert(item, typeof(string), null, null)}")); 
		});

		public ToolbarItems Toolbar => new ToolbarItems {
			new ToolBarButtonViewModel { Text = "Clear", Icon = Icons.Delete, Command = ClearCommand },
			new ToolBarButtonViewModel { Text = "Save...", Icon = Icons.SaveAs, Command = SaveLogCommand }
		};

	}
}
