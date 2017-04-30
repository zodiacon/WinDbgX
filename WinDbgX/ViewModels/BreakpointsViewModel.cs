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
		public BreakpointViewModel[] Breakpoints {
			get {
				if (DebugManager.Status != DebuggerEngine.Interop.DEBUG_STATUS.BREAK)
					return null;

				var breakpoints = DebugManager.Debugger.GetBreakpoints();
				if (breakpoints == null)
					return null;

				_breakpoints = breakpoints.Select(bp => new BreakpointViewModel(bp, DebugManager)).ToArray();
				RaisePropertyChanged(nameof(AnyBreakpoints));
				return _breakpoints;
			}
		}

		public void OnImportsSatisfied() {
			DebugManager.Debugger.StatusChanged += Debugger_StatusChanged;
			DebugManager.Debugger.BreakpointChanged += Debugger_BreakpointChanged;
		}

		private void Debugger_BreakpointChanged(object sender, BreakpointChangedEventArgs e) {
			var id = e.BreakpointId;
			UIManager.InvokeAsync(() => {
				if (id != uint.MaxValue) {
					var bp = _breakpoints.FirstOrDefault(b => b.Id == id);
					if (bp != null)
						bp.Refresh();
					else
						RaisePropertyChanged(nameof(Breakpoints));
				}
				else {
					RaisePropertyChanged(nameof(Breakpoints));
				}
			});
		}

		private void Debugger_StatusChanged(object sender, StatusChangedEventArgs e) {
			var state = e.NewStatus;
			UIManager.InvokeAsync(() => {
				if (state == DEBUG_STATUS.BREAK) {
					RaisePropertyChanged(nameof(Breakpoints));
				}
			});
		}

		public ToolbarItems Toolbar => new ToolbarItems {
			new ToolBarButtonViewModel { Text = "New...", Icon = Icons.NewBreakpoint, Command = NewBreakpointCommand },
			new ToolBarButtonViewModel { Text = "Enable All", Icon = Icons.EnableBreakpoint, Command = EnableAllBreakpointsCommand },
			new ToolBarButtonViewModel { Text = "Disable All", Icon = Icons.DisableBreakpoint, Command = DisableAllBreakpointsCommand },
			new ToolBarButtonViewModel { Text = "Delete All", Icon = Icons.DeleteBreakpoints, Command = DeleteAllBreakpointsCommand },
		};

		bool AnyBreakpoints => _breakpoints != null && _breakpoints.Any();

		public ICommand EnableAllBreakpointsCommand => new DelegateCommand(() => EnableBreakpoints(true), () => AnyBreakpoints).ObservesProperty(() => AnyBreakpoints);
		public ICommand DisableAllBreakpointsCommand => new DelegateCommand(() => EnableBreakpoints(false), () => AnyBreakpoints).ObservesProperty(() => AnyBreakpoints);

		public ICommand DeleteAllBreakpointsCommand => new DelegateCommand(() => {
			DebugManager.Debugger.DeleteAllBreakpoints();
			RaisePropertyChanged(nameof(Breakpoints));
		}, () => AnyBreakpoints).ObservesProperty(() => AnyBreakpoints);

		public ICommand NewBreakpointCommand => new DelegateCommand(() => {
		});

		private void EnableBreakpoints(bool enable) {
			foreach (var bp in _breakpoints)
				bp.IsEnabled = enable;
		}
	}
}

