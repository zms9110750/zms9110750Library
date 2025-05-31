using System.Runtime.InteropServices;
using zms9110750.TreeCollection.Abstract;

namespace zms9110750.TreeCollection.Trie;

/// <summary>
/// 字典树的节点抽象
/// </summary>
public abstract class TrieBase : INode<TrieBase>
{
	/// <summary>
	/// 获取指定字符对应的子节点，如果不存在则创建
	/// </summary>
	/// <param name="c"></param>
	/// <returns></returns>
	protected TrieNode this[char c]
	{
		get
		{
			ref var childNode = ref CollectionsMarshal.GetValueRefOrAddDefault(Children, c, out _);
			return childNode ??= new TrieNode(Root);
		}
	}

	/// <inheritdoc/>
	public int Depth { get; }

	/// <inheritdoc/>
	public TrieBase? Parent { get; }

	/// <inheritdoc/>
	public Trie Root { get; }

	/// <summary>
	/// 子节点集合
	/// </summary>
	protected Dictionary<char, TrieNode> Children { get; } = new();

	/// <summary>
	/// 分隔符集合
	/// </summary>
	internal abstract IReadOnlySet<char> Separator { get; }

	TrieBase INode<TrieBase>.Root => Root;

	/// <summary>
	/// 传入一个父节点。初始化自身的深度、父节点、根节点
	/// </summary>
	/// <param name="parent"></param>
	protected TrieBase(TrieBase parent)
	{
		Depth = parent.Depth + 1;
		Parent = parent;
		Root = parent.Root;
	}

	/// <summary>
	/// 构造一个根节点。只允许<see cref="Trie"/>的派生调用
	/// </summary>
	protected TrieBase()
	{
		Depth = 0;
		Parent = null;
		Root = (Trie)this;
	}

	/// <summary>
	/// 添加单词到字典树中
	/// </summary>
	/// <param name="word">单词</param>
	public abstract void Add(string word);

	/// <summary>
	/// 释放指定索引的Token
	/// </summary>
	/// <param name="tokenIndex"></param>
	internal virtual void ReleaseToken(int tokenIndex)
	{
		foreach (var child in Children.Values)
		{
			child.ReleaseToken(tokenIndex);
		}
	}
}
