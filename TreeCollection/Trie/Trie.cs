using System.Collections.Specialized;

namespace zms9110750.TreeCollection.Trie;

/// <summary>
/// 字典树根节点
/// </summary>
/// <param name="separator">分隔符集合</param>
public class Trie(HashSet<char>? separator = null) : TrieBase
{
	private BitVector32 _token = new BitVector32();
	private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
	internal override IReadOnlySet<char> Separator { get; } = separator ?? DefaultSeparator;
	static IReadOnlySet<char> DefaultSeparator { get; } = new HashSet<char>();

	/// <summary>
	/// 获取<see cref="_token"/>指定bit位的值
	/// </summary>
	/// <param name="index">索引</param>
	bool this[int index]
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

	/// <inheritdoc/>

	public override void Add(string word)
	{
		ArgumentException.ThrowIfNullOrEmpty(word);
		base[word[0]].Add(word );
	}

	/// <summary>
	/// 搜索字典树中是否存在指定前缀的单词
	/// </summary>
	/// <param name="prefix">前缀</param>
	/// <remarks>分隔符会把文本拆分为多个串。分隔符本身也是一个串。<br/>参数里的分隔符后的字符会查询之后的串的前缀。<br/>
	/// <code>
	/// a b匹配:
	/// ac b
	/// ac cb b
	/// 不匹配
	/// ab
	/// ac cb
	/// </code>
	/// </remarks>
	public IEnumerable<string> Search(string prefix)
	{
		if (string.IsNullOrEmpty(prefix))
			yield break;

		int tokenIndex = AllocateTokenIndex();
		try
		{
			foreach (var child in Children.Values)
			{
				foreach (var result in child.Search(prefix, 0, tokenIndex))
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

	/// <summary>
	/// 分配一个未被占用的token索引
	/// </summary>
	/// <returns></returns>
	/// <exception cref="InvalidOperationException"></exception>
	/// <remarks>用来传入<seealso cref="TrieNode.Search(string, int, int)"/></remarks>
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

	/// <summary>
	/// 释放token索引
	/// </summary>
	/// <param name="tokenIndex"></param>
	internal override void ReleaseToken(int tokenIndex)
	{
		base.ReleaseToken(tokenIndex);
		this[tokenIndex] = false;
	}
}
