using DebuggerEngine.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDbgX.Models;
using WinDbgX.UICore;

#pragma warning disable 649

namespace WinDbgX.ViewModels {
	[TabItem("Call Stack", Icon = "/icons/callstack.ico")]
	[Export] 
	class CallStackViewModel : TabItemViewModelBase, IPartImportsSatisfiedNotification {

		[Import]
		DebugManager DebugManager;


		List<StackFrameViewModel> _callStack;

		public IEnumerable<StackFrameViewModel> CallStack {
			get {
				if (DebugManager.Status != DEBUG_STATUS.BREAK)
					return null;

				return _callStack;
			}
		}

		public void OnImportsSatisfied() {
			DebugManager.Debugger.StatusChanged += Debugger_StatusChanged;
		}

		private void Debugger_StatusChanged(object sender, DebuggerEngine.StatusChangedEventArgs e) {
		}
	}
}
