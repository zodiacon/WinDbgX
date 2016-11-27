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
	sealed class DebugContext : BindableBase, IDisposable {
		public readonly DebugClient Debugger;
		ObservableCollection<TargetProcess> _processes = new ObservableCollection<TargetProcess>();
		Dispatcher _dispatcher;

		public IEnumerable<TargetProcess> Processes => _processes;

		private DebugContext() {
			Debugger = DebugClient.CreateAsync().Result;
			Debugger.StatusChanged += Debugger_StatusChanged;
			Debugger.ProcessCreated += Debugger_ProcessCreated;
			_dispatcher = Dispatcher.CurrentDispatcher;
		}

		private void Debugger_ProcessCreated(object sender, ProcessCreatedEventArgs e) {
			_dispatcher.InvokeAsync(() => _processes.Add(e.Process));
		}

		private void Debugger_StatusChanged(object sender, StatusChangedEventArgs e) {
			_dispatcher.InvokeAsync(() => Status = e.NewStatus);
		}

		[Import]
		public IFileDialogService FileDialogService { get; private set; }

		[Import]
		public IDialogService DialogService { get; private set; }

		[Import]
		public IMessageBoxService MessageBoxService { get; private set; }

		[Import]
		public UIContext UI { get; private set; }

		public void ReportError(Exception ex) {
			Debug.Assert(MessageBoxService != null);
			MessageBoxService.ShowMessage(ex.Message, Constants.Title);
		}

		static DebugContext _context;
		public static DebugContext Instance {
			get {
				if (_context == null) {
					_context = App.Container.GetExportedValue<DebugContext>();
					Debug.Assert(_context != null);
					App.Container.ComposeExportedValue(_context);
				}
				return _context;
			}
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
