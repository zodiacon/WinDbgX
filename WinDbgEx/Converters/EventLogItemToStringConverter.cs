using DebuggerEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using WinDbgEx.Models;

namespace WinDbgEx.Converters {
	class EventLogItemToStringConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			var item = (EventLogItem)value;
			switch (item.Type) {
				case EventLogItemType.ProcessCreate:
					var process = ((EventLogItem<TargetProcess>)item).EventData;
					return $"Process {process.PID} created";

				case EventLogItemType.ThreadCreate:
					var thread = ((EventLogItem<TargetThread>)item).EventData;
					return $"Thread {thread.TID} created";
			}
			return Binding.DoNothing;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
