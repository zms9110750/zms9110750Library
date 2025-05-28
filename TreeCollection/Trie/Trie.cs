using System.Collections.Specialized;

namespace zms9110750.TreeCollection.Trie;

public class Trie(HashSet<char>? separator = null) : TrieBase
{
	private BitVector32 _token = new BitVector32();
	private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
	internal override IReadOnlySet<char> Separator { get; } = separator ?? DefaultSeparator;
	static IReadOnlySet<char> DefaultSeparator { get; } = new HashSet<char>();
	protected bool this[int index]
	{
		get
		{
			_lock.EnterReadLock();
			try
			{
				return _token[1 << index];
			}
			finally
			{
				_lock.ExitReadLock();
			}
		}
		set
		{
			_lock.EnterWriteLock();
			try
			{
				_token[1 << index] = value;
				Thread.MemoryBarrier();
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}
	}
	public void Add(string word)
	{
		ArgumentException.ThrowIfNullOrEmpty(word);
		base[word[0]].Add(word, 0);
	}
	public IEnumerable<string> Search(string s)
	{
		if (string.IsNullOrEmpty(s))
			yield break;

		int tokenIndex = AllocateTokenIndex();
		try
		{
			foreach (var child in Children.Values)
			{
				foreach (var result in child.Search(s, 0, tokenIndex))
				{
					yield return result;
				}
			}
		}
		finally
		{
			ReleaseToken(tokenIndex); // 根节点负责释放
		}
	}
	private int AllocateTokenIndex()
	{
		for (int retry = 0; retry < 1024; retry++)
		{
			if (_token.Data == -1)
			{
				Thread.Yield();
				continue;
			}
			for (int i = 0; i < 32; i++)
			{
				if (!this[i])
				{
					// 升级为写锁进行修改
					try
					{
						_lock.EnterUpgradeableReadLock();
						if (!_token[1 << i])
						{
							this[i] = true;
							return i;
						}
					}
					finally
					{
						_lock.ExitUpgradeableReadLock();
					}
				}
			}
		}
		throw new InvalidOperationException("No available token index.");
	}

	internal override void ReleaseToken(int tokenIndex)
	{
		base.ReleaseToken(tokenIndex);
		this[tokenIndex] = false;
	}
}
