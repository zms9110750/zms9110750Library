using System.Collections.Concurrent; 
namespace zms9110750Library.Obsolete.StateMachine;

public class StateConfiguration<TState>
{
    public event Func<Task>? OnExcite;
    public event Func<Task>? OnEntry;
    public event Func<Task>? OnExit;
    readonly ConcurrentDictionary<Type, object> transitionTable = new ConcurrentDictionary<Type, object>();

    #region 进入
    public Task Entry()
    {
        return OnEntry.WhenAll();
    }
    public async Task Entry<TArg>(TArg arg) where TArg : notnull
    {
        await Entry();
        await Table<TArg>().EntryArg(arg);
    }
    #endregion
    #region 退出
    public Task Exit()
    {
        return OnExit.WhenAll();
    }
    public async Task Exit<TArg>(TArg arg) where TArg : notnull
    {
        await Table<TArg>().ExitArg(arg);
        await Exit();
    }
    #endregion
    #region 激发
    public Task Excite()
    {
        return OnExcite.WhenAll();
    }
    public async Task Excite<TArg>(TArg arg) where TArg : notnull
    {
        await Excite();
        await Table<TArg>().ExciteArg(arg);
    }
    #endregion
    #region 转换表
    public StateTransitionTable<TState, TArg> Table<TArg>() where TArg : notnull
    {
        return (transitionTable.GetOrAdd(typeof(TArg), static _ => new StateTransitionTable<TState, TArg>()) as StateTransitionTable<TState, TArg>)!;
    }
    public StateTriggerType Consult<TArg>(TArg arg, out TState state) where TArg : notnull
    {
        return Table<TArg>().Consult(arg, out state);
    }
    #endregion
}
