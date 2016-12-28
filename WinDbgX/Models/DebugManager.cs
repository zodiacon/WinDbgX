using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DebuggerEngine;
using Zodiacon.WPF;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows;
using WinDbgX.UICore;
using Prism.Mvvm;
using DebuggerEngine.Interop;
using System.Windows.Threading;
using System.Collections.ObjectModel;

namespace WinDbgX.Models {
	[Export]
	sealed class DebugManager : BindableBase, IDisposable {
		public readonly DebugClient Debugger;
		readonly ObservableCollection<EventLogItem> _log = new ObservableCollection<EventLogItem>();

#pragma warning disable 649
		[Import]
		UIManager UI;
#pragma warning restore 649

		public IReadOnlyList<EventLogItem> Log => _log;

		public IReadOnlyList<TargetProcess> Processes => Debugger.Processes;

		public void ClearLog() {
			_log.Clear();
		}

		private DebugManager() {
			Debugger = DebugClient.Create();
			Debugger.StatusChanged += Debugger_StatusChanged;
			Debugger.ProcessCreated += Debugger_ProcessCreated;
			Debugger.ThreadCreated += Debugger_ThreadCreated;
			Debugger.ThreadExited += Debugger_ThreadExited;
			Debugger.ProcessExited += Debugger_ProcessExited;
			Debugger.ModuleLoaded += Debugger_ModuleLoaded;
			Debugger.ModuleUnloaded += Debugger_ModuleUnloaded;
		}

		private void Debugger_ModuleUnloaded(object sender, ModuleEventArgs e) {
			UI.InvokeAsync(() => {
				_log.Add(new EventLogItem<TargetModule>(EventLogItemType.ModuleUnload, DateTime.Now, e.Module));
			});
		}

		private void Debugger_ModuleLoaded(object sender, ModuleEventArgs e) {
			UI.InvokeAsync(() => {
				_log.Add(new EventLogItem<TargetModule>(EventLogItemType.ModuleLoad, DateTime.Now, e.Module));
			});
		}

		public IEnumerable<TargetThread> GetAllThreads() {
			return Processes.SelectMany(p => p.Threads);
		}

		private void Debugger_ProcessExited(object sender, ProcessExitedEventArgs e) {
			UI.InvokeAsync(() => {
				_log.Add(new EventLogItem<TargetProcess>(EventLogItemType.ProcessExit, DateTime.Now, e.Process));
			});
		}

		private void Debugger_ThreadExited(object sender, ThreadExitedEventArgs e) {
			UI.InvokeAsync(() => {
				_log.Add(new EventLogItem<TargetThread>(EventLogItemType.ThreadExit, DateTime.Now, e.Thread));
			});
		}

		private void Debugger_ThreadCreated(object sender, ThreadCreatedEventArgs e) {
			UI.InvokeAsync(() => {
				_log.Add(new EventLogItem<TargetThread>(EventLogItemType.ThreadCreate, DateTime.Now, e.Thread));
			});
		}

		private void Debugger_ProcessCreated(object sender, ProcessCreatedEventArgs e) {
			UI.InvokeAsync(() => {
				_log.Add(new EventLogItem<TargetProcess>(EventLogItemType.ProcessCreate, DateTime.Now, e.Process));
			});
		}

		private void Debugger_StatusChanged(object sender, StatusChangedEventArgs e) {
			UI.InvokeAsync(() => {
				Status = e.NewStatus;
				var oldStatus = e.OldStatus;
				OnPropertyChanged(nameof(Processes));
				if (Status == DEBUG_STATUS.NO_DEBUGGEE || oldStatus == DEBUG_STATUS.NO_DEBUGGEE) {
					var info = Debugger.GetTargetInfo();
					if (info != null)
						IsDumpFile = !info.Live;
				}
			});
		}

		private DEBUG_STATUS _status = DEBUG_STATUS.NO_DEBUGGEE;

		public DEBUG_STATUS Status {
			get { return _status; }
			private set { SetProperty(ref _status, value); }
		}

		public void Dispose() {
			Debugger.Dispose();
		}

		public bool IsDumpFile { get; internal set; }
	}
}
