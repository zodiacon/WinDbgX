using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Markup;
using WinDbgX.Models;

namespace WinDbgX.Extensions {
	[MarkupExtensionReturnType(typeof(ICommand))]
	class CommandExtension : MarkupExtension {
		static UIManager UI = AppManager.Instance.UI;

		public string Name { get; }
		public CommandExtension(string name) {
			Name = name;
		}

		public override object ProvideValue(IServiceProvider sp) {
			return UI.GetCommand(Name);
		}
	}
}
