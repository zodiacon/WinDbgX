using System;

namespace WinDbgEx.UICore {
    [Serializable]
	public sealed class RgbColor {
		public byte R { get; set; }
		public byte G { get; set; }
		public byte B { get; set; }
	}
}
