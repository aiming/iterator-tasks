using System;
using System.Collections;
using System.Collections.Generic;

namespace Aiming.IteratorTasks
{
	/// <summary>
	/// .NET 4 の Task 的にコルーチンを実行するためのクラス。
	/// 戻り値なし版。
	/// </summary>
	public partial class Task : IEnumerator, IDisposable
	{
		protected IEnumerator Routine { get; set; }

		private AggregateException _error;
		
		/// <summary>
		/// タスク中で発生した例外。
		/// </summary>
		/// <remarks>
		/// 並行動作で、複数のタスクで同時に例外が起きたりする場合があるので、AggregateException にしてある。
		/// </remarks>
		public AggregateException Error
		{
			get
			{
				var internalTask = Routine as Task;
				if(internalTask != null && internalTask.Error != null)
				{
					AddError(internalTask.Error);
					internalTask.ClearError();
				}
				return _error;
			}
			private set
			{
				_error = value;
			}
		}

		private List<Exception> _errors;

		/// <summary>
		/// Task.Error を AggregateException にしたので、例外の追加は this.Error = exc; じゃなくて、この AddError を介して行う。
		/// 引数に AggregateException を渡した場合は、子要素を抜き出して統合。
		/// </summary>
		/// <param name="exc">追加したい例外。</param>
		protected void AddError(Exception exc)
		{
			if (_error == null)
			{
				_errors = new List<Exception>();
				_error = new AggregateException(_errors);
			}

			var agg = exc as AggregateException;
			if (agg != null)
			{
				foreach (var e in agg.Exceptions)
				{
					_errors.Add(e);
				}
			}
			else
			{
				_errors.Add(exc);
			}
		}
		
		protected void ClearError()
		{
			if (_error != null)
			{
				_errors = null;
				_error = null;
			}
		}
		
		#region IEnumerator

		// Current と MoveNext も明示的実装して隠したかったけども、それやると StartCoroutine で動かなくなるみたい。

		public virtual object Current
		{
			get
			{
				if (IsCanceled) return null;
				return Routine == null ? null : Routine.Current;
			}
		}

		public virtual bool MoveNext()
		{
			if (Status == TaskStatus.Created) Status = TaskStatus.Running;
			if (Status != TaskStatus.Running) return false;
			if (Routine == null) return false;

			bool hasNext;

			try
			{
				hasNext = Routine.MoveNext();
			}
			catch (Exception exc)
			{
				AddError(exc);
				hasNext = false;
			}

			if (!hasNext)
			{
				Complete();
			}
			return hasNext;
		}

		public void Dispose()
		{
			var d = Routine as IDisposable;
			if (d != null) d.Dispose();
			Routine = null;
		}

		void IEnumerator.Reset()
		{
			throw new NotImplementedException();
		}

		public void Start(TaskScheduler scheduler)
		{
			if (Status != TaskStatus.Created)
				throw new InvalidOperationException();

			scheduler.QueueTask(this);
		}

		public TaskStatus Status { get; private set; }

		public bool IsDone { get { return IsCompleted || IsCanceled || IsFaulted; } }
		public bool IsCompleted { get { return Status == TaskStatus.RanToCompletion; } }
		public bool IsCanceled { get { return Status == TaskStatus.Canceled; } }
		public bool IsFaulted { get { return Status == TaskStatus.Faulted; } }

		protected void Complete()
		{
			if (Error != null)
				Status = TaskStatus.Faulted;
			else
				Status = TaskStatus.RanToCompletion;

			if (_callback.Count != 0)
			{
				foreach (var c in _callback)
				{
					Invoke(c);
				}
			}
			_callback.Clear();
		}

		private void Invoke(Action<Task> c)
		{
			try
			{
				c(this);
			}
			catch (Exception exc)
			{
				AddError(exc);
			}
		}

		#endregion

		List<Action<Task>> _callback = new List<Action<Task>>();

		/// <summary>
		/// コルーチン完了後に呼ばれるコールバックを登録。
		/// </summary>
		/// <param name="callback">コールバック。</param>
		/// <returns>自分自身（fluent interface）。</returns>
		/// <remarks>
		/// ちなみに、callback 内で発生した例外も Error に統合されて、後段のタスクが実行されなくなる。
		/// その意味で、OnComplete というよりは、HookComplete （完了に割って入る）の方が正しい気も。
		/// </remarks>
		public Task OnComplete(Action<Task> callback)
		{
			if (IsDone)
				Invoke(callback);
			else
				_callback.Add(callback);

			return this;
		}

		/// <summary>
		/// コルーチンがエラー終了（例外発生）した時だけ呼ばれるコールバックを登録。
		/// </summary>
		/// <param name="errorHandler">エラー処理コールバック。</param>
		/// <returns>自分自身（fluent interface）。</returns>
		/// <remarks>
		/// 内部的には OnComplete を呼んでるので、挙動はそちらに準じる。
		/// </remarks>
		public Task OnError<T>(Action<T> errorHandler)
			where T : Exception
		{
			return this.OnComplete(t =>
			{
				if (t.Error != null)
					foreach (var e in t.Error.Exceptions)
					{
						var et = e as T;
						if (et != null)
							errorHandler(et);
					}
			});
		}

		/// <remarks>
		/// OnError、最初から IEnumerable{T} 受け取るようにしとけばよかった…
		/// 後からオーバーロード足そうとしたら、既存コードがエラーになったので、しょうがなく別メソッドに。
		/// AsOne ってのも微妙だけども、2 とか Ex とかつけるよりはマシなので。
		/// </remarks>
		public Task OnErrorAsOne(Action<IEnumerable<Exception>> errorHandler)
		{
			return this.OnComplete(t =>
			{
				if (t.Error != null)
					errorHandler(t.Error.Exceptions);
			});
		}

		/// <summary>
		/// タスクをキャンセルします。
		/// </summary>
		public void Cancel()
		{
			if (Cancellation == null)
				throw new InvalidOperationException("Can't cancel Task.");
				
			Cancellation.Cancel();

			MoveNext();
		}

		public void Cancel(Exception e)
		{
			if (Cancellation == null)
				throw new InvalidOperationException("Can't cancel Task.");

			Cancellation.Cancel(e);

			MoveNext();
		}

		public void ForceCancel()
		{
			ForceCancel(new TaskCanceledException("Task force canceled."));
		}
		
		/// <summary>
		/// タスクを強制キャンセルします。OnCompleteは呼ばれません。
		/// </summary>
		public void ForceCancel(Exception e)
		{
			Status = TaskStatus.Canceled;
			AddError(e);
			this.Dispose();
		}

		protected Task()
		{
		}

		/// <summary>
		/// 最初から完了済みのタスクを生成。
		/// Empty とか Return 用。
		/// </summary>
		/// <param name="completed"></param>
		protected Task(TaskStatus status)
		{
			Status = status;
		}

		/// <summary>
		/// コルーチンを与えて初期化。
		/// </summary>
		/// <param name="routine">コルーチン。</param>
		public Task(IEnumerator routine) { Routine = routine; }
		
		/// <summary>
		/// コルーチン生成メソッドを与えて初期化。
		/// </summary>
		/// <param name="starter"></param>
		public Task(Func<IEnumerator> starter)
		{
			Routine = starter();
		}

		public CancellationTokenSource Cancellation { get; set; }
		
		/// <summary>
		/// タスクが Error を持っていたらそれを throw。
		/// </summary>
		/// <returns>自分自身（fluent interface）。</returns>
		private Task Check()
		{
			if (_error != null)
			{
				throw _error;
			}
			return this;
		}

		/// <summary>
		/// 空タスク（作った時点で完了済み）を生成。
		/// </summary>
		/// <returns>空タスク。</returns>
		public static Task Empty()
		{
			return _empty;
		}

		private readonly static Task _empty = new Task(TaskStatus.RanToCompletion);

		/// <summary>
		/// 単に値を返す（作った時点で完了済み、最初から Return の値を持つ）タスクを生成。
		/// </summary>
		/// <typeparam name="T">戻り値の型。</typeparam>
		/// <param name="value">返したい値。</param>
		/// <returns>完了済みタスク。</returns>
		public static Task<T> Return<T>(T value)
		{
			return new Task<T>(value);
		}

		public Task<U> Select<U>(Func<U> selector)
		{
			return this.ContinueWithTask<U>(() => Task.Return(selector()));
		}
	}

	/// <summary>
	/// .NET 4 の Task 的にコルーチンを実行するためのクラス。
	/// 戻り値あり版。
	/// </summary>
	/// <typeparam name="T">最終的に返す型。</typeparam>
	public partial class Task<T> : Task
	{
		/// <summary>
		/// 最終的に返したい値。
		/// </summary>
		virtual public T Result
		{
			get
			{
				if (Error != null) throw Error;
				return _result;
			}
		}
		private T _result;

		internal Task() { }

		/// <summary>
		/// Task.Return 用。
		/// </summary>
		/// <param name="result"></param>
		internal Task(T result) : base(TaskStatus.RanToCompletion)
		{
			_result = result;
		}

		/// <summary>
		/// コルーチン生成メソッドを与えて初期化。
		/// </summary>
		/// <param name="starter"></param>
		public Task(Func<Action<T>, IEnumerator> starter)
		{
			Routine = starter(r => { _result = r; });
		}
		
		/// <summary>
		/// コルーチン完了後に呼ばれるコールバックを登録。
		/// </summary>
		/// <param name="callback">コールバック。</param>
		/// <returns>自分自身（fluent interface）。</returns>
		public Task<T> OnComplete(Action<Task<T>> callback)
		{
			base.OnComplete(t => callback(this));
			return this;
		}

		/// <summary>
		/// Task.Check と同様。
		/// </summary>
		/// <returns>自分自身（fluent interface）。</returns>
		private Task<T> Check()
		{
			if (Error != null)
			{
				throw Error;
			}
			return this;
		}

		public Task<U> Select<U>(Func<T, U> selector)
		{
			return this.ContinueWithTask<U>(x => Task.Return(selector(x)));
		}
	}
}
