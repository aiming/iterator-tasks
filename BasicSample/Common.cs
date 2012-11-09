using System;
using Aiming.IteratorTasks;

namespace Sample
{
	class Common
	{
		public static Task ShowFrameTask(int numFrames)
		{
			return new Task(() => ShowFrame(numFrames));
		}

		/// <summary>
		/// 毎フレーム、500ミリ秒スリープして、フレーム数を表示する。
		/// </summary>
		/// <returns></returns>
		public static System.Collections.IEnumerator ShowFrame(int numFrames)
		{
			for (int i = 0; i < numFrames; i++)
			{
				System.Threading.Thread.Sleep(500);
				Console.WriteLine("frame: {0}", i);
				yield return null;
			}
		}
	}
}
