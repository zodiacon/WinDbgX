using DebuggerEngine;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDbgEx.ViewModels {
	class ThreadViewModel : BindableBase {
		public TargetThread Thread { get; }

		public ThreadViewModel(TargetThread thread) {
			Thread = thread;
		}

		public int Index => (int)Thread.Index;
		public int OSID => (int)Thread.TID;
		public ulong Teb => Thread.Teb;
		public ulong StartAddress => Thread.StartAddress;
	}
}
