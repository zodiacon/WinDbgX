using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Prism.Commands;

namespace WinDbgEx.Commands {
    static class FileCommands {
        public static DelegateCommandBase AttachToLocalKernel { get; } = new DelegateCommand<DebugContext>(async context => {
            try {
                await context.Debugger.AttachToLocalKernel();
            }
            catch(Exception ex) {

            }
        });

        public static DelegateCommandBase AttachToProcess { get; } = new DelegateCommand<DebugContext>(async context => {
            await context.Debugger.OpenDumpFile(@"d:\temp\memory.dmp");
        });

        public static DelegateCommandBase AttachToKernel { get; } = new DelegateCommand<DebugContext>(context => { });

        public static DelegateCommandBase RunExecutable { get; } = new DelegateCommand<DebugContext>(context => { });

        public static DelegateCommandBase Exit { get; } = new DelegateCommand(() => Application.Current.Shutdown());

        public static DelegateCommandBase OpenDumpFile { get; } = new DelegateCommand<DebugContext>(async context => {
            var dlg = context.FileDialogService.GetFileForOpen("Dump files (*.dmp)|*.dmp", "Select Dump File");
            if(dlg == null) return;

            await context.Debugger.OpenDumpFile(dlg);
            ulong miState = await context.Debugger.GetGlobalAddress("nt", "MiState");
            int zz = 9;
        });

    }
}
