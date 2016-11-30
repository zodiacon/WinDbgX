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
using WinDbgEx.Models;
using WinDbgEx.UICore;

namespace WinDbgEx.ViewModels {
	[TabItem("Processes & Threads", Icon = "/icons/gears.ico")]
	[Export]
	class ThreadsViewModel : TabItemViewModelBase {
		Dispatcher _dispatcher;
		ObservableCollection<ProcessViewModel> _processes = new ObservableCollection<ProcessViewModel>();

		public IList<ProcessViewModel> Processes => _processes;

		readonly DebugManager DebugContext;

		[ImportingConstructor]
		public ThreadsViewModel(DebugManager debug) {
			DebugContext = debug;
			_dispatcher = Dispatcher.CurrentDispatcher;
			foreach (var process in DebugContext.Processes) {
				var processVM = new ProcessViewModel(process);
				foreach (var th in process.Threads)
					processVM.Threads.Add(new ThreadViewModel(th));
				_processes.Add(processVM);
			}

			Status = DebugContext.Status;
			DebugContext.Debugger.StatusChanged += Debugger_StatusChanged;

			DebugContext.Debugger.ProcessCreated += Debugger_ProcessCreated;
			DebugContext.Debugger.ThreadCreated += Debugger_ThreadCreated;
			DebugContext.Debugger.ThreadExited += Debugger_ThreadExited;
			DebugContext.Debugger.ProcessExited += Debugger_ProcessExited;
		}

		private void Debugger_ProcessExited(object sender, ProcessExitedEventArgs e) {
			_dispatcher.InvokeAsync(() => {
				_processes.Remove(_processes.First(p => p.ProcessId == e.Process.PID));
			});
		}

		private void Debugger_ThreadExited(object sender, ThreadExitedEventArgs e) {
			_dispatcher.InvokeAsync(() => {
				var threads = _processes.First(p => p.ProcessId == e.Process.PID).Threads;
				threads.Remove(threads.First(th => th.OSID == e.Thread.TID));
			});
		}

		private void Debugger_ThreadCreated(object sender, ThreadCreatedEventArgs e) {
			_dispatcher.InvokeAsync(() => {
				_processes[(int)e.Thread.ProcessIndex].Threads.Add(new ThreadViewModel(e.Thread));
			});
		}

		private void Debugger_ProcessCreated(object sender, ProcessCreatedEventArgs e) {
			_dispatcher.InvokeAsync(() => {
				_processes.Add(new ProcessViewModel(e.Process));
			});
		}

		private void Debugger_StatusChanged(object sender, StatusChangedEventArgs e) {
			var status = e.NewStatus;
			_dispatcher.InvokeAsync(() => {
				Status = status;
				if (Status == DEBUG_STATUS.NO_DEBUGGEE) {
					_dispatcher.InvokeAsync(() => Processes.Clear());
				}
			});
		}

		private DEBUG_STATUS _status;

		public DEBUG_STATUS Status {
			get { return _status; }
			private set {
				if (SetProperty(ref _status, value)) {
					OnPropertyChanged(nameof(IsEnabled));
				}
			}
		}

		public bool IsEnabled => Status == DEBUG_STATUS.BREAK;

	}
}
