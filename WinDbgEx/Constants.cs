using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDbgEx {
	static class Constants {
		public const string Title = "WinDbgEx";
		public const string Busy = "*BUSY*";
		public const string NoTarget = "(No target)";

	}

	static class Icons {
		private const string Prefix = "/icons/";
		public const string Refresh = Prefix + "refresh.ico";
		public const string SaveAs = Prefix + "save_as.ico";
		public const string Delete = Prefix + "delete.ico";

	}
}
