using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using WinDbgX.Converters;
using WinDbgX.Models;
using WinDbgX.UICore;

namespace WinDbgX.ViewModels {
	[TabItem("Event Log", Icon = "/icons/flash.ico")]
	[Export]
	class EventLogViewModel : TabItemViewModelBase, IPartImportsSatisfiedNotification {
		public IEnumerable<EventLogItem> Log => DebugManager.Log;
		static EventLogItemToStringConverter _converter = new EventLogItemToStringConverter();

#pragma warning disable 649
		[Import]
		DebugManager DebugManager;

		[Import]
		UIManager UI;
#pragma warning restore 649

		ICollectionView _view;

		public EventLogViewModel() {
		}
		public ICommand ClearCommand => new DelegateCommand(() => DebugManager.ClearLog());

		public ICommand SaveLogCommand => new DelegateCommand(() => {
			if (Log.Count() == 0)
				return;

			var filename = UI.FileDialogService.GetFileForSave();
			if (filename == null)
				return;

			var converter = new EventLogItemToStringConverter();
			File.WriteAllLines(filename, Log.Select(item => $"{item.Time}\t{item.Type}\t{converter.Convert(item, typeof(string), null, null)}"));
		});

		public ToolbarItems Toolbar => new ToolbarItems {
			new ToolBarButtonViewModel { Text = "Clear", Icon = Icons.Delete, Command = ClearCommand },
			new ToolBarButtonViewModel { Text = "Save...", Icon = Icons.SaveAs, Command = SaveLogCommand }
		};

		private string _searchText;

		public string SearchText {
			get { return _searchText; }
			set {
				if (SetProperty(ref _searchText, value)) {
					if (string.IsNullOrWhiteSpace(value))
						_view.Filter = null;
					else
						_view.Filter = obj => SearchItem(value.ToLower(), (EventLogItem)obj);
				}
			}
		}

		public bool SearchItem(string text, EventLogItem item) {
			return item.Type.ToString().ToLower().Contains(text) || _converter.Convert(item, typeof(string), null,
				CultureInfo.CurrentUICulture).ToString().ToLowerInvariant().Contains(text);
		}

		public void OnImportsSatisfied() {
			_view = CollectionViewSource.GetDefaultView(Log);
		}
	}
}
