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

	}
}
