using DebuggerEngine;
using DebuggerEngine.Interop;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using WinDbgX.UICore;
using WinDbgX.ViewModels;
using Zodiacon.WPF;
using System.Windows;
using Prism.Mvvm;
using WinDbgX.Extensions;

#pragma warning disable 649

namespace WinDbgX.Models {
	[Export]
	sealed class UIManager : BindableBase, IPartImportsSatisfiedNotification {
		readonly ObservableCollection<MainViewModel> _windows = new ObservableCollection<MainViewModel>();
		readonly ObservableCollection<string> _recentWorkspaces = new ObservableCollection<string>();
		ObservableCollection<Executable> _recentExecutables;
		readonly MenuItemCollectionViewModel _recentExecutablesMenuItems = new MenuItemCollectionViewModel();

		readonly IDictionary<string, ICommand> _commands = new Dictionary<string, ICommand>(32, StringComparer.InvariantCultureIgnoreCase);

		public IList<string> RecentWorkspaces => _recentWorkspaces;
		public IEnumerable<Executable> RecentExecutables => _recentExecutables ?? (_recentExecutables = new ObservableCollection<Executable>());

		public MenuItemCollectionViewModel RecentExecutablesMenuItems {
			get {
				SetRecentMenuItems();
				Debug.Assert(_recentExecutablesMenuItems != null);
				return _recentExecutablesMenuItems;
			}
		}

		MenuViewModel _menu;
		public MenuViewModel MainMenu {
			get {
				if (_menu == null) {
					_menu = Application.Current.FindResource("DefaultMenu") as MenuViewModel;
					_menu[nameof(RecentExecutables)].Items = _recentExecutablesMenuItems;
					SetRecentMenuItems();
				}
				return _menu;
			}
		}

		ToolbarItems _mainToolBar;
		public ToolbarItems MainToolBar {
			get {
				if (_mainToolBar == null)
					_mainToolBar = Application.Current.FindResource("DefaultToolbar") as ToolbarItems;
				return _mainToolBar;
			}
		}

		[Import]
		DebugManager DebugManager;

		[Import]
		AppManager AppManager;

		public Dispatcher Dispatcher { get; } = Dispatcher.CurrentDispatcher;

		private UIManager() {
		}

		private void Debugger_Error(object sender, ErrorEventArgs e) {
			Dispatcher.InvokeAsync(() => {
				MessageBoxService.SetOwner(Application.Current.MainWindow);
				MessageBoxService.ShowMessage($"Error: {ErrorToString(e)}", Constants.Title, MessageBoxButton.OK, MessageBoxImage.Error);
			});
		}

		public void AddExecutable(Executable executable) {
			var index = _recentExecutables.IndexOf(executable);
			if (index >= 0) {
				_recentExecutables.RemoveAt(index);
				_recentExecutablesMenuItems.RemoveAt(index);

			}

			_recentExecutables.Insert(0, executable);
			var args = executable.Arguments ?? string.Empty;
			args = args.Substring(0, Math.Min(50, args.Length));

			_recentExecutablesMenuItems.Insert(0, new MenuItemViewModel {
				Text = $"{executable.Path} {args}",
				Command = RunRecentExecutableCommand,
				CommandParameter = executable
			});

			if (_recentExecutables.Count == 11) {
				_recentExecutables.RemoveAt(10);
				_recentExecutablesMenuItems.RemoveAt(10);
			}
			AppManager.Settings.RecentExecutables = _recentExecutables;

		}

		public DelegateCommandBase RunRecentExecutableCommand { get; private set; }

		private object ErrorToString(ErrorEventArgs e) {
			switch (e.Error) {
				case DebuggerError.LocalKernelAttachFailed:
					return "Failed to attach to Local Kernel";
			}
			return e.Error.ToString();
		}

		public DispatcherOperation InvokeAsync(Action action) {
			return Dispatcher.InvokeAsync(action);
		}

		public DispatcherOperation<T> InvokeAsync<T>(Func<T> action) {
			return Dispatcher.InvokeAsync(action);
		}

		[Import]
		public IFileDialogService FileDialogService { get; private set; }

		[Import]
		public IDialogService DialogService { get; private set; }

		[Import]
		public IMessageBoxService MessageBoxService { get; private set; }

		public void ReportError(Exception ex) {
			Debug.Assert(MessageBoxService != null);
			MessageBoxService.ShowMessage(ex.Message, Constants.Title);
		}

		DEBUG_STATUS _status = DEBUG_STATUS.NO_DEBUGGEE;
		private void Debugger_StatusChanged(object sender, StatusChangedEventArgs e) {
			Dispatcher.InvokeAsync(() => {
				if (e.NewStatus == _status)
					return;

				_status = e.NewStatus;
				UpdateCommands();
				RunRecentExecutableCommand.RaiseCanExecuteChanged();
			});
		}

		public MainViewModel CurrentWindow { get; internal set; }

		public IList<MainViewModel> Windows => _windows;

		public T FindTab<T>(out MainViewModel vm) where T : TabItemViewModelBase {
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

		[ImportMany]
		List<ICommandCollection> AllCommands;

		public Workspace LoadWorkspace(string path) {
			return null;
		}

		public void OnImportsSatisfied() {
			DebugManager.Debugger.StatusChanged += Debugger_StatusChanged;
			DebugManager.Debugger.Error += Debugger_Error;

			RunRecentExecutableCommand = new DelegateCommand<Executable>(async executable => {
				await DebugManager.Debugger.DebugProcess(executable.Path, executable.Arguments, AttachProcessFlags.Invasive, true);
				AddExecutable(executable);
			}, _ => DebugManager.Status == DEBUG_STATUS.NO_DEBUGGEE);

			var commandList = AllCommands.SelectMany(list => list.GetCommands());
			foreach (var pair in commandList)
				_commands.Add(pair);

		}

		private void SetRecentMenuItems() {
			_recentExecutables = AppManager.Settings.RecentExecutables;

			_recentExecutables.Select(executable => new MenuItemViewModel {
				Text = $"{executable.Path} {executable.Arguments}",
				Command = RunRecentExecutableCommand,
				CommandParameter = executable
			}).ForEach(item => _recentExecutablesMenuItems.Add(item));
		}

		internal void UpdateCommands() {
			foreach (var cmd in _commands.Values.OfType<DelegateCommandBase>())
				cmd.RaiseCanExecuteChanged();
		}

		public ICommand GetCommand(string name) {
			ICommand cmd;
			_commands.TryGetValue(name, out cmd);
			return cmd;
		}
	}
}
