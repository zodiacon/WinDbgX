using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace DebuggerEngine.Interop {
	[SuppressUnmanagedCodeSecurity]
	static partial class NativeMethods {
		[DllImport("kernel32")]
		public static extern ThreadPriorityLevel GetThreadPriority(IntPtr hThread);

		[DllImport("kernel32")]
		public static extern bool SetThreadPriority(IntPtr hThread, ThreadPriorityLevel level);

		[DllImport("kernel32")]
		public static extern int SuspendThread(IntPtr hThread);

		[DllImport("kernel32")]
		public static extern int ResumeThread(IntPtr hThread);

	}
}
