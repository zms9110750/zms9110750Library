using zms9110750Library.StateMachine.Abstract;
using zms9110750Library.StateMachine.Mode;

namespace zms9110750Library.StateMachine;
public class HierarchicalStateMachine<TState>(TState initialState, IHierarchyStatusQuery<TState> statusQuery) : StateMachine<TState>(initialState) where TState : notnull
{
	/// <summary>
	/// 检查给定状态是否处于某状态下
	/// </summary>
	/// <param name="state">要检查的状态。</param>
	/// <returns>如果状态匹配，返回 true；否则返回 false。</returns>
	public bool IsInState(TState state)
	{
		return statusQuery.TryGetCommonAncestor(State, state, out var common) && common.Equals(state);
	}

	protected override async ValueTask Transition<TArg>(TState state, TriggerMode mode, TArg arg = default!, bool useArg = false)
	{
		ObjectDisposedException.ThrowIf(IsDisposed, this);
		if (!mode.HasFlag(TriggerMode.Intercept))
		{
			return;
		}
		var old = State;
		statusQuery.TryGetCommonAncestor(old, state, out var ancestor);
		if (mode.HasFlag(TriggerMode.StateSwitchFlag))
		{
			State = state;
		}
		if (mode.HasFlag(TriggerMode.OnExit))
		{
			if (useArg)
			{
				foreach (var item in statusQuery.GetLeftToRootPath(old, ancestor))
				{
					await this[item].TransitionExitAsync(arg);
				}
			}
			else
			{
				foreach (var item in statusQuery.GetLeftToRootPath(old, ancestor))
				{
					await this[item].TransitionExitAsync();
				}
			}
		}
		if (mode.HasFlag(TriggerMode.OnEntry))
		{
			if (useArg)
			{
				foreach (var item in statusQuery.GetRootToRightPath(ancestor, state))
				{
					await this[item].TransitionEntryAsync(arg);
				}
			}
			else
			{
				foreach (var item in statusQuery.GetRootToRightPath(ancestor, state))
				{
					await this[item].TransitionEntryAsync();
				}
			}
		}
	}
}
