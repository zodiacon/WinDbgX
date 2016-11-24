using DebuggerEngine.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebuggerEngine {
	public enum DumpType {
		Kernel,
		Full,
		Small
	}

	public sealed class DebugTarget {
		public bool IsDump { get; }
		public bool IsKernel { get; }
		public bool IsLocalKernel { get; }

		public int Index { get; internal set; }

		public DumpType DumpType { get; }

		public readonly DEBUG_CLASS DebuggeeClass;

		public readonly DEBUG_CLASS_QUALIFIER DebuggeeClassQualifier;

		internal DebugTarget(DebugClient client, int index) {
			Index = index;
			client.Control.GetDebuggeeType(out DebuggeeClass, out DebuggeeClassQualifier);
			switch (DebuggeeClass) {
				case DEBUG_CLASS.KERNEL:
					IsKernel = true;
					break;

				case DEBUG_CLASS.IMAGE_FILE:
					IsDump = true;
					break;
			}

			switch (DebuggeeClassQualifier) {
				case DEBUG_CLASS_QUALIFIER.KERNEL_LOCAL:
					IsLocalKernel = true;
					break;

				case DEBUG_CLASS_QUALIFIER.KERNEL_DUMP:
					DumpType = DumpType.Kernel;
					break;

				case DEBUG_CLASS_QUALIFIER.KERNEL_SMALL_DUMP:
					DumpType = DumpType.Small;
					break;

				case DEBUG_CLASS_QUALIFIER.KERNEL_FULL_DUMP:
					DumpType = DumpType.Full;
					IsDump = true;
					break;

			}

		}
	}
}
