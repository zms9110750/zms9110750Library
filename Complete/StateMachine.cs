using System.Collections.Concurrent;

namespace zms9110750Library.Complete;
[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2007:考虑对等待的任务调用 ConfigureAwait", Justification = "<挂起>")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:使用主构造函数", Justification = "<挂起>")]
public sealed class StateMachine<TState> : IDisposable where TState : notnull
{
	readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
	readonly ConcurrentDictionary<TState, StateConfiguration<TState>> configuration = new ConcurrentDictionary<TState, StateConfiguration<TState>>();
	public TState State { get; set; }

	public StateMachine(TState state)
	{
		State = state;
	}
	public StateConfiguration<TState> Current => this[State];
	public StateConfiguration<TState> this[TState key] => configuration.GetOrAdd(key, static _ => new StateConfiguration<TState>());
	public StateTransitionTable<TState, TArg> Table<TArg>(TState state) where TArg : notnull
	{
		return this[state].Table<TArg>();
	}
	public bool IsOnState(TState state)
	{
		for (var i = this[state]; i != null; i = i.Substate)
		{
			if (i == Current)
			{
				return true;
			}
		}
		return false;
	}
	#region 转换
	public async Task Excite(TState state)
	{
		try
		{
			await semaphore.WaitAsync();
			await this[state].Excite(null);
		}
		finally
		{
			semaphore.Release();
		}

	}
	public async Task Transition(TState state)
	{
		try
		{
			await semaphore.WaitAsync();
			await Current.Transition(this[state]);
			State = state;
		}
		finally
		{
			semaphore.Release();
		}
	}
	public async Task Transition<TArg>(TArg arg) where TArg : notnull
	{
		try
		{
			await semaphore.WaitAsync();
			switch (Current.Transition(arg, out var state))
			{
				case StateTriggerType.Transition:
					await Current.Transition(this[state], arg);
					State = state;
					break;
				case StateTriggerType.Excite:
					await this[state].Excite(arg);
					break;
			}
		}
		finally
		{
			semaphore.Release();
		}
	}
	#endregion
	#region 注册和撤销
	public void Register<TArg>(TState state, TArg arg, StateTriggerType type) where TArg : notnull
	{
		this[state].Register(arg, state, type);
	}
	public void Revoke<TArg>(TState state, TArg arg) where TArg : notnull
	{
		this[state].Revoke(arg);
	}
	public LinkedListNode<Func<TArg, (TState, StateTriggerType)>> Register<TArg>(TState state, Func<TArg, (TState, StateTriggerType)> func) where TArg : notnull
	{
		return this[state].Register(func);
	}
	public void Revoke<TArg>(TState state, LinkedListNode<Func<TArg, (TState, StateTriggerType)>> node) where TArg : notnull
	{
		this[state].Revoke(node);
	}
	#endregion
	public void Dispose() => ((IDisposable)semaphore).Dispose();
}
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
		StateConfiguration<TState>? ancestor = tree.AncestorCommon(target.tree)?.Value;
		await Exit(ancestor);
		await target.Entry(ancestor);
	}
	public async Task Transition<TArg>(StateConfiguration<TState> target, TArg arg) where TArg : notnull
	{
		StateConfiguration<TState>? ancestor = tree.AncestorCommon(target.tree)?.Value;
		await Exit(arg, ancestor);
		await target.Entry(arg, ancestor);
	}
	public StateTriggerType Transition<TArg>(TArg arg, out TState state) where TArg : notnull
	{
		return Table<TArg>().Consult(arg, out state);
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
[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2007:考虑对等待的任务调用 ConfigureAwait", Justification = "<挂起>")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1003:使用泛型事件处理程序实例", Justification = "<挂起>")]
public class StateTransitionTable<TState, TArg> where TArg : notnull
{
	public event Func<TArg, Task> OnEntryFrom;
	public event Func<TArg, Task> OnExitFrom;
	public event Func<TArg, Task> OnExciteFrom;
	readonly Dictionary<TArg, (TState, StateTriggerType)> reserveTable = new Dictionary<TArg, (TState, StateTriggerType)>();
	readonly LinkedList<Func<TArg, (TState, StateTriggerType)>> dynamicTable = new LinkedList<Func<TArg, (TState, StateTriggerType)>>();
	public StateTransitionTable()
	{
		OnEntryFrom += PreventNull;
		OnExitFrom += PreventNull;
		OnExciteFrom += PreventNull;
	}
	static Task PreventNull(TArg arg)
	{
		return Task.CompletedTask;
	}
	#region 执行委托
	public async Task EntryArg(TArg arg)
	{
		await Task.WhenAll(OnEntryFrom.GetInvocationList().OfType<Func<TArg, Task>>().Select(func => func.Invoke(arg)));
	}
	public async Task ExitArg(TArg arg)
	{
		await Task.WhenAll(OnExitFrom.GetInvocationList().OfType<Func<TArg, Task>>().Select(func => func.Invoke(arg)));
	}
	public async Task ExciteArg(TArg arg)
	{
		await Task.WhenAll(OnExciteFrom.GetInvocationList().OfType<Func<TArg, Task>>().Select(func => func.Invoke(arg)));
	}
	#endregion
	#region 注册和撤销
	public void Register(TArg arg, TState state, StateTriggerType type)
	{
		reserveTable[arg] = (state, type);
	}
	public void Revoke(TArg arg)
	{
		reserveTable.Remove(arg);
	}
	public LinkedListNode<Func<TArg, (TState, StateTriggerType)>> Register(Func<TArg, (TState, StateTriggerType)> func)
	{
		return dynamicTable.AddFirst(func);
	}
	public void Revoke(LinkedListNode<Func<TArg, (TState, StateTriggerType)>> node)
	{
		dynamicTable.Remove(node);
	}
	#endregion
	public StateTriggerType Consult(TArg arg, out TState state)
	{
		StateTriggerType type;
		foreach (var func in dynamicTable)
		{
			(state, type) = func.Invoke(arg);
			if (type != StateTriggerType.Ignore)
			{
				return type;
			}
		}
		if (reserveTable.TryGetValue(arg, out var result))
		{
			(state, type) = result;
			return type;
		}
		state = default!;
		return default;
	}
}
public enum StateTriggerType
{
	Ignore,
	Transition,
	Excite
}