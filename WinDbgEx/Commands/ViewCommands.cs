using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDbgEx.Models;

namespace WinDbgEx.Commands {
	static class ViewCommands {
		public static DelegateCommandBase Modules { get; } = new DelegateCommand<DebugContext>(context => {
			
		});
	}
}
