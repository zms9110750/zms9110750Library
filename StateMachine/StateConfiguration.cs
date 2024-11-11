
using Microsoft.Extensions.DependencyInjection;
using zms9110750Library.StateMachine.Abstract;
using zms9110750Library.StateMachine.Extension;
using zms9110750Library.StateMachine.Mode;
using static System.Formats.Asn1.AsnWriter;

namespace zms9110750Library.StateMachine;
public class StateConfiguration<TState>(IServiceScope scope) : ITransitionEvent
{
	public event Func<ValueTask>? OnEntry;
	public event Func<ValueTask>? OnExit;

	public ValueTask TransitionEntryAsync() { return OnEntry.WhenAll(); }
	public ValueTask TransitionEntryAsync<TArg>(TArg arg) where TArg : notnull { return scope.ServiceProvider.GetRequiredService<StateTransitionTable<TState, TArg>>().TransitionEntryAsync(arg); }
	public ValueTask TransitionExitAsync() { return OnExit.WhenAll(); }
	public ValueTask TransitionExitAsync<TArg>(TArg arg) where TArg : notnull { return scope.ServiceProvider.GetRequiredService<StateTransitionTable<TState, TArg>>().TransitionExitAsync(arg); }

	/// <summary>
	/// 查验当前参数如何转换
	/// </summary>
	/// <typeparam name="TArg">参数类型</typeparam>
	/// <param name="arg">参数</param>
	/// <param name="state">目标状态</param>
	/// <returns>转换方式</returns>
	/// 
	public TriggerMode Consult<TArg>(TArg arg, out TState state) where TArg : notnull
	{
		return scope.ServiceProvider.GetRequiredService<StateTransitionTable<TState, TArg>>().Consult(arg, out state);
	}
}
