using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDbgEx.UICore;

namespace WinDbgEx.ViewModels {
	[TabItem("Registers", Icon = "/icons/cpu.ico")]
	[Export]
	class RegistersViewModel : TabItemViewModelBase {
	}
}
