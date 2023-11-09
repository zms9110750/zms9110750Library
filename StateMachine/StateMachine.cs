using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using zms9110750Library.Complete;

namespace zms9110750Library.StateMachine;

public sealed class StateMachine<TState>(TState state) : IObservable<Transition<TState>>, IAsyncEnumerable<Transition<TState>>, IDisposable where TState : notnull
{
	#region 字段
	readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
	readonly ConcurrentDictionary<TState, StateConfiguration<TState>> configuration = new ConcurrentDictionary<TState, StateConfiguration<TState>>();
	readonly ObservableAsyncEnumerable<Transition<TState>> observable = new ObservableAsyncEnumerable<Transition<TState>>(new Transition<TState>(state, state, StateTriggerType.NoProcess, null));
	public TState State { get; private set; } = state;
	public bool Disposed => observable.Disposed;
	#endregion
	#region 获取配置
	public StateConfiguration<TState> CurrentConfiguration => this[State];
	public StateConfiguration<TState> this[TState key] => configuration.GetOrAdd(key, static _ => new StateConfiguration<TState>());
	public StateTransitionTable<TState, TArg> Table<TArg>(TState state) where TArg : notnull
	{
		return this[state].Table<TArg>();
	}
	#endregion
	#region 查看和设置层级状态
	public bool IsInState(TState state)
	{
		var target = this[state];
		for (var i = CurrentConfiguration; i != null; i = i.Substate)
		{
			if (i == target)
			{
				return true;
			}
		}
		return false;
	}
	public void SetParentState(TState substate, params TState[] child)
	{
		ArgumentNullException.ThrowIfNull(child);
		foreach (var item in child)
		{
			this[item].Substate = this[substate];
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
			await this[state].ExciteFromAncestors(null);
			observable.Current = new Transition<TState>(State, state, StateTriggerType.Excite, null);
		});
	}
	public async Task Excite<TArg>(TState state, TArg arg) where TArg : notnull
	{
		await ExecuteIfNotDisposed(async () =>
		{
			await this[state].ExciteFromAncestors(arg, null);
			observable.Current = new Transition<TState>(State, state, StateTriggerType.Excite, arg);
		});
	}
	public async Task Transition(TState state)
	{
		await ExecuteIfNotDisposed(async () =>
		{
			var current = State;
			await CurrentConfiguration.Transition(this[state]);
			State = state;
			observable.Current = new Transition<TState>(current, State, StateTriggerType.Transition, null);
		});
	}
	public async Task Consult<TArg>(TArg arg) where TArg : notnull
	{
		await ExecuteIfNotDisposed(async () =>
		{
			var type = CurrentConfiguration.Consult(arg, out var response);
			var current = State;
			switch (type)
			{
				case StateTriggerType.Transition:
					await CurrentConfiguration.Transition(this[response], arg);
					State = response;
					break;
				case StateTriggerType.Excite:
					await this[response].Excite(arg);
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
	public void Dispose()
	{
		if (Disposed)
		{
			return;
		} 
		semaphore.Dispose();
		observable.Dispose();
	}

	public IDisposable Subscribe(IObserver<Transition<TState>> observer) => observable.Subscribe(observer);
	public IAsyncEnumerator<Transition<TState>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => ((IAsyncEnumerable<Transition<TState>>)observable).GetAsyncEnumerator(cancellationToken);
	#endregion
}


