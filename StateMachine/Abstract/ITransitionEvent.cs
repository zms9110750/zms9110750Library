namespace zms9110750Library.StateMachine.Abstract;
/// <summary>
/// 转换事件接口
/// </summary>
public interface ITransitionEvent
{
	/// <summary>
	/// 进入事件
	/// </summary>
	public event Func<ValueTask>? OnEntry;

	/// <summary>
	/// 退出事件
	/// </summary>
	public event Func<ValueTask>? OnExit;

	#region 进入
	/// <summary>
	/// 进入事件
	/// </summary>
	/// <returns>事件执行完毕</returns>
	public ValueTask TransitionEntryAsync();

	/// <summary>
	/// 进入事件
	/// </summary>
	/// <typeparam name="TArg">参数类型</typeparam>
	/// <param name="arg">参数</param>
	/// <returns>事件执行完毕</returns>
	public ValueTask TransitionEntryAsync<TArg>(TArg arg) where TArg : notnull;
	#endregion

	#region 退出
	/// <summary>
	/// 退出事件
	/// </summary>
	/// <returns>事件执行完毕</returns>
	public ValueTask TransitionExitAsync();

	/// <summary>
	/// 退出事件
	/// </summary>
	/// <typeparam name="TArg">参数类型</typeparam>
	/// <param name="arg">参数</param>
	/// <returns>事件执行完毕</returns>
	public ValueTask TransitionExitAsync<TArg>(TArg arg) where TArg : notnull;

	#endregion
}
/// <summary>
/// 表示一个转换事件的接口
/// </summary>
/// <typeparam name="TArg">参数类型</typeparam>
interface ITransitionEvent<TArg> where TArg : notnull
{
	/// <summary>
	/// 进入事件
	/// </summary>
	public event Func<TArg, ValueTask>? OnEntryFrom;
	/// <summary>
	/// 退出事件
	/// </summary>
	public event Func<TArg, ValueTask>? OnExitFrom;

	/// <summary>
	/// 进入事件
	/// </summary>
	/// <param name="arg">参数</param>
	/// <returns>事件执行完毕</returns>
	public ValueTask TransitionEntryAsync(TArg arg);

	/// <summary>
	/// 退出事件
	/// </summary>
	/// <param name="arg">参数</param>
	/// <returns>事件执行完毕</returns>
	public ValueTask TransitionExitAsync(TArg arg);
}
