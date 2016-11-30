using DebuggerEngine.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DebuggerEngine {
	partial class DebugClient : IDebugEventCallbacksWide {
		List<TargetProcess> _processes = new List<TargetProcess>(2);

		bool _stateChanged;
		bool _breakpointHit;

		public IReadOnlyList<TargetProcess> Processes => _processes;

		public event EventHandler<ProcessCreatedEventArgs> ProcessCreated;
		public event EventHandler<ThreadCreatedEventArgs> ThreadCreated;
		public event EventHandler<ProcessExitedEventArgs> ProcessExited;
		public event EventHandler<ThreadExitedEventArgs> ThreadExited;
		public event EventHandler<ModuleEventArgs> ModuleLoaded;
		public event EventHandler<ModuleEventArgs> ModuleUnloaded;
		public event EventHandler<BreakpointChangedEventArgs> BreakpointChanged;

		void OnProcessCreated(TargetProcess process) {
			ProcessCreated?.Invoke(this, new ProcessCreatedEventArgs(process));
		}

		void OnThreadCreated(ThreadCreatedEventArgs e) {
			ThreadCreated?.Invoke(this, e);
		}

		void OnProcessExited(ProcessExitedEventArgs e) {
			ProcessExited?.Invoke(this, e);
		}

		void OnThreadExited(ThreadExitedEventArgs e) {
			ThreadExited?.Invoke(this, e);
		}

		void OnModuleLoaded(ModuleEventArgs e) {
			ModuleLoaded?.Invoke(this, e);
		}

		void OnModuleUnloaded(ModuleEventArgs e) {
			ModuleUnloaded?.Invoke(this, e);
		}

		void OnBreakpointChanged(BreakpointChangedEventArgs e) {
			BreakpointChanged?.Invoke(this, e);
		}

		int IDebugEventCallbacksWide.GetInterestMask(out DEBUG_EVENT Mask) {
			Mask = DEBUG_EVENT.BREAKPOINT | DEBUG_EVENT.CHANGE_DEBUGGEE_STATE | DEBUG_EVENT.CHANGE_ENGINE_STATE | DEBUG_EVENT.CHANGE_SYMBOL_STATE | DEBUG_EVENT.CREATE_PROCESS
				| DEBUG_EVENT.CREATE_THREAD | DEBUG_EVENT.EXCEPTION | DEBUG_EVENT.EXIT_PROCESS | DEBUG_EVENT.EXIT_THREAD | DEBUG_EVENT.LOAD_MODULE |
				DEBUG_EVENT.SESSION_STATUS | DEBUG_EVENT.SYSTEM_ERROR | DEBUG_EVENT.UNLOAD_MODULE;

			return 0;
		}

		int IDebugEventCallbacksWide.Breakpoint(IDebugBreakpoint2 Bp) {
			_breakpointHit = true;
			_stateChanged = true;
			UpdateStatus();

			//Control.OutputCurrentState(DEBUG_OUTCTL.THIS_CLIENT, DEBUG_CURRENT.DEFAULT);

			return (int)DEBUG_STATUS.BREAK;
		}

		int IDebugEventCallbacksWide.Exception(EXCEPTION_RECORD64 Exception, uint FirstChance) {
			_breakpointHit = Exception.ExceptionCode == 0x80000004 || Exception.ExceptionCode == 0x80000003 || FirstChance == 0;
			_stateChanged = true;

			//if (_breakpointHit)
			//	Control.OutputCurrentState(DEBUG_OUTCTL.THIS_CLIENT, DEBUG_CURRENT.DEFAULT);

			//UpdateStatus();
			return _breakpointHit ? (int)DEBUG_STATUS.BREAK : (int)DEBUG_STATUS.NO_CHANGE;
		}

		int IDebugEventCallbacksWide.CreateThread(ulong Handle, ulong DataOffset, ulong StartOffset) {
			uint id, tid, pindex, pid;
			SystemObjects.GetCurrentProcessId(out pindex);
			SystemObjects.GetCurrentThreadId(out id);
			SystemObjects.GetCurrentProcessSystemId(out pid);
			SystemObjects.GetCurrentThreadSystemId(out tid);
			Debug.Assert(tid > 0 && pid > 0);

			var process = _processes.First(p => p.PID == pid);

			var thread = new TargetThread(process) {
				Index = id,
				TID = tid,
				StartAddress = StartOffset,
				Teb = DataOffset,
				Handle = Handle,
				ProcessIndex = pindex
			};

			process.AddThread(thread);

			OnThreadCreated(new ThreadCreatedEventArgs(thread, process));

			return (int)DEBUG_STATUS.NO_CHANGE;
		}

		int IDebugEventCallbacksWide.ExitThread(uint ExitCode) {
			uint id, pindex, tid, pid;
			SystemObjects.GetCurrentThreadId(out id);
			SystemObjects.GetCurrentProcessId(out pindex);
			SystemObjects.GetCurrentProcessSystemId(out pid);
			SystemObjects.GetCurrentThreadSystemId(out tid);

			var process = _processes.First(p => p.PID == pid);
			var thread = process.Threads.First(t => t.TID == tid);
			thread.ExitCode = ExitCode;

			process.RemoveThread(thread);

			OnThreadExited(new ThreadExitedEventArgs(thread, process));

			return (int)DEBUG_STATUS.NO_CHANGE;
		}

		int IDebugEventCallbacksWide.CreateProcess(ulong ImageFileHandle, ulong Handle, ulong BaseOffset, uint ModuleSize, string ModuleName, string ImageName,
			uint CheckSum, uint TimeDateStamp, ulong InitialThreadHandle, ulong ThreadDataOffset, ulong StartOffset) {
			Debug.WriteLine("IDebugEventCallbacksWide.CreateProcess");

			uint id;
			SystemObjects.GetCurrentProcessId(out id);
			ulong peb;
			SystemObjects.GetCurrentProcessPeb(out peb);
			uint pid;
			SystemObjects.GetCurrentProcessSystemId(out pid);

			var process = new TargetProcess {
				PID = pid,
				hProcess = Handle,
				hFile = ImageFileHandle,
				BaseOffset = BaseOffset,
				ModuleSize = ModuleSize,
				ImageName = ImageName,
				TimeStamp = DateTime.FromFileTime(TimeDateStamp),
				ModuleName = ModuleName,
				Index = (int)id,
				Peb = peb
			};

			_processes.Add(process);

			OnProcessCreated(process);

			uint tindex, tid;
			SystemObjects.GetCurrentThreadId(out tindex);
			SystemObjects.GetCurrentThreadSystemId(out tid);
			var thread = new TargetThread (process) {
				Index = tindex,
				TID = tid,
				StartAddress = StartOffset,
				Teb = ThreadDataOffset,
				Handle = InitialThreadHandle,
				ProcessIndex = id
			};

			process.AddThread(thread);

			OnThreadCreated(new ThreadCreatedEventArgs(thread, process));

			return (int)DEBUG_STATUS.NO_CHANGE;
		}

		int IDebugEventCallbacksWide.ExitProcess(uint ExitCode) {
			Debug.WriteLine("IDebugEventCallbacksWide.ExitProcess");

			uint pid;
			SystemObjects.GetCurrentProcessSystemId(out pid);
			var process = _processes.First(p => p.PID == pid);
			process.ExitCode = ExitCode;

			OnProcessExited(new ProcessExitedEventArgs(process));

			UpdateStatus();
			return (int)DEBUG_STATUS.NO_CHANGE;
		}

		int IDebugEventCallbacksWide.LoadModule(ulong ImageFileHandle, ulong BaseOffset, uint ModuleSize, string ModuleName, string ImageName, uint CheckSum, uint TimeDateStamp) {
			uint id, pid;
			SystemObjects.GetCurrentProcessId(out id);
			SystemObjects.GetCurrentProcessSystemId(out pid);

			var module = new TargetModule {
				ImageName = ImageName,
				Name = ModuleName,
				BaseAddress = BaseOffset,
				Size = ModuleSize,
				TimeStamp = TimeDateStamp,
				Handle = ImageFileHandle,
				ProcessIndex = id,
				PID = pid
			};

			var process = _processes.First(p => p.PID == pid);
			process.AddModule(module);

			OnModuleLoaded(new ModuleEventArgs(process, module));

			return (int)DEBUG_STATUS.NO_CHANGE;
		}

		int IDebugEventCallbacksWide.UnloadModule(string ImageBaseName, ulong BaseOffset) {
			uint id, pid;
			SystemObjects.GetCurrentProcessId(out id);
			SystemObjects.GetCurrentProcessSystemId(out pid);

			var process = _processes.First(p => p.PID == pid);
			var module = process.Modules.First(m => m.BaseAddress == BaseOffset);

			process.RemoveModule(module);
			OnModuleUnloaded(new ModuleEventArgs(process, module));
			 
			return (int)DEBUG_STATUS.NO_CHANGE;
		}

		int IDebugEventCallbacksWide.SystemError(uint Error, uint Level) {
			return (int)DEBUG_STATUS.NO_CHANGE;
		}

		int IDebugEventCallbacksWide.SessionStatus(DEBUG_SESSION Status) {
			Debug.WriteLine("IDebugEventCallbacksWide.SessionStatus");
			UpdateStatus();
			return (int)DEBUG_STATUS.NO_CHANGE;
		}

		int IDebugEventCallbacksWide.ChangeDebuggeeState(DEBUG_CDS Flags, ulong Argument) {
			Debug.WriteLine("IDebugEventCallbacksWide.ChangeDebuggeeState");

			_stateChanged = true;
			//UpdateStatus();

			return (int)DEBUG_STATUS.NO_CHANGE;
		}

		int IDebugEventCallbacksWide.ChangeEngineState(DEBUG_CES Flags, ulong Argument) {
			Debug.WriteLine("IDebugEventCallbacksWide.ChangeEngineState");

			if (Flags.HasFlag(DEBUG_CES.BREAKPOINTS)) {
				// some breakpoints changed
				OnBreakpointChanged(new BreakpointChangedEventArgs((uint)Argument));
			}

			_stateChanged = true;
			UpdateStatus(Flags.HasFlag(DEBUG_CES.CURRENT_THREAD));
			return (int)DEBUG_STATUS.NO_CHANGE;
		}

		int IDebugEventCallbacksWide.ChangeSymbolState(DEBUG_CSS Flags, ulong Argument) {
			//if (Flags == DEBUG_CSS.LOADS) {
			//	var module = GetCurrentTarget().Modules.First(m => m.LoadAddress == Argument);
			//	OnSymbolsLoaded(module);
			//}
			return (int)DEBUG_STATUS.NO_CHANGE;
		}
	}
}
