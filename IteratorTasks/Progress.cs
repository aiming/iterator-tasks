using System;

namespace Aiming.IteratorTasks
{
	/// <summary>
	/// イベントを使って進捗報告を受け取るためのクラス。
	/// </summary>
	/// <typeparam name="T">進捗度合を表す型。</typeparam>
	public class Progress<T> : IProgress<T>
	{
		public Progress() { }

		public Progress(Action<T> onProgressChanged) { ProgressChanged += onProgressChanged; }

		/// <summary>
		/// 進捗状況が変化したときに起こすイベント。
		/// </summary>
		public event Action<T> ProgressChanged;

		void IProgress<T>.Report(T value)
		{
			var d = ProgressChanged;
			if (d != null) d(value);
		}
	}
}
