using DebuggerEngine.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDbgX.ViewModels {
	class StackFrameViewModel {
		public DEBUG_STACK_FRAME_EX StackFrame { get; }

		public StackFrameViewModel(DEBUG_STACK_FRAME_EX stackFrame) {
			StackFrame = stackFrame;
		}
	}
}
