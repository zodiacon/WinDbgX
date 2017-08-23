using DebuggerEngine.Interop;
using System;
using System.Text;
using System.Threading;

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
			if (ConsumeOutput && Mask.HasFlag(DEBUG_OUTPUT.NORMAL)) {
				OutputText.Append(Text);
			}
			else if (ConsumeOutput) {
				using (var ev = new EventWaitHandle(false, EventResetMode.AutoReset, OutputTextEventName)) {
					ev.Set();
				}
				ConsumeOutput = false;
			}
			else {
				OnOutputCallback(new OutputCallbackEventArgs(Text, Mask));
			}
			return 0;
		}

		public event EventHandler<OutputCallbackEventArgs> OutputCallback;

		private void OnOutputCallback(OutputCallbackEventArgs e) {
			OutputCallback?.Invoke(this, e);
		}

		public StringBuilder OutputText { get; } = new StringBuilder(512);

		public bool ConsumeOutput { get; private set; }
	}
}