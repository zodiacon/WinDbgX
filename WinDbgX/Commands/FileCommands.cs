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

#pragma warning disable 649

namespace WinDbgX.Commands {
	[Export(typeof(ICommandCollection))]
	class FileCommands : ICommandCollection {
		[Import]
		AppManager AppManager;

		[Import]
		DebugManager DebugManager;

		[Import]
		UIManager UIManager;

		public DelegateCommandBase AttachToLocalKernel { get; }

		public DelegateCommandBase AttachToProcess { get; } 

		public DelegateCommandBase AttachToKernel { get; }

		public DelegateCommand RunExecutable { get; } 

		public DelegateCommandBase Exit { get; } = new DelegateCommand(() => Application.Current.Shutdown());

		public DelegateCommandBase OpenDumpFile { get; }

		public DelegateCommandBase OpenSourceFile { get; } 

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

		private FileCommands() {
			AttachToLocalKernel = new DelegateCommand(() => {
				try {
					if (!DebugManager.Debugger.IsLocalKernelEnabled()) {
						UIManager.MessageBoxService.ShowMessage("Local kernel debugging is unavailable. Use bcdedit -debug on, and restart the system.",
							Constants.Title, MessageBoxButton.OK, MessageBoxImage.Information);
						return;
					}
					DebugManager.Debugger.AttachToLocalKernel();
				}
				catch (Exception ex) {
					UIManager.ReportError(ex);
				}
			}, () => DebugManager.Status == DEBUG_STATUS.NO_DEBUGGEE);

			AttachToProcess = new DelegateCommand(() => {
				var vm = UIManager.DialogService.CreateDialog<AttachToProcessViewModel, GenericWindow>();
				if (vm.ShowDialog() == true) {
					var pid = vm.SelectedProcess.Id;
					DebugManager.Debugger.AttachToProcess(pid, AttachProcessFlags.Invasive);
				}
			}, () => DebugManager.Status == DEBUG_STATUS.NO_DEBUGGEE);

			RunExecutable = new DelegateCommand(async () => {
				var vm = UIManager.DialogService.CreateDialog<RunExecutableViewModel, GenericWindow>(UIManager.FileDialogService);
				if (vm.ShowDialog() == true) {
					try {
						await DebugManager.Debugger.DebugProcess(vm.ExecutablePath, vm.CommandLine, AttachProcessFlags.Invasive, vm.DebugChildren);
						UIManager.AddExecutable(new Executable {
							Path = vm.ExecutablePath,
							Arguments = vm.CommandLine
						});
					}
					catch (AggregateException ex) {
						UIManager.ReportError(ex.GetBaseException());
					}
				}
			}, () => DebugManager.Status == DEBUG_STATUS.NO_DEBUGGEE);

			AttachToKernel = new DelegateCommand(() => {
			}, () => DebugManager.Status == DEBUG_STATUS.NO_DEBUGGEE);

			OpenDumpFile = new DelegateCommand(() => {
				var filename = UIManager.FileDialogService.GetFileForOpen("Dump files (*.dmp)|*.dmp", "Select Dump File");
				if (filename == null) return;

				DebugManager.Debugger.OpenDumpFile(filename);
			}, () => DebugManager.Status == DEBUG_STATUS.NO_DEBUGGEE);

			OpenSourceFile = new DelegateCommand(() => {
				var filename = UIManager.FileDialogService.GetFileForOpen("Source files|*.c;*.cpp;*.h;*.cxx;*.cs", "Select File");
				if (filename == null) return;

				var tab = AppManager.Container.GetExportedValue<SourceCodeViewModel>();
				tab.OpenFile(filename);
				UIManager.CurrentWindow.AddItem(tab);
			});
		}
	}
}
