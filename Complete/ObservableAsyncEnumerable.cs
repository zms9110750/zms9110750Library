using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace zms9110750Library.Complete;
[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2012:正确使用 ValueTask", Justification = "<挂起>")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0028:简化集合初始化", Justification = "<挂起>")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:使用主构造函数", Justification = "<挂起>")]

public sealed class ObservableAsyncEnumerable<T> : IObservable<T>, IObserver<T>, IDisposable, IAsyncEnumerable<T>
{
	#region 字段 
	readonly HashSet<ConcurrentQueue<ValueTask<T>>> buffer = new HashSet<ConcurrentQueue<ValueTask<T>>>();
	readonly HashSet<UnSubscribe> observers = new HashSet<UnSubscribe>();
	readonly HashSet<UnSubscribe> sources = new HashSet<UnSubscribe>();
	readonly SemaphoreSlim wait = new SemaphoreSlim(0);
	readonly CancellationTokenSource close = new CancellationTokenSource();
	T current;
	public bool Disposed { get; private set; }
	public ObservableAsyncEnumerable(T value)
	{
		current = value;
	}
	#endregion
	#region 注册和订阅
	public void Register(IObservable<T> observable)
	{
		ObjectDisposedException.ThrowIf(Disposed, this);
		ArgumentNullException.ThrowIfNull(observable, nameof(observable));
		sources.Add(new UnSubscribe(sources, this, observable.Subscribe(this)));
	}
	public async ValueTask Register(IAsyncEnumerable<T> asyncEnumerable)
	{
		ObjectDisposedException.ThrowIf(Disposed, this);
		IObserver<T> observer = this;
		var token = close.Token;
		try
		{
			await foreach (var item in asyncEnumerable.WithCancellation(token))
			{
				observer.OnNext(item);
			}
		}
		catch (Exception e)
		{
			observer.OnError(e);
			throw;
		}
		token.ThrowIfCancellationRequested();
	}

	public IDisposable Subscribe(IObserver<T> observer)
	{
		ObjectDisposedException.ThrowIf(Disposed, this);
		var obs = new UnSubscribe(observers, observer);
		observers.Add(obs);
		return obs;
	}
	#endregion
	#region 观察者方法
	public T Current
	{
		get => current; set
		{
			ObjectDisposedException.ThrowIf(Disposed, this);
			current = value;
			foreach (var item in observers)
			{
				item.OnNext(value);
			}
			foreach (var item in buffer)
			{
				item.Enqueue(ValueTask.FromResult(value));
			}
			ResetWait();
		}
	}


	public void OnError(Exception error)
	{
		ObjectDisposedException.ThrowIf(Disposed, this);
		foreach (var item in observers)
		{
			item.OnError(error);
		}
		foreach (var item in buffer)
		{
			item.Enqueue(ValueTask.FromException<T>(error));
		}
		ResetWait();
	}
	void IObserver<T>.OnNext(T value) => Current = value;
	void IObserver<T>.OnCompleted()
	{
		ObjectDisposedException.ThrowIf(Disposed, this);
	}
	#endregion
	#region 释放   
	public void Dispose()
	{
		if (!Disposed)
		{
			close.Cancel();
			foreach (var item in sources)
			{
				item.Dispose();
			}
			foreach (var item in observers)
			{
				item.OnCompleted();
			}
			ResetWait();
			buffer.Clear();
			observers.Clear();
			sources.Clear();
			close.Dispose();
			wait.Dispose();
			Disposed = true;
			GC.SuppressFinalize(this);
		}
	}
	void ResetWait()
	{
		if (buffer.Count > 0)
		{
			wait.Release(buffer.Count);
		}
	}
	#endregion
	#region 迭代器
	async IAsyncEnumerator<T> IAsyncEnumerable<T>.GetAsyncEnumerator(CancellationToken cancellationToken)
	{
		if (Disposed)
		{
			yield break;
		}
		var token = close.Token;
		ConcurrentQueue<ValueTask<T>> queue = new ConcurrentQueue<ValueTask<T>>();
		buffer.Add(queue);
		try
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				if (queue.TryDequeue(out var result))
				{
					yield return await result.ConfigureAwait(true);
				}
				else if (!token.IsCancellationRequested)
				{
					await wait.WaitAsync(token).ConfigureAwait(true);
				}
				else
				{
					break;
				}
			}
			cancellationToken.ThrowIfCancellationRequested();
		}
		finally
		{
			buffer.Remove(queue);
			queue.Clear();
		}
	}
	#endregion
	#region 辅助类 
	private readonly struct UnSubscribe : IObserver<T>, IDisposable
	{
		readonly ICollection<UnSubscribe> observers;
		readonly IObserver<T> observer;
		readonly IDisposable? disposable;

		public UnSubscribe(ICollection<UnSubscribe> observers, IObserver<T> observer, IDisposable? disposable = null)
		{
			this.observers = observers;
			this.observer = observer;
			this.disposable = disposable;
		}
		public void OnNext(T value) => observer.OnNext(value);
		public void OnError(Exception error) => observer.OnError(error);
		public void OnCompleted()
		{
			observer.OnCompleted();
			Dispose();
		}
		public void Dispose()
		{
			disposable?.Dispose();
			observers.Remove(this);
		}
	}
	#endregion
}
