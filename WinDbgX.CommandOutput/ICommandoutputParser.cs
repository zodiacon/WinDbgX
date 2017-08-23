using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDbgX.CommandOutput {
	public sealed class CommandOutputParserAttribute : Attribute {
		public string Command { get; }
		public string Arguments { get; set; }

		public CommandOutputParserAttribute(string command) {
			Command = command;
		}
	}

	public enum ParseType {
		UlongAsHex,
		Ulong,
		IntAsHex,
		Int,
		String,
		BooleanNoParse
	}

	public sealed class ParseMap : List<Tuple<string, ParseType>> {
		public void Add(string propertyName, ParseType parseType) {
			base.Add(Tuple.Create(propertyName, parseType));
		}
	}

	public abstract class CommandoutputParser {
		public abstract IEnumerable<IDictionary<string, object>> ParseCommandOutput(string conmmand, string output);

		protected CommandoutputParser() {

		}

		protected ulong? ParseHexAsUlong(string value) {
			return ulong.TryParse(value.Trim(trimChars), NumberStyles.HexNumber | NumberStyles.AllowHexSpecifier, null, out var result) ? result : (ulong?)null;
		}

		protected int? ParseHexAsInt(string value) {
			return int.TryParse(value.Trim(trimChars), NumberStyles.HexNumber | NumberStyles.AllowHexSpecifier, null, out var result) ? result : (int?)null;
		}

		protected int? ParseInt(string value) {
			return int.TryParse(value.Trim(trimChars), out var result) ? result : (int?)null;
		}

		static char[] trimChars = new char[] { ' ', '\r', '\n', '\b', '\t' };

		protected IEnumerable<IDictionary<string, object>> ParseWithMap(string[] words, IDictionary<string, (string name, ParseType type)> parseMap) {
			IDictionary<string, object> properties = new ExpandoObject();
			for (int i = 0; i < words.Length; i += 2) {
				var word = words[i].Trim(trimChars);
				if (parseMap.TryGetValue(word, out var item)) {
					var propertyName = item.name;
					if (properties.ContainsKey(propertyName)) {
						// next pass
						yield return properties;
						properties = new ExpandoObject();
					}
					switch (item.type) {
						case ParseType.BooleanNoParse:
							properties.Add(propertyName, true);
							break;

						case ParseType.Int:
							properties.Add(propertyName, ParseInt(words[i + 1]));
							break;

						case ParseType.IntAsHex:
							properties.Add(propertyName, ParseHexAsInt(words[i + 1]));
							break;

						case ParseType.String:
							properties.Add(propertyName, words[i + 1].Trim(trimChars));
							break;

						case ParseType.UlongAsHex:
							properties.Add(propertyName, ParseHexAsUlong(words[i + 1]));
							break;

						default:
							Debug.Assert(false, "Missing parse map type handling");
							break;
					}
				}
			}
			if (properties.Count > 0)
				yield return properties;
		}
	}
}
