namespace zms9110750Library.Wrapper;
/// <summary>
/// 异步锁包装器
/// </summary>
/// <remarks><code>
/// using var scope = await _lock.EnterScopeAsync();
/// </code></remarks>
/// <param name="initialCount">同时可进入的线程数</param>
public sealed class AsyncSemaphoreWrapper(int initialCount = 1) : IAsyncDisposable
{
	private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(initialCount, initialCount);
	private Lazy<TaskCompletionSource> _waitExitScope = new Lazy<TaskCompletionSource>();

	private int _disposed;
	/// <summary>
	/// 已经释放了
	/// </summary>
	public bool IsDisposed => _disposed != 0;

	/// <summary>
	/// 等待之前的任务执行完。然后获取域
	/// </summary>
	/// <returns>域</returns>
	/// <remarks>域是<seealso cref="IDisposable"/>，释放它以允许下一个访问者进入。</remarks>
	public async Task<IDisposable> EnterScopeAsync(int millisecondsTimeout = Timeout.Infinite, CancellationToken cancellationToken = default)
	{
		ObjectDisposedException.ThrowIf(IsDisposed, this);
		await _semaphore.WaitAsync(millisecondsTimeout, cancellationToken);
		return new Scope(this);
	}

	/// <summary>
	/// 下一次退出锁的时候。
	/// </summary>
	/// <returns></returns>
	public Task ExitScopeAsync(CancellationToken cancellationToken = default)
	{
		ObjectDisposedException.ThrowIf(IsDisposed, this);
		var waitExitScope = _waitExitScope;
		if (waitExitScope.IsValueCreated && waitExitScope.Value.Task.IsCompleted)
		{
			lock (waitExitScope)
			{
				if (_waitExitScope == waitExitScope)
				{
					_waitExitScope = new Lazy<TaskCompletionSource>();
				}
			}
		}
		return cancellationToken == default ? _waitExitScope.Value.Task : _waitExitScope.Value.Task.WaitAsync(cancellationToken);
	}

	public async ValueTask DisposeAsync()
	{
		if (Interlocked.Exchange(ref _disposed, 1) != 0)
		{
			return;
		}
		await _semaphore.WaitAsync();
		_semaphore.Dispose();
		if (_waitExitScope.IsValueCreated)
		{
			_waitExitScope.Value.SetResult();
		}
	}

	/// <summary>
	/// 锁域
	/// </summary> 
	struct Scope(AsyncSemaphoreWrapper wrapper) : IDisposable
	{
		private int _disposed;

		public void Dispose()
		{
			if (Interlocked.Exchange(ref _disposed, 1) != 0)
			{
				return;
			}
			var waitExitScope = wrapper._waitExitScope;
			if (waitExitScope.IsValueCreated)
			{
				waitExitScope.Value.SetResult();
			}
			wrapper._semaphore.Release();
		}
	}
}
