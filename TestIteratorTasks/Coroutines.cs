using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aiming.IteratorTasks;

namespace TestIteratorTasks
{
	/// <summary>
	/// テスト用に使うコルーチンいろいろ。
	/// </summary>
	class Coroutines
	{
		#region 同期処理

		// 中身には深い意味なし。
		// 単に、F3(F2(F1(x))) みたいに繋ぎたいだけ。

		public static double F1(double x)
		{
			return x * x;
		}

		public static string F2(double x)
		{
			return x.ToString();
		}

		public static int F3(string s)
		{
			return s.Length;
		}

		public static int FError()
		{
			throw new NotSupportedException();
		}

		#endregion
		#region イテレーター非同期処理

		public static Task NFrameTask(int n)
		{
			return new Task(() => NFrame(n));
		}

		public static System.Collections.IEnumerator NFrame(int n)
		{
			for (int i = 0; i < n; i++)
			{
				yield return null;
			}
		}

		/// <summary>
		/// 5フレーム後にF1の結果を返す。
		/// </summary>
		public static System.Collections.IEnumerator F1Async(double x, Action<double> completed)
		{
			for (int i = 0; i < 5; i++)
			{
				yield return null;
			}

			var result = F1(x);
			completed(result);
		}

		/// <summary>
		/// 5フレーム後にF2の結果を返す。
		/// </summary>
		public static System.Collections.IEnumerator F2Async(double x, Action<string> completed)
		{
			for (int i = 0; i < 5; i++)
			{
				yield return null;
			}

			var result = F2(x);
			completed(result);
		}

		/// <summary>
		/// 5フレーム後にF3の結果を返す。
		/// </summary>
		public static System.Collections.IEnumerator F3Async(string s, Action<int> completed)
		{
			for (int i = 0; i < 5; i++)
			{
				yield return null;
			}

			var result = F3(s);
			completed(result);
		}

		/// <summary>
		/// 5フレーム後にNotSupportedExceptionを出す。
		/// </summary>
		public static System.Collections.IEnumerator FErrorAsync()
		{
			for (int i = 0; i < 5; i++)
			{
				yield return null;
			}

			FError();
		}

		/// <summary>
		/// 5フレーム後にNotSupportedExceptionを出す。
		/// </summary>
		public static System.Collections.IEnumerator FErrorAsync(Action<int> campleted)
		{
			for (int i = 0; i < 5; i++)
			{
				yield return null;
			}

			var result = FError();
			campleted(result);
		}

		#endregion
		#region キャンセル機能付き

		/// <summary>
		/// n フレーム後に F1(x)を返すコルーチン。
		/// キャンセル機能付き。
		/// </summary>
		/// <param name="x">入力値。</param>
		/// <param name="n">コルーチン稼働のフレーム数。</param>
		/// <param name="completed">完了時に呼ばれるデリゲート。</param>
		/// <param name="ct">キャンセル用トークン。</param>
		/// <returns></returns>
		public static System.Collections.IEnumerator F1Cancelable(double x, int n, Action<double> completed, CancellationToken ct)
		{
			for (int i = 0; i < n; i++)
			{
				// キャンセルへの対処はあくまでコルーチン側の債務
				// 例外を出して止める。
				ct.ThrowIfCancellationRequested();

				yield return null;
			}

			completed(F1(x));
		}

		#endregion
	}
}
