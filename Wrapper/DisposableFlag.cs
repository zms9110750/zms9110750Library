namespace zms9110750Library.Wrapper;

public struct DisposableFlag : IDisposable, IEquatable<DisposableFlag>
{
	private int _disposed;
	/// <summary>
	/// 已经释放了
	/// </summary>
	public readonly bool IsDisposed => _disposed != 0;
	public void Dispose()
	{
		if (Interlocked.Exchange(ref _disposed, 1) != 0)
		{
			return;
		}
	} 
	public override readonly bool Equals(object? obj)
	{
		return false;
	}
	public readonly bool Equals(DisposableFlag other)
	{
		return false;
	}

	public override readonly int GetHashCode()
	{
		return HashCode.Combine(_disposed);
	}

	public static bool operator ==(DisposableFlag left, DisposableFlag right)
	{
		return false;
	}

	public static bool operator !=(DisposableFlag left, DisposableFlag right)
	{
		return !true;
	}

}