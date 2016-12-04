using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDbgX {
	static class Constants {
		public const string Title = "WinDbgEx";
		public const string Busy = "*BUSY*";
		public const string NoTarget = "(No target)";

	}

	static class Icons {
		private const string Base = "/icons/";
		public const string Refresh = Base + "refresh.ico";
		public const string SaveAs = Base + "save_as.ico";
		public const string Delete = Base + "delete.ico";
		public const string Breakpoints = Base + "breakpoints.ico";

	}
}
