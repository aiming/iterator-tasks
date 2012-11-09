using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Aiming.IteratorTasks
{
	/// <summary>
	/// 複数の例外を束ねる例外クラス。
	/// 並行動作してると、複数のタスク内で同時に例外が発生する可能性があるので。
	/// </summary>
	public class AggregateException : Exception
	{
		private List<Exception> _exceptions;
		public IEnumerable<Exception> Exceptions { get { return _exceptions.ToArray(); } }

		public AggregateException(List<Exception> exceptions)
		{
			_exceptions = exceptions;
		}

		public override string Message
		{
			get
			{
				var count = _exceptions.Count;

				if (count == 1) return _exceptions[0].Message;
				else if (count > 1) return string.Format("AggregateException: {0} errors", count);
				else return base.Message;
			}
		}
	}

	// もともと、Util とか MultiTask みたいな名前の静的クラスに持たせようかと思っていたものの、
	// 結局、Task クラス自身に並行動作用のメソッドを持たせることに。
	public partial class Task
	{
		public static Task WhenAllTask(params Task[] routines)
		{
			return new Task(WhenAll(routines));
		}

		/// <summary>
		/// 複数の Task を並行実行する。
		/// </summary>
		/// <remarks>
		/// 1フレームにつき、1つの Task の MoveNext しか呼ばないので、
		/// N 個の Task を並行実行すると、個々の MoveNext は N フレームに1回しか呼ばれない。
		/// </remarks>
		/// <param name="routines">同時に動かしたい Task。</param>
		/// <returns>束ねたコルーチン。</returns>
		public static IEnumerator WhenAll(params Task[] routines)
		{
			int successCount = 0;
			var errors = new List<Exception>();

			foreach (var r in routines)
			{
				r.OnComplete(t =>
				{
					if (t.Error == null) ++successCount;
					else errors.AddRange(t.Error.Exceptions);
				});
			}

			do
			{
				for (int i = 0; i < routines.Length; i++)
				{
					var r = routines[i];
					if (r == null) continue;

					if (r.MoveNext())
					{
						yield return r.Current;
					}
					else
						routines[i] = null;
				}
			} while (routines.Any(x => x != null));

			if (errors.Count != 0) throw new AggregateException(errors);
		}
	}
}
