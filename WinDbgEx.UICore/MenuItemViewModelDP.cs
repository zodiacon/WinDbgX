using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace WinDbgEx.UICore {
	[ContentProperty("Items")]
	public class MenuItemViewModel : DependencyObject {
		public string Text {
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		public static readonly DependencyProperty TextProperty =
			DependencyProperty.Register("Text", typeof(string), typeof(MenuItemViewModel), new PropertyMetadata(string.Empty));


		public string Icon {
			get { return (string)GetValue(IconProperty); }
			set { SetValue(IconProperty, value); }
		}

		public static readonly DependencyProperty IconProperty =
			DependencyProperty.Register("Icon", typeof(string), typeof(MenuItemViewModel), new PropertyMetadata(null));




		public bool IsChecked {
			get { return (bool)GetValue(IsCheckedProperty); }
			set { SetValue(IsCheckedProperty, value); }
		}

		public static readonly DependencyProperty IsCheckedProperty =
			DependencyProperty.Register("IsChecked", typeof(bool), typeof(MenuItemViewModel), new PropertyMetadata(false));




		public ICommand Command {
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CommandProperty =
			DependencyProperty.Register("Command", typeof(ICommand), typeof(MenuItemViewModel), new PropertyMetadata(null));



		public object CommandParameter {
			get { return (object)GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
		}

		// Using a DependencyProperty as the backing store for CommandParameter.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CommandParameterProperty =
			DependencyProperty.Register("CommandParameter", typeof(object), typeof(MenuItemViewModel), new PropertyMetadata(null));



		public MenuItemCollectionViewModel Items {
			get { return (MenuItemCollectionViewModel)GetValue(ItemsProperty); }
			set { SetValue(ItemsProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Items.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ItemsProperty =
			DependencyProperty.Register("Items", typeof(MenuItemCollectionViewModel), typeof(MenuItemViewModel), new PropertyMetadata(new MenuItemCollectionViewModel()));



		public bool IsSeparator {
			get { return (bool)GetValue(IsSeparatorProperty); }
			set { SetValue(IsSeparatorProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IsSeparator.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsSeparatorProperty =
			DependencyProperty.Register("IsSeparator", typeof(bool), typeof(MenuItemViewModel), new PropertyMetadata(false));



		public string GestureText {
			get { return (string)GetValue(GestureTextProperty); }
			set { SetValue(GestureTextProperty, value); }
		}

		// Using a DependencyProperty as the backing store for GestureText.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty GestureTextProperty =
			DependencyProperty.Register("GestureText", typeof(string), typeof(MenuItemViewModel), new PropertyMetadata(null));


	}
}
