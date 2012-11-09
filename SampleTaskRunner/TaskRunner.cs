using System.Collections.Generic;
using Aiming.IteratorTasks;

namespace SampleTaskRunner
{
	public class TaskRunner : TaskScheduler
	{
		private List<Task> _runningTasks = new List<Task>();
		private List<Task> _toBeRemoved = new List<Task>();

		protected override void QueueTask(Task task)
		{
			_runningTasks.Add(task);
		}

		/// <summary>
		/// 1フレーム処理を進める。
		/// </summary>
		public void Update()
		{
			foreach (var t in _runningTasks)
			{
				if (!t.MoveNext())
					_toBeRemoved.Add(t);
			}

			foreach (var t in _toBeRemoved)
			{
				_runningTasks.Remove(t);
			}
			_toBeRemoved.Clear();
		}

		/// <summary>
		/// 与えたフレーム数分、処理を進める。
		/// </summary>
		/// <param name="numFrames">進めたいフレーム数。</param>
		public void Update(int numFrames)
		{
			for (int i = 0; i < numFrames; i++)
			{
				this.Update();
			}
		}
	}
}
