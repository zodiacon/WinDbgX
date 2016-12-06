using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDbgX.Models {
	public sealed class Executable : IEquatable<Executable> {
		public string Path { get; set; }
		public string Arguments { get; set; }

		public bool Equals(Executable other) {
			return Path == other.Path && Arguments == other.Arguments;
		}
	}
}
