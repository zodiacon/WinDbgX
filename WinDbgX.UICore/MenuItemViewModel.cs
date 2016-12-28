using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Markup;
using Prism.Mvvm;
using System.Windows;
using System.Globalization;
using System.Collections.Specialized;
using Prism.Commands;

namespace WinDbgX.UICore {
    [ContentProperty("Items")]
	[DictionaryKeyProperty("Key")]
	public class MenuItemViewModel : BindableBase {
		private static readonly ICommand EmptyCommand = new DelegateCommand(() => { }, () => false);

		public static object DefaultCommandParameter;

		private string _text;

		public string Text {
			get { return _text; }
			set { SetProperty(ref _text, value); }
		}

		public string Key { get; set; }

		private string _icon;

		public string Icon {
			get { return _icon; }
			set { SetProperty(ref _icon, value); }
		}

		private bool _isSeparator;

		public bool IsSeparator {
			get { return _isSeparator; }
			set { SetProperty(ref _isSeparator, value); }
		}

		internal MenuItemCollectionViewModel _items;

		public MenuItemCollectionViewModel Items {
			get {
				if (_items == null) {
					_items = new MenuItemCollectionViewModel();

				}
				return _items; 
			}
			set {
				_items = value;
			}
		}

		private bool _isChecked;

		public bool IsChecked {
			get { return _isChecked; }
			set { SetProperty(ref _isChecked, value); }
		}

		private ICommand _command;

		public ICommand Command {
			get {
				return _command ?? (_items == null ? EmptyCommand : null);
			}
			set {
				if (SetProperty(ref _command, value) && value != null && KeyGesture != null) {
					AddInputBinding();
				}
			}
		}

		private object _commandParameter;

		public object CommandParameter {
			get { return _commandParameter ?? DefaultCommandParameter; }
			set { SetProperty(ref _commandParameter, value); }
		}

		private string _shortcutText;

		public string ShortcutText {
			get { return _shortcutText; }
			set { SetProperty(ref _shortcutText, value); }
		}

		private KeyGesture _keyGesture;

		public KeyGesture KeyGesture {
			get { return _keyGesture; }
			set {
				if (SetProperty(ref _keyGesture, value)) {
					if (Command != null)
						AddInputBinding();
				}
			}
		}

		public bool Shared { get; set; } = true;

		public void AddInputBinding() {
			if (GestureText == null)
				GestureText = KeyGesture.GetDisplayStringForCulture(CultureInfo.CurrentUICulture);
		}

		public string Description { get; set; }

        public string GestureText { get; set; }

		public MenuItemViewModel Clone() => (MenuItemViewModel)MemberwiseClone();
	}

    public class MenuItemCollectionViewModel : ObservableCollection<MenuItemViewModel> {
		public MenuItemCollectionViewModel(IEnumerable<MenuItemViewModel> items) : base(items) {
		}

		public MenuItemCollectionViewModel() {
		}

		public bool ReplaceItem(MenuItemViewModel oldItem, MenuItemViewModel newItem) {
			for (int i = 0; i < Items.Count; i++) {
				if (Items[i] == oldItem) {
					RemoveAt(i);
					Insert(i, newItem);
					return true;
				}
				if (Items[i]._items != null)
					if (Items[i]._items.ReplaceItem(oldItem, newItem))
						return true;
			}
			return false;
		}
	}
}
