using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebuggerEngine {
	public class TargetThread {
		public uint Index { get; internal set; }
		public ulong Teb { get; internal set; }
		public uint TID { get; internal set; }
		public ulong StartAddress { get; internal set; }
		public ulong Handle { get; internal set; }
		public uint ProcessIndex { get; internal set; }
		public uint ExitCode { get; set; }
	}
}
