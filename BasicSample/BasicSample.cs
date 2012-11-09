using System;
using Aiming.IteratorTasks;

namespace Sample
{
	/// <summary>
	/// 3つのタスクを同時に動かす例。
	/// </summary>
	class BasicSample
	{
		public static void Run()
		{
			var runner = new SampleTaskRunner.TaskRunner();

			Common.ShowFrameTask(50).Start(runner);

			new Task<string>(Worker1)
				.OnComplete(t => Console.WriteLine("Worker 1 Done: " + t.Result))
				.Start(runner);

			new Task<int>(Worker2)
				.OnComplete(t => Console.WriteLine("Worker 2 Done: " + t.Result))
				.Start(runner);

			runner.Update(200);
		}

		/// <summary>
		/// 30フレーム掛けて何かやった体で、文字列を返すコルーチン。
		/// 
		/// 1フレームに処理が集中しないように分割して実行するイメージ。
		/// </summary>
		/// <param name="completed"></param>
		/// <returns></returns>
		private static System.Collections.IEnumerator Worker1(Action<string> completed)
		{
			Console.WriteLine("Start Worker 1");

			for (int i = 0; i < 30; i++)
			{
				yield return null;
			}

			completed("Result");
		}

		/// <summary>
		/// 3秒掛けて何かやった体で、数値を返すコルーチン。
		/// 
		/// スレッドを立ててスリープしている部分を、時間がかかる計算や、ネットワーク待ちに置き換えて考えていただけると。
		/// </summary>
		/// <param name="completed"></param>
		/// <returns></returns>
		private static System.Collections.IEnumerator Worker2(Action<int> completed)
		{
			bool done = false;
			int result = 0;

			System.Threading.ThreadPool.QueueUserWorkItem(state =>
			{
				System.Threading.Thread.Sleep(3000);
				result = 999;
				done = true;
			});

			Console.WriteLine("Start Worker 2");

			while (!done)
				yield return null;

			completed(result);
		}
	}
}
