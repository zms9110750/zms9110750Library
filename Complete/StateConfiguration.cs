using System.Collections.Concurrent;

namespace zms9110750Library.Complete;
[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2007:考虑对等待的任务调用 ConfigureAwait", Justification = "<挂起>")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1003:使用泛型事件处理程序实例", Justification = "<挂起>")]

public class StateConfiguration<TState>
{
	public event Func<Task> OnExcite;
	public event Func<Task> OnEntry;
	public event Func<Task> OnExit;
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
		OnExcite += PreventNull;
		OnEntry += PreventNull;
		OnExit += PreventNull;
	}
	static Task PreventNull()
	{
		return Task.CompletedTask;
	}
	public StateTransitionTable<TState, TArg> Table<TArg>() where TArg : notnull
	{
		return (transitionTable.GetOrAdd(typeof(TArg), static _ => new StateTransitionTable<TState, TArg>()) as StateTransitionTable<TState, TArg>)!;
	}
	#region 进入
	public async Task Entry()
	{
		if (OnEntry == null)
		{
			return;
		}
		await Task.WhenAll(OnEntry.GetInvocationList().OfType<Func<Task>>().Select(func => func.Invoke()));
	}
	async Task Entry(StateConfiguration<TState>? ancestorCommon)
	{
		if (Substate != null && Substate != ancestorCommon)
		{
			await Substate.Entry(ancestorCommon);
		}
		await Entry();
	}
	public async Task Entry<TArg>(TArg arg) where TArg : notnull
	{
		await Entry();
		await Table<TArg>().EntryArg(arg);
	}
	async Task Entry<TArg>(TArg arg, StateConfiguration<TState>? ancestorCommon) where TArg : notnull
	{
		if (Substate != null && Substate != ancestorCommon)
		{
			await Substate.Entry(arg, ancestorCommon);
		}
		await Entry(arg);
	}
	#endregion
	#region 退出
	public async Task Exit()
	{
		await Task.WhenAll(OnExit.GetInvocationList().OfType<Func<Task>>().Select(func => func.Invoke()));
	}
	async Task Exit(StateConfiguration<TState>? ancestorCommon)
	{
		if (Substate != null && Substate != ancestorCommon)
		{
			await Substate.Exit(ancestorCommon);
		}
		await Exit();
	}
	public async Task Exit<TArg>(TArg arg) where TArg : notnull
	{
		await Table<TArg>().ExitArg(arg);
		await Exit();
	}
	async Task Exit<TArg>(TArg arg, StateConfiguration<TState>? ancestorCommon) where TArg : notnull
	{
		await Exit(arg);
		if (Substate != null && Substate != ancestorCommon)
		{
			await Substate.Exit(arg, ancestorCommon);
		}
	}
	#endregion
	#region 激发
	public async Task Excite()
	{
		await Task.WhenAll(OnExcite.GetInvocationList().OfType<Func<Task>>().Select(func => func.Invoke()));
	}
	public async Task Excite(StateConfiguration<TState>? ancestorCommon)
	{
		if (Substate != null && Substate != ancestorCommon)
		{
			await Substate.Excite(ancestorCommon);
		}
		await Excite();
	}
	public async Task Excite<TArg>(TArg arg) where TArg : notnull
	{
		await Excite();
		await Table<TArg>().ExciteArg(arg);
	}
	public async Task Excite<TArg>(TArg arg, StateConfiguration<TState>? ancestorCommon) where TArg : notnull
	{
		if (Substate != null && Substate != ancestorCommon)
		{
			await Substate.Entry(arg, ancestorCommon);
		}
		await Excite(arg);
	}
	#endregion
	#region 转换
	public async Task Transition(StateConfiguration<TState> target)
	{
		ArgumentNullException.ThrowIfNull(target, nameof(target));
		StateConfiguration<TState>? ancestor = tree.AncestorCommon(target.tree)?.Value;
		await Exit(ancestor);
		await target.Entry(ancestor);
	}

	public async Task Transition<TArg>(StateConfiguration<TState> target, TArg arg) where TArg : notnull
	{
		ArgumentNullException.ThrowIfNull(target, nameof(target));
		StateConfiguration<TState>? ancestor = tree.AncestorCommon(target.tree)?.Value;
		await Exit(arg, ancestor);
		await target.Entry(arg, ancestor);
	}
	public StateTriggerType Transition<TArg>(TArg arg, out TState state) where TArg : notnull
	{
		var type = Table<TArg>().Consult(arg, out state);
		return type == StateTriggerType.Ignore && Substate != null ? Substate.Transition(arg, out state) : type;
	}
	#endregion
	#region 注册和撤销
	public void Register<TArg>(TArg arg, TState state, StateTriggerType type) where TArg : notnull
	{
		Table<TArg>().Register(arg, state, type);
	}
	public void Revoke<TArg>(TArg arg) where TArg : notnull
	{
		Table<TArg>().Revoke(arg);
	}
	public LinkedListNode<Func<TArg, (TState, StateTriggerType)>> Register<TArg>(Func<TArg, (TState, StateTriggerType)> func) where TArg : notnull
	{
		return Table<TArg>().Register(func);
	}
	public void Revoke<TArg>(LinkedListNode<Func<TArg, (TState, StateTriggerType)>> node) where TArg : notnull
	{
		Table<TArg>().Revoke(node);
	}
	#endregion
}
