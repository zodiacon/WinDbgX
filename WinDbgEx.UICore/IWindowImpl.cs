using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WinDbgEx.UICore {
	public class IWindowImpl : IWindow {
		readonly Window _window;
		public IWindowImpl(Window window) {
			if (window == null)
				throw new NullReferenceException(nameof(window));

			_window = window;
		}

		public bool Topmost {
			get { return _window.Topmost; }
			set { _window.Topmost = value; }
		}

		public DependencyObject WindowObject => _window;

		public bool Activate() {
			return _window.Activate();
		}

		public T FindResource<T>(object name) where T : class {
			return _window.TryFindResource(name) as T;
		}
	}
}

