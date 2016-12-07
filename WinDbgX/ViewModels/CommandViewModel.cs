using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using WinDbgX.UICore;
using System.ComponentModel.Composition;
using WinDbgX.Models;
using DebuggerEngine;
using DebuggerEngine.Interop;
using System.Windows.Threading;
using System.Windows.Input;
using Prism.Commands;
using System.IO;
using System.Windows.Controls;

namespace WinDbgX.ViewModels {
	[Export]
	[TabItem("Command", Icon = "/icons/console.ico", CanClose = false)]
	class CommandViewModel : TabItemViewModelBase {
		readonly ObservableCollection<CommandHistoryItem> _history = new ObservableCollection<CommandHistoryItem>();
		ObservableCollection<string> _commandHistory = new ObservableCollection<string>();
		Dictionary<DEBUG_OUTPUT, RgbColor> _historyColors;
		private string _commandText;
		int _commandHistoryIndex;

		public string CommandText {
			get { return _commandText; }
			set { SetProperty(ref _commandText, value); }
		}

		public IList<CommandHistoryItem> History => _history;

		private string _prompt = Constants.NoTarget;

		public string Prompt {
			get { return _prompt; }
			set { SetProperty(ref _prompt, value); }
		}

		DebugClient _debugger;
		Dispatcher _dispatcher;

		readonly DebugManager DebugManager;
		readonly UIManager UI;

		[ImportingConstructor]
		public CommandViewModel(DebugManager debug, UIManager ui) {
			DebugManager = debug;
			UI = ui;

			_dispatcher = Dispatcher.CurrentDispatcher;
			_historyColors = new Dictionary<DEBUG_OUTPUT, RgbColor> {
				[DEBUG_OUTPUT.ERROR] = new RgbColor { R = 255 },
				[DEBUG_OUTPUT.EXTENSION_WARNING] = new RgbColor { R = 128 },
				[DEBUG_OUTPUT.WARNING] = new RgbColor { R = 128, G = 128 },
				[DEBUG_OUTPUT.DEBUGGEE] = new RgbColor { B = 255 },
				[DEBUG_OUTPUT.SYMBOLS] = new RgbColor { G = 128 }
			};

			_debugger = DebugManager.Debugger;
			_debugger.StatusChanged += _debugger_StatusChanged;
			_debugger.OutputCallback += _debugger_OutputCallback;
		}

		private void _debugger_OutputCallback(object sender, OutputCallbackEventArgs e) {
			_dispatcher.InvokeAsync(() => {
				if (e.Type == DEBUG_OUTPUT.PROMPT) {
					Prompt = e.Text;
				}
				else {
					History.Add(new CommandHistoryItem { Text = e.Text, Color = ColorFromType(e.Type) });
				}
			});
		}

		private RgbColor ColorFromType(DEBUG_OUTPUT type) {
			RgbColor color;
			if (_historyColors.TryGetValue(type, out color))
				return color;

			return new RgbColor();
		}

		private bool _isNotBusy;

		public bool IsNotBusy {
			get { return _isNotBusy; }
			set { SetProperty(ref _isNotBusy, value); }
		}

		public ToolbarItems Toolbar => new ToolbarItems {
			new ToolBarButtonViewModel { Text = "Clear", Command = ClearHistoryCommand, Icon = Icons.Delete },
			new ToolBarButtonViewModel { Text = "Save...", Command = SaveHistoryCommand, Icon = Icons.SaveAs },
		};

		public ICommand ClearHistoryCommand => new DelegateCommand(() => _history.Clear());

		public ICommand SaveHistoryCommand => new DelegateCommand(() => {
			if (_history.Count == 0) return;

			var filename = UI.FileDialogService.GetFileForSave();
			if (filename == null) return;

			File.WriteAllLines(filename, _history.Select(h => h.Text));

		});

		private void _debugger_StatusChanged(object sender, StatusChangedEventArgs e) {
			var state = e.NewStatus;
			_dispatcher.InvokeAsync(() => {
				IsNotBusy = state == DEBUG_STATUS.BREAK;

				if (state == DEBUG_STATUS.NO_DEBUGGEE) {
					Prompt = Constants.NoTarget;
					CommandText = string.Empty;
				}
				else if (!IsNotBusy)
					Prompt = Constants.Busy;
				else
					_debugger.OutputPrompt();
			});
		}

		public ICommand ExecuteCommand => new DelegateCommand(() => {
			_dispatcher.InvokeAsync(async () => {
				_history.Add(new CommandHistoryItem {
					Text = Prompt + " " + CommandText + Environment.NewLine,
					Color = ColorFromType(DEBUG_OUTPUT.NORMAL)
				});
				_commandHistory.Add(CommandText);
				var cmd = CommandText;
				CommandText = string.Empty;
				var target = await _debugger.Execute(cmd);
				_commandHistoryIndex = _commandHistory.Count;

			});
		}, () => !string.IsNullOrWhiteSpace(CommandText))
			.ObservesProperty(() => CommandText);

		public ICommand NextCommand => new DelegateCommand<TextBox>(tb => {
			if (_commandHistoryIndex >= _commandHistory.Count - 1)
				return;
			CommandText = _commandHistory[++_commandHistoryIndex];
			if (tb != null)
				tb.CaretIndex = CommandText.Length;
		});

		public ICommand PreviousCommand => new DelegateCommand<TextBox>(tb => {
			if (_commandHistoryIndex == 0)
				return;
			CommandText = _commandHistory[--_commandHistoryIndex];
			if(tb != null)
				tb.CaretIndex = CommandText.Length;
		});
	}
}
