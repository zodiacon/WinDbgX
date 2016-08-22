using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Prism.Mvvm;

namespace WinDbgEx.UICore {
	class ToolBarItemViewModel : BindableBase {
		private string _text;

		public string Text {
			get { return _text; }
			set { SetProperty(ref _text, value); }
		}

		private bool _isSeparator;

		public bool IsSeparator {
			get { return _isSeparator; }
			set { SetProperty(ref _isSeparator, value); }
		}

		private string _toolTip;

		public string ToolTip {
			get { return _toolTip; }
			set { SetProperty(ref _toolTip, value); }
		}

		private double _leftMargin = 2;

		public double LeftMargin {
			get { return _leftMargin; }
			set {
				SetProperty(ref _leftMargin, value);
				Margin = new Thickness(_leftMargin, 0, 0, 0);
			}
		}

		private Thickness _margin;

		public Thickness Margin {
			get { return _margin; }
			set { _margin = value; }
		}

	}

	class ToolBarButtonViewModel : ToolBarItemViewModel {
		private string _icon;

		public string Icon {
			get { return _icon; }
			set { SetProperty(ref _icon, value); }
		}

		private ICommand _command;

		public ICommand Command {
			get { return _command; }
			set { SetProperty(ref _command, value); }
		}

		protected object _commandParameter;

		public object CommandParameter {
			get { return _commandParameter; }
			set { SetProperty(ref _commandParameter, value); }
		}

	}

	class ToolBarButtonViewModel<T> : ToolBarButtonViewModel {
		public new Func<T> CommandParameter {
			get { return _commandParameter as Func<T>; }
			set { SetProperty(ref _commandParameter, value); }
		}

	}

	class ToolBarComboBoxViewModel : ToolBarItemViewModel {
		private IEnumerable _items;

		public IEnumerable Items {
			get { return _items; }
			set { SetProperty(ref _items, value); }
		}

		private object _selectedItem;

		public object SelectedItem {
			get { return _selectedItem; }
			set { SetProperty(ref _selectedItem, value); }
		}

		private ICommand _command;

		public ICommand Command {
			get { return _command; }
			set { SetProperty(ref _command, value); }
		}

		private Func<object> _commandParameter;

		public Func<object> CommandParameter {
			get { return _commandParameter; }
			set { SetProperty(ref _commandParameter, value); }
		}

		private bool _isEnabled;

		public bool IsEnabled {
			get { return _isEnabled; }
			set { SetProperty(ref _isEnabled, value); }
		}

		public DataTemplateSelector ItemTemplateSelector { get; set; }
	}

	class ToolBarComboBoxViewModel<T> : ToolBarComboBoxViewModel where T : class {
		public new T SelectedItem {
			get {
				return base.SelectedItem as T;
			}
			set {
				base.SelectedItem = value;
			}
		}

		public new Func<T> CommandParameter {
			get {
				return base.CommandParameter as Func<T>;
			}
			set {
				base.CommandParameter = value;
			}
		}
	}
}
