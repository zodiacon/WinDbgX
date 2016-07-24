using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Runtime.Interop;

namespace DebuggerEngine {
    public partial class DebugClient : CriticalFinalizerObject, IDisposable {
        internal readonly IDebugClient6 Client;
        internal readonly IDebugAdvanced3 Advanced;
        internal readonly IDebugDataSpaces4 DataSpaces;
        internal readonly IDebugSymbols5 Symbols;
        internal readonly IDebugSystemObjects3 SystemObjects;
        internal readonly IDebugControl6 Control;

        readonly TaskScheduler _scheduler;

        private DebugClient(object client, TaskScheduler scheduler) {
            _scheduler = scheduler;

            Client = (IDebugClient6)client;
            Control = (IDebugControl6)client;
            DataSpaces = (IDebugDataSpaces4)client;
            SystemObjects = (IDebugSystemObjects3)client;
            Symbols = (IDebugSymbols5)client;
            Advanced = (IDebugAdvanced3)client;

            Client.SetEventCallbacksWide(new EventCallbacks(Control)).ThrowIfFailed();
            Client.SetOutputCallbacksWide(new OutputCallbacks()).ThrowIfFailed();
        }

        [DllImport("dbgeng", PreserveSig = true)]
        private static extern int DebugCreate(ref Guid iid, [MarshalAs(UnmanagedType.Interface)] out object iface);

        public static Task<DebugClient> CreateAsync() {
            var scheduler = new SingleThreadedTaskScheduler();
            return Task.Factory.StartNew(() => {
                var iid = typeof(IDebugClient).GUID;
                object client;
                DebugCreate(ref iid, out client).ThrowIfFailed("Failed to create debug client");
                return new DebugClient(client, scheduler);
            }, CancellationToken.None, TaskCreationOptions.None, scheduler);
        }


        private Task<T> RunAsync<T>(Func<T> method) {
            return Task.Factory.StartNew(() => method(), CancellationToken.None, TaskCreationOptions.None, _scheduler);
        }

        private Task RunAsync(Action method) {
            return Task.Factory.StartNew(method, CancellationToken.None, TaskCreationOptions.None, _scheduler);
        }

        private void WaitForEvent(uint msec = uint.MaxValue) {
            Control.WaitForEvent(DEBUG_WAIT.DEFAULT, msec).ThrowIfFailed();
        }

        public Task AttachToLocalKernel() {
            return RunAsync(() => {
                Client.AttachKernel(DEBUG_ATTACH.LOCAL_KERNEL, null).ThrowIfFailed();
                WaitForEvent();
            });
        }

        public Task OpenDumpFileAsync(string file) {
            return RunAsync(() => {
                Client.OpenDumpFileWide(file, 0).ThrowIfFailed();
                WaitForEvent();
            });
        }

        public IEnumerable<SymbolInfo> FindSymbols(string pattern) {
            ulong handle;
            Symbols.StartSymbolMatchWide(pattern, out handle).ThrowIfFailed();
            var sb = new StringBuilder(256);
            uint matchSize;
            ulong offset;
            for(;;) {
                int hr = Symbols.GetNextSymbolMatchWide(handle, sb, 255, out matchSize, out offset);
                if(hr < 0)
                    break;

                yield return new SymbolInfo {
                    Name = sb.ToString(),
                    Offset = offset
                };
            }
            Symbols.EndSymbolMatch(handle);
        }

        public Task<ModuleInfo[]> GetModulesAsync() {
            return RunAsync(() => {
                uint loaded, unloaded;
                Symbols.GetNumberModules(out loaded, out unloaded).ThrowIfFailed();
                for (uint i = 0; i < loaded; i++) {
                    ulong moduleBase;
                    Symbols.GetModuleByIndex(i, out moduleBase).ThrowIfFailed();
                }
                var modules = new DEBUG_MODULE_PARAMETERS[loaded + unloaded];
                Symbols.GetModuleParameters(loaded + unloaded, null, 0, modules).ThrowIfFailed();
                return modules.Select(param => ModuleInfo.FromModuleParameters(param)).ToArray();
            });
        }

        public Task ExecuteAsync(string command) {
            return RunAsync(() => {
                Control.ExecuteWide(DEBUG_OUTCTL.ALL_CLIENTS, command, DEBUG_EXECUTE.ECHO);
            });
        }

        public Task GetProcesses() {
            return RunAsync(() => {

                uint n;
                SystemObjects.GetNumberSystems(out n);
                uint totalThreads, totalProcesses, largestProcessThreads, largestSystemThreads, largestSystemProcesses;
                SystemObjects.GetTotalNumberThreadsAndProcesses(out totalThreads, out totalProcesses, out largestProcessThreads, out largestSystemThreads, out largestSystemProcesses);

                ulong pEprocess;
                SystemObjects.GetCurrentProcessDataOffset(out pEprocess);

                ulong offset;
                Symbols.GetOffsetByNameWide("nt!_EPROCESS", out offset);

                Control.ExecuteWide(DEBUG_OUTCTL.THIS_CLIENT, "x nt!_eproc*", DEBUG_EXECUTE.ECHO);
            });
        }

        public void Dispose() {
            Dispose(true);
        }

        ~DebugClient() {
            Dispose(false);
        }

        protected virtual void Dispose(bool isDisposing) {
            if(isDisposing) {
                GC.SuppressFinalize(this);
            }
            ((IDisposable)_scheduler).Dispose();
            Client.EndSession(DEBUG_END.ACTIVE_DETACH);
        }
    }
}
