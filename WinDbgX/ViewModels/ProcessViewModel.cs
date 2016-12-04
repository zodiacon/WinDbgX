using DebuggerEngine;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WinDbgX.Models;

namespace WinDbgX.ViewModels {
	[Export]
	class ProcessViewModel : BindableBase {
		ObservableCollection<ThreadViewModel> _threads = new ObservableCollection<ThreadViewModel>();
		public IList<ThreadViewModel> Threads => _threads;

		public TargetProcess Process { get; }

		public ProcessViewModel(TargetProcess process) {
			Process = process;
		}

		[DisplayName("Process ID")]
		public uint ProcessId => Process.PID;

		[DisplayName("PEB")]
		public string Peb => "0x" + Process.Peb.ToString("X");

		[DisplayName("Image Name")]
		public string ImageName => Process.ImageName;

		[DisplayName("Checksum")]
		public string Checksum => "0x" + Process.Checksum.ToString("X");

		IEnumerable<object> _properties;
		public IEnumerable<object> Properties => _properties ?? ((
			_properties = from pi in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
						  let nameAttribute = pi.GetCustomAttribute<DisplayNameAttribute>()
						  where nameAttribute != null
						  orderby pi.Name
						  select new {
							  Name = nameAttribute.DisplayName,
							  Value = pi.GetValue(this)
						  }).ToArray());
	}
}
