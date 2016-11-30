using DebuggerEngine.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebuggerEngine {
	public class Breakpoint {
		readonly IDebugBreakpoint3 _bp;
		readonly DebugClient _client;
		TargetThread _thread;
		DEBUG_BREAKPOINT_PARAMETERS _parameters;

		internal Breakpoint(DebugClient client, IDebugBreakpoint3 bp) {
			_bp = bp;
			_client = client;
			bp.GetParameters(out _parameters);
		}

		public Task SetOffset(ulong offset) {
			return Task.Run(() => _bp.SetOffset(offset));
		}

		static StringBuilder _text = new StringBuilder(128);

		public unsafe string GetOffsetExpression() {
			return _client.RunAsync(() => {
				uint size;
				_bp.GetOffsetExpressionWide(_text, _text.Capacity, &size);
				return _text.ToString();
			}).Result;
		}

		public ulong Offset => _parameters.Offset;
		public uint Id => _parameters.Id;
		public DEBUG_BREAKPOINT_TYPE Type => _parameters.BreakType;

		public void SetThread(TargetThread thread) {
			_client.RunAsync(() => {
				uint id;
				_client.SystemObjects.GetThreadIdBySystemId(thread.TID, out id).ThrowIfFailed();
				_bp.SetMatchThreadId(id);
				_thread = thread;
				thread.Index = id;
			}).Wait();
		}

		public TargetThread GetThread() {
			return _client.RunAsync(() => {
				uint id;
				if (_bp.GetMatchThreadId(out id) < 0)
					return null;
				return _thread;
			}).Result;
		}

		public void Enable(bool enable) {
			_client.RunAsync(() => {
				if (enable)
					_bp.AddFlags(DEBUG_BREAKPOINT_FLAG.ENABLED);
				else
					_bp.RemoveFlags(DEBUG_BREAKPOINT_FLAG.ENABLED);
				_bp.GetFlags(out _parameters.Flags);
			}).Wait();
		}

		public bool IsEnabled => _parameters.Flags.HasFlag(DEBUG_BREAKPOINT_FLAG.ENABLED);

		public void SetOffsetExpression(string expression) {
			_client.RunAsync(() => {
				_bp.SetOffsetExpressionWide(expression).ThrowIfFailed();
			}).Wait();
		}

		internal void Remove() {
			_client.Control.RemoveBreakpoint(_bp).ThrowIfFailed();
		}

		
	}
}
