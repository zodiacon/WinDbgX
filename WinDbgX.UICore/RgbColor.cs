using System;

namespace WinDbgX.UICore {
    [Serializable]
	public sealed class RgbColor {
		public byte R { get; set; }
		public byte G { get; set; }
		public byte B { get; set; }
	}
}
