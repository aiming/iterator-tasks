using System;
using Aiming.IteratorTasks;

namespace Sample
{
	/// <summary>
	/// 継続処理のサンプル。
	/// 
	/// 同期処理とイテレーター非同期処理の対応関係を説明。
	/// U F(T x) → IEnumerator FAsync(T x, Action&lt;U&lt;);
	/// F2(F1(x)) → new Task&lt;U&lt;(c => F1Async(x, c).ContinueWith&lt;V&lt;(F2Async);
	/// </summary>
	class ContinuationSample
	{
		public static void Run()
		{
			var x = 1.41421356;

			// 同期処理
			var result = F3(F2(F1(x)));
			Console.WriteLine("同期処理の結果: " + result);

			// イテレーター非同期処理
			var task = new Task<double>(c => F1Async(x, c))
				.ContinueWith<string>(F2Async)
				.ContinueWith<int>(F3Async)
				.OnComplete(t => Console.WriteLine("非同期処理の結果: " + t.Result));

			var runner = new SampleTaskRunner.TaskRunner();

			task.Start(runner);
			Common.ShowFrameTask(50).Start(runner);

			runner.Update(20);
		}

		#region 同期処理

		// 中身には深い意味なし。
		// 単に、F3(F2(F1(x))) みたいに繋ぎたいだけ。

		private static double F1(double x)
		{
			return x * x;
		}

		private static string F2(double x)
		{
			return x.ToString();
		}

		private static int F3(string s)
		{
			return s.Length;
		}

		#endregion
		#region イテレーター非同期処理

		/// <summary>
		/// 5フレーム後にF1の結果を返す。
		/// </summary>
		private static System.Collections.IEnumerator F1Async(double x, Action<double> completed)
		{
			for (int i = 0; i < 5; i++)
			{
				yield return null;
			}

			Console.WriteLine("F1Async 終了");
			var result = F1(x);
			completed(result);
		}

		/// <summary>
		/// 5フレーム後にF2の結果を返す。
		/// </summary>
		private static System.Collections.IEnumerator F2Async(double x, Action<string> completed)
		{
			for (int i = 0; i < 5; i++)
			{
				yield return null;
			}

			Console.WriteLine("F2Async 終了");
			var result = F2(x);
			completed(result);
		}

		/// <summary>
		/// 5フレーム後にF3の結果を返す。
		/// </summary>
		private static System.Collections.IEnumerator F3Async(string s, Action<int> completed)
		{
			for (int i = 0; i < 5; i++)
			{
				yield return null;
			}

			Console.WriteLine("F3Async 終了");
			var result = F3(s);
			completed(result);
		}

		#endregion
	}
}
