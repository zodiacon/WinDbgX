using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using WinDbgEx.UICore;
using System.ComponentModel.Composition;

namespace WinDbgEx.ViewModels {
	[Export]
    class CommandViewModel : BindableBase {
		readonly ObservableCollection<CommandHistoryItem> _history = new ObservableCollection<CommandHistoryItem>();
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

	}
}
