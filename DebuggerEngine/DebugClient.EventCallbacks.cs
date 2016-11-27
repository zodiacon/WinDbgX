using DebuggerEngine.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DebuggerEngine {
	partial class DebugClient : IDebugEventCallbacksWide {
		bool _stateChanged;
		bool _breakpointHit;

		public event EventHandler<ProcessCreatedEventArgs> ProcessCreated;
		public event EventHandler<ThreadCreatedEventArgs> ThreadCreated;

		void OnProcessCreated(TargetProcess process) {
			ProcessCreated?.Invoke(this, new ProcessCreatedEventArgs(process));
		}

		void OnThreadCreated(TargetThread thread) {
			ThreadCreated?.Invoke(this, new ThreadCreatedEventArgs(thread));
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

			if (_breakpointHit)
				Control.OutputCurrentState(DEBUG_OUTCTL.THIS_CLIENT, DEBUG_CURRENT.DEFAULT);

			//UpdateStatus();
			return _breakpointHit ? (int)DEBUG_STATUS.BREAK : (int)DEBUG_STATUS.NO_CHANGE;
		}

		int IDebugEventCallbacksWide.CreateThread(ulong Handle, ulong DataOffset, ulong StartOffset) {
			//uint id, tid;
			//SystemObjects.GetCurrentThreadId(out id);
			//SystemObjects.GetCurrentThreadSystemId(out tid);
			//var thread = new TargetThread {
			//	Index = id,
			//	Tid = tid,
			//	StartAddress = StartOffset,
			//	Teb = DataOffset
			//};
			//GetCurrentTarget().Threads.Add(thread);

			return (int)DEBUG_STATUS.NO_CHANGE;
		}

		int IDebugEventCallbacksWide.ExitThread(uint ExitCode) {
			//uint id;
			//SystemObjects.GetCurrentThreadId(out id);
			//var threads = GetCurrentTarget().Threads;
			//threads.Remove(threads.First(t => t.Index == id));

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
				PID = (int)pid,
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

			OnProcessCreated(process);

			//var target = DebuggerTarget.LiveUser(this, (int)WindowsAPI.GetProcessId(new IntPtr((long)Handle)), ImageName);
			//_targets.Add(target);
			//OnTargetAdded(new TargetAddedEventArgs(target));

			//uint id, tid;
			//SystemObjects.GetCurrentThreadId(out id);
			//SystemObjects.GetCurrentThreadSystemId(out tid);

			//var thread = new TargetThread {
			//	Tid = tid,
			//	Index = id,
			//	StartAddress = StartOffset,
			//	Teb = ThreadDataOffset
			//};
			//target.Threads.Add(thread);

			return (int)DEBUG_STATUS.NO_CHANGE;
		}

		int IDebugEventCallbacksWide.ExitProcess(uint ExitCode) {
			Debug.WriteLine("IDebugEventCallbacksWide.ExitProcess");

			//_targets.Remove(GetCurrentTarget());

			UpdateStatus();
			return (int)DEBUG_STATUS.NO_CHANGE;
		}

		int IDebugEventCallbacksWide.LoadModule(ulong ImageFileHandle, ulong BaseOffset, uint ModuleSize, string ModuleName, string ImageName, uint CheckSum, uint TimeDateStamp) {

			//GetCurrentTarget().Modules.Add(new TargetModule {
			//	Path = ImageName,
			//	Name = ModuleName,
			//	LoadAddress = BaseOffset
			//});

			return (int)DEBUG_STATUS.NO_CHANGE;
		}

		int IDebugEventCallbacksWide.UnloadModule(string ImageBaseName, ulong BaseOffset) {
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
