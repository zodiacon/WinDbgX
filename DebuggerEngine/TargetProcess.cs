using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebuggerEngine {
	public class TargetProcess {
		public int PID { get; internal set; }
		public ulong Peb { get; internal set; }
		public int Index { get; internal set; }
		public ulong hProcess { get; internal set; }
		public ulong hFile { get; internal set; }
		public uint Checksum { get; internal set; }
		public ulong BaseOffset { get; internal set; }
		public uint ModuleSize { get; internal set; }
		public string ImageName { get; internal set; }
		public string ModuleName { get; internal set; }
		public DateTime TimeStamp { get; internal set; }

		List<TargetThread> _threads = new List<TargetThread>();

		public IList<TargetThread> Threads => _threads;
	}
}
