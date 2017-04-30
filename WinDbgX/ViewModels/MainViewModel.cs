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
using DebuggerEngine.Interop;
using System.Windows;
using MahApps.Metro;

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

			CurrentAccent = Accents.First(accent => accent.Name == "Cobalt");

			DebugManager.Debugger.StatusChanged += Debugger_StatusChanged;
		}

		private void Debugger_StatusChanged(object sender, StatusChangedEventArgs e) {
			var oldStatus = e.OldStatus;
			var newStatus = e.NewStatus;
			UIManager.InvokeAsync(() => {
				if (oldStatus == DEBUG_STATUS.NO_DEBUGGEE || newStatus == DEBUG_STATUS.NO_DEBUGGEE) {
					RaisePropertyChanged(nameof(UserOrKernel));
					RaisePropertyChanged(nameof(LiveOrDump));
					RaisePropertyChanged(nameof(TargetDetail));
				}
			});
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

		private bool _isAlwaysOnTop;

		public bool IsAlwaysOnTop {
			get { return _isAlwaysOnTop; }
			set {
				if (SetProperty(ref _isAlwaysOnTop, value)) {
					Window.Topmost = value;
				}
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

		public string UserOrKernel {
			get {
				var info = DebugManager.Debugger.GetTargetInfo();
				if (info == null)
					return "(Not Connected)";
				return info.UserMode ? "User" : "Kernel";
			}
		}

		public string LiveOrDump {
			get {
				var info = DebugManager.Debugger.GetTargetInfo();
				if (info == null || info.LocalKernel)
					return string.Empty;
				return info.Live ? "Live" : "File";
			}
		}

		public string TargetDetail {
			get {
				var info = DebugManager.Debugger.GetTargetInfo();
				if (info == null)
					return string.Empty;
				if (info.LocalKernel)
					return "Local";

				if (!info.Live)
					return info.DumpType.ToString();
				return string.Empty;
			}
		}

		public AccentViewModel CurrentAccent { get; private set; }

		public ICommand ChangeAccentCommand => new DelegateCommand<AccentViewModel>(accent => {
			if (CurrentAccent != null)
				CurrentAccent.IsCurrent = false;
			accent.ChangeAccentColor(Window.WindowObject as Window);
			(CurrentAccent = accent).IsCurrent = true;
		});

		AccentViewModel[] _accents;
		public AccentViewModel[] Accents => _accents ?? (_accents = ThemeManager.Accents.Select(accent => new AccentViewModel(accent)).ToArray());
	}
}
