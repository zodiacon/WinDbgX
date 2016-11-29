using DebuggerEngine;
using DebuggerEngine.Interop;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using WinDbgEx.Commands;
using WinDbgEx.UICore;
using WinDbgEx.ViewModels;
using Zodiacon.WPF;
using System.Windows;

namespace WinDbgEx.Models {
	[Export]
	sealed class UIManager : IPartImportsSatisfiedNotification {
		ObservableCollection<MainViewModel> _windows = new ObservableCollection<MainViewModel>();
		ObservableCollection<string> _recentWorkspaces = new ObservableCollection<string>();
		static readonly List<DelegateCommandBase> _commands = new List<DelegateCommandBase>(32);

		//public static readonly UIManager Instance = new UIManager();

		static UIManager() {
			Type[] types = {
				typeof(FileCommands), typeof(ViewCommands), typeof(OptionsCommands), typeof(DebugCommands)
			};
			foreach(var type in types)
				_commands.AddRange(type.GetProperties(BindingFlags.Public | BindingFlags.Static)
					.Where(pi => pi.PropertyType == typeof(DelegateCommandBase))
					.Select(pi => pi.GetValue(null) as DelegateCommandBase));
		}

		DebugManager DebugManager;
		Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

		[ImportingConstructor]
		private UIManager(DebugManager debugManager) {
			DebugManager = debugManager;
			DebugManager.Debugger.StatusChanged += Debugger_StatusChanged;
			DebugManager.Debugger.Error += Debugger_Error;
		}

		private void Debugger_Error(object sender, ErrorEventArgs e) {
			_dispatcher.InvokeAsync(() => {
				MessageBoxService.SetOwner(Application.Current.MainWindow);
				MessageBoxService.ShowMessage($"Error: {ErrorToString(e)}", Constants.Title, MessageBoxButton.OK, MessageBoxImage.Error);
			});
		}

		private object ErrorToString(ErrorEventArgs e) {
			switch (e.Error) {
				case DebuggerError.LocalKernelAttachFailed:
					return "Failed to attach to Local Kernel";
			}
			return e.Error.ToString();
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
			_dispatcher.InvokeAsync(() => {
				if (e.NewStatus == _status)
					return;

				_status = e.NewStatus;
				UpdateCommands();
			});
		}

		public IReadOnlyList<string> RecentWorkspaces => _recentWorkspaces;

		public MainViewModel CurrentWindow { get; internal set; }

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

		public void LoadWorkspace(string path) {
		}

		public void OnImportsSatisfied() {
			UpdateCommands();
		}

		private void UpdateCommands() {
			foreach (var cmd in _commands)
				cmd.RaiseCanExecuteChanged();

		}
	}
}
