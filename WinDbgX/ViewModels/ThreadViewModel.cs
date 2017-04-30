using DebuggerEngine;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDbgX.Models;

namespace WinDbgX.ViewModels {
	class ThreadViewModel : BindableBase {
		DebugManager _debugManager;

		public TargetThread Thread { get; }

		public ThreadViewModel(TargetThread thread, DebugManager debugManager) {
			Thread = thread;
			_debugManager = debugManager;

			InitAsync();
		}

		private async void InitAsync() {
			var symbol = await _debugManager.Debugger.GetSymbolByOffsetAsync(Thread.StartAddress);
			if (symbol != null) {
				StartAddressSymbol = symbol;
				RaisePropertyChanged(nameof(StartAddressSymbol));
			}
		}

		public int Index => (int)Thread.Index;
		public int OSID => (int)Thread.TID;
		public ulong Teb => Thread.Teb;
		public ulong StartAddress => Thread.StartAddress;

		public string StartAddressSymbol { get; private set; }

		private bool _isLastEvent;

		public bool IsLastEvent {
			get { return _isLastEvent; }
			set { SetProperty(ref _isLastEvent, value); }
		}

		private bool _isCurrent;

		public bool IsCurrent {
			get { return _isCurrent; }
			set { SetProperty(ref _isCurrent, value); }
		}

		public ThreadPriorityLevel Priority => Thread.GetPriority();

		public void Refresh() {
			RaisePropertyChanged(nameof(Priority));
		}

		public void SetPriority(ThreadPriorityLevel priority) {
			Thread.SetPriority(priority);
			RaisePropertyChanged(nameof(Priority));
		}
	}
}
