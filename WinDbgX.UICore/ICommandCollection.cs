using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WinDbgX.UICore {
	public interface ICommandCollection {
		IDictionary<string, ICommand> GetCommands();
	}
}
