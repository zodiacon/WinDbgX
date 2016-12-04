using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDbgX.UICore;

namespace WinDbgX.ViewModels {
	[TabItem("Registers", Icon = "/icons/cpu.ico")]
	[Export]
	class RegistersViewModel : TabItemViewModelBase {
	}
}
