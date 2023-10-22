using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;

namespace zms9110750Library.Complete;
[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2007:考虑对等待的任务调用 ConfigureAwait", Justification = "<挂起>")]
public sealed class StateMachine<TState> : IObservable<Transition<TState>>, IAsyncEnumerable<Transition<TState>>, IDisposable where TState : notnull
{
	#region 字段
	readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
	readonly ConcurrentDictionary<TState, StateConfiguration<TState>> configuration = new ConcurrentDictionary<TState, StateConfiguration<TState>>();
	readonly ObservableAsyncEnumerable<Transition<TState>> observable = new ObservableAsyncEnumerable<Transition<TState>>(default);
	private TState state;
	public bool Disposed { get; private set; }
	public StateMachine(TState state)
	{
		this.state = state;
		observable.Current = new Transition<TState>(state, state, StateTriggerType.Ignore, null);
	}
	#endregion
	#region 获取配置
	public StateConfiguration<TState> CurrentConfiguration => this[state];
	public StateConfiguration<TState> this[TState key] => configuration.GetOrAdd(key, static _ => new StateConfiguration<TState>());
	public StateTransitionTable<TState, TArg> Table<TArg>(TState state) where TArg : notnull
	{
		return this[state].Table<TArg>();
	}
	#endregion
	#region 查看状态
	public bool IsInState(TState state)
	{
		for (var i = this[state]; i != null; i = i.Substate)
		{
			if (i == CurrentConfiguration)
			{
				return true;
			}
		}
		return false;
	}
	#endregion
	#region 转换
	public TState State
	{
		get => state;
		set
		{
			observable.Current = new Transition<TState>(state, value, StateTriggerType.Ignore, null);
			state = value;
		}
	}
	public async Task Excite(TState state)
	{
		if (!Disposed)
		{
			try
			{
				await semaphore.WaitAsync();
				if (!Disposed)
				{
					await this[state].Excite(null);
					observable.Current = new Transition<TState>(this.state, state, StateTriggerType.Excite, null);
				}
			}
			finally
			{
				semaphore.Release();
			}
		}
	}
	public async Task Transition(TState state)
	{
		if (!Disposed)
		{
			try
			{
				await semaphore.WaitAsync();
				if (!Disposed)
				{
					await CurrentConfiguration.Transition(this[state]);
					observable.Current = new Transition<TState>(this.state, state, StateTriggerType.Transition, null);
					this.state = state;
				}
			}
			finally
			{
				semaphore.Release();
			}
		}
	}
	public async Task Transition<TArg>(TArg arg) where TArg : notnull
	{
		if (!Disposed)
		{
			try
			{
				await semaphore.WaitAsync();
				if (!Disposed)
				{
					switch (CurrentConfiguration.Transition(arg, out var state))
					{
						case StateTriggerType.Transition:
							await CurrentConfiguration.Transition(this[state], arg);
							observable.Current = new Transition<TState>(this.state, state, StateTriggerType.Transition, arg);
							this.state = state;
							break;
						case StateTriggerType.Excite:
							await this[state].Excite(arg);
							observable.Current = new Transition<TState>(this.state, state, StateTriggerType.Excite, arg);
							break;
					}
				}
			}
			finally
			{
				semaphore.Release();
			}
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
	#region 接口  
	public void Dispose()
	{
		if (Disposed)
		{
			return;
		}
		Disposed = true;
		semaphore.Wait();
		semaphore.Dispose();
		observable.Dispose();
	}

	public IDisposable Subscribe(IObserver<Transition<TState>> observer) => observable.Subscribe(observer);
	public IAsyncEnumerator<Transition<TState>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => ((IAsyncEnumerable<Transition<TState>>)observable).GetAsyncEnumerator(cancellationToken);
	#endregion
}


