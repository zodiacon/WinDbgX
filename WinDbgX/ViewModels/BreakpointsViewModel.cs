using DebuggerEngine;
using DebuggerEngine.Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using WinDbgX.Models;
using WinDbgX.UICore;

#pragma warning disable 649

namespace WinDbgX.ViewModels {
	[TabItem("Breakpoints", Icon = Icons.Breakpoints)]
	[Export]
	class BreakpointsViewModel : TabItemViewModelBase, IPartImportsSatisfiedNotification {

		[Import]
		DebugManager DebugManager;

		[Import]
		UIManager UIManager;

		public IEnumerable<BreakpointViewModel> Breakpoints {
			get {
				if (DebugManager.Status != DebuggerEngine.Interop.DEBUG_STATUS.BREAK)
					return null;

				var breakpoints = DebugManager.Debugger.GetBreakpoints();
				return breakpoints.Select(bp => new BreakpointViewModel(bp, DebugManager)).ToArray();
			}
		}

		public void OnImportsSatisfied() {
			DebugManager.Debugger.StatusChanged += Debugger_StatusChanged;
			DebugManager.Debugger.BreakpointChanged += Debugger_BreakpointChanged;
		}

		private void Debugger_BreakpointChanged(object sender, BreakpointChangedEventArgs e) {
			UIManager.Dispatcher.InvokeAsync(() => {
				foreach (var bp in Breakpoints)
					bp.Refresh();
			});
		}

		private void Debugger_StatusChanged(object sender, StatusChangedEventArgs e) {
			var state = e.NewStatus;
			UIManager.Dispatcher.InvokeAsync(() => {
				if (state == DEBUG_STATUS.BREAK) {
					OnPropertyChanged(nameof(Breakpoints));
				}
			});
		}
	}
}

