using DebuggerEngine;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDbgEx.ViewModels {
	class BreakpointViewModel : BindableBase {
		public Breakpoint Breakpoint { get; }

		public BreakpointViewModel(Breakpoint bp) {
			Breakpoint = bp;
		}

		public uint Id => Breakpoint.Id;
		public bool IsEnabled {
			get { return Breakpoint.IsEnabled; }
			set { Breakpoint.Enable(value); }
		}

		public string Type => Breakpoint.Type.ToString();

		public ulong Offset => Breakpoint.Offset;
		public string OffsetExpression => Breakpoint.GetOffsetExpression();
	}
}
