using System;
using System.Collections.Concurrent;
using System.Threading.Channels;
using System.Threading.Tasks.Dataflow;

namespace zms9110750Library;

[Obsolete]
public sealed class ObservableAsyncEnumerable<T>(T current) : IObservable<T>, IObserver<T>, IAsyncEnumerable<T>, IDisposable
{
    #region 字段 
    readonly HashSet<ConcurrentQueue<ValueTask<T>>> _buffer = [];
    readonly HashSet<UnSubscribe> _observers = [];
    readonly HashSet<UnSubscribe> _sources = [];
    readonly SemaphoreSlim _wait = new SemaphoreSlim(0);
    readonly CancellationTokenSource _close = new CancellationTokenSource();
    public bool Disposed => _close.IsCancellationRequested;
    public CancellationToken CancellationToken => _close.Token;
    #endregion
    #region 注册和订阅
    public void Register(IObservable<T> observable)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        ArgumentNullException.ThrowIfNull(observable, nameof(observable));
        _sources.Add(new UnSubscribe(_sources, this, observable.Subscribe(this)));
    }
    public async ValueTask Register(IAsyncEnumerable<T> asyncEnumerable)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        IObserver<T> observer = this;
        var token = CancellationToken;
        try
        {
            await foreach (var item in asyncEnumerable.WithCancellation(token))
            {
                observer.OnNext(item);
            }
        }
        catch (ObjectDisposedException e) when (e.ObjectName == GetType().FullName)
        {
            throw;
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
        ArgumentNullException.ThrowIfNull(observer);
        var obs = new UnSubscribe(_observers, observer);
        _observers.Add(obs);
        return obs;
    }
    #endregion
    #region 观察者方法
    public T Current
    {
        get => current;
        set
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            current = value;
            foreach (var item in _observers)
            {
                item.OnNext(value);
            }
            foreach (var item in _buffer)
            {
                item.Enqueue(ValueTask.FromResult(value));
            }
            ResetWait();
        }
    }
    public void OnError(Exception error)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        foreach (var item in _observers)
        {
            item.OnError(error);
        }
        foreach (var item in _buffer)
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
            _close.Cancel();
            foreach (var item in _sources)
            {
                item.Dispose();
            }
            foreach (var item in _observers)
            {
                item.OnCompleted();
            }
            ResetWait();
            _buffer.Clear();
            _observers.Clear();
            _sources.Clear();
            _close.Dispose();
            _wait.Dispose();
            GC.SuppressFinalize(this);
        }
    }
    void ResetWait()
    {
        if (_buffer.Count > _wait.CurrentCount)
        {
            _wait.Release(_buffer.Count);
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
        var token = CancellationToken;
        ConcurrentQueue<ValueTask<T>> queue = [];
        _buffer.Add(queue);
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
                    await _wait.WaitAsync(token).ConfigureAwait(true);
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
            _buffer.Remove(queue);
            queue.Clear();
        }
    }
    #endregion
    #region 辅助类 
    private readonly struct UnSubscribe(ICollection<UnSubscribe> observers, IObserver<T> observer, IDisposable? disposable = null) : IObserver<T>, IDisposable
    {
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
