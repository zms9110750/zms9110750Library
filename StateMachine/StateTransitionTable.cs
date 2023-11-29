using System;

namespace zms9110750Library.StateMachine;


public class StateTransitionTable<TState, TArg> where TArg : notnull
{
    #region 字段
    readonly Dictionary<TArg, (TState, StateTriggerType)> reserveTable = [];
    readonly LinkedList<Func<TArg, (TState, StateTriggerType)>> dynamicTable = [];
    public event Func<TArg, Task>? OnEntryFrom;
    public event Func<TArg, Task>? OnExitFrom;
    public event Func<TArg, Task>? OnExciteFrom;
    #endregion
    #region 执行委托 
    public Task EntryArg(TArg arg)
    {
        return OnEntryFrom.WhenAll(arg);
    }
    public Task ExitArg(TArg arg)
    {
        return OnExitFrom.WhenAll(arg);
    }
    public Task ExciteArg(TArg arg)
    {
        return OnExciteFrom.WhenAll(arg);
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