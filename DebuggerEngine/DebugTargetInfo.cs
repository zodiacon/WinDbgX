using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebuggerEngine {
	public enum DumpType {
		Full,
		Kernel,
		Small,
		Mini
	}

	public sealed class DebugTargetInfo {
		public bool UserMode { get; internal set; }
		public bool Live { get; internal set; }
		public bool LocalKernel { get; internal set; }
		public DumpType DumpType { get; set; }
	}
}
