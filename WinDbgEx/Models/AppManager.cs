using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDbgEx.Models {
	[Export]
	class AppManager {
		[Import]
		public DebugManager Debug { get; private set; }

		[Import]
		public UIManager UI { get; private set; }
	}
}
