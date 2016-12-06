using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WinDbgX.ViewModels;

namespace WinDbgX.Controls {
	sealed class BreakpointMargin : AbstractMargin {
		Brush _background = new SolidColorBrush(Colors.LightGray) { Opacity = .8 };
		List<object> _breakpoints = new List<object>();

		public BreakpointMargin() {
			Loaded += BreakpointMargin_Loaded;
		}

		private void BreakpointMargin_Loaded(object sender, System.Windows.RoutedEventArgs e) {
			Width = 30;
		}

		protected override void OnRender(DrawingContext dc) {
			var size = RenderSize;
			if (size.Width < 1)
				return;

			size.Width -= 2;
			dc.DrawRectangle(_background, null, new Rect(size));
		}
	}
}
