using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Aiming.IteratorTasks;

namespace TestIteratorTasks
{
	[TestClass]
	public class TaskTest
	{

		[TestMethod]
		public void Nフレーム実行するイテレーターが丁度NフレームIsCompleted_falseになってることを確認()
		{
			const int N = 50;

			var runnner = new SampleTaskRunner.TaskRunner();
			var task = Coroutines.NFrameTask(N);
			task.Start(runnner);

			for (int i = 0; i < 2 * N; i++)
			{
				runnner.Update();

				if (i < N)
					Assert.IsFalse(task.IsCompleted);
				else
					Assert.IsTrue(task.IsCompleted);
			}
		}

		[TestMethod]
		public void Task_Tで正常終了するとResultに結果が入る()
		{
			var x = 10;
			var y = Coroutines.F1(x);

			var task = new Task<double>(c => Coroutines.F1Async(x, c))
				.OnComplete(t => Assert.AreEqual(t.Result, y));

			var runnner = new SampleTaskRunner.TaskRunner();
			task.Start(runnner);
			runnner.Update(10);

			Assert.AreEqual(y, task.Result);
		}

		[TestMethod]
		public void 一度完了したタスク_何度でも結果が取れる()
		{
			var x = 10;
			var y = Coroutines.F1(x);

			var task = new Task<double>(c => Coroutines.F1Async(x, c));

			var runnner = new SampleTaskRunner.TaskRunner();
			task.Start(runnner);
			runnner.Update(10);

			Assert.AreEqual(y, task.Result);
			Assert.AreEqual(y, task.Result);
			Assert.AreEqual(y, task.Result);
			Assert.AreEqual(y, task.Result);
		}

		[TestMethod]
		public void タスク完了時にOnCompleteが呼ばれる()
		{
			var runnner = new SampleTaskRunner.TaskRunner();

			var x = 10;
			var y = Coroutines.F1(x);

			bool called = false;

			var task = new Task<double>(c => Coroutines.F1Async(x, c));
			task.OnComplete(t => called = true);
			task.Start(runnner);

			Assert.IsFalse(called);

			runnner.Update(10);

			Assert.IsTrue(called);
		}

		[TestMethod]
		public void 完了済みのタスクでOnCompleteすると_即座にコールバックが呼ばれる()
		{
			var runnner = new SampleTaskRunner.TaskRunner();

			var x = 10;
			var y = Coroutines.F1(x);

			var task = new Task<double>(c => Coroutines.F1Async(x, c));
			task.Start(runnner);
			runnner.Update(10);

			Assert.IsTrue(task.IsCompleted);

			bool called = false;

			task.OnComplete(t => called = true);

			Assert.IsTrue(called);
		}

		[TestMethod]
		public void 開始前_実行中_正常終了_エラー終了_キャンセルされた_がわかる()
		{
			var x = 10.0;
			var task = new Task<double>(c => Coroutines.F1Async(x, c));

			Assert.AreEqual(TaskStatus.Created, task.Status);

			var runnner = new SampleTaskRunner.TaskRunner();
			task.Start(runnner);

			runnner.Update();
			Assert.AreEqual(TaskStatus.Running, task.Status);

			runnner.Update(10);
			Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);

			var errorTask = new Task(Coroutines.FErrorAsync);
			Assert.AreEqual(TaskStatus.Created, errorTask.Status);

			errorTask.Start(runnner);
			runnner.Update();
			Assert.AreEqual(TaskStatus.Running, errorTask.Status);

			runnner.Update(10);
			Assert.AreEqual(TaskStatus.Faulted, errorTask.Status);
		}

		[TestMethod]
		public void 実行途中のタスクを再スタートしようとしたら例外を出す()
		{
			var x = 10.0;
			var task = new Task<double>(c => Coroutines.F1Async(x, c));

			Assert.AreEqual(TaskStatus.Created, task.Status);

			var runnner = new SampleTaskRunner.TaskRunner();
			task.Start(runnner);

			runnner.Update();
			Assert.AreEqual(TaskStatus.Running, task.Status);

			try
			{
				task.Start(runnner);
			}
			catch (InvalidOperationException)
			{
				return;
			}

			Assert.Fail();
		}

		[TestMethod]
		public void タスク中で例外が出たらErrorプロパティに例外が入る()
		{
			var task = new Task<int>(Coroutines.FErrorAsync)
				.OnComplete(t =>
				{
					Assert.IsTrue(t.IsFaulted);
					Assert.IsNotNull(t.Error);
				})
				.ContinueWith(呼ばれてはいけない);

			var runner = new SampleTaskRunner.TaskRunner();
			task.Start(runner);
			runner.Update(20);
		}

		[TestMethod]
		public void タスク中で例外が出たらOnErrorが呼ばれる_特定の型の例外だけ拾う()
		{
			var notSupportedCalled = false;
			var outOfRangeCalled = false;

			var task = new Task(Coroutines.FErrorAsync)
				.OnError<NotSupportedException>(e => notSupportedCalled = true)
				.OnError<IndexOutOfRangeException>(e => outOfRangeCalled = true);

			var runner = new SampleTaskRunner.TaskRunner();
			task.Start(runner);
			runner.Update(20);

			Assert.IsTrue(notSupportedCalled);
			Assert.IsFalse(outOfRangeCalled);
		}

		[TestMethod]
		public void タスク中で例外が出たときにResultをとろうとすると例外再スロー()
		{
			var task = new Task<int>(Coroutines.FErrorAsync)
				.OnComplete(t =>
				{
					Assert.IsTrue(t.IsFaulted);

					try
					{
						var result = t.Result;
					}
					catch
					{
						return;
					}
					Assert.Fail();
				})
				.ContinueWith(呼ばれてはいけない);

			var runner = new SampleTaskRunner.TaskRunner();
			task.Start(runner);
			runner.Update(20);
		}

		[TestMethod]
		public void ContinueWithで継続処理を実行できる()
		{
			var x = 10.0;
			var x1 = Coroutines.F1(x);
			var x2 = Coroutines.F2(x1);
			var x3 = Coroutines.F3(x2);

			var task = new Task<double>(c => Coroutines.F1Async(x, c))
				.OnComplete(t => Assert.AreEqual(t.Result, x1))
				.ContinueWith<string>(Coroutines.F2Async)
				.OnComplete(t => Assert.AreEqual(t.Result, x2))
				.ContinueWith<int>(Coroutines.F3Async)
				.OnComplete(t => Assert.AreEqual(t.Result, x2))
				;

			var runner = new SampleTaskRunner.TaskRunner();
			task.Start(runner);

			runner.Update(20);
		}

		[TestMethod]
		public void ContinueWithは前段が正常終了したときにだけ呼ばれる()
		{
			var onCompletedCalled = false;

			var task = new Task(Coroutines.FErrorAsync)
				.OnComplete(t =>
					{
						Assert.IsTrue(t.IsFaulted);
						onCompletedCalled = true;
					})
				.ContinueWith(呼ばれてはいけない);

			var runner = new SampleTaskRunner.TaskRunner();
			task.Start(runner);
			runner.Update(20);

			Assert.IsTrue(onCompletedCalled);
		}

		private System.Collections.IEnumerator 呼ばれてはいけない()
		{
			Assert.Fail();
			yield return null;
		}

		[TestMethod]
		public void OnCompleteは_直前のタスク完了時_エラーも正常終了も_どちらも呼ばれる()
		{
			var errorTaskCalled = false;
			var normalTaskCalled = false;

			var normalTask = new Task(() => Coroutines.NFrame(5))
				.OnComplete(t => normalTaskCalled = true);
			var errorTask = new Task<int>(Coroutines.FErrorAsync)
				.OnComplete(t => errorTaskCalled = true);

			var runner = new SampleTaskRunner.TaskRunner();
			errorTask.Start(runner);
			normalTask.Start(runner);
			runner.Update(20);

			Assert.IsTrue(normalTaskCalled);
			Assert.IsTrue(errorTaskCalled);
		}

		[TestMethod]
		public void WhenAllでタスクの並行動作できる()
		{
			var t1 = new Task(() => Coroutines.NFrame(3));
			var t2 = new Task(() => Coroutines.NFrame(5));
			var t3 = new Task(() => Coroutines.NFrame(7));

			var task = Task.WhenAllTask(t1, t2, t3)
				.OnComplete(t =>
				{
					Assert.IsTrue(t1.IsCompleted);
					Assert.IsTrue(t2.IsCompleted);
					Assert.IsTrue(t3.IsCompleted);
				});

			var runner = new SampleTaskRunner.TaskRunner();
			task.Start(runner);

			runner.Update(20);

			Assert.IsTrue(task.IsCompleted);
		}
	}
}
