using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDbgEx.Models {
	enum EventLogItemType {
		ProcessCreate,
		ThreadCreate,
		ProcessExit,
		ThreadExit,
		ModuleLoad,
		ModuleUnload,
		Exception
	}

	abstract class EventLogItem {
		protected EventLogItem(EventLogItemType type, DateTime time) {
			Type = type;
			Time = time;
		}

		public EventLogItemType Type { get; }
		public DateTime Time { get; }
	}

	class EventLogItem<T> : EventLogItem {
		public T EventData { get; }

		public EventLogItem(EventLogItemType type, DateTime time, T data) : base(type, time) {
			EventData = data;
		}
	}
}
