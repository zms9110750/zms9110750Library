namespace zms9110750.TreeCollection.Ordered;

public static class OrderedTreeExtensions
{
	#region Properties

	// 属性访问扩展
	public static int GetIndex<TValue, TNode>(this IOrderedTree<TValue, TNode> tree)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree.Index;

	// 获得第一个子节点
	public static TNode? GetFirstChild<TValue, TNode>(this IOrderedTree<TValue, TNode> tree)
		where TNode : IOrderedTree<TValue, TNode>
	{
		return tree.Count > 0 ? tree[0] : default;
	}

	// 获得最后一个子节点
	public static TNode? GetLastChild<TValue, TNode>(this IOrderedTree<TValue, TNode> tree)
		where TNode : IOrderedTree<TValue, TNode>
	{
		return tree.Count > 0 ? tree[^1] : default;
	}

	// 获得前一个兄弟节点
	public static TNode? GetPreviousSibling<TValue, TNode>(this IOrderedTree<TValue, TNode> node)
		where TNode : IOrderedTree<TValue, TNode>
	{
		if (node.Parent == null || node.Index <= 0)
			return default;

		return node.Parent[node.Index - 1];
	}

	// 获得后一个兄弟节点
	public static TNode? GetNextSibling<TValue, TNode>(this IOrderedTree<TValue, TNode> node)
		where TNode : IOrderedTree<TValue, TNode>
	{
		if (node.Parent == null || node.Index >= node.Parent.Count - 1)
			return default;

		return node.Parent[node.Index + 1];
	}

	#endregion

	#region AddAt

	public static TNode AddAt<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, Index index, TNode node)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree.AddAt(index.GetOffset(tree.Count), node);

	public static void AddAt<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, Index index, IEnumerable<TNode> nodes)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree.AddAt(index.GetOffset(tree.Count), nodes);

	public static void AddAt<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, Index index, params ReadOnlySpan<TNode> nodes)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree.AddAt(index.GetOffset(tree.Count), nodes);

	public static TNode AddAt<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, Index index, TValue value)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree.AddAt(index.GetOffset(tree.Count), value);

	public static void AddAt<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, Index index, params IEnumerable<TValue> values)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree.AddAt(index.GetOffset(tree.Count), values);
	#endregion

	#region AddLast
	public static void Add<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, params IEnumerable<TValue> values)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree.AddAt(tree.Count, values);

	public static void Add<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, TValue values)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree.AddAt(tree.Count, values);

	public static void Add<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, params IEnumerable<TNode> values)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree.AddAt(tree.Count, values);

	public static void Add<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, TNode values)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree.AddAt(tree.Count, values);
	#endregion

	#region AddFirst
	public static void AddFirst<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, params IEnumerable<TValue> values)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree.AddAt(0, values);

	public static void AddFirst<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, TValue values)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree.AddAt(0, values);

	public static void AddFirst<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, params IEnumerable<TNode> values)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree.AddAt(0, values);

	public static void AddFirst<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, TNode values)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree.AddAt(0, values);
	#endregion

	#region AddSiblings

	// 添加节点到自己的前面
	public static TNode AddBefore<TValue, TNode>(this IOrderedTree<TValue, TNode> node, TNode newNode)
		where TNode : IOrderedTree<TValue, TNode>
	{
		if (node.Parent == null)
			throw new InvalidOperationException("Root node cannot have siblings");

		return node.Parent.AddAt(node.Index, newNode);
	}

	// 添加值到自己的前面
	public static TNode AddBefore<TValue, TNode>(this IOrderedTree<TValue, TNode> node, TValue value)
		where TNode : IOrderedTree<TValue, TNode>
	{
		if (node.Parent == null)
			throw new InvalidOperationException("Root node cannot have siblings");

		return node.Parent.AddAt(node.Index, value);
	}

	// 添加节点到自己的后面
	public static TNode AddAfter<TValue, TNode>(this IOrderedTree<TValue, TNode> node, TNode newNode)
		where TNode : IOrderedTree<TValue, TNode>
	{
		if (node.Parent == null)
			throw new InvalidOperationException("Root node cannot have siblings");

		return node.Parent.AddAt(node.Index + 1, newNode);
	}
	// 添加值到自己的后面

	public static TNode AddAfter<TValue, TNode>(this IOrderedTree<TValue, TNode> node, TValue value)
		where TNode : IOrderedTree<TValue, TNode>
	{
		if (node.Parent == null)
			throw new InvalidOperationException("Root node cannot have siblings");

		return node.Parent.AddAt(node.Index + 1, value);
	}

	#endregion

	#region Remove 

	public static TNode? Remove<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, TValue value, Range? range = null)
		where TNode : IOrderedTree<TValue, TNode>
		=> range is not Range ran ? tree.Remove(value) : tree[ran].Remove(value);

	public static TNode? Remove<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, TNode value)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree.Remove(value);

	public static TNode? RemoveAt<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, Index index)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree.RemoveAt(index.GetOffset(tree.Count));

	public static int RemoveAll<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, Predicate<TNode>? match = null, Range? range = null)
		where TNode : IOrderedTree<TValue, TNode>
		=> range is not Range ran ? tree.RemoveAll(match) : tree[ran].RemoveAll(match);

	public static int RemoveAll<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, Predicate<TValue>? match = null, Range? range = null)
		where TNode : IOrderedTree<TValue, TNode>
		=> (range, match) switch
		{
			(null, null) => tree.RemoveAll((Predicate<TNode>?)null),
			(Range ran, null) => tree[ran].RemoveAll(null),
			(null, Predicate<TValue> m) => tree.RemoveAll(node => m(node.Value)),
			(Range ran, Predicate<TValue> m) => tree[ran].RemoveAll(node => m(node.Value))
		};
	#endregion

	public static bool Replace<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, TNode oldValue, TNode newValue)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree.Replace(oldValue, newValue);

	public static void MoveChild<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, Index fromIndex, Index toIndex)
	  where TNode : IOrderedTree<TValue, TNode>
	  => tree.MoveChild(fromIndex.GetOffset(tree.Count), toIndex.GetOffset(tree.Count));

	public static IOrderedTree<TValue, TNode>.ISlice Slice<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, int start, int length)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree.Slice(start, length);

	public static bool Contains<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, TValue value, Range? range = null)
		where TNode : IOrderedTree<TValue, TNode>
		=> range is not Range ran ? tree.Contains(value) : tree[ran].Contains(value);

	public static int IndexOf<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, TValue value, Range? range = null)
		where TNode : IOrderedTree<TValue, TNode>
		=> range is not Range ran ? tree.IndexOf(value) : tree[ran].IndexOf(value);

	public static void UpdateIndex<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, Range range)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree[range].UpdateIndex();
}
