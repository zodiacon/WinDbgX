using DebuggerEngine;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

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

		public void Refresh() {
			foreach (var th in Threads)
				th.Refresh();
		}


		ThreadViewModel[] _selectedThreads = new ThreadViewModel[4];

		public IEnumerable<ThreadViewModel> SelectedThreads => _selectedThreads.TakeWhile(th => th != null);

		public DelegateCommandBase SetSelectedItemsCommand => new DelegateCommand<IList>(items => {
			if (_selectedThreads.Length < items.Count)
				Array.Resize(ref _selectedThreads, items.Count);
			items.CopyTo(_selectedThreads, 0);
		});


	}
}
