using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDbgEx.UICore;

namespace WinDbgEx.ViewModels {
	[TabItem("Modules", Icon = "/icons/components.ico")]
	[Export]
	class ModulesViewModel : TabItemViewModelBase {
		ObservableCollection<ProcessViewModel> _processes = new ObservableCollection<ProcessViewModel>();

		public IList<ProcessViewModel> Processes => _processes;
	}
}
