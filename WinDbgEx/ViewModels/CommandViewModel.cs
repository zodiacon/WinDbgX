using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using WinDbgEx.UICore;
using System.ComponentModel.Composition;
using WinDbgEx.Models;
using DebuggerEngine;
using DebuggerEngine.Interop;
using System.Windows.Threading;
using System.Windows.Input;
using Prism.Commands;

namespace WinDbgEx.ViewModels {
	[Export]
	[TabItem("Command", Icon = "/icons/console.ico", CanClose = false)]
	class CommandViewModel : TabViewModelBase {
		readonly ObservableCollection<CommandHistoryItem> _history = new ObservableCollection<CommandHistoryItem>();
		Dictionary<DEBUG_OUTPUT, RgbColor> _historyColors;
		private string _commandText;

		public string CommandText {
			get { return _commandText; }
			set { SetProperty(ref _commandText, value); }
		}

		public IList<CommandHistoryItem> History => _history;

		private string _prompt;

		public string Prompt {
			get { return _prompt; }
			set { SetProperty(ref _prompt, value); }
		}

		DebugClient _debugger;
		Dispatcher _dispatcher;

		readonly DebugManager DebugManager;

		[ImportingConstructor]
		public CommandViewModel(DebugManager debug) {
			DebugManager = debug;
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

		private bool _isNotBusy = false;

		public bool IsNotBusy {
			get { return _isNotBusy; }
			set { SetProperty(ref _isNotBusy, value); }
		}

		private void _debugger_StatusChanged(object sender, StatusChangedEventArgs e) {
			var state = e.NewStatus;
			_dispatcher.InvokeAsync(async () => {
				IsNotBusy = state == DEBUG_STATUS.BREAK;
				await _debugger.OutputPrompt();

				if (state == DEBUG_STATUS.NO_DEBUGGEE)
					Prompt = Constants.NoTarget;
				else if (!IsNotBusy)
					Prompt = Constants.Busy;
			});
		}

		public ICommand ExecuteCommand => new DelegateCommand(async () => {
			var target = await _debugger.Execute(CommandText);
			_history.Add(new CommandHistoryItem { Text = Prompt + " " + CommandText + Environment.NewLine, Color = ColorFromType(DEBUG_OUTPUT.NORMAL) });

			CommandText = string.Empty;
			if (!target)
				Prompt = Constants.NoTarget;
		}, () => !string.IsNullOrWhiteSpace(CommandText))
			.ObservesProperty(() => CommandText);
	}
}
