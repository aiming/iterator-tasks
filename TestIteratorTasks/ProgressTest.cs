using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Aiming.IteratorTasks;

namespace TestIteratorTasks
{
	[TestClass]
	public class ProgressTest
	{
		[TestMethod]
		public void Progressでは_Reportが呼ばれるたびにProgressChangedイベントが起こる()
		{
			var progress = new Progress<int>();
			var reportedItems = new List<int>();

			progress.ProgressChanged += i =>
			{
				reportedItems.Add(i);
			};

			var t = new Task<int>(c => 進捗報告付きのコルーチン(c, progress));

			var runner = new SampleTaskRunner.TaskRunner();
			t.Start(runner);

			for (int i = 0; i < 100; i++)
			{
				Assert.AreEqual(i, reportedItems.Count);
				runner.Update();
				Assert.AreEqual(i + 1, reportedItems.Count);
				Assert.AreEqual(i, reportedItems.Last());
			}
		}

		static System.Collections.IEnumerator 進捗報告付きのコルーチン(Action<int> completed, IProgress<int> progress)
		{
			for (int i = 0; i < 100; i++)
			{
				progress.Report(i);
				yield return null;
			}

			completed(100);
		}
	}
}
