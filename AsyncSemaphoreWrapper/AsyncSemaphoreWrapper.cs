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
	public async ValueTask<Scope> EnterScopeAsync(int millisecondsTimeout = Timeout.Infinite, CancellationToken cancellationToken = default)
	{
		ObjectDisposedException.ThrowIf(_disposed != 0, this);
		await _semaphore.WaitAsync(millisecondsTimeout, cancellationToken);
		return new Scope(this);
	}

	/// <summary>
	/// 下一次退出锁的时候。
	/// </summary>
	/// <returns></returns>
	public Task ExitScopeAsync(CancellationToken cancellationToken = default)
	{
		ObjectDisposedException.ThrowIf(_disposed != 0, this);
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
		if (_waitExitScope.IsValueCreated)
		{
			_waitExitScope.Value.SetResult();
		}
	}

	/// <summary>
	/// 锁域
	/// </summary>
	public struct Scope : IDisposable, IEquatable<Scope>
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
			var waitExitScope = _wrapper._waitExitScope;
			if (waitExitScope.IsValueCreated)
			{
				waitExitScope.Value.SetResult();
			}
			_wrapper._semaphore.Release();
		}

		public override readonly bool Equals(object? obj)
		{
			return obj is Scope scope && Equals(scope);
		}

		public override readonly int GetHashCode()
		{
			return HashCode.Combine(_wrapper, _disposed);
		}

		public static bool operator ==(Scope left, Scope right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Scope left, Scope right)
		{
			return !(left == right);
		}

		public readonly bool Equals(Scope other)
		{
			return other._disposed == _disposed && other._wrapper == _wrapper;
		}
	}
}