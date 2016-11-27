using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Prism.Commands;
using WinDbgEx.Models;
using WinDbgEx.ViewModels;
using WinDbgEx.Windows;
using DebuggerEngine;
using DebuggerEngine.Interop;
using System.Reflection;

namespace WinDbgEx.Commands {
    static class FileCommands {
		public static DelegateCommandBase AttachToLocalKernel { get; } = new DelegateCommand<DebugContext>(context => {
			try {
				context.Debugger.AttachToLocalKernel();
			}
			catch (Exception ex) {
				context.ReportError(ex);
			}
		}, context => context.Status == DEBUG_STATUS.NO_DEBUGGEE);

        public static DelegateCommandBase AttachToProcess { get; } = new DelegateCommand<DebugContext>(context => {
            
        }, context => context.Status == DEBUG_STATUS.NO_DEBUGGEE);

        public static DelegateCommandBase AttachToKernel { get; } = new DelegateCommand<DebugContext>(context => { });

        public static DelegateCommandBase RunExecutable { get; } = new DelegateCommand<DebugContext>(async context => {
			var vm = context.DialogService.CreateDialog<RunExecutableViewModel, GenericWindow>(context.FileDialogService);
			if (vm.ShowDialog() == true) {
				try {
					await context.Debugger.DebugProcess(vm.ExecutablePath, vm.CommandLine, AttachProcessFlags.Invasive, vm.DebugChildren);
				}
				catch (AggregateException ex) {
					context.ReportError(ex.GetBaseException());
				}
			}
		}, context => context.Status == DEBUG_STATUS.NO_DEBUGGEE);

        public static DelegateCommandBase Exit { get; } = new DelegateCommand(() => Application.Current.Shutdown());

        public static DelegateCommandBase OpenDumpFile { get; } = new DelegateCommand<DebugContext>(async context => {
            var dlg = context.FileDialogService.GetFileForOpen("Dump files (*.dmp)|*.dmp", "Select Dump File");
            if(dlg == null) return;

            await context.Debugger.OpenDumpFile(dlg);
            ulong miState = await context.Debugger.GetGlobalAddress("nt", "MiState");
        }, context => context.Status == DEBUG_STATUS.NO_DEBUGGEE);

    }
}
