
namespace Aiming.IteratorTasks
{
	public enum TaskStatus
	{
		/// <summary>
		/// 実行前。
		/// </summary>
		Created,

		/// <summary>
		/// 実行中。
		/// </summary>
		Running,

		/// <summary>
		/// 正常終了。
		/// </summary>
		RanToCompletion,

		/// <summary>
		/// キャンセルされた。
		/// </summary>
		Canceled,

		/// <summary>
		/// 例外が出て止まった。
		/// </summary>
		Faulted,
	}
}
