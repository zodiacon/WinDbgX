using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace WinDbgEx {
	[SuppressUnmanagedCodeSecurity]
	static class NativeMethods {
		[Flags]
		public enum ProcessAccessRights {
			QUERY_LIMITED_INFORMATION = 0x1000,
		}

		[DllImport("kernel32", SetLastError = true)]
		public static extern IntPtr OpenProcess(ProcessAccessRights accessMask, int pid, bool inheritHandle = false);

		[DllImport("kernel32", SetLastError = true)]
		public static extern bool IsProcessCritical(IntPtr hProcess, out bool critical);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool IsWow64Process(IntPtr process, out bool wow64Process);
	}
}
