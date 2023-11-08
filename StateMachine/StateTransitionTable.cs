using System;

namespace zms9110750Library.StateMachine;


public class StateTransitionTable<TState, TArg> where TArg : notnull
{
	#region 字段
	readonly Dictionary<TArg, (TState, StateTriggerType)> reserveTable = new Dictionary<TArg, (TState, StateTriggerType)>();
	readonly LinkedList<Func<TArg, (TState, StateTriggerType)>> dynamicTable = new LinkedList<Func<TArg, (TState, StateTriggerType)>>();
	public event Func<TArg, Task>? OnEntryFrom;
	public event Func<TArg, Task>? OnExitFrom;
	public event Func<TArg, Task>? OnExciteFrom;
	#endregion
	#region 执行委托
	static Task WhenAllEvent(Delegate? @delegate, TArg arg)
	{
		return @delegate == null
			? Task.CompletedTask
			: Task.WhenAll(@delegate.GetInvocationList().OfType<Func<TArg, Task>>().Select(func => func.Invoke(arg)));
	}
	public Task EntryArg(TArg arg)
	{
		return WhenAllEvent(OnEntryFrom, arg);
	}
	public Task ExitArg(TArg arg)
	{
		return WhenAllEvent(OnExitFrom, arg);
	}
	public Task ExciteArg(TArg arg)
	{
		return WhenAllEvent(OnExciteFrom, arg);
	}
	#endregion
	#region 注册和撤销
	public void Register(TArg arg, TState state, StateTriggerType type)
	{
		if (type == StateTriggerType.Unregistered)
		{
			throw new ArgumentException($"{nameof(StateTriggerType.Unregistered)}应由状态机报告，不应将其注册进转换表");
		}
		else if (type == StateTriggerType.Ignore)
		{
			throw new ArgumentException($"{nameof(StateTriggerType.Ignore)}仅适用于附参数的动态表。");
		}
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
		StateTriggerType type = StateTriggerType.Unregistered;
		foreach (var func in dynamicTable)
		{
			(state, type) = func.Invoke(arg);
			if (type == StateTriggerType.Unregistered)
			{
				throw new ArgumentException($"动态转换表中查询出了{nameof(StateTriggerType.Unregistered)}。此类型应由状态机报告，不应将其注册进转换表。");
			}
			else if (type != StateTriggerType.Ignore)
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
		return StateTriggerType.Unregistered;
	}
}