using DebuggerEngine;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDbgX.Models;

namespace WinDbgX.ViewModels {
	class BreakpointViewModel : BindableBase {
		sealed class DummyThread : TargetThread {
			public static readonly TargetThread Instance = new DummyThread();

			private DummyThread() : base(null) { }
		}

		static TargetThread _dymmyThread = DummyThread.Instance;

		DebugManager _debugManager;

		public Breakpoint Breakpoint { get; }

		public BreakpointViewModel(Breakpoint bp, DebugManager debugManager) {
			Breakpoint = bp;
			_debugManager = debugManager;
			var thread = bp.GetThread();
			SelectedThread = thread ?? DummyThread.Instance;
		}

		public uint Id => Breakpoint.Id;

		public bool IsEnabled {
			get { return Breakpoint.IsEnabled; }
			set {
				Breakpoint.IsEnabled = value;
				OnPropertyChanged(nameof(IsEnabled));
			}
		}

		public string Type => Breakpoint.Type.ToString();

		public ulong Offset {
			get { return Breakpoint.Offset; }
			set {
				Breakpoint.SetOffset(value);
				OnPropertyChanged(nameof(Offset));
				OnPropertyChanged(nameof(OffsetExpression));
			}
		}

		public string OffsetExpression {
			get { return Breakpoint.GetOffsetExpression(); }
			set {
				Breakpoint.SetOffsetExpression(value);
				OnPropertyChanged(nameof(OffsetExpression));
				OnPropertyChanged(nameof(Offset));
			}
		}

		public bool OneShot {
			get { return Breakpoint.IsOneShot; }
			set { Breakpoint.IsOneShot = value; }
		}

		public void Refresh() {
			OnPropertyChanged(nameof(SelectedThread));
			OnPropertyChanged(nameof(OneShot));
			OnPropertyChanged(nameof(IsEnabled));
			OnPropertyChanged(nameof(Offset));
			OnPropertyChanged(nameof(OffsetExpression));
		}

		private TargetThread _selectedThread;

		public TargetThread SelectedThread {
			get { return _selectedThread; }
			set {
				if (SetProperty(ref _selectedThread, value)) {
					Breakpoint.SetThread(value == DummyThread.Instance ? null : value);
				}
			}
		}

		public uint HitCount => Breakpoint.HitCount;

		public uint HitTarget {
			get { return Breakpoint.HitTarget; }
			set {
				Breakpoint.HitTarget = value;
				OnPropertyChanged(nameof(HitTarget));
				OnPropertyChanged(nameof(HitTarget));
			}
		}

		public IEnumerable<TargetThread> Threads => _debugManager.GetAllThreads().Concat(Enumerable.Range(0, 1).
			Select(i => DummyThread.Instance)).OrderBy(t => t.TID).ToArray();
	}
}
