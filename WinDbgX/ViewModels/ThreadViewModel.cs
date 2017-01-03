using DebuggerEngine;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
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
				OnPropertyChanged(() => StartAddressSymbol);
			}
		}

		public int Index => (int)Thread.Index;
		public int OSID => (int)Thread.TID;
		public ulong Teb => Thread.Teb;
		public ulong StartAddress => Thread.StartAddress;

		public string StartAddressSymbol { get; private set; }
	}
}
