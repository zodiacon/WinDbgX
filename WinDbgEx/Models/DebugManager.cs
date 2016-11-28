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
using WinDbgEx.UICore;
using Prism.Mvvm;
using DebuggerEngine.Interop;
using System.Windows.Threading;
using System.Collections.ObjectModel;

namespace WinDbgEx.Models {
	[Export]
	sealed class DebugManager : BindableBase, IDisposable {
		public readonly DebugClient Debugger;
		ObservableCollection<TargetProcess> _processes = new ObservableCollection<TargetProcess>();
		Dispatcher _dispatcher;
		ObservableCollection<EventLogItem> _log = new ObservableCollection<EventLogItem>();

		public IReadOnlyList<EventLogItem> Log => _log;

		public IReadOnlyList<TargetProcess> Processes => _processes;

		public void ClearLog() {
			_log.Clear();
		}

		private DebugManager() {
			Debugger = DebugClient.CreateAsync().Result;
			Debugger.StatusChanged += Debugger_StatusChanged;
			Debugger.ProcessCreated += Debugger_ProcessCreated;
			Debugger.ThreadCreated += Debugger_ThreadCreated;
			Debugger.ThreadExited += Debugger_ThreadExited;
			Debugger.ProcessExited += Debugger_ProcessExited;

			_dispatcher = Dispatcher.CurrentDispatcher;
		}

		private void Debugger_ProcessExited(object sender, ProcessExitedEventArgs e) {
			_dispatcher.InvokeAsync(() => {
				var process = _processes[(int)e.Index];
				process.ExitCode = e.ExitCode;
				_log.Add(new EventLogItem<TargetProcess>(EventLogItemType.ProcessExit, DateTime.Now, process));
				_processes.RemoveAt((int)e.Index);
			});
		}

		private void Debugger_ThreadExited(object sender, ThreadExitedEventArgs e) {
			_dispatcher.InvokeAsync(() => {
				var process = _processes[(int)e.ProcessIndex];
				var thread = process.Threads.FirstOrDefault(t => t.TID == e.TID);
				Debug.Assert(thread != null);

				thread.ExitCode = e.ExitCode;
				_log.Add(new EventLogItem<TargetThread>(EventLogItemType.ThreadExit, DateTime.Now, thread));
				process.Threads.Remove(thread);
			});
		}

		private void Debugger_ThreadCreated(object sender, ThreadCreatedEventArgs e) {
			_dispatcher.InvokeAsync(() => {
				_processes[(int)e.Thread.ProcessIndex].Threads.Add(e.Thread);
				_log.Add(new EventLogItem<TargetThread>(EventLogItemType.ThreadCreate, DateTime.Now, e.Thread));
			});
		}

		private void Debugger_ProcessCreated(object sender, ProcessCreatedEventArgs e) {
			_dispatcher.InvokeAsync(() => _processes.Add(e.Process));
			_log.Add(new EventLogItem<TargetProcess>(EventLogItemType.ProcessCreate, DateTime.Now, e.Process));
		}

		private void Debugger_StatusChanged(object sender, StatusChangedEventArgs e) {
			_dispatcher.InvokeAsync(() => Status = e.NewStatus);
		}

		
		private DEBUG_STATUS _status = DEBUG_STATUS.NO_DEBUGGEE;

		public DEBUG_STATUS Status {
			get { return _status; }
			private set {
				if (SetProperty(ref _status, value)) {
				}
			}
		}

		public void Dispose() {
			Debugger.Dispose();
		}
	}
}
