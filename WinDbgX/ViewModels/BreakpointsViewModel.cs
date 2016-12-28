using DebuggerEngine;
using DebuggerEngine.Interop;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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

		BreakpointViewModel[] _breakpoints;
		public IEnumerable<BreakpointViewModel> Breakpoints {
			get {
				if (DebugManager.Status != DebuggerEngine.Interop.DEBUG_STATUS.BREAK)
					return null;

				var breakpoints = DebugManager.Debugger.GetBreakpoints();
				_breakpoints = breakpoints.Select(bp => new BreakpointViewModel(bp, DebugManager)).ToArray();
				return _breakpoints;
			}
		}

		public void OnImportsSatisfied() {
			DebugManager.Debugger.StatusChanged += Debugger_StatusChanged;
			DebugManager.Debugger.BreakpointChanged += Debugger_BreakpointChanged;
		}

		private void Debugger_BreakpointChanged(object sender, BreakpointChangedEventArgs e) {
			UIManager.InvokeAsync(() => {
				if (e.BreakpointId != uint.MaxValue) {
					var bp = _breakpoints.FirstOrDefault(b => b.Id == e.BreakpointId);
					if (bp != null)
						bp.Refresh();
					else
						OnPropertyChanged(nameof(Breakpoints));
				}
				else {
					OnPropertyChanged(nameof(Breakpoints));
				}
			});
		}

		private void Debugger_StatusChanged(object sender, StatusChangedEventArgs e) {
			var state = e.NewStatus;
			UIManager.InvokeAsync(() => {
				if (state == DEBUG_STATUS.BREAK) {
					OnPropertyChanged(nameof(Breakpoints));
				}
			});
		}

		public ToolbarItems Toolbar => new ToolbarItems {
			new ToolBarButtonViewModel { Text = "New...", Icon = Icons.NewBreakpoint, Command = NewBreakpointCommand },
			new ToolBarButtonViewModel { Text = "Enable All", Icon = Icons.EnableBreakpoint, Command = EnableAllBreakpointsCommand },
			new ToolBarButtonViewModel { Text = "Disable All", Icon = Icons.DisableBreakpoint, Command = DisableAllBreakpointsCommand },
			new ToolBarButtonViewModel { Text = "Delete All", Icon = Icons.DeleteBreakpoints, Command = DeleteAllBreakpointsCommand },
		};

		public ICommand EnableAllBreakpointsCommand => new DelegateCommand(() => EnableBreakpoints(true));
		public ICommand DisableAllBreakpointsCommand => new DelegateCommand(() => EnableBreakpoints(false));
		public ICommand DeleteAllBreakpointsCommand => new DelegateCommand(() => {
			DebugManager.Debugger.DeleteAllBreakpoints();
			OnPropertyChanged(nameof(Breakpoints));
		});

		public ICommand NewBreakpointCommand => new DelegateCommand(() => {
		});

		private void EnableBreakpoints(bool enable) {
			foreach (var bp in _breakpoints)
				bp.IsEnabled = enable;
		}
	}
}

