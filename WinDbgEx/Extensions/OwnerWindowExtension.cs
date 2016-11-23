using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Xaml;

namespace WinDbgEx.Extensions {
	class OwnerWindowExtension : MarkupExtension {
		public override object ProvideValue(IServiceProvider sp) {
			var target = sp.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
			if (target != null) {
				var win = Window.GetWindow(target.TargetObject as DependencyObject);
				return win;
			}
			throw new InvalidOperationException();
		}
	}
}
