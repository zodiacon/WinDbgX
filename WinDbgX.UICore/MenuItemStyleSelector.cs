using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WinDbgX.UICore {
	public sealed class MenuItemStyleSelector : StyleSelector {
		public object MenuItemStyleKey { get; set; }
		public object SeparatorStyleKey { get; set; }
		public override Style SelectStyle(object item, DependencyObject container) {
			var menuItem = (MenuItemViewModel)item;
			var key = menuItem.IsSeparator ? SeparatorStyleKey : MenuItemStyleKey;
			return ((FrameworkElement)container).FindResource(key) as Style;
		}
	}
}
