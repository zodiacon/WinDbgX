using DebuggerEngine.Interop;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DebuggerEngine {
	partial class DebugClient {
		List<DebugTarget> _targets = new List<DebugTarget>(2);

		public Task<IList<DebugTarget>> GetTargets() {
			return RunAsync(() => {
				uint processes;
				SystemObjects.GetNumberProcesses(out processes).ThrowIfFailed();

				uint[] ids = new uint[processes];
				uint[] pids = new uint[processes];

				SystemObjects.GetProcessIdsByIndex(0, processes, ids, pids);
				uint current;
				SystemObjects.GetCurrentProcessId(out current);
				var targets = (IList<DebugTarget>)Enumerable.Range(0, (int)processes).Select(i => GetTarget(i)).ToList();
				SystemObjects.SetCurrentProcessId(current);
				return targets;

			});
		}

		public Task<DebugTarget> GetCurrentTarget() {
			return RunAsync(() => GetCurrentTargetInternal());
		}

		DebugTarget GetCurrentTargetInternal() {
			uint id;
			int hr = SystemObjects.GetCurrentProcessId(out id);
			return hr < 0 ? null : _targets.FirstOrDefault(t => t.Index == id);
		}

		//public Task<TargetThread> GetCurrentThreadAsync() {
		//	return StartTask(() => GetCurrentThread());
		//}

		//internal TargetThread GetCurrentThread() {
		//	uint id;
		//	System.GetCurrentThreadId(out id);
		//	return GetCurrentTarget()?.Threads.FirstOrDefault(t => t.Index == id);
		//}

		private DebugTarget GetTarget(int i) {
			if (SystemObjects.SetCurrentProcessId((uint)i) < 0)
				return null;

			DEBUG_CLASS debugClass;
			DEBUG_CLASS_QUALIFIER qualifier;
			Control.GetDebuggeeType(out debugClass, out qualifier);
			switch (debugClass) {
				case DEBUG_CLASS.IMAGE_FILE:
					break;
			}
			return null;
		}

		public Task SetCurrentTarget(DebugTarget target) {
			return RunAsync(() => {
				var id = _targets.IndexOf(target);
				if (id < 0)
					return;

				uint currentId;
				SystemObjects.GetCurrentProcessId(out currentId);
				if (currentId != id) {
					SystemObjects.SetCurrentProcessId((uint)id);
					Control.OutputPromptWide(DEBUG_OUTCTL.THIS_CLIENT, null);
				}
			});
		}
	}
}
