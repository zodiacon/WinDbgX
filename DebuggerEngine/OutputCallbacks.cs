using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Runtime.Interop;

namespace DebuggerEngine {
	class OutputCallbacks : IDebugOutputCallbacks2 /*, IDebugOutputCallbacks2 */ {
	public int Output(DEBUG_OUTPUT Mask, string Text) {
		switch(Mask) {
			case DEBUG_OUTPUT.DEBUGGEE:
				Console.ForegroundColor = ConsoleColor.Gray;
				break;

			case DEBUG_OUTPUT.PROMPT:
				Console.ForegroundColor = ConsoleColor.Magenta;
				break;

			case DEBUG_OUTPUT.ERROR:
				Console.ForegroundColor = ConsoleColor.Red;
				break;

			case DEBUG_OUTPUT.EXTENSION_WARNING:
			case DEBUG_OUTPUT.WARNING:
				Console.ForegroundColor = ConsoleColor.Yellow;
				break;

			case DEBUG_OUTPUT.SYMBOLS:
				Console.ForegroundColor = ConsoleColor.Cyan;
				break;

			default:
				Console.ForegroundColor = ConsoleColor.White;
				break;
		}

		Console.Write(Text);
		return 0;
	}


	public int GetInterestMask(out DEBUG_OUTCBI Mask) {
		Mask = DEBUG_OUTCBI.TEXT;
		return 0;
	}

	public int Output2(DEBUG_OUTCB Which, DEBUG_OUTCBF Flags, ulong Arg, string Text) {
		if(Which != DEBUG_OUTCB.DML)
			return Output(DEBUG_OUTPUT.NORMAL, Text);

		Console.Write("DML: {0}", Text);
		return 0;
	}
}}
