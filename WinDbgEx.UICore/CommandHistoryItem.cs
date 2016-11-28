using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDbgEx.UICore {
	public class CommandHistoryItem {
		public string Text { get; set; }
		public string CommandText { get; set; }
		public RgbColor Color { get; set; }
		public bool Bold { get; set; }
	}
}
