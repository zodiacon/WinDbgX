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
	[TabItem("Threads", Icon = "/icons/thread.ico")]
	[Export]
	class ThreadsViewModel : TabViewModelBase {
		Dispatcher _dispatcher;
		ObservableCollection<ProcessViewModel> _processes = new ObservableCollection<ProcessViewModel>();

		public IList<ProcessViewModel> Processes => _processes;

		DebugContext DebugContext;

		public ThreadsViewModel() {
			DebugContext = DebugContext.Instance;
			_dispatcher = Dispatcher.CurrentDispatcher;
			foreach (var process in DebugContext.Processes)
				_processes.Add(new ProcessViewModel(process));

			DebugContext.Debugger.StatusChanged += Debugger_StatusChanged;
			DebugContext.Debugger.ProcessCreated += Debugger_ProcessCreated;
		}

		private void Debugger_ProcessCreated(object sender, ProcessCreatedEventArgs e) {
			_dispatcher.InvokeAsync(() => {
				_processes.Add(new ProcessViewModel(e.Process));
			});
		}

		private void Debugger_StatusChanged(object sender, StatusChangedEventArgs e) {
			var status = e.NewStatus;
			_dispatcher.InvokeAsync(() => {
			});
		}

		private bool _isEnabled;

		public bool IsEnabled {
			get { return _isEnabled; }
			set { SetProperty(ref _isEnabled, value); }
		}

	}
}
