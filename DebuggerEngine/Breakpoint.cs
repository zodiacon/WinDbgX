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

		public void SetOffset(ulong offset) {
			Task.Run(() => _bp.SetOffset(offset)).Wait();
		}

		static StringBuilder _text = new StringBuilder(128);

		public unsafe string GetOffsetExpression() {
			return _client.RunAsync(() => {
				uint size;
				_bp.GetOffsetExpressionWide(_text, _text.Capacity, &size);
				return _text.ToString();
			}).Result;
		}

		public bool IsOneShot {
			get {
				return _parameters.Flags.HasFlag(DEBUG_BREAKPOINT_FLAG.ONE_SHOT);
			}
			set {
				_client.RunAsync(() => {
					if (value) {
						_bp.AddFlags(DEBUG_BREAKPOINT_FLAG.ONE_SHOT);
						_parameters.Flags |= DEBUG_BREAKPOINT_FLAG.ONE_SHOT;
					}
					else {
						_bp.RemoveFlags(DEBUG_BREAKPOINT_FLAG.ONE_SHOT);
						_parameters.Flags &= ~DEBUG_BREAKPOINT_FLAG.ONE_SHOT;
					}
				}).Wait();
			}
		}

		public ulong Offset => _parameters.Offset;
		public uint Id => _parameters.Id;
		public DEBUG_BREAKPOINT_TYPE Type => _parameters.BreakType;

		public void SetThread(TargetThread thread) {
			_client.RunAsync(() => {
				uint id = uint.MaxValue;
				if (thread != null) {
					_client.SystemObjects.GetThreadIdBySystemId(thread.TID, out id).ThrowIfFailed();
					thread.Index = id;
				}
				uint oldid = uint.MaxValue;
				_bp.GetMatchThreadId(out oldid);
				if (oldid != id) {
					_bp.SetMatchThreadId(id);
					_thread = thread;
				}
			}).Wait();
		}

		public TargetThread GetThread() {
			return _client.RunAsync(() => {
				uint id;
				if (_bp.GetMatchThreadId(out id) < 0 || id == uint.MaxValue)
					return null;
				return (_thread = _client.Processes.SelectMany(p => p.Threads).First(t => t.Index == id));
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
