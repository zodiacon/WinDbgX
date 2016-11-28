using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDbgEx.Models;
using WinDbgEx.UICore;

namespace WinDbgEx.ViewModels {
	[TabItem("Event Log", Icon = "/icons/flash.ico")]
	[Export]
	class EventLogViewModel : TabViewModelBase {
		ObservableCollection<EventLogItem> _log = new ObservableCollection<EventLogItem>();

		public IList<EventLogItem> Log => _log;

		public EventLogViewModel() {

		}
	}
}
