using System;

namespace zms9110750Library.Complete;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2007:考虑对等待的任务调用 ConfigureAwait", Justification = "<挂起>")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1003:使用泛型事件处理程序实例", Justification = "<挂起>")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0028:简化集合初始化", Justification = "<挂起>")]

public class StateTransitionTable<TState, TArg> where TArg : notnull
{
	#region 字段
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
	#endregion
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
		StateTriggerType type= default;
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
		return type;
	}
}