using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDbgX.UICore;

namespace WinDbgX.Models {
	[Export]
	class AppManager {
		[Import]
		public DebugManager Debug { get; private set; }

		[Import]
		public UIManager UI { get; private set; }

		public static AppManager Instance { get; private set; }

		[Import]
		public CompositionContainer Container { get; private set; }

		internal AppManager() {
			Instance = this;
		}
	}
}
