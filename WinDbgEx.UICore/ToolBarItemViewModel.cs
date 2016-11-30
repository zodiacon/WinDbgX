using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Prism.Mvvm;
using System.Windows.Markup;
using System.Collections.ObjectModel;

namespace WinDbgEx.UICore {
	public abstract class ToolBarItemViewModel : BindableBase {
		private string _text;

		public string Text {
			get { return _text; }
			set { SetProperty(ref _text, value); }
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

		private Thickness _margin = new Thickness(2, 0, 0, 0);

		public Thickness Margin {
			get { return _margin; }
			set { _margin = value; }
		}

	}

	public sealed class ToolbarItems : ObservableCollection<ToolBarItemViewModel> {
	}

	public class ToolBarButtonViewModel : ToolBarItemViewModel {
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

		private object _commandParameter;

		public object CommandParameter {
			get { return _commandParameter; }
			set { SetProperty(ref _commandParameter, value); }
		}

	}

	[ContentProperty("Items")]
	public class ToolBarComboBoxViewModel : ToolBarItemViewModel {
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

}
