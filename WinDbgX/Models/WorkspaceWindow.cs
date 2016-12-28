using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDbgX.Models {
	public class WorkspaceWindow {
		public List<WorkspaceTab> Tabs { get; set; }

		public string AccentName { get; set; }
	}
}
