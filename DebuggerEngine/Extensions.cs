using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebuggerEngine {
	using System.Runtime.InteropServices;

	public static class Extensions {
		public static void ThrowIfFailed(this int hr) {
            if(hr < 0)
                Marshal.ThrowExceptionForHR(hr);
		}
	}
}
