using System.Runtime.InteropServices;
using zms9110750.TreeCollection.Abstract;

namespace zms9110750.TreeCollection.Trie;

public abstract class TrieBase : INode<TrieBase>
{
	protected TrieNode this[char c]
	{
		get
		{
			ref var childNode = ref CollectionsMarshal.GetValueRefOrAddDefault(Children, c, out _);
			return childNode ??= new TrieNode(Root);
		}
	}
	public int Depth { get; }
	public TrieBase? Parent { get; }
	public Trie Root { get; }
	protected Dictionary<char, TrieNode> Children { get; } = new();
	internal abstract IReadOnlySet<char> Separator { get; }
	TrieBase INode<TrieBase>.Root => Root;
	protected TrieBase(TrieBase parent)
	{
		Depth = parent.Depth + 1;
		Parent = parent;
		Root = parent.Root;
	}
	protected TrieBase()
	{
		Depth = 0;
		Parent = null;
		Root = (Trie)this;
	}
	internal virtual void ReleaseToken(int tokenIndex)
	{
		foreach (var child in Children.Values)
		{
			child.ReleaseToken(tokenIndex);
		}
	}
}
