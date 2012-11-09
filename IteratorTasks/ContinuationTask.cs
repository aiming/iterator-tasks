using System;
using System.Collections;

namespace Aiming.IteratorTasks
{
	public partial class Task
	{
		/// <summary>
		/// 1つのタスク完了後に、続けて処理をする新しいタスクを生成。
		/// </summary>
		/// <param name="continuation">継続処理のタスク生成メソッド。</param>
		/// <returns>新しいタスク。</returns>
		public Task ContinueWithTask(Func<Task> continuation)
		{
			return new ContinuationTask(this, continuation);
		}

		/// <summary>
		/// 1つのタスク完了後に、続けて処理をする新しいタスクを生成。
		/// </summary>
		/// <param name="continuation">継続処理のコルーチン生成メソッド。</param>
		/// <returns>新しいタスク。</returns>
		public Task ContinueWith(Func<IEnumerator> continuation)
		{
			return this.ContinueWithTask(() => new Task(continuation()));
		}

		/// <summary>
		/// 1つのタスク完了後に、続けて処理をする新しいタスクを生成。
		/// </summary>
		/// <typeparam name="U">継続タスクの戻り値の型。</typeparam>
		/// <param name="continuation">継続処理のタスク生成メソッド。</param>
		/// <returns>新しいタスク。</returns>
		public Task<U> ContinueWithTask<U>(Func<Task<U>> continuation)
		{
			return new ContinuationTask<U>(this, continuation);
		}

		/// <summary>
		/// 1つのタスク完了後に、続けて処理をする新しいタスクを生成。
		/// </summary>
		/// <typeparam name="U">継続タスクの戻り値の型。</typeparam>
		/// <param name="continuation">継続処理のコルーチン生成メソッド。</param>
		/// <returns>新しいタスク。</returns>
		public Task<U> ContinueWith<U>(Func<Action<U>, IEnumerator> continuation)
		{
			return this.ContinueWithTask(() => new Task<U>(continuation));
		}
	}

	public partial class Task<T>
	{
		/// <summary>
		/// 1つのタスク完了後に、続けて処理をする新しいタスクを生成。
		/// </summary>
		/// <param name="continuation">継続処理のタスク生成メソッド。</param>
		/// <returns>新しいタスク。</returns>
		public Task ContinueWithTask(Func<T, Task> continuation)
		{
			return new ContinuationTask(this, () =>
			{
				var res = this.Result;
				this._result = default(T);
				GC.Collect();
				return continuation(res);
			});
		}

		/// <summary>
		/// 1つのタスク完了後に、続けて処理をする新しいタスクを生成。
		/// </summary>
		/// <param name="continuation">継続処理のコルーチン生成メソッド。</param>
		/// <returns>新しいタスク。</returns>
		public Task ContinueWith(Func<T, IEnumerator> continuation)
		{
			return this.ContinueWithTask(() => new Task(continuation(this.Result)));
		}

		/// <summary>
		/// 1つのタスク完了後に、続けて処理をする新しいタスクを生成。
		/// </summary>
		/// <typeparam name="U">継続タスクの戻り値の型。</typeparam>
		/// <param name="continuation">継続処理のタスク生成メソッド。</param>
		/// <returns>新しいタスク。</returns>
		public Task<U> ContinueWithTask<U>(Func<T, Task<U>> continuation)
		{
			return new ContinuationTask<U>(this, () =>
			{
				var res = this.Result;
				return continuation(res);
			});
		}
		
		/// <summary>
		/// 1つのタスク完了後に、続けて処理をする新しいタスクを生成。
		/// </summary>
		/// <typeparam name="U">継続タスクの戻り値の型。</typeparam>
		/// <param name="continuation">継続処理のコルーチン生成メソッド。</param>
		/// <returns>新しいタスク。</returns>
		public Task<U> ContinueWith<U>(Func<T, Action<U>, IEnumerator> continuation)
		{
			Func<T, Task<U>> f = x => new Task<U>(a => continuation(x, a));
			return this.ContinueWithTask<U>(f);
		}
	}

	/// <summary>
	/// ContinuationTask のジェネリック版と非ジェネリック版でほとんど同じ処理なのに、継承とかで処理を使いまわせないのでやむなく別クラスに。
	/// </summary>
	internal class ContinuationTaskInternal
	{
		private Task _task;
		private Func<Task> _continuation;
		private Action<Exception> _addError;
		private Action _complete;
		internal Task LastTask { get; private set; }

		internal ContinuationTaskInternal(Task firstTask, Func<Task> continuation, Action<Exception> addError, Action complete)
		{
			_task = firstTask;
			_continuation = continuation;
			_addError = addError;
			_complete = complete;
			LastTask = null;
		}

		public object Current
		{
			get
			{
				if (_task != null)
					return _task.Current;
				return null;
			}
		}

		public bool MoveNext()
		{
			if (_task != null)
			{
				var hasNext = _task.MoveNext();

				if (hasNext) return true;

				if (_task.Error != null)
				{
					_addError(_task.Error);
					Complete();
					return false;
				}

				if (_continuation != null)
				{
					_task = _continuation();
					_continuation = null;
					return true;
				}
				else
				{
					Complete();
					return false;
				}
			}

			return false;
		}

		private void Complete()
		{
			LastTask = _task;
			_complete();
			_task = null;
			_continuation = null;
		}
	}

	/// <summary>
	/// 継続処理用のタスク クラス。
	/// </summary>
	internal class ContinuationTask : Task
	{
		ContinuationTaskInternal _inner;

		internal ContinuationTask(Task firstTask, Func<Task> continuation)
		{
			_inner = new ContinuationTaskInternal(firstTask, continuation, this.AddError, this.Complete);
		}

		public override object Current { get { return _inner.Current; } }

		public override bool MoveNext()
		{
			return _inner.MoveNext();
		}
	}

	/// <summary>
	/// 継続処理用のタスク クラス。
	/// ジェネリック版（戻り値あり）。
	/// </summary>
	internal class ContinuationTask<U> : Task<U>
	{
		ContinuationTaskInternal _inner;

		internal ContinuationTask(Task firstTask, Func<Task<U>> continuation)
		{
			_inner = new ContinuationTaskInternal(firstTask, () => continuation(), this.AddError, this.Complete);
		}

		public override object Current { get { return _inner.Current; } }

		public override bool MoveNext()
		{
			return _inner.MoveNext();
		}

		public override U Result
		{
			get
			{
				var t = _inner.LastTask as Task<U>;
				if (t != null)
				{
					return t.Result;
				}
				else
				{
					return default(U);
				}
			}
		}
	}
}
