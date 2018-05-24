using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDbgX.KernelParser;
using WinDbgX.Models;
using WinDbgX.UICore;

namespace WinDbgX.ViewModels {
	[Export, TabItem("Processes", Icon = "/icons/gears.ico")]
	sealed class KernelProcessesViewModel : TabItemViewModelBase {
		ObservableCollection<ExecutiveProcess> _processes;

		[Import]
		DebugManager Debug;

		public IEnumerable<ExecutiveProcess> Processes {
			get {
				if (_processes != null)
					return _processes;
				_processes = new ObservableCollection<ExecutiveProcess>();
				EnumProcesses();
				RaisePropertyChanged(nameof(Processes));
				return _processes;
			}
		}

		private async void EnumProcesses() {
			if (Debug.IsLocalKernel || Debug.Status == DebuggerEngine.Interop.DEBUG_STATUS.BREAK) {
				var processListHead = await Debug.Debugger.GetGlobalAddress("nt", "PsActiveProcessHead");
				processListHead = await Debug.Debugger.ReadPointer(processListHead);
				var firstProcess = await ExecutiveProcess.FromListEntry(Debug.Debugger, processListHead);
				_processes.Add(firstProcess);

				while (true) {
					var listEntry = (ulong)await Debug.Debugger.GetFieldValue(ExecutiveProcess.KernelBase, ExecutiveProcess.EProcessTypeId, "ActiveProcessLinks", firstProcess.EProcess);
					if (listEntry == 0)
						break;
					firstProcess = await ExecutiveProcess.FromListEntry(Debug.Debugger, listEntry);
					_processes.Add(firstProcess);
				}

			}
		}
	}
}
