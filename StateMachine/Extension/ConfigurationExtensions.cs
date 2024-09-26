using System.ComponentModel;

namespace zms9110750Library.StateMachine.Extension;

/// <summary>
/// 配置扩展类
/// </summary>
static class ConfigurationExtensions
{

	#region 事件同时启动

	/// <summary>
	/// 扩展方法，获取委托的调用列表
	/// </summary>
	/// <typeparam name="T">委托类型</typeparam>
	/// <param name="func">委托实例</param>
	/// <returns>委托调用列表</returns>
	public static IEnumerable<T> EnumInvocationList<T>(this T? func) where T : Delegate
	{
		return func?.GetInvocationList().OfType<T>() ?? [];
	}

	/// <summary>
	/// 当所有任务完成时，返回一个 ValueTask
	/// </summary>
	/// <typeparam name="TArg">参数类型</typeparam>
	/// <param name="fun">函数委托</param>
	/// <param name="arg">参数</param>
	/// <returns>表示所有任务完成的 ValueTask</returns>
	public static ValueTask WhenAll<TArg>(this Func<TArg, ValueTask>? fun, TArg arg)
	{
		var arr = fun.EnumInvocationList()
			.Select(s => s.Invoke(arg))
			.Where(s => !s.IsCompleted)
			.Select(s => s.AsTask())
			.ToArray();
		return arr.Length == 0 ? ValueTask.CompletedTask : new ValueTask(Task.WhenAll(arr));
	}

	/// <summary>
	/// 当所有任务完成时，返回一个 ValueTask
	/// </summary>
	/// <param name="fun">无参数的函数委托</param>
	/// <returns>表示所有任务完成的 ValueTask</returns>
	public static ValueTask WhenAll(this Func<ValueTask>? fun)
	{
		var arr = fun.EnumInvocationList()
			.Select(s => s.Invoke())
			.Where(s => !s.IsCompleted)
			.Select(s => s.AsTask())
			.ToArray();
		return arr.Length == 0 ? ValueTask.CompletedTask : new ValueTask(Task.WhenAll(arr));
	}
	#endregion
}