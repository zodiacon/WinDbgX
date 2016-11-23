using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
			if (item._items != null)
				foreach (var i in item.Items) {
					if (i.Key != null)
						AddItem(i);
					AddSubItems(i);
				}
		}

		public MenuItemViewModel this[string key] => _keyedItems[key];

		public void AddItem(MenuItemViewModel item) {
			_keyedItems.Add(item.Key, item);
		}
	}
}
