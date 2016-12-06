using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Prism.Commands;
using WinDbgX.Models;
using WinDbgX.ViewModels;
using WinDbgX.Windows;
using DebuggerEngine;
using DebuggerEngine.Interop;
using System.Reflection;
using System.ComponentModel.Composition;
using WinDbgX.UICore;
using System.Windows.Input;

namespace WinDbgX.Commands {
	[Export(typeof(ICommandCollection))]
	class FileCommands : ICommandCollection {
		public DelegateCommandBase AttachToLocalKernel { get; } = new DelegateCommand<AppManager>(context => {
			try {
				if (!context.Debug.Debugger.IsLocalKernelEnabled()) {
					context.UI.MessageBoxService.ShowMessage("Local kernel debugging is unavailable. Use bcdedit -debug on, and restart the system.",
						Constants.Title, MessageBoxButton.OK, MessageBoxImage.Information);
					return;
				}
				context.Debug.Debugger.AttachToLocalKernel();
			}
			catch (Exception ex) {
				context.UI.ReportError(ex);
			}
		}, context => (context ?? AppManager.Instance).Debug.Status == DEBUG_STATUS.NO_DEBUGGEE);


		public DelegateCommand<AppManager> AttachToProcess { get; } = new DelegateCommand<AppManager>(context => {
			var vm = context.UI.DialogService.CreateDialog<AttachToProcessViewModel, GenericWindow>();
			if (vm.ShowDialog() == true) {
				var pid = vm.SelectedProcess.Id;
				context.Debug.Debugger.AttachToProcess(pid, AttachProcessFlags.Invasive);
			}
		}, context => (context ?? AppManager.Instance).Debug.Status == DEBUG_STATUS.NO_DEBUGGEE);

		public DelegateCommandBase AttachToKernel { get; } = new DelegateCommand<AppManager>(context => {
		}, context => context == null || context.Debug.Status == DEBUG_STATUS.NO_DEBUGGEE);

		public DelegateCommand<AppManager> RunExecutable { get; } = new DelegateCommand<AppManager>(async context => {
			var vm = context.UI.DialogService.CreateDialog<RunExecutableViewModel, GenericWindow>(context.UI.FileDialogService);
			if (vm.ShowDialog() == true) {
				try {
					await context.Debug.Debugger.DebugProcess(vm.ExecutablePath, vm.CommandLine, AttachProcessFlags.Invasive, vm.DebugChildren);
					context.UI.AddExecutable(new Executable {
						Path = vm.ExecutablePath,
						Arguments = vm.CommandLine
					});
				}
				catch (AggregateException ex) {
					context.UI.ReportError(ex.GetBaseException());
				}
			}
		}, context => (context ?? AppManager.Instance).Debug.Status == DEBUG_STATUS.NO_DEBUGGEE);

		public DelegateCommandBase Exit { get; } = new DelegateCommand(() => Application.Current.Shutdown());

		public DelegateCommand<AppManager> OpenDumpFile { get; } = new DelegateCommand<AppManager>(context => {
			var filename = context.UI.FileDialogService.GetFileForOpen("Dump files (*.dmp)|*.dmp", "Select Dump File");
			if (filename == null) return;

			context.Debug.Debugger.OpenDumpFile(filename);
		}, context => (context ?? AppManager.Instance).Debug.Status == DEBUG_STATUS.NO_DEBUGGEE);

		public DelegateCommandBase OpenSourceFile { get; } = new DelegateCommand<AppManager>(context => {
			var filename = context.UI.FileDialogService.GetFileForOpen("Source files|*.dmp;*.c;*.cpp;*.h;*.cxx;*.cs", "Select File");
			if (filename == null) return;

			var tab = context.Container.GetExportedValue<SourceCodeViewModel>();
			tab.OpenFile(filename);
			context.UI.CurrentWindow.AddItem(tab);
		});

		public IDictionary<string, ICommand> GetCommands() {
			return new Dictionary<string, ICommand> {
				{ nameof(RunExecutable), RunExecutable },
				{ nameof(AttachToProcess), AttachToProcess },
				{ nameof(AttachToLocalKernel), AttachToLocalKernel },
				{ nameof(AttachToKernel), AttachToKernel },
				{ nameof(OpenDumpFile), OpenDumpFile },
				{ nameof(OpenSourceFile), OpenSourceFile },
				{ nameof(Exit), Exit }
			};
		}
	}
}
