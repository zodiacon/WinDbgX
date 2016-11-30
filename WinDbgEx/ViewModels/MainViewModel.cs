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
using WinDbgEx.Commands;
using System.Reflection;

namespace WinDbgEx.ViewModels {
	sealed class MainViewModel : BindableBase {
		MenuViewModel _menu;
		ObservableCollection<TabItemViewModelBase> _tabItems = new ObservableCollection<TabItemViewModelBase>();

		public IList<TabItemViewModelBase> TabItems => _tabItems;

		public string Title => Constants.Title + (Helpers.IsAdmin ? " (Administrator)" : string.Empty);

		public bool IsMain { get; }
		public IWindow Window { get; }

		readonly UIManager UIManager;
		readonly DebugManager DebugManager;

		public AppManager AppManager { get; }

		public MainViewModel(bool main, IWindow window) {
			IsMain = main;
			Window = window;
			AppManager = App.Container.GetExportedValue<AppManager>();
			DebugManager = AppManager.Debug;
			UIManager = AppManager.UI;

			UIManager.Windows.Add(this);

			if (IsMain) {
				var commandView = App.Container.GetExportedValue<CommandViewModel>();
				AddItem(commandView);
			}
		}

		public object Hello {
			get {
				return AppManager;
			}
		}

		public MenuViewModel Menu {
			get {
				if (_menu == null) {
					_menu = Window.FindResource<MenuViewModel>("DefaultMenu");
					if (_menu != null)
						_menu.AddKeyBindings(Window.WindowObject, AppManager);
				}
				return _menu;
			}
		}

		ToolbarItems _toolbar;

		public ToolbarItems Toolbar {
			get {
				if (_toolbar == null) {
					_toolbar = Window.FindResource<ToolbarItems>("DefaultToolbar");
				}
				return _toolbar;
			}
		}

		private TabItemViewModelBase _selectedTab;

		public TabItemViewModelBase SelectedTab {
			get { return _selectedTab; }
			set {
				var old = _selectedTab;
				if (SetProperty(ref _selectedTab, value)) {
					if (old != null)
						old.IsActive = false;
					if (value != null)
						value.IsActive = true;
				}
			}
		}

		public ICommand ActivateCommand => new DelegateCommand<AppManager>(context => context.UI.CurrentWindow = this);

		public ICommand CloseTabCommand => new DelegateCommand<TabItemViewModelBase>(tab => TabItems.Remove(tab));

		public void AddItem(TabItemViewModelBase tab, bool select = true) {
			TabItems.Add(tab);
			if (select)
				SelectedTab = tab;
		}
	}
}
