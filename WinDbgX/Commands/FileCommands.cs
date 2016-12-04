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

namespace WinDbgX.Commands {
    static class FileCommands {
		public static DelegateCommandBase AttachToLocalKernel { get; } = new DelegateCommand<AppManager>(async context => {
			try {
				if (! await context.Debug.Debugger.IsLocalKernelEnabled()) {
					context.UI.MessageBoxService.ShowMessage("Local kernel debugging is unavailable. Use bcdedit -debug on, and restart the system.",
						Constants.Title, MessageBoxButton.OK, MessageBoxImage.Information);
					return;
				}
				context.Debug.Debugger.AttachToLocalKernel();
			}
			catch (Exception ex) {
				context.UI.ReportError(ex);
			}
		}, context => context.Debug.Status == DEBUG_STATUS.NO_DEBUGGEE);

        public static DelegateCommandBase AttachToProcess { get; } = new DelegateCommand<AppManager>(context => {
			var vm = context.UI.DialogService.CreateDialog<AttachToProcessViewModel, GenericWindow>();
			if (vm.ShowDialog() == true) {
				var pid = vm.SelectedProcess.Id;
				context.Debug.Debugger.AttachToProcess(pid, AttachProcessFlags.Invasive);
			}
        }, context => context.Debug.Status == DEBUG_STATUS.NO_DEBUGGEE);

        public static DelegateCommandBase AttachToKernel { get; } = new DelegateCommand<AppManager>(context => { });

        public static DelegateCommandBase RunExecutable { get; } = new DelegateCommand<AppManager>(async context => {
			var vm = context.UI.DialogService.CreateDialog<RunExecutableViewModel, GenericWindow>(context.UI.FileDialogService);
			if (vm.ShowDialog() == true) {
				try {
					await context.Debug.Debugger.DebugProcess(vm.ExecutablePath, vm.CommandLine, AttachProcessFlags.Invasive, vm.DebugChildren);
				}
				catch (AggregateException ex) {
					context.UI.ReportError(ex.GetBaseException());
				}
			}
		}, context => context.Debug.Status == DEBUG_STATUS.NO_DEBUGGEE);

        public static DelegateCommandBase Exit { get; } = new DelegateCommand(() => Application.Current.Shutdown());

        public static DelegateCommandBase OpenDumpFile { get; } = new DelegateCommand<AppManager>(async context => {
            var dlg = context.UI.FileDialogService.GetFileForOpen("Dump files (*.dmp)|*.dmp", "Select Dump File");
            if(dlg == null) return;

            await context.Debug.Debugger.OpenDumpFile(dlg);
            ulong miState = await context.Debug.Debugger.GetGlobalAddress("nt", "MiState");
        }, context => context.Debug.Status == DEBUG_STATUS.NO_DEBUGGEE);

    }
}
