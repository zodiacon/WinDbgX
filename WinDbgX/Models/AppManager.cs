using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDbgX.Models {
	[Export]
	class AppManager {
		[Import]
		public DebugManager Debug { get; private set; }

		[Import]
		public UIManager UI { get; private set; }

		public static AppManager Instance { get; private set; }

		internal AppManager() {
			Instance = this;
		}
	}
}
