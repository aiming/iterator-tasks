using System;

namespace Aiming.IteratorTasks
{
	/// <summary>
	/// タスクのキャンセル用トークン。
	/// キャンセルを受ける側。
	/// </summary>
	public struct CancellationToken
	{
		private CancellationTokenSource _source;

		internal CancellationToken(CancellationTokenSource source) { _source = source; }

		/// <summary>
		/// キャンセル要求が出ているかどうか。
		/// </summary>
		public bool IsCancellationRequested
		{
			get
			{
				if (_source == null)
					return false;
				else
					return _source.IsCancellationRequested;
			}
		}

		/// <summary>
		/// キャンセル要求時に通知を受け取るためのデリゲートを登録。
		/// </summary>
		/// <param name="onCanceled">キャンセル要求時に呼ばれるデリゲート。</param>
		public void Register(Action onCanceled)
		{
			if (_source != null)
				_source.Canceled += onCanceled;
		}

		/// <summary>
		/// 空のトークン。
		/// </summary>
		public static CancellationToken None = new CancellationToken();

		/// <summary>
		/// キャンセル要求が出ている場合、OperationCanceledException をスローする。
		/// </summary>
		public void ThrowIfCancellationRequested()
		{
			if (IsCancellationRequested)
			{
				var e = _source.CancelReason != null ?
					_source.CancelReason :
					new TaskCanceledException();
				throw e;
			}
		}
	}
}
