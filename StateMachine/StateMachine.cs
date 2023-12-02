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
    public async void SetState(TState value)
    {
        await ExecuteIfNotDisposed(() =>
             {
                 var current = State;
                 State = value;
                 observable.Current = new Transition<TState>(current, value, StateTriggerType.NoProcess, null);
                 return Task.CompletedTask;
             });
    }
    public async Task Excite(TState state)
    {
        await ExecuteIfNotDisposed(async () =>
        {
            var target = tree!.GetValueOrDefault(state, null);
            foreach (var item in (null | target).Select(n => n.Value).DefaultIfEmpty(state))
            {
                await this[item].Excite();
            }
            observable.Current = new Transition<TState>(State, state, StateTriggerType.Excite, null);
        });
    }
    public async Task Excite<TArg>(TState state, TArg arg) where TArg : notnull
    {
        await ExecuteIfNotDisposed(async () =>
        {
            var target = tree!.GetValueOrDefault(state, null);
            foreach (var item in (null | target).Select(n => n.Value).DefaultIfEmpty(state))
            {
                await this[item].Excite(arg);
            }
            observable.Current = new Transition<TState>(State, state, StateTriggerType.Excite, arg);
        });
    }
    public async Task Transition(TState state)
    {
        await ExecuteIfNotDisposed(async () =>
        {
            var old = State;
            var target = tree!.GetValueOrDefault(state, null);
            var current = tree!.GetValueOrDefault(State, null);
            var ancestor = target & current;
            foreach (var item in (current | ancestor).Select(n => n.Value).DefaultIfEmpty(State))
            {
                await this[item].Exit();
            }
            foreach (var item in (ancestor | target).Select(n => n.Value).DefaultIfEmpty(state))
            {
                await this[item].Entry();
            }
            State = state;
            observable.Current = new Transition<TState>(old, State, StateTriggerType.Transition, null);
        });
    }
    public async Task Transition<TArg>(TState state, TArg arg) where TArg : notnull
    {
        await ExecuteIfNotDisposed(async () =>
        {
            var old = State;
            var target = tree!.GetValueOrDefault(state, null);
            var current = tree!.GetValueOrDefault(State, null);
            var ancestor = target & current;
            foreach (var item in (current | ancestor).Select(n => n.Value).DefaultIfEmpty(State))
            {
                await this[item].Exit(arg);
            }
            foreach (var item in (ancestor | target).Select(n => n.Value).DefaultIfEmpty(state))
            {
                await this[item].Entry(arg);
            }
            State = state;
            observable.Current = new Transition<TState>(old, State, StateTriggerType.Transition, null);
        });
    }
    public async Task Consult<TArg>(TArg arg) where TArg : notnull
    {
        await ExecuteIfNotDisposed(async () =>
        {
            StateTriggerType type = StateTriggerType.Unregistered;
            TState response;
            var old = State;
            var temp = State;
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
                    var target = tree!.GetValueOrDefault(response, null);
                    var current = tree!.GetValueOrDefault(State, null);
                    var ancestor = target & current;
                    foreach (var item in (current | ancestor).Select(n => n.Value).DefaultIfEmpty(State))
                    {
                        await this[item].Exit(arg);
                    }
                    foreach (var item in (ancestor | target).Select(n => n.Value).DefaultIfEmpty(response))
                    {
                        await this[item].Entry(arg);
                    }
                    State = response;
                    break;
                case StateTriggerType.Excite:
                    target = tree!.GetValueOrDefault(response, null);
                    foreach (var item in (null | target).Select(n => n.Value).DefaultIfEmpty(response))
                    {
                        await this[item].Excite(arg);
                    }
                    break;
                case StateTriggerType.NoProcess:
                    State = response;
                    break;
                default:
                    response = old;
                    break;
            }
            observable.Current = new Transition<TState>(old, response, type, arg);
        });
    }

    #endregion
    #region Help
    async Task ExecuteIfNotDisposed(Func<Task> action)
    {
        if (Disposed)
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
        }
        else
        {
            try
            {
                await semaphore.WaitAsync(observable.CancellationToken);
                if (!Disposed)
                {
                    await action();
                }
            }
            finally
            {
                if (!Disposed)
                {
                    semaphore.Release();
                }
            }
        }
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
        await ExecuteIfNotDisposed(() =>
        {
            semaphore.Dispose();
            observable.Dispose();
            return Task.CompletedTask;
        });
    }
    #endregion
}


