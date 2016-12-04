using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WinDbgX.Models;
using WinDbgX.UICore;
using Zodiacon.WPF;

namespace WinDbgX.ViewModels {
	class AttachToProcessViewModel : DialogViewModelBase {
		public AttachToProcessViewModel(Window dialog) : base(dialog) {
			CanExecuteOKCommand = () => SelectedProcess != null;
			OKCommand.ObservesProperty(() => SelectedProcess);
		}

		public string Title => "Attach to Process";
		public double MaxHeight => 600;
		public double MinWidth => 400;
		public double MaxWidth => 600;
		public double MinHeight => 300;
		public ResizeMode ResizeMode => ResizeMode.NoResize;

		public ToolbarItems Toolbar => new ToolbarItems {
			new ToolBarButtonViewModel { Text = "Refresh", Icon = Icons.Refresh, Command = RefreshCommand }
		};

		internal class ProcessViewModel {
			Process Process { get; }
			public ProcessViewModel(Process p) {
				Process = p;
			}

			public int Id => Process.Id;
			public string Name => Process.ProcessName;
			public int Session => Process.SessionId;
			public string Username => Process.StartInfo.UserName;
		}

		private ProcessViewModel _selectedProcess;

		public ProcessViewModel SelectedProcess {
			get { return _selectedProcess; }
			set { SetProperty(ref _selectedProcess, value); }
		}

		public IEnumerable<ProcessViewModel> Processes {
			get {
				var processes = Process.GetProcesses().Where(p => p.Id > 4);
				if (!Environment.Is64BitProcess && Environment.Is64BitOperatingSystem)
					processes = processes.Where(p => Is32Bit(p));
				return processes.Select(p => new ProcessViewModel(p));
			}
		}

		private bool Is32Bit(Process p) {
			bool wow64 = false;
			var hProcess = NativeMethods.OpenProcess(NativeMethods.ProcessAccessRights.QUERY_LIMITED_INFORMATION, p.Id);
			if (hProcess == IntPtr.Zero)
				return false;
			NativeMethods.IsWow64Process(hProcess, out wow64);
			return wow64;
		}

		public ICommand RefreshCommand => new DelegateCommand(() => OnPropertyChanged(nameof(Processes)));
	}
}
