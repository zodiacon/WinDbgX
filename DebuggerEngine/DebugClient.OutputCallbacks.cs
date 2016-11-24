using DebuggerEngine.Interop;
using System;

namespace DebuggerEngine {
	public sealed class OutputCallbackEventArgs : EventArgs {
		public readonly string Text;
		public readonly DEBUG_OUTPUT Type;

		internal OutputCallbackEventArgs(string text, DEBUG_OUTPUT type) {
			Text = text;
			Type = type;
		}
	}


	partial class DebugClient : IDebugOutputCallbacksWide {
		int IDebugOutputCallbacksWide.Output(DEBUG_OUTPUT Mask, string Text) {
			OnOutputCallback(new OutputCallbackEventArgs(Text, Mask));
			return 0;
		}

		public event EventHandler<OutputCallbackEventArgs> OutputCallback;

		private void OnOutputCallback(OutputCallbackEventArgs e) {
			OutputCallback?.Invoke(this, e);
		}

	}
}