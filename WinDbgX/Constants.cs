using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDbgX {
	static class Constants {
		public const string Title = "WinDbgX";
		public const string Copyright = "Version 0.1 (C)2016 by Pavel Yosifovich";
		public const string FullTitle = Title + " " + Copyright;
		public const string Busy = "*BUSY*";
		public const string NoTarget = "(No target)";

	}

	static class Icons {
		private const string Base = "/icons/";
		public const string Refresh = Base + "refresh.ico";
		public const string SaveAs = Base + "save_as.ico";
		public const string Delete = Base + "delete.ico";
		public const string Breakpoints = Base + "breakpoints.ico";
		public const string EnableBreakpoint = Base + "breakpoint_enable.ico";
		public const string DisableBreakpoint = Base + "breakpoint_disable.ico";
		public const string DeleteBreakpoints = Base + "breakpoints_delete.ico";
		public const string NewBreakpoint = Base + "breakpoint_new.ico";
		public const string GenericSourceFile = Base + "code.ico";
		public const string SourceFileC = Base + "text_code_c.ico";
		public const string SourceFileCpp = Base + "text_code_cplusplus.ico";

		public const string RunExecutable = Base + "debug_run.ico";
		public const string AttachToProcess = Base + "debug_attach.ico";
		public const string OpenDumpFile = Base + "data_out.ico";

		public const string Copy = Base + "copy.ico";
		public const string Find = Base + "find.ico";
	}
}
