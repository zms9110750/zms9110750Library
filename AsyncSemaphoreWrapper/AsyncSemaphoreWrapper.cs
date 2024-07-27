﻿using System.Runtime.CompilerServices;

namespace zms9110750Library.StateMachine;
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
	private Lazy<TaskCompletionSource<bool>> _waitExitScope = new Lazy<TaskCompletionSource<bool>>();
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
	public async ValueTask<Scope> EnterScopeAsync(int millisecondsTimeout = Timeout.Infinite, CancellationToken cancellationToken = default)
	{
		ObjectDisposedException.ThrowIf(_disposed != 0, this);
		await _semaphore.WaitAsync(millisecondsTimeout, cancellationToken);
		return new Scope(this);
	}

	/// <summary>
	/// 下一次退出锁的时候。
	/// </summary>
	/// <returns>退出锁时已经释放</returns>
	public Task<bool> ExitScopeAsync(CancellationToken cancellationToken = default)
	{
		ObjectDisposedException.ThrowIf(_disposed != 0, this);
		return cancellationToken == default ? _waitExitScope.Value.Task.WaitAsync(cancellationToken) : _waitExitScope.Value.Task;
	}


	public async ValueTask DisposeAsync()
	{
		if (Interlocked.Exchange(ref _disposed, 1) != 0)
		{
			return;
		}
		await _semaphore.WaitAsync();
		_semaphore.Dispose();
	}

	/// <summary>
	/// 锁域
	/// </summary>
	public struct Scope : IDisposable
	{
		private int _disposed;
		AsyncSemaphoreWrapper _wrapper;
		internal Scope(AsyncSemaphoreWrapper wrapper)
		{
			_wrapper = wrapper;
		}
		public void Dispose()
		{
			if (Interlocked.Exchange(ref _disposed, 1) != 0)
			{
				return;
			}
			if (_wrapper._waitExitScope.IsValueCreated)
			{
				_wrapper._waitExitScope.Value.SetResult(_wrapper.IsDisposed);
				_wrapper._waitExitScope = new Lazy<TaskCompletionSource<bool>>();
			}
			_wrapper._semaphore.Release();

		}
	}
}