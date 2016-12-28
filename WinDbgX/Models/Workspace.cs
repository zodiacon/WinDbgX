using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDbgX.Models {
	public class Workspace {
		public string Name { get; set; }

		public List<WorkspaceWindow> Windows { get; set; }

	}
}
