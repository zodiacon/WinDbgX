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
            await context.Debugger.OpenDumpFileAsync(@"d:\temp\memory.dmp");
        });

        public static DelegateCommandBase AttachToKernel { get; } = new DelegateCommand<DebugContext>(context => { });

        public static DelegateCommandBase RunExecutable { get; } = new DelegateCommand<DebugContext>(context => { });

        public static DelegateCommandBase Exit { get; } = new DelegateCommand(() => Application.Current.Shutdown());
    }
}
