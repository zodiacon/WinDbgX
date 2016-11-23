using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using DebuggerEngine;
using Prism.Mvvm;
using WinDbgEx.UICore;
using Zodiacon.WPF;
using WinDbgEx.Models;
using Prism;
using System.Windows.Input;
using Prism.Commands;

namespace WinDbgEx.ViewModels {
	[Export]
	class MainViewModel : BindableBase {
		MenuViewModel _menu;
		ObservableCollection<TabViewModelBase> _tabItems = new ObservableCollection<TabViewModelBase>();

		public IList<TabViewModelBase> TabItems => _tabItems;

		public string Title => Constants.Title + (Helpers.IsAdmin ? " (Administrator)" : string.Empty);

		public bool IsMain { get; }
		public IWindow Window { get; }

		public MainViewModel(bool main, IWindow window) {
			IsMain = main;
			Window = window;

			if (IsMain) {
				_tabItems.Add(new CommandViewModel());
				_tabItems.Add(new ModulesViewModel());
			}
		}

		public MenuViewModel Menu {
			get {
				if (_menu == null) {
					_menu = Window.FindResource<MenuViewModel>("DefaultMenu");
				}
				return _menu;
			}
		}

		public ICommand ActivateCommand => new DelegateCommand(() => DebugContext.Instance.UI.Current = this);
	}
}
