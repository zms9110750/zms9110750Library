using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using zms9110750Library.Complete;

namespace zms9110750Library.StateMachine;

public sealed class StateMachine<TState>(TState state) : IObservable<Transition<TState>>, IAsyncEnumerable<Transition<TState>>, IAsyncDisposable where TState : notnull
{
    #region 字段
    readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
    readonly ConcurrentDictionary<TState, StateConfiguration<TState>> configuration = new ConcurrentDictionary<TState, StateConfiguration<TState>>();
    readonly ConcurrentDictionary<TState, TreeNode<TState>> tree = new ConcurrentDictionary<TState, TreeNode<TState>>();
    readonly ObservableAsyncEnumerable<Transition<TState>> observable = new ObservableAsyncEnumerable<Transition<TState>>(new Transition<TState>(state, state, StateTriggerType.NoProcess, null));
    public TState State { get; private set; } = state;
    public bool Disposed => observable.Disposed;
    #endregion
    #region 获取配置 
    public StateConfiguration<TState> CurrentConfiguration => this[State];
    public StateConfiguration<TState> this[TState state] => configuration.GetOrAdd(state, _ => new StateConfiguration<TState>());
    public StateTransitionTable<TState, TArg> Table<TArg>(TState state) where TArg : notnull
    {
        return this[state].Table<TArg>();
    }
    #endregion
    #region 查看和设置层级状态
    public bool IsInState(TState state)
    {
        return tree.TryGetValue(state, out var target)
            && tree.TryGetValue(State, out var current)
            && (current & target) == target;
    }
    public void SetChildState(TState substate, params TState[] child)
    {
        ArgumentNullException.ThrowIfNull(child);
        var target = tree.GetOrAdd(substate, key => new TreeNode<TState>(key));
        foreach (var item in child)
        {
            tree.GetOrAdd(item, key => new TreeNode<TState>(key)).Parent = item.Equals(substate) ? null : target;
        }
    }

    #endregion
    #region 转换 
    public async Task SetState(TState value, bool waitRemaining = true)
    {
        if (await CompleteRemain(waitRemaining))
        {
            return;
        }
        var current = State;
        State = value;
        observable.Current = new Transition<TState>(current, value, StateTriggerType.NoProcess, null);
        if (waitRemaining)
        {
            semaphore.Release();
        }
    }
    public async Task Excite(TState state, bool waitRemaining = true)
    {
        if (await CompleteRemain(waitRemaining))
        {
            return;
        }
        var target = tree!.GetValueOrDefault(state, null);
        foreach (var item in (null | target).Select(n => n.Value).DefaultIfEmpty(state))
        {
            await this[item].Excite();
        }
        observable.Current = new Transition<TState>(State, state, StateTriggerType.Excite, null);
        if (waitRemaining)
        {
            semaphore.Release();
        }
    }
    public async Task Excite<TArg>(TState state, TArg arg, bool waitRemaining = true) where TArg : notnull
    {
        if (await CompleteRemain(waitRemaining))
        {
            return;
        }
        var target = tree!.GetValueOrDefault(state, null);
        foreach (var item in (null | target).Select(n => n.Value).DefaultIfEmpty(state))
        {
            await this[item].Excite(arg);
        }
        observable.Current = new Transition<TState>(State, state, StateTriggerType.Excite, arg);
        if (waitRemaining)
        {
            semaphore.Release();
        }
    }
    public async Task Transition(TState state, bool waitRemaining = true)
    {
        if (await CompleteRemain(waitRemaining))
        {
            return;
        }
        var old = State;
        var target = tree!.GetValueOrDefault(state, null);
        var current = tree!.GetValueOrDefault(State, null);
        var ancestor = target & current;
        State = state;
        foreach (var item in (current | ancestor).Select(n => n.Value).DefaultIfEmpty(State))
        {
            await this[item].Exit();
        }
        foreach (var item in (ancestor | target).Select(n => n.Value).DefaultIfEmpty(state))
        {
            await this[item].Entry();
        } 
        observable.Current = new Transition<TState>(old, State, StateTriggerType.Transition, null);
        if (waitRemaining)
        {
            semaphore.Release();
        }
    }
    public async Task Transition<TArg>(TState state, TArg arg, bool waitRemaining = true) where TArg : notnull
    {
        if (await CompleteRemain(waitRemaining))
        {
            return;
        }
        var old = State;
        var target = tree!.GetValueOrDefault(state, null);
        var current = tree!.GetValueOrDefault(State, null);
        var ancestor = target & current;
        State = state;
        foreach (var item in (current | ancestor).Select(n => n.Value).DefaultIfEmpty(State))
        {
            await this[item].Exit(arg);
        }
        foreach (var item in (ancestor | target).Select(n => n.Value).DefaultIfEmpty(state))
        {
            await this[item].Entry(arg);
        } 
        observable.Current = new Transition<TState>(old, State, StateTriggerType.Transition, null);
        if (waitRemaining)
        {
            semaphore.Release();
        }
    }
    public async Task Consult<TArg>(TArg arg, bool waitRemaining = true) where TArg : notnull
    {
        if (await CompleteRemain(waitRemaining))
        {
            return;
        }
        var temp = State;
        TState response;
        StateTriggerType type;
        do
        {
            type = this[temp].Consult(arg, out response);
            if (tree.TryGetValue(temp, out var node) && node.Parent != null)
            {
                temp = node.Parent.Value;
            }
            else
            {
                break;
            }
        } while (type == StateTriggerType.Unregistered);
        switch (type)
        {
            case StateTriggerType.Transition:
                await Transition(response, arg, false);
                break;
            case StateTriggerType.Excite:
                await Excite(response, arg, false);
                break;
            case StateTriggerType.NoProcess:
                await SetState(response, false);
                break;
            default:
                observable.Current = new Transition<TState>(State, State, type, arg);
                break;
        }
        if (waitRemaining)
        {
            semaphore.Release();
        }
    }

    #endregion
    #region Help
    async Task<bool> CompleteRemain(bool waitRemaining)
    {
        if (Disposed)
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
        }
        if (waitRemaining)
        {
            await semaphore.WaitAsync();
        }
        return Disposed;
    }
    #endregion
    #region 接口    
    public IDisposable Subscribe(IObserver<Transition<TState>> observer) => observable.Subscribe(observer);
    public IAsyncEnumerator<Transition<TState>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => ((IAsyncEnumerable<Transition<TState>>)observable).GetAsyncEnumerator(cancellationToken);
    public async ValueTask DisposeAsync()
    {
        if (Disposed)
        {
            return;
        }
        await CompleteRemain(true);
        observable.Dispose();
        semaphore.Release();
        semaphore.Dispose();
    }
    #endregion 
}