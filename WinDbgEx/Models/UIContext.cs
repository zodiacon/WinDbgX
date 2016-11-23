using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDbgEx.UICore;
using WinDbgEx.ViewModels;

namespace WinDbgEx.Models {
	[Export]
	sealed class UIContext {
		ObservableCollection<MainViewModel> _windows = new ObservableCollection<MainViewModel>();
		public MainViewModel Current { get; internal set; }

		public IList<MainViewModel> Windows => _windows;

		public T FindTab<T>(out MainViewModel vm) where T : TabViewModelBase {
			vm = null;
			foreach (var win in Windows) {
				var tab = win.TabItems.OfType<T>().FirstOrDefault();
				if (tab != null) {
					vm = win;
					return tab;
				}
			}
			return null;
		}
	}
}
