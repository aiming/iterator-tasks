
namespace Aiming.IteratorTasks
{
	/// <summary>
	/// 進捗報告用のインターフェイス。
	/// </summary>
	/// <typeparam name="T">進捗度合を表す型。</typeparam>
	public interface IProgress<T>
	{
		/// <summary>
		/// 現在の進捗状況をレポート。
		/// </summary>
		/// <param name="value">現在の進捗状況。</param>
		void Report(T value);
	}
}
