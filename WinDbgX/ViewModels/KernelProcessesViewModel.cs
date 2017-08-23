using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDbgX.Models;
using WinDbgX.UICore;

namespace WinDbgX.ViewModels {
	[Export, TabItem("Processes", Icon = "/icons/gears.ico")]
	sealed class KernelProcessesViewModel : TabItemViewModelBase {
		[Import]
		DebugManager Debug;

		IEnumerable<dynamic> _processes;

		public IEnumerable<dynamic> Processes {
			get {
				if (_processes != null)
					return _processes;
				EnumProcesses();
				return null;
			}
		}

		private async void EnumProcesses() {
			if (Debug.IsLocalKernel || Debug.Status == DebuggerEngine.Interop.DEBUG_STATUS.BREAK) {
				_processes = null;
				var processListHead = await Debug.Debugger.GetGlobalAddress("nt", "PsActiveProcessHead");
				int zz = 9;
			}
		}
	}
}
