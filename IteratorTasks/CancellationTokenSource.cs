using System;

namespace Aiming.IteratorTasks
{
	/// <summary>
	/// タスクのキャンセル用トークン。
	/// キャンセルする側。
	/// </summary>
	public class CancellationTokenSource
	{
		public CancellationTokenSource()
		{
			Token = new CancellationToken(this);
		}

		/// <summary>
		/// キャンセル用トークン。
		/// </summary>
		public CancellationToken Token { get; private set; }

		/// <summary>
		/// キャンセル要求を出したかどうか。
		/// </summary>
		public bool IsCancellationRequested { get; private set; }

		/// <summary>
		/// キャンセル。
		/// </summary>
		public void Cancel()
		{
			var d = Canceled;
			if (d != null) d();

			IsCancellationRequested = true;
		}

		/// <summary>
		/// キャンセルの原因となる例外を指定してのキャンセル要求。
		/// </summary>
		public void Cancel(Exception cancelReason)
		{
			_cancelReason = cancelReason;

			this.Cancel();
		}

		internal event Action Canceled;

		private Exception _cancelReason;
		internal Exception CancelReason { get { return _cancelReason; } }
	}
}
