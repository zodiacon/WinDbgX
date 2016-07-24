using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DebuggerEngine {
	class SingleThreadedTaskScheduler : TaskScheduler, IDisposable {
		BlockingCollection<Task> _tasks;
		Thread _thread;

		public SingleThreadedTaskScheduler(ThreadPriority priority = ThreadPriority.Normal) {
			_tasks = new BlockingCollection<Task>();
			_thread = new Thread(() => {
				foreach(var task in _tasks.GetConsumingEnumerable()) {
					if(base.TryExecuteTask(task))
						task.Wait();
				}
			});
			_thread.Priority = priority;
			_thread.IsBackground = true;
			_thread.Start();
		}

		protected override IEnumerable<Task> GetScheduledTasks() {
			return _tasks.ToArray();
		}

		protected override void QueueTask(Task task) {
			_tasks.Add(task);
		}

		protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) {
			return false;

		}

		public void Dispose() {
			_tasks.CompleteAdding();
			GC.SuppressFinalize(this);
		}

		~SingleThreadedTaskScheduler() {
			Dispose();
		}
	}
}
