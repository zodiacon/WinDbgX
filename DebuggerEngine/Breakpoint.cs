using DebuggerEngine.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebuggerEngine {
	public class Breakpoint {
		readonly IDebugBreakpoint3 _bp;
		readonly DebugClient _client;
		TargetThread _thread;

		internal Breakpoint(DebugClient client, IDebugBreakpoint3 bp) {
			_bp = bp;
			_client = client;
		}

		public int SetOffset(ulong offset) {
			return _bp.SetOffset(offset);
		}

		public ulong GetOffset() {
			ulong offset = 0;
			_bp.GetOffset(out offset);
			return offset;
		}

		public void SetThread(TargetThread thread) {
			uint id;
			_client.SystemObjects.GetThreadIdBySystemId(thread.TID, out id).ThrowIfFailed();
			_bp.SetMatchThreadId(id);
			_thread = thread;
			thread.Index = id;
		}

		public TargetThread GetThread() {
			uint id;
			if (_bp.GetMatchThreadId(out id) < 0)
				return null;
			return _thread;
		}
	}
}
