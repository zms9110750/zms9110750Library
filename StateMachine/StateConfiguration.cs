using System.Collections.Concurrent;
using zms9110750Library.Complete;

namespace zms9110750Library.StateMachine;

public class StateConfiguration<TState>
{
	public event Func<Task>? OnExcite;
	public event Func<Task>? OnEntry;
	public event Func<Task>? OnExit;
	readonly TreeNode<StateConfiguration<TState>> tree;
	readonly ConcurrentDictionary<object, object> transitionTable = new ConcurrentDictionary<object, object>();
	public StateConfiguration<TState>? Substate
	{
		get => tree.Parent?.Value;
		set => tree.Parent = value?.tree;
	}
	public StateConfiguration()
	{
		tree = new TreeNode<StateConfiguration<TState>>(this);
	}
	public StateTransitionTable<TState, TArg> Table<TArg>() where TArg : notnull
	{
		return (transitionTable.GetOrAdd(typeof(TArg), static _ => new StateTransitionTable<TState, TArg>()) as StateTransitionTable<TState, TArg>)!;
	}
	static Task WhenAllEvent(Delegate? @delegate)
	{
		return @delegate == null
			? Task.CompletedTask
			: Task.WhenAll(@delegate.GetInvocationList().OfType<Func<Task>>().Select(func => func.Invoke()));
	}
	#region 进入
	public Task Entry()
	{
		return WhenAllEvent(OnEntry);
	}
	public async Task EntryFromAncestors(StateConfiguration<TState>? ancestors)
	{
		if (Substate != null && Substate != ancestors)
		{
			await Substate.EntryFromAncestors(ancestors);
		}
		await Entry();
	}
	public async Task Entry<TArg>(TArg arg) where TArg : notnull
	{
		await Entry();
		await Table<TArg>().EntryArg(arg);
	}
	public async Task EntryFromAncestors<TArg>(TArg arg, StateConfiguration<TState>? ancestors) where TArg : notnull
	{
		if (Substate != null && Substate != ancestors)
		{
			await Substate.EntryFromAncestors(arg, ancestors);
		}
		await Entry(arg);
	}
	#endregion
	#region 退出
	public Task Exit()
	{
		return WhenAllEvent(OnExit);
	}
	public async Task ExitToAncestors(StateConfiguration<TState>? ancestors)
	{
		if (Substate != null && Substate != ancestors)
		{
			await Substate.ExitToAncestors(ancestors);
		}
		await Exit();
	}
	public async Task Exit<TArg>(TArg arg) where TArg : notnull
	{
		await Table<TArg>().ExitArg(arg);
		await Exit();
	}
	public async Task ExitToAncestors<TArg>(TArg arg, StateConfiguration<TState>? ancestors) where TArg : notnull
	{
		await Exit(arg);
		if (Substate != null && Substate != ancestors)
		{
			await Substate.ExitToAncestors(arg, ancestors);
		}
	}
	#endregion
	#region 激发
	public Task Excite()
	{
		return WhenAllEvent(OnExcite);
	}
	public async Task ExciteFromAncestors(StateConfiguration<TState>? ancestors)
	{
		if (Substate != null && Substate != ancestors)
		{
			await Substate.ExciteFromAncestors(ancestors);
		}
		await Excite();
	}
	public async Task Excite<TArg>(TArg arg) where TArg : notnull
	{
		await Excite();
		await Table<TArg>().ExciteArg(arg);
	}
	public async Task ExciteFromAncestors<TArg>(TArg arg, StateConfiguration<TState>? ancestors) where TArg : notnull
	{
		if (Substate != null && Substate != ancestors)
		{
			await Substate.ExciteFromAncestors(arg, ancestors);
		}
		await Excite(arg);
	}
	#endregion
	#region 转换
	public async Task Transition(StateConfiguration<TState> target)
	{
		ArgumentNullException.ThrowIfNull(target, nameof(target));
		StateConfiguration<TState>? ancestor = tree.AncestorCommon(target.tree)?.Value;
		await ExitToAncestors(ancestor);
		await target.EntryFromAncestors(ancestor);
	}

	public async Task Transition<TArg>(StateConfiguration<TState> target, TArg arg) where TArg : notnull
	{
		ArgumentNullException.ThrowIfNull(target, nameof(target));
		StateConfiguration<TState>? ancestor = tree.AncestorCommon(target.tree)?.Value;
		await ExitToAncestors(arg, ancestor);
		await target.EntryFromAncestors(arg, ancestor);
	}
	public StateTriggerType Consult<TArg>(TArg arg, out TState state) where TArg : notnull
	{
		var type = Table<TArg>().Consult(arg, out state);
		return type == StateTriggerType.Unregistered && Substate != null ? Substate.Consult(arg, out state)
			: type;
	}
	#endregion
}
