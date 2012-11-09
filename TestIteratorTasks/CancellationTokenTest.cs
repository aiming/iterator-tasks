using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Aiming.IteratorTasks;

namespace TestIteratorTasks
{
	[TestClass]
	public class CancellationTokenTest
	{
		[TestMethod]
		public void キャンセルトークンを渡しても_Cancelを呼ばなければ正常終了()
		{
			var x = 10;
			var runner = new SampleTaskRunner.TaskRunner();

			var t = new Task<double>(c => Coroutines.F1Cancelable(x, 20, c, CancellationToken.None));
	
			t.Start(runner);
			while (!t.IsCompleted)
				runner.Update();

			Assert.AreEqual(Coroutines.F1(x), t.Result);
		}

		[TestMethod]
		public void キャンセルしたときにOperationCanceld例外発生()
		{
			var x = 10;
			var runner = new SampleTaskRunner.TaskRunner();

			var cts = new CancellationTokenSource();
			var t = new Task<double>(c => Coroutines.F1Cancelable(x, 20, c, cts.Token));

			t.Start(runner);
			runner.Update(5);
			cts.Cancel();

			// 次の1回の実行でタスクが終わるはず
			runner.Update();

			// この場合は IsCanceled にならない
			Assert.IsTrue(t.IsFaulted);
			Assert.AreEqual(typeof(TaskCanceledException), t.Error.Exceptions.Single().GetType());
		}

		[TestMethod]
		public void TaskのForceCancelで強制的にタスクを止めたときはOnCompleteも呼ばれない()
		{
			var x = 10;
			var runner = new SampleTaskRunner.TaskRunner();

			var cts = new CancellationTokenSource();
			var t = new Task<double>(c => Coroutines.F1Cancelable(x, 20, c, cts.Token));

			t.OnComplete(_ =>
			{
				Assert.Fail();
			});

			t.Start(runner);
			runner.Update(5);
			t.ForceCancel();

			runner.Update();

			// この場合は IsCanceled に
			Assert.IsTrue(t.IsCanceled);
		}

		[TestMethod]
		public void TaskにCancellationTokenSourceを渡しておいて_TaskのCancelメソッド経由でキャンセルできる()
		{
			var x = 10;
			var runner = new SampleTaskRunner.TaskRunner();

			var cts = new CancellationTokenSource();
			var t = new Task<double>(c => Coroutines.F1Cancelable(x, 20, c, cts.Token));

			t.Cancellation = cts;

			t.Start(runner);
			runner.Update(5);
			t.Cancel(); // Task.Cancel の中で1度 MoveNext して、即座にキャンセル処理が動くようにする

			// 挙動自体は cts.Cancel(); と同じ
			Assert.IsTrue(t.IsFaulted);
			Assert.AreEqual(typeof(TaskCanceledException), t.Error.Exceptions.Single().GetType());
		}

		[TestMethod]
		public void Cancel時にRegisterで登録したデリゲートが呼ばれる()
		{
			var runner = new SampleTaskRunner.TaskRunner();

			{
				// キャンセルしない場合
				var cts = new CancellationTokenSource();
				var t = new Task<string>(c => Cancelで戻り値が切り替わるコルーチン(10, c, cts.Token));
				t.Start(runner);
				runner.Update(20);

				Assert.IsTrue(t.IsCompleted);
				Assert.AreEqual(CompletedMessage, t.Result);
			}

			{
				// キャンセルする場合
				var cts = new CancellationTokenSource();
				var t = new Task<string>(c => Cancelで戻り値が切り替わるコルーチン(10, c, cts.Token));
				t.Start(runner);
				runner.Update(5);
				cts.Cancel();
				runner.Update(5);

				Assert.IsTrue(t.IsCompleted);
				Assert.AreEqual(CanceledMessage, t.Result);
			}
		}

		const string CompletedMessage = "最後まで実行された時の戻り値";
		const string CanceledMessage = "キャンセルされた時の戻り値";

		static System.Collections.IEnumerator Cancelで戻り値が切り替わるコルーチン(int n, Action<string> completed, CancellationToken ct)
		{
			var message = CompletedMessage;

			ct.Register(() =>
			{
				message = CanceledMessage;
			});

			for (int i = 0; i < n; i++)
			{
				if (ct.IsCancellationRequested)
					break;

				yield return null;
			}

			completed(message);
		}
	}
}
