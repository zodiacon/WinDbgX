using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace WinDbgEx.Behaviors {
	sealed class FocusOnKeyDownBehavior : Behavior<Control> {
		public Key Key {
			get { return (Key)GetValue(KeyProperty); }
			set { SetValue(KeyProperty, value); }
		}

		public static readonly DependencyProperty KeyProperty =
			DependencyProperty.Register(nameof(Key), typeof(Key), typeof(FocusOnKeyDownBehavior), new PropertyMetadata(Key.None));


		public bool IsPreview {
			get { return (bool)GetValue(IsPreviewProperty); }
			set { SetValue(IsPreviewProperty, value); }
		}

		public static readonly DependencyProperty IsPreviewProperty =
			DependencyProperty.Register(nameof(IsPreview), typeof(bool), typeof(FocusOnKeyDownBehavior), new PropertyMetadata(false));

		protected override void OnAttached() {
			base.OnAttached();

			AssociatedObject.Loaded += AssociatedObject_Loaded;
		}

		private void AssociatedObject_Loaded(object sender, RoutedEventArgs e) {
			var window = Window.GetWindow(AssociatedObject);

			if (IsPreview)
				window.PreviewKeyDown += OnKeyDown;
			else
				window.KeyDown += OnKeyDown;
		}

		protected override void OnDetaching() {
			var window = Window.GetWindow(AssociatedObject);

			if (IsPreview)
				window.PreviewKeyDown -= OnKeyDown;
			else
				window.KeyDown -= OnKeyDown;

			base.OnDetaching();
		}

		private void OnKeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key) {
				AssociatedObject.Focus();
				e.Handled = true;
			}
		}
	}
}
