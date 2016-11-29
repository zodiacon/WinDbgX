using DebuggerEngine.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebuggerEngine {
	public sealed class StatusChangedEventArgs : EventArgs {
		public readonly DEBUG_STATUS OldStatus, NewStatus;

		internal StatusChangedEventArgs(DEBUG_STATUS oldStatus, DEBUG_STATUS newStatus) {
			OldStatus = oldStatus;
			NewStatus = newStatus;
		}
	}

	public sealed class ProcessCreatedEventArgs : EventArgs {
		public readonly TargetProcess Process;

		internal ProcessCreatedEventArgs(TargetProcess process) {
			Process = process;
		}
	}

	public sealed class ThreadCreatedEventArgs : EventArgs {
		public readonly TargetThread Thread;

		internal ThreadCreatedEventArgs(TargetThread thread) {
			Thread = thread;
		}
	}

	public sealed class ProcessExitedEventArgs : EventArgs {
		public readonly TargetProcess Process;

		internal ProcessExitedEventArgs(TargetProcess process) {
			Process = process;
		}
	}

	public sealed class ThreadExitedEventArgs : EventArgs {
		public readonly TargetThread Thread;
		public readonly TargetProcess Process;

		internal ThreadExitedEventArgs(TargetThread thread, TargetProcess process) {
			Process = process;
			Thread = thread;
		}
	}

	public sealed class ModuleLoadedEventArgs : EventArgs {
		public readonly TargetModule Module;

		internal ModuleLoadedEventArgs(TargetModule module) {
			Module = module;
		}
	}
}
