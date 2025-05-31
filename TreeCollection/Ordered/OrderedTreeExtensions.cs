namespace zms9110750.TreeCollection.Ordered;

/// <summary>
/// 为<see cref="IOrderedTree{TValue, TNode}"/>提供扩展方法。并为实现类提供对接口默认实现的调用。
/// </summary>
public static class OrderedTreeExtensions
{
	#region Properties

	/// <typeparam name="TValue">节点值的类型</typeparam>
	/// <typeparam name="TNode">节点类型</typeparam>
	/// <param name="tree">扩展方法依附的实例</param> 
	/// <remarks>当接口具有默认实现时，此方法以扩展方法提供成员调用</remarks>
	/// <inheritdoc cref="IOrderedTree{TValue, TNode}.Index"/>
	public static int GetIndex<TValue, TNode>(this IOrderedTree<TValue, TNode> tree)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree.Index;

	/// <summary>
	/// 获得第一个子节点
	/// </summary>
	/// <remarks>没有合法节点时返回null</remarks>
	/// <inheritdoc cref="GetIndex{TValue, TNode}(IOrderedTree{TValue, TNode})"/>
	public static TNode? GetFirstChild<TValue, TNode>(this IOrderedTree<TValue, TNode> tree)
		where TNode : IOrderedTree<TValue, TNode>
	{
		return tree.Count > 0 ? tree[0] : default;
	}

	/// <summary>
	/// 获得最后一个子节点
	/// </summary>
	/// <inheritdoc cref="GetFirstChild{TValue, TNode}(IOrderedTree{TValue, TNode})"/>

	public static TNode? GetLastChild<TValue, TNode>(this IOrderedTree<TValue, TNode> tree)
		where TNode : IOrderedTree<TValue, TNode>
	{
		return tree.Count > 0 ? tree[^1] : default;
	}

	/// <summary>
	/// 获得前一个兄弟节点
	/// </summary>
	/// <inheritdoc cref="GetFirstChild{TValue, TNode}(IOrderedTree{TValue, TNode})"/>
	public static TNode? GetPreviousSibling<TValue, TNode>(this IOrderedTree<TValue, TNode> node)
		where TNode : IOrderedTree<TValue, TNode>
	{
		return node.Parent == null || node.Index <= 0 ? default : node.Parent[node.Index - 1];
	}

	/// <summary>
	/// 获得后一个兄弟节点
	/// </summary>
	/// <inheritdoc cref="GetFirstChild{TValue, TNode}(IOrderedTree{TValue, TNode})"/>
	public static TNode? GetNextSibling<TValue, TNode>(this IOrderedTree<TValue, TNode> node)
		where TNode : IOrderedTree<TValue, TNode>
	{
		return node.Parent == null || node.Index >= node.Parent.Count - 1 ? default : node.Parent[node.Index + 1];
	}

	#endregion

	#region AddAt
	/// <inheritdoc cref="IOrderedTree{TValue, TNode}.AddAt(int, TNode)"/>
	public static TNode AddAt<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, Index index, TNode node)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree.AddAt(index.GetOffset(tree.Count), node);

	/// <inheritdoc cref="IOrderedTree{TValue, TNode}.AddAt(int, IEnumerable{TNode})"/>
	public static void AddAt<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, Index index, IEnumerable<TNode> node)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree.AddAt(index.GetOffset(tree.Count), node);

	/// <inheritdoc cref="IOrderedTree{TValue, TNode}.AddAt(int, ReadOnlySpan{TNode})"/>
	public static void AddAt<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, Index index, params ReadOnlySpan<TNode> node)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree.AddAt(index.GetOffset(tree.Count), node);

	/// <inheritdoc cref="IOrderedTree{TValue, TNode}.AddAt(int, TValue)"/>
	public static TNode AddAt<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, Index index, TValue node)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree.AddAt(index.GetOffset(tree.Count), node);

	/// <inheritdoc cref="IOrderedTree{TValue, TNode}.AddAt(int, IEnumerable{TValue})"/>
	public static void AddAt<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, Index index, params IEnumerable<TValue> node)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree.AddAt(index.GetOffset(tree.Count), node);
	#endregion

	#region AddLast

	/// <summary>
	/// 将节点添加到最后
	/// </summary>
	/// <inheritdoc cref="GetIndex{TValue, TNode}(IOrderedTree{TValue, TNode})"/>
	public static TNode Add<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, TNode values)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree.AddAt(tree.Count, values);

	/// <inheritdoc cref="Add{TValue, TNode}(IOrderedTree{TValue, TNode}, TNode)"/>
	public static void Add<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, params IEnumerable<TNode> values)
			where TNode : IOrderedTree<TValue, TNode>
			=> tree.AddAt(tree.Count, values);

	/// <inheritdoc cref="Add{TValue, TNode}(IOrderedTree{TValue, TNode}, TNode)"/>
	public static void Add<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, ReadOnlySpan<TNode> values)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree.AddAt(tree.Count, values);

	/// <summary>
	/// 将值添加到最后
	/// </summary>
	/// <inheritdoc cref="Add{TValue, TNode}(IOrderedTree{TValue, TNode}, TNode)"/>
	public static TNode Add<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, TValue values)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree.AddAt(tree.Count, values);

	/// <inheritdoc cref="Add{TValue, TNode}(IOrderedTree{TValue, TNode}, TValue)"/>
	public static void Add<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, params IEnumerable<TValue> values)
	   where TNode : IOrderedTree<TValue, TNode>
	   => tree.AddAt(tree.Count, values);


	#endregion

	#region AddFirst
	/// <summary>
	/// 将节点添加到开头
	/// </summary>
	/// <inheritdoc cref="GetIndex{TValue, TNode}(IOrderedTree{TValue, TNode})"/>
	public static TNode AddFirst<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, TNode values)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree.AddAt(0, values);

	/// <inheritdoc cref="AddFirst{TValue, TNode}(IOrderedTree{TValue, TNode}, TNode)"/>
	public static void AddFirst<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, params IEnumerable<TNode> values)
			where TNode : IOrderedTree<TValue, TNode>
			=> tree.AddAt(0, values);

	/// <inheritdoc cref="AddFirst{TValue, TNode}(IOrderedTree{TValue, TNode}, TNode)"/>
	public static void AddFirst<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, params ReadOnlySpan<TNode> values)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree.AddAt(0, values);

	/// <summary>
	/// 将值添加到开头
	/// </summary>
	/// <inheritdoc cref="AddFirst{TValue, TNode}(IOrderedTree{TValue, TNode}, TNode)"/>
	public static TNode AddFirst<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, TValue values)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree.AddAt(0, values);

	/// <inheritdoc cref="AddFirst{TValue, TNode}(IOrderedTree{TValue, TNode}, TValue)"/>
	public static void AddFirst<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, params IEnumerable<TValue> values)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree.AddAt(0, values);

	#endregion

	#region AddSiblings

	/// <summary>
	/// 将节点添加到自己的前面
	/// </summary>
	/// <inheritdoc cref="GetIndex{TValue, TNode}(IOrderedTree{TValue, TNode})"/>
	public static TNode AddBefore<TValue, TNode>(this IOrderedTree<TValue, TNode> node, TNode newNode)
		where TNode : IOrderedTree<TValue, TNode> => node.Parent == null
			? throw new InvalidOperationException("Root node cannot have siblings")
			: node.Parent.AddAt(node.Index, newNode);

	/// <inheritdoc cref="AddBefore{TValue, TNode}(IOrderedTree{TValue, TNode}, TNode)"/>
	public static TNode AddBefore<TValue, TNode>(this IOrderedTree<TValue, TNode> node, TValue value)
		where TNode : IOrderedTree<TValue, TNode> => node.Parent == null
			? throw new InvalidOperationException("Root node cannot have siblings")
			: node.Parent.AddAt(node.Index, value);

	/// <summary>
	/// 将节点添加到自己的后面
	/// </summary>
	/// <inheritdoc cref="AddBefore{TValue, TNode}(IOrderedTree{TValue, TNode}, TNode)"/>
	public static TNode AddAfter<TValue, TNode>(this IOrderedTree<TValue, TNode> node, TNode newNode)
		where TNode : IOrderedTree<TValue, TNode> => node.Parent == null
			? throw new InvalidOperationException("Root node cannot have siblings")
			: node.Parent.AddAt(node.Index + 1, newNode);


	/// <inheritdoc cref="AddAfter{TValue, TNode}(IOrderedTree{TValue, TNode}, TNode)"/>
	public static TNode AddAfter<TValue, TNode>(this IOrderedTree<TValue, TNode> node, TValue value)
		where TNode : IOrderedTree<TValue, TNode> => node.Parent == null
			? throw new InvalidOperationException("Root node cannot have siblings")
			: node.Parent.AddAt(node.Index + 1, value);

	#endregion

	#region Remove 

	/// <inheritdoc cref="IOrderedTree{TValue, TNode}.Remove(TNode)"/>
	public static TNode? Remove<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, TNode value)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree.Remove(value);

	/// <param name="range">如果具有范围则转由<see cref="IOrderedTree{TValue, TNode}.ISlice"/>执行</param>
	/// <inheritdoc cref="IOrderedTree{TValue, TNode}.Remove(TValue)"/>
	public static TNode? Remove<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, TValue value, Range? range = null)
			where TNode : IOrderedTree<TValue, TNode>
			=> range is not Range ran ? tree.Remove(value) : tree[ran].Remove(value);

	/// <inheritdoc cref="IOrderedTree{TValue, TNode}.RemoveAt(int)"/>
	public static TNode? RemoveAt<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, Index index)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree.RemoveAt(index.GetOffset(tree.Count));

	/// <inheritdoc cref="IOrderedTree{TValue, TNode}.RemoveAll(Predicate{TNode})"/>
	public static int RemoveAll<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, Predicate<TNode>? match = null, Range? range = null)
		where TNode : IOrderedTree<TValue, TNode>
		=> range is not Range ran ? tree.RemoveAll(match) : tree[ran].RemoveAll(match);

	/// <inheritdoc cref="IOrderedTree{TValue, TNode}.RemoveAll(Predicate{TNode})"/>
	/// <remarks>会转为对<see cref="IOrderedTree{TValue, TNode}.RemoveAll(Predicate{TNode})"/>的调用和对<see cref="IOrderedTree{TValue, TNode}.ISlice"/>的调用</remarks>
	public static int RemoveAll<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, Predicate<TValue>? match = null, Range? range = null)
		where TNode : IOrderedTree<TValue, TNode>
		=> (range, match) switch
		{
			(null, null) => tree.RemoveAll(null),
			(Range ran, null) => tree[ran].RemoveAll(null),
			(null, Predicate<TValue> m) => tree.RemoveAll(node => m(node.Value)),
			(Range ran, Predicate<TValue> m) => tree[ran].RemoveAll(node => m(node.Value))
		};
	#endregion

	/// <inheritdoc cref="IOrderedTree{TValue, TNode}.Replace(TNode, TNode)"/>
	public static bool Replace<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, TNode oldValue, TNode newValue)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree.Replace(oldValue, newValue);

	/// <inheritdoc cref="IOrderedTree{TValue, TNode}.MoveChild(int, int)"/>
	public static void MoveChild<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, Index fromIndex, Index toIndex)
	  where TNode : IOrderedTree<TValue, TNode>
	  => tree.MoveChild(fromIndex.GetOffset(tree.Count), toIndex.GetOffset(tree.Count));

	/// <inheritdoc cref="IOrderedTree{TValue, TNode}.Slice(int, int)"/>
	public static IOrderedTree<TValue, TNode>.ISlice Slice<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, int start, int length)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree.Slice(start, length);

	/// <inheritdoc cref="IOrderedTree{TValue, TNode}.Contains(TValue)"/>
	/// <remarks>如果具有范围则转由<see cref="IOrderedTree{TValue, TNode}.ISlice"/>执行</remarks>
	public static bool Contains<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, TValue value, Range? range = null)
		where TNode : IOrderedTree<TValue, TNode>
		=> range is not Range ran ? tree.Contains(value) : tree[ran].Contains(value);

	/// <inheritdoc cref="IOrderedTree{TValue, TNode}.IndexOf(TValue)"/>
	/// <remarks>如果具有范围则转由<see cref="IOrderedTree{TValue, TNode}.ISlice"/>执行</remarks>
	public static int IndexOf<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, TValue value, Range? range = null)
		where TNode : IOrderedTree<TValue, TNode>
		=> range is not Range ran ? tree.IndexOf(value) : tree[ran].IndexOf(value);

	/// <inheritdoc cref="IOrderedTree{TValue, TNode}.LastIndexOf(TValue)"/>
	/// <remarks>如果具有范围则转由<see cref="IOrderedTree{TValue, TNode}.ISlice"/>执行</remarks>
	public static int LastIndexOf<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, TValue value, Range? range = null)
		where TNode : IOrderedTree<TValue, TNode>
		=> range is not Range ran ? tree.LastIndexOf(value) : tree[ran].LastIndexOf(value);

	/// <inheritdoc cref="IOrderedTree{TValue, TNode}.ISlice.UpdateIndex"/>
	/// <remarks>由<see cref="IOrderedTree{TValue, TNode}.ISlice"/>执行</remarks>
	public static void UpdateIndex<TValue, TNode>(this IOrderedTree<TValue, TNode> tree, Range range)
		where TNode : IOrderedTree<TValue, TNode>
		=> tree[range].UpdateIndex();
}
