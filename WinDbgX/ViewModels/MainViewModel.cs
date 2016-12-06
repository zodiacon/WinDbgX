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
using WinDbgX.UICore;
using Zodiacon.WPF;
using WinDbgX.Models;
using Prism;
using System.Windows.Input;
using Prism.Commands;
using WinDbgX.Commands;
using System.Reflection;

namespace WinDbgX.ViewModels {
	sealed class MainViewModel : BindableBase {
		MenuViewModel _menu;
		ObservableCollection<TabItemViewModelBase> _tabItems = new ObservableCollection<TabItemViewModelBase>();

		public IList<TabItemViewModelBase> TabItems => _tabItems;

		public string Title => Constants.FullTitle + (Helpers.IsAdmin ? " (Administrator)" : string.Empty);

		public bool IsMain { get; }
		public IWindow Window { get; }

		readonly UIManager UIManager;
		readonly DebugManager DebugManager;

		public AppManager AppManager { get; }

		public MainViewModel(bool main, IWindow window) {
			IsMain = main;
			Window = window;
			AppManager = AppManager.Instance;
			DebugManager = AppManager.Debug;
			UIManager = AppManager.UI;

			UIManager.Windows.Add(this);

			if (IsMain) {
				var commandView = AppManager.Container.GetExportedValue<CommandViewModel>();
				AddItem(commandView);
			}
		}

		public MenuViewModel Menu {
			get {
				if (_menu == null) {
					_menu = UIManager.MainMenu;
					_menu.AddKeyBindings(Window.WindowObject);
				}
				return _menu;
			}
		}


		public ToolbarItems Toolbar => UIManager.MainToolBar;

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

		public ICommand CloseWindowCommand => new DelegateCommand(() => UIManager.Windows.Remove(this));

		public ICommand InitCommand => new DelegateCommand(() => UIManager.UpdateCommands(), () => IsMain);
	}
}
