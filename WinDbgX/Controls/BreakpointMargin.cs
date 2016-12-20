using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
		ObservableCollection<BreakpointViewModel> _breakpoints = new ObservableCollection<BreakpointViewModel>();

		public IList<BreakpointViewModel> Breakpoints => _breakpoints;

		public BreakpointMargin() {
			Loaded += BreakpointMargin_Loaded;
			_breakpoints.CollectionChanged += delegate { InvalidateVisual(); };
		}

		private void BreakpointMargin_Loaded(object sender, System.Windows.RoutedEventArgs e) {
			Width = 30;
            TextView.ScrollOffsetChanged += delegate { InvalidateVisual(); };
        }


        protected override void OnRender(DrawingContext dc) {
			var size = RenderSize;
			if (size.Width < 1)
				return;

			size.Width -= 2;
			dc.DrawRectangle(_background, null, new Rect(size));

			foreach (var bp in _breakpoints) {
				var y = (bp.Line - 1) * TextView.DefaultLineHeight - TextView.VerticalOffset;
                if(y >= -20 && y < RenderSize.Height + 20)
				    dc.DrawEllipse(Brushes.Red, new Pen(Brushes.Black, 1), new Point(8, y + 8), 8, 8);
			}
		}

	}
}
