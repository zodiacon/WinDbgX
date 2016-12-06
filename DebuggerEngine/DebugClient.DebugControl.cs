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

		public Task<ulong> GetOffsetByLineAsync(int line, string fileName) {
			return RunAsync(() => {
				ulong offset = 0;
				Symbols.GetOffsetByLineWide((uint)line - 1, fileName, out offset);
				return offset;
			});
		}
	}
}
