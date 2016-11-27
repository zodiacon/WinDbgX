using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebuggerEngine {
	public class TargetThread {
		public uint Index { get; internal set; }
		public ulong Teb { get; internal set; }
		public uint OSID { get; internal set; }
		public int ProcessId { get; internal set; }
	}
}
