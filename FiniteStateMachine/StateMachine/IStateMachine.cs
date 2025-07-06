

using zms9110750Library.StateMachine.Mode;

namespace zms9110750Library.StateMachine.Abstract;

/// <summary>
/// 状态机接口。
/// </summary>
/// <typeparam name="TState">状态类型</typeparam>
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
	/// 判断当前状态是否为指定状态。
	/// </summary>
	/// <param name="state">状态</param> 
	/// <returns>是否为指定状态</returns>
	bool IsInState(TState state);

	/// <summary>
	/// 切换到指定状态。
	/// </summary>
	/// <param name="state">目标状态</param>
	/// <param name="mode">触发模式</param> 
	void Transition(TState state, QueryBehavior mode = QueryBehavior.Transition);

	/// <summary>
	/// 切换到指定状态，同时传递参数。
	/// </summary>
	/// <typeparam name="TArg">参数类型</typeparam>
	/// <param name="arg">参数</param> 
	void Transition<TArg>(TArg arg) where TArg : notnull;

	/// <summary>
	/// 切换到指定状态，同时传递参数和触发模式。
	/// </summary>
	/// <typeparam name="TArg">参数类型</typeparam>
	/// <param name="state">目标状态</param>
	/// <param name="arg">参数</param>
	/// <param name="mode">触发模式</param> 
	void Transition<TArg>(TState state, TArg arg, QueryBehavior mode = QueryBehavior.Transition) where TArg : notnull;
}
