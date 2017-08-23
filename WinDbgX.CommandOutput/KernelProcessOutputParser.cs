using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDbgX.CommandOutput {
    [CommandOutputParser("!process"), Export]
    public sealed class KernelProcessOutputParser : CommandoutputParser {
        static Dictionary<string, (string, ParseType)> parseMap = new Dictionary<string, (string, ParseType)>(16, StringComparer.InvariantCultureIgnoreCase) {
            { "process", ( "EProcess", ParseType.UlongAsHex ) },
            { "sessionid:", ("SessionId", ParseType.UlongAsHex) },
            { "cid:", ("ProcessId", ParseType.IntAsHex) },
            { "parentcid:", ("ParentProcessId", ParseType.IntAsHex) },
            { "dirbase:", ( "DirBase", ParseType.UlongAsHex) },
            { "peb:", ("PEB", ParseType.UlongAsHex) },
			{ "deepfreeze", ("DeepFreeze", ParseType.BooleanNoParse) },
			{ "image:", ("Image", ParseType.String) },

        };

        public override IEnumerable<IDictionary<string, object>> ParseCommandOutput(string command, string output) {
            Debug.Assert(command.StartsWith("!process"));

			output = output.Substring(output.IndexOf('\n'));

            var words = output.Split(new char[] { ' ', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            return ParseWithMap(words, parseMap);
        }
    }
}
