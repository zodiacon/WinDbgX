using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace WinDbgEx.UICore {
	public class MenuViewModel : MenuItemCollectionViewModel {
		Dictionary<string, MenuItemViewModel> _keyedItems = new Dictionary<string, MenuItemViewModel>(StringComparer.InvariantCultureIgnoreCase);

		public MenuViewModel() {
		}

		protected override void InsertItem(int index, MenuItemViewModel item) {
			base.InsertItem(index, item);

			if (item.Key != null)
				AddItem(item);

			AddSubItems(item);
		}

		void AddSubItems(MenuItemViewModel item) {
			if (item._items != null) {
				foreach (var i in item.Items) {
					if (i.Key != null)
						AddItem(i);
					AddSubItems(i);
				}
			}
		}

		public MenuItemViewModel this[string key] => _keyedItems[key];

		public void AddItem(MenuItemViewModel item) {
			_keyedItems.Add(item.Key, item);
		}

		public void AddKeyBindings(DependencyObject dp, object parameter) {
			var win = Window.GetWindow(dp);
			AddKeyBindings(win, this, parameter);
		}

		void AddKeyBindings(Window win, MenuItemCollectionViewModel items, object parameter) {
			foreach (var item in items) {
				if (item.KeyGesture != null && item.Command != null)
					win.InputBindings.Add(new KeyBinding(item.Command, item.KeyGesture) { CommandParameter = parameter });
				if (item._items != null)
					AddKeyBindings(win, item._items, parameter);
			}
		}
	}
}
