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
	class ThreadsViewModel : TabViewModelBase {
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
			_dispatcher.InvokeAsync(() => _processes.RemoveAt((int)e.Index));
		}

		private void Debugger_ThreadExited(object sender, ThreadExitedEventArgs e) {
			_dispatcher.InvokeAsync(() => {
				var threads = _processes[(int)e.ProcessIndex].Threads;
				threads.Remove(threads.First(th => th.OSID == e.TID));
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
			Status = e.NewStatus;
		}

		private DEBUG_STATUS _status;

		public DEBUG_STATUS Status {
			get { return _status; }
			private set { SetProperty(ref _status, value); }
		}

		private bool _isEnabled;

		public bool IsEnabled {
			get { return _isEnabled; }
			set { SetProperty(ref _isEnabled, value); }
		}

	}
}
