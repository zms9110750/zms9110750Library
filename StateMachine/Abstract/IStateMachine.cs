using zms9110750Library.StateMachine.Mode;

namespace zms9110750Library.StateMachine.Abstract;
public interface IStateMachine<TState> where TState : notnull
{
	/// <summary>
	/// 获取指定状态的配置。
	/// </summary>
	/// <param name="state">状态</param>
	/// <returns>对应状态的配置</returns>
	public ITransitionEvent this[TState state] { get; }

	/// <summary>
	/// 获取当前配置。
	/// </summary>
	public ITransitionEvent CurrentConfiguration { get; }

	/// <summary>
	/// 获取当前状态。
	/// </summary>
	TState State { get; }

	/// <summary>
	/// 切换到指定状态。
	/// </summary>
	/// <param name="state">目标状态</param>
	/// <param name="mode">触发模式</param>
	/// <returns>异步任务</returns>
	ValueTask TransitionAsync(TState state, TriggerMode mode = TriggerMode.Transition);

	/// <summary>
	/// 切换到指定状态，同时传递参数。
	/// </summary>
	/// <typeparam name="TArg">参数类型</typeparam>
	/// <param name="arg">参数</param>
	/// <returns>异步任务</returns>
	ValueTask TransitionAsync<TArg>(TArg arg) where TArg : notnull;

	/// <summary>
	/// 切换到指定状态，同时传递参数和触发模式。
	/// </summary>
	/// <typeparam name="TArg">参数类型</typeparam>
	/// <param name="state">目标状态</param>
	/// <param name="arg">参数</param>
	/// <param name="mode">触发模式</param>
	/// <returns>异步任务</returns>
	ValueTask TransitionAsync<TArg>(TState state, TArg arg, TriggerMode mode = TriggerMode.Transition) where TArg : notnull;
}
