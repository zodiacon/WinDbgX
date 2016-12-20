using DebuggerEngine.Interop;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DebuggerEngine {
	partial class DebugClient {
		public TargetProcess GetCurrentProcess() {
			return RunAsync(() => {
				uint id;
				SystemObjects.GetCurrentProcessSystemId(out id);
				return _processes.First(p => p.PID == id);
			}).Result;
		}

		public void DeleteAllBreakpoints() {
			RunAsync(() => {
				do {
					IDebugBreakpoint bp;
					if (Control.GetBreakpointByIndex(0, out bp) < 0)
						break;
					Control.RemoveBreakpoint(bp);
				} while (true);
			}).Wait();
		}

		public unsafe DEBUG_STACK_FRAME_EX[] GetCallStack(ulong frameOffset, ulong stackOffset, int maxItems) {
			return RunAsync(() => {
				var frames = new DEBUG_STACK_FRAME_EX[maxItems];
				uint totalFrames;
				Control.GetStackTraceEx(frameOffset, stackOffset, 0, frames, maxItems, &totalFrames).ThrowIfFailed();
				Array.Resize(ref frames, (int)totalFrames);
				return frames;
			}).Result;
		}

		public ulong GetOffsetByLine(int line, string fileName) {
			return RunAsync(() => {
				ulong offset = 0;
				Symbols.GetOffsetByLineWide((uint)line, fileName, out offset);
				return offset;
			}).Result;
		}

        public unsafe Task<DEBUG_SYMBOL_SOURCE_ENTRY> GetClosestSourceEntryByLineAsync(int line, string filename, DEBUG_GSEL flags = DEBUG_GSEL.ALLOW_HIGHER | DEBUG_GSEL.NEAREST_ONLY) {
            return RunAsync(() => {
                var entry = new DEBUG_SYMBOL_SOURCE_ENTRY[1];
                Symbols.GetSourceEntriesByLineWide((uint)line, filename, (uint)flags, entry, 1, null).ThrowIfFailed();
                return entry[0];
            });
        }
	}
}
