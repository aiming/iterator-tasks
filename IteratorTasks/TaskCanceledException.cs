using System;

namespace Aiming.IteratorTasks
{
	public class TaskCanceledException : OperationCanceledException
	{
		public TaskCanceledException() { }
		public TaskCanceledException(string message) : base(message) { }
		public TaskCanceledException(string message, Exception innerException) : base(message, innerException) { }
	}
}
