using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aiming.IteratorTasks
{
	public abstract class TaskScheduler
	{
		protected internal abstract void QueueTask(Task task);
	}
}
