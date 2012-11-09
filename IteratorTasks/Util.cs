using System;
using System.Collections;

namespace Aiming.IteratorTasks
{
	public static class Util
	{
		/// <summary>
		/// Unity の StartCoroutine を呼ぶ際に、null を渡しても平気かどうかわからなかったので、ダミーの 0 要素の IEnumeartor を返す。
		/// 要検証かも。
		/// </summary>
		/// <returns></returns>
		public static IEnumerator EmptyRoutine()
		{
			return new object[0].GetEnumerator();
		}
		
		public static IEnumerator Concat(Func<IEnumerator> e1, Func<IEnumerator> e2)
		{
			var x1 = e1();
			while (x1.MoveNext()) yield return x1.Current;
			Dispose(x1);

			var x2 = e2();
			while (x2.MoveNext()) yield return x2.Current;
			Dispose(x2);
		}

		public static void Dispose(object obj)
		{
			var d = obj as IDisposable;
			if (d != null) d.Dispose();
		}
	}
}
