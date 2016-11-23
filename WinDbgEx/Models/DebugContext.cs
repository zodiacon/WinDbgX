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

namespace WinDbgEx.Models {
	[Export]
    sealed class DebugContext : IDisposable {
        public readonly DebugClient Debugger;
		
        private DebugContext() {
			Debugger = DebugClient.CreateAsync().Result;
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
				}
				return _context;
			}
		}

		public void Dispose() {
			Debugger.Dispose();
		}
	}
}
