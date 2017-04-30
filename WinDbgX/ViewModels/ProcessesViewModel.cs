using DebuggerEngine;
using DebuggerEngine.Interop;
using Prism.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using WinDbgX.Models;
using WinDbgX.UICore;

#pragma warning disable 649

namespace WinDbgX.ViewModels {
	internal class ThreadPriority {
		public string Text { get; set; }
		public ThreadPriorityLevel Priority { get; set; }
	}

	[TabItem("Processes & Threads", Icon = "/icons/gears.ico")]
	[Export]
	class ProcessesViewModel : TabItemViewModelBase, IPartImportsSatisfiedNotification {
		ObservableCollection<ProcessViewModel> _processes = new ObservableCollection<ProcessViewModel>();

		public IList<ProcessViewModel> Processes => _processes;

		[Import]
		DebugManager DebugManager;

		[Import]
		UIManager UI;

		public ProcessesViewModel() {
		}

		private void Debugger_ProcessExited(object sender, ProcessExitedEventArgs e) {
			UI.InvokeAsync(() => {
				_processes.Remove(_processes.First(p => p.ProcessId == e.Process.PID));
			});
		}

		private void Debugger_ThreadExited(object sender, ThreadExitedEventArgs e) {
			UI.InvokeAsync(() => {
				var process = _processes.FirstOrDefault(p => p.ProcessId == e.Process.PID);
				if (process == null)
					return;
				var threads = process.Threads;
				threads.Remove(threads.First(th => th.OSID == e.Thread.TID));
			});
		}

		private void Debugger_ThreadCreated(object sender, ThreadCreatedEventArgs e) {
			UI.InvokeAsync(() => {
				_processes[(int)e.Thread.ProcessIndex].Threads.Add(new ThreadViewModel(e.Thread, DebugManager));
			});
		}

		private void Debugger_ProcessCreated(object sender, ProcessCreatedEventArgs e) {
			UI.InvokeAsync(() => {
				_processes.Add(new ProcessViewModel(e.Process));
			});
		}

		private void Debugger_StatusChanged(object sender, StatusChangedEventArgs e) {
			var status = e.NewStatus;
			UI.InvokeAsync(() => {
				Status = status;
				if (Status == DEBUG_STATUS.NO_DEBUGGEE) {
					UI.InvokeAsync(() => Processes.Clear());
				}
				else if (Status == DEBUG_STATUS.BREAK) {
					foreach (var p in Processes)
						p.Refresh();
				}
			});
		}

		public void OnImportsSatisfied() {
			foreach (var process in DebugManager.Processes) {
				var processVM = new ProcessViewModel(process);
				foreach (var th in process.Threads)
					processVM.Threads.Add(new ThreadViewModel(th, DebugManager));
				_processes.Add(processVM);
			}

			Status = DebugManager.Status;
			DebugManager.Debugger.StatusChanged += Debugger_StatusChanged;

			DebugManager.Debugger.ProcessCreated += Debugger_ProcessCreated;
			DebugManager.Debugger.ThreadCreated += Debugger_ThreadCreated;
			DebugManager.Debugger.ThreadExited += Debugger_ThreadExited;
			DebugManager.Debugger.ProcessExited += Debugger_ProcessExited;
		}

		private DEBUG_STATUS _status;

		public DEBUG_STATUS Status {
			get { return _status; }
			private set {
				if (SetProperty(ref _status, value)) {
					RaisePropertyChanged(nameof(IsEnabled));
				}
			}
		}

		public ThreadPriority[] ThreadPriorities { get; } = new ThreadPriority[] {
			new ThreadPriority { Text = "Idle", Priority = ThreadPriorityLevel.Idle},
			new ThreadPriority { Text = "Lowest", Priority = ThreadPriorityLevel.Lowest},
			new ThreadPriority { Text = "Below Normal", Priority = ThreadPriorityLevel.BelowNormal },
			new ThreadPriority { Text = "Normal", Priority = ThreadPriorityLevel.Normal },
			new ThreadPriority { Text = "Above Normal", Priority = ThreadPriorityLevel.AboveNormal },
			new ThreadPriority { Text = "Highest", Priority = ThreadPriorityLevel.Highest },
			new ThreadPriority { Text = "Time Critical", Priority = ThreadPriorityLevel.TimeCritical },
		};

		public bool IsEnabled => Status == DEBUG_STATUS.BREAK;

		public DelegateCommandBase SetThreadPriorityCommand => new DelegateCommand<ThreadPriority>(tp => {
			foreach (var process in Processes)
				foreach (var thread in process.SelectedThreads)
					thread.SetPriority(tp.Priority);
		});

		public DelegateCommandBase SuspendThreadsCommand => new DelegateCommand<ThreadPriority>(tp => {
			foreach (var process in Processes)
				foreach (var thread in process.SelectedThreads)
					thread.Thread.Suspend();
		});

		public DelegateCommandBase ResumeThreadsCommand => new DelegateCommand<ThreadPriority>(tp => {
			foreach (var process in Processes)
				foreach (var thread in process.SelectedThreads)
					thread.Thread.Resume();
		});

	}
}
