using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDbgEx.UICore;

namespace WinDbgEx.ViewModels {
	[TabItem("Modules", Icon = "/icons/components.ico")]
	[Export]
	class ModulesViewModel : TabViewModelBase {
	}
}
