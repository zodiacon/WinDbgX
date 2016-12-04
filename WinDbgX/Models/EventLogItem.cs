using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDbgX.Models {
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
		public object EventData { get; protected set; }
	}

	class EventLogItem<T> : EventLogItem {
		public new T EventData => (T)base.EventData;

		public EventLogItem(EventLogItemType type, DateTime time, T data) : base(type, time) {
			base.EventData = data;
		}
	}
}
