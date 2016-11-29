using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebuggerEngine {
	public class TargetProcess {
		public uint PID { get; internal set; }
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
		public uint ExitCode { get; set; }

		List<TargetThread> _threads = new List<TargetThread>(8);
		List<TargetModule> _modules = new List<TargetModule>(32);

		public IReadOnlyList<TargetThread> Threads => _threads;
		public IReadOnlyList<TargetModule> Modules => _modules;

		internal bool RemoveThread(TargetThread thread) {
			return _threads.Remove(thread);
		}

		internal void AddThread(TargetThread thread) {
			_threads.Add(thread);
		}

		internal void AddModule(TargetModule module) {
			_modules.Add(module);
		}

		internal void RemoveModule(TargetModule module) {
			_modules.Remove(module);
		}
	}
}
