using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace WinDbgX {
	[SuppressUnmanagedCodeSecurity]
	static class NativeMethods {
		[Flags]
		public enum ProcessAccessRights {
			QueryLimitedInformation = 0x1000,
			SuspendResume = 0x800,
			Terminate = 1,
			Synchronize = 0x100000,
			All = 0xF0000 | Synchronize | 0xffff
		}

		[Flags]
		public enum ThreadAccessRights {
			SuspendResume = 2,
			Terminate = 1,
			Synchronize = 0x100000,
			QueryInformation = 0x40,
			QueryLimitedInformation = 0x800,
			All = 0xF0000 | Synchronize | 0xffff
		}

		[DllImport("kernel32", SetLastError = true)]
		public static extern IntPtr OpenProcess(ProcessAccessRights accessMask, bool inheritHandle, int pid);

		[DllImport("kernel32", SetLastError = true)]
		public static extern IntPtr OpenThread(ThreadAccessRights accessMask, bool inheritHandle, int tid);

		[DllImport("kernel32", SetLastError = true)]
		public static extern bool IsProcessCritical(IntPtr hProcess, out bool critical);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool IsWow64Process(IntPtr process, out bool wow64Process);

		[DllImport("kernel32", SetLastError = true)]
		public static extern int SuspendThread(IntPtr hThread);

		[DllImport("kernel32", SetLastError = true)]
		public static extern int ResumeThread(IntPtr hThread);

	}
}
