using DebuggerEngine.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebuggerEngine {
	public sealed class StatusChangedEventArgs : EventArgs {
		public readonly DEBUG_STATUS OldStatus, NewStatus;
//		public readonly DebuggerTarget CurrentTarget;

		internal StatusChangedEventArgs(DEBUG_STATUS oldStatus, DEBUG_STATUS newStatus) {
			OldStatus = oldStatus;
			NewStatus = newStatus;
			//if (newStatus == DEBUG_STATUS.BREAK)
			//	CurrentTarget = client.GetCurrentTarget();
		}
	}
}
