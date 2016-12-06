using DebuggerEngine.Interop;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System;

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

		public bool IsLocalKernelEnabled() {
			return RunAsync(() => {
				return Client.IsKernelDebuggerEnabled() == S_OK;
			}).Result;
		}

		ulong GetThreadTeb(uint index) {
			uint id;
			ulong teb;
			SystemObjects.GetCurrentThreadId(out id).ThrowIfFailed();
			SystemObjects.SetCurrentThreadId(index).ThrowIfFailed();
			SystemObjects.GetCurrentThreadTeb(out teb);
			return teb;
		}


		//public Task<TargetThread[]> GetThreads(int start = 0, int count = 0) {
		//	return RunAsync(() => {
		//		uint total, largest;
		//		SystemObjects.GetTotalNumberThreads(out total, out largest).ThrowIfFailed();
		//		if (count == 0)
		//			count = (int)total;
		//		uint[] id = new uint[count];
		//		uint[] osid = new uint[count];
		//		SystemObjects.GetThreadIdsByIndex((uint)start, (uint)count, id, osid).ThrowIfFailed();
		//		var threads = new TargetThread[count];
		//		for (int i = 0; i < count; i++) {
		//			threads[i] = new TargetThread(process) {
		//				Index = id[i],
		//				TID = osid[i],
		//				Teb = GetThreadTeb(id[i])
		//			};
		//		}
		//		return threads;
		//	});
		//}

		public Task<DebugTarget> GetCurrentTarget() {
			return RunAsync(() => GetCurrentTargetInternal());
		}

		DebugTarget GetCurrentTargetInternal() {
			uint id;
			int hr = SystemObjects.GetCurrentProcessId(out id);
			return hr < 0 ? null : _targets.FirstOrDefault(t => t.Index == id);
		}

		public Task<int> GetCurrentThreadIndex() {
			return RunAsync(() => GetCurrentThreadInternal());
		}

		internal int GetCurrentThreadInternal() {
			uint id;
			SystemObjects.GetCurrentThreadId(out id);
			return (int)id;
		}

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

		public Task<int> SetCurrentThreadIndex(int index) {
			return RunAsync(() => {
				uint id;
				SystemObjects.GetCurrentThreadId(out id).ThrowIfFailed();
				SystemObjects.SetCurrentThreadId((uint)index).ThrowIfFailed();
				return (int)id;
			});
		}

		public Task<TargetThread> SetThreadExtraInfo(TargetThread thread) {
			return RunAsync(() => {
				// get extra information from the TEB
				if (thread.Teb == 0)
					return thread;

				uint tebTypeId;
				ulong ntdllModulebase;
				if (SUCCEEDED(GetSymbolTypeIdWide("ntdll!_teb", out tebTypeId, out ntdllModulebase))) {
					ulong pid;
					GetFieldValue(ntdllModulebase, tebTypeId, "ClientId.UniqueProcess", thread.Teb, out pid);
				}
				return thread;
			});
		}
	}
}
