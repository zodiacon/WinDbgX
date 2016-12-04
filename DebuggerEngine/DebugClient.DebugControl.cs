using System;
using System.Linq;

namespace DebuggerEngine {
	partial class DebugClient {
		public TargetProcess GetCurrentProcess() {
			return RunAsync(() => {
				uint id;
				SystemObjects.GetCurrentProcessSystemId(out id);
				return _processes.First(p => p.PID == id);
			}).Result;
		}
	}
}
