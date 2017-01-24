using DebuggerEngine.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebuggerEngine {
	public class TargetThread {
		public TargetProcess Process { get; }

		public TargetThread(TargetProcess process) {
			Process = process;
		}

		public uint Index { get; internal set; }
		public ulong Teb { get; internal set; }
		public uint TID { get; internal set; }
		public ulong StartAddress { get; internal set; }
		public ulong Handle { get; internal set; }
		public uint ProcessIndex { get; internal set; }
		public uint ExitCode { get; set; }

		public ThreadPriorityLevel GetPriority() => NativeMethods.GetThreadPriority(new IntPtr((long)Handle));

		public bool SetPriority(ThreadPriorityLevel level) => NativeMethods.SetThreadPriority(new IntPtr((long)Handle), level);

		public int Suspend() => NativeMethods.SuspendThread(new IntPtr((long)Handle));
		public int Resume() => NativeMethods.ResumeThread(new IntPtr((long)Handle));
	}
}
