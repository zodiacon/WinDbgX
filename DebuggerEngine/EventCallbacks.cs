using System;
using System.Runtime.InteropServices;
using DebuggerEngine.Interop;

namespace DebuggerEngine {
    class EventCallbacks : IDebugEventCallbacksWide {
		readonly IDebugControl5 _control;

		public EventCallbacks(IDebugControl5 control) {
			_control = control;
		}

		public int GetInterestMask(out DEBUG_EVENT Mask) {
			Mask = DEBUG_EVENT.BREAKPOINT | DEBUG_EVENT.CHANGE_DEBUGGEE_STATE | DEBUG_EVENT.CHANGE_ENGINE_STATE | DEBUG_EVENT.CHANGE_SYMBOL_STATE | DEBUG_EVENT.CREATE_PROCESS
				| DEBUG_EVENT.CREATE_THREAD | DEBUG_EVENT.EXCEPTION | DEBUG_EVENT.EXIT_PROCESS | DEBUG_EVENT.EXIT_THREAD | DEBUG_EVENT.LOAD_MODULE |
				DEBUG_EVENT.SESSION_STATUS | DEBUG_EVENT.SYSTEM_ERROR | DEBUG_EVENT.UNLOAD_MODULE;

			Mask = DEBUG_EVENT.CHANGE_ENGINE_STATE | DEBUG_EVENT.CREATE_PROCESS;

			return 0;
		}

		public int Breakpoint(IDebugBreakpoint2 Bp) {
			BreakpointHit = true;
			StateChanged = true;
			return (int)DEBUG_STATUS.BREAK;
		}

		public int Exception(ref EXCEPTION_RECORD64 Exception, uint FirstChance) {
			BreakpointHit = true;
			return (int)DEBUG_STATUS.BREAK;
		}

		public int CreateThread(ulong Handle, ulong DataOffset, ulong StartOffset) {
			return (int)DEBUG_STATUS.NO_CHANGE;
		}

		public int ExitThread(uint ExitCode) {
			return (int)DEBUG_STATUS.NO_CHANGE;
		}

		public int CreateProcess(ulong ImageFileHandle, ulong Handle, ulong BaseOffset, uint ModuleSize, string ModuleName, string ImageName,
			uint CheckSum, uint TimeDateStamp, ulong InitialThreadHandle, ulong ThreadDataOffset, ulong StartOffset) {

			//IDebugBreakpoint2 bp;
			//_control.AddBreakpoint2(DEBUG_BREAKPOINT_TYPE.CODE, uint.MaxValue, out bp);
			//bp.SetOffset(StartOffset);
			//bp.SetFlags(DEBUG_BREAKPOINT_FLAG.ENABLED);
			//bp.SetCommandWide(".echo Stopping on process attach");

			return (int)DEBUG_STATUS.NO_CHANGE;
		}

		public int ExitProcess(uint ExitCode) {
			return (int)DEBUG_STATUS.NO_CHANGE;
		}

		public int LoadModule(ulong ImageFileHandle, ulong BaseOffset, uint ModuleSize, string ModuleName, string ImageName, uint CheckSum, uint TimeDateStamp) {
			return (int)DEBUG_STATUS.NO_CHANGE;
		}

		public int UnloadModule(string ImageBaseName, ulong BaseOffset) {
			return (int)DEBUG_STATUS.NO_CHANGE;
		}

		public int SystemError(uint Error, uint Level) {
			return (int)DEBUG_STATUS.NO_CHANGE;
		}

		public int SessionStatus(DEBUG_SESSION Status) {
			return (int)DEBUG_STATUS.NO_CHANGE;
		}

		public int ChangeDebuggeeState(DEBUG_CDS Flags, ulong Argument) {
			StateChanged = true;
			return (int)DEBUG_STATUS.NO_CHANGE;
		}

		public int ChangeEngineState(DEBUG_CES Flags, ulong Argument) {
			return (int)DEBUG_STATUS.NO_CHANGE;
		}

		public int ChangeSymbolState(DEBUG_CSS Flags, ulong Argument) {
			return (int)DEBUG_STATUS.NO_CHANGE;
		}

        public int Exception([In] EXCEPTION_RECORD64 Exception, [In] uint FirstChance) {
            return 0;
        }

        public bool StateChanged { get; set; }

		public bool BreakpointHit { get; set; }
	}
}
