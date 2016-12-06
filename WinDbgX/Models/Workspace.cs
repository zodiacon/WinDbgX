using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDbgX.Models {
	class Workspace {
		public string Name { get; set; }

		List<WorkspaceWindow> _windows = new List<WorkspaceWindow>();

		public IList<WorkspaceWindow> Windows => _windows;
	}
}
