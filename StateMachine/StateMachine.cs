using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using zms9110750Library.Complete;

namespace zms9110750Library.StateMachine;

public sealed class StateMachine<TState>(TState state) : IObservable<Transition<TState>>, IAsyncEnumerable<Transition<TState>>, IAsyncDisposable where TState : notnull
{
    #region 字段
    readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
    readonly ConcurrentDictionary<TState, TreeNode<StateConfiguration<TState>>> configuration = new ConcurrentDictionary<TState, TreeNode<StateConfiguration<TState>>>();
    readonly ObservableAsyncEnumerable<Transition<TState>> observable = new ObservableAsyncEnumerable<Transition<TState>>(new Transition<TState>(state, state, StateTriggerType.NoProcess, null));
    readonly TreeNode<StateConfiguration<TState>> tree = new TreeNode<StateConfiguration<TState>>(new StateConfiguration<TState>());
    public TState State { get; private set; } = state;
    public bool Disposed => observable.Disposed;
    #endregion
    #region 获取配置 
    public StateConfiguration<TState> CurrentConfiguration => this[State];
    public StateConfiguration<TState> this[TState state] => GetTreeNode(state).Value;
    public StateTransitionTable<TState, TArg> Table<TArg>(TState state) where TArg : notnull
    {
        return this[state].Table<TArg>();
    }
    TreeNode<StateConfiguration<TState>> GetTreeNode(TState state)
    {
        return configuration.GetOrAdd(state, _ => tree.AddLast(new StateConfiguration<TState>())[0]);
    }
    #endregion
    #region 查看和设置层级状态
    public bool IsInState(TState state)
    {
        TreeNode<StateConfiguration<TState>>? target = GetTreeNode(state);
        var current = GetTreeNode(State);
        return (current & target) == target;
    }
    public void SetChildState(TState substate, params TState[] child)
    {
        ArgumentNullException.ThrowIfNull(child);
        var target = GetTreeNode(substate);
        foreach (var item in child)
        {
            GetTreeNode(item).Parent = target;
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
            foreach (var item in tree | GetTreeNode(state))
            {
                await item!.Value.Excite();
            }
            observable.Current = new Transition<TState>(State, state, StateTriggerType.Excite, null);
        });
    }
    public async Task Excite<TArg>(TState state, TArg arg) where TArg : notnull
    {
        await ExecuteIfNotDisposed(async () =>
        {
            foreach (var item in tree | GetTreeNode(state))
            {
                await item!.Value.Excite(arg);
            }
            observable.Current = new Transition<TState>(State, state, StateTriggerType.Excite, arg);
        });
    }
    public async Task Transition(TState state)
    {
        await ExecuteIfNotDisposed(async () =>
        {
            var current = State;
            var ancestor = GetTreeNode(State) & GetTreeNode(state);
            foreach (var item in GetTreeNode(State) | ancestor)
            {
                await item!.Value.Exit();
            }
            foreach (var item in ancestor | GetTreeNode(state))
            {
                await item!.Value.Entry();
            }
            State = state;
            observable.Current = new Transition<TState>(current, State, StateTriggerType.Transition, null);
        });
    }
    public async Task Consult<TArg>(TArg arg) where TArg : notnull
    {
        await ExecuteIfNotDisposed(async () =>
        {
            StateTriggerType type = StateTriggerType.Unregistered;
            TState response;
            var current = State;
            var node = GetTreeNode(State);
            do
            {
                type = node.Value.Consult(arg, out response);
                node = node.Parent;
            } while (node != null && type == StateTriggerType.Unregistered);
            switch (type)
            {
                case StateTriggerType.Transition:
                    var ancestor = GetTreeNode(State) & GetTreeNode(response);
                    foreach (var item in GetTreeNode(State) | ancestor)
                    {
                        await item!.Value.Exit(arg);
                    }
                    foreach (var item in ancestor | GetTreeNode(response))
                    {
                        await item!.Value.Entry(arg);
                    }
                    State = response;
                    break;
                case StateTriggerType.Excite:
                    foreach (var item in tree | GetTreeNode(response))
                    {
                        await item!.Value.Excite(arg);
                    }
                    break;
                case StateTriggerType.NoProcess:
                    State = response;
                    break;
                default:
                    response = current;
                    break;
            }
            observable.Current = new Transition<TState>(current, response, type, arg);
        });
    }
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


