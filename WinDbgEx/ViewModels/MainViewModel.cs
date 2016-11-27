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
	[Export]
	class MainViewModel : BindableBase {
		MenuViewModel _menu;
		ObservableCollection<TabViewModelBase> _tabItems = new ObservableCollection<TabViewModelBase>();

		public IList<TabViewModelBase> TabItems => _tabItems;

		public string Title => Constants.Title + (Helpers.IsAdmin ? " (Administrator)" : string.Empty);

		public bool IsMain { get; }
		public IWindow Window { get; }

		static readonly List<DelegateCommandBase> _commands = new List<DelegateCommandBase>(32);

		static MainViewModel() {
			Type[] types = {
				typeof(FileCommands), typeof(ViewCommands), typeof(OptionsCommands), typeof(DebugCommands)
			};
			foreach(var type in types)
				_commands.AddRange(type.GetProperties(BindingFlags.Public | BindingFlags.Static)
					.Where(pi => pi.PropertyType == typeof(DelegateCommandBase))
					.Select(pi => pi.GetValue(null) as DelegateCommandBase));
		}

		public MainViewModel(bool main, IWindow window) {
			IsMain = main;
			Window = window;

			if (IsMain) {
				_tabItems.Add(new CommandViewModel());
				_tabItems.Add(new ModulesViewModel());
				SelectedTab = _tabItems[0];

				DebugContext.Instance.Debugger.StatusChanged += Debugger_StatusChanged;
			}

			DebugContext.Instance.UI.Windows.Add(this);
		}

		private static void Debugger_StatusChanged(object sender, StatusChangedEventArgs e) {
			if (e.NewStatus == e.OldStatus)
				return;

			foreach (var cmd in _commands)
				cmd.RaiseCanExecuteChanged();
		}

		public MenuViewModel Menu {
			get {
				if (_menu == null) {
					_menu = Window.FindResource<MenuViewModel>("DefaultMenu");
					if (_menu != null)
						_menu.AddKeyBindings(Window.WindowObject, DebugContext.Instance);
				}
				return _menu;
			}
		}

		private TabViewModelBase _selectedTab;

		public TabViewModelBase SelectedTab {
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

		public ICommand ActivateCommand => new DelegateCommand(() => DebugContext.Instance.UI.Current = this);

		public ICommand CloseTabCommand => new DelegateCommand<TabViewModelBase>(tab => TabItems.Remove(tab));

		public void AddItem(TabViewModelBase tab, bool select = true) {
			TabItems.Add(tab);
			if (select)
				SelectedTab = tab;
		}
	}
}
