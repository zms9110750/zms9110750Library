using zms9110750.TreeCollection.Abstract;

namespace zms9110750.TreeCollection.Ordered;
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释


/// <summary>
/// 为<see cref="IOrderedTree{TValue, TNode}"/>提供扩展方法。并为实现类提供对接口默认实现的调用。
/// </summary>
public static partial class IOrderedTreeExtension
{
	extension<TValue, TNode>(IOrderedTree<TValue, TNode> instance)
			where TNode : IOrderedTree<TValue, TNode>
	{
		/// <summary>
		/// 获得第一个子节点。不存在时返回null
		/// </summary>
		public TNode? FirstChild => instance.Count > 0 ? instance[0] : default;
		/// <summary>
		/// 获得最后个子节点。不存在时返回null
		/// </summary>
		public TNode? LastChild => instance.Count > 0 ? instance[0] : default;

		/// <summary>
		/// 获得前一个兄弟节点。不存在时返回null
		/// </summary>
		public TNode? PreviousSibling => instance.Parent == null || instance.Index <= 0 ? default : instance.Parent[instance.Index - 1];

		/// <summary>
		/// 获得后一个兄弟节点。不存在时返回null
		/// </summary>
		public TNode? NextSibling => instance.Parent == null || instance.Index >= instance.Parent.Count - 1 ? default : instance.Parent[instance.Index + 1];

		/// <inheritdoc cref="IOrderedTree{TValue, TNode}.AddAt(int, TNode)"/>
		public TNode AddAt(Index index, TNode node) => instance.AddAt(index.GetOffset(instance.Count), node);

		/// <inheritdoc cref="IOrderedTree{TValue, TNode}.AddAt(int, IEnumerable{TNode})"/>
		public void AddAt(Index index, IEnumerable<TNode> nodes) => instance.AddAt(index.GetOffset(instance.Count), nodes);

		/// <inheritdoc cref="IOrderedTree{TValue, TNode}.AddAt(int, ReadOnlySpan{TNode})"/>
		public void AddAt(Index index, params ReadOnlySpan<TNode> nodes) => instance.AddAt(index.GetOffset(instance.Count), nodes);

		/// <inheritdoc cref="IOrderedTree{TValue, TNode}.AddAt(int, TValue)"/>
		public TNode AddAt(Index index, TValue node) => instance.AddAt(index.GetOffset(instance.Count), node);

		/// <inheritdoc cref="IOrderedTree{TValue, TNode}.AddAt(int, IEnumerable{TValue})"/>
		public void AddAt(Index index, params IEnumerable<TValue> nodes) => instance.AddAt(index.GetOffset(instance.Count), nodes);

		/// <summary>
		/// 将节点添加到最后
		/// </summary>
		public TNode Add(TNode node) => instance.AddAt(instance.Count, node);

		/// <inheritdoc cref="Add{TValue, TNode}(IOrderedTree{TValue, TNode}, TNode)"/>
		public void Add(IEnumerable<TNode> nodes) => instance.AddAt(instance.Count, nodes);

		/// <inheritdoc cref="Add{TValue, TNode}(IOrderedTree{TValue, TNode}, TNode)"/>

		public void Add(params ReadOnlySpan<TNode> nodes) => instance.AddAt(instance.Count, nodes);

		/// <summary>
		/// 将值添加到最后
		/// </summary>
		public TNode Add(TValue value) => instance.AddAt(instance.Count, value);

		/// <inheritdoc cref="Add{TValue, TNode}(IOrderedTree{TValue, TNode}, TNode)"/>
		public void Add(params IEnumerable<TValue> values) => instance.AddAt(instance.Count, values);

		/// <summary>
		/// 将节点添加到开头
		/// </summary>
		public TNode AddFirst(TNode node) => instance.AddAt(0, node);

		/// <inheritdoc cref="AddFirst{TValue, TNode}(IOrderedTree{TValue, TNode}, TNode)"/>
		public void AddFirst(IEnumerable<TNode> nodes) => instance.AddAt(0, nodes);

		/// <inheritdoc cref="AddFirst{TValue, TNode}(IOrderedTree{TValue, TNode}, TNode)"/>
		public void AddFirst(params ReadOnlySpan<TNode> nodes) => instance.AddAt(0, nodes);

		/// <summary>
		/// 将值添加到开头
		/// </summary>
		public TNode AddFirst(TValue value) => instance.AddAt(0, value);

		/// <inheritdoc cref="AddFirst{TValue, TNode}(IOrderedTree{TValue, TNode}, TValue)"/>
		public void AddFirst(params IEnumerable<TValue> values) => instance.AddAt(0, values);

		/// <summary>
		/// 将节点添加到自己的前面
		/// </summary>
		public TNode AddBefore(TNode node) => instance.RequiredParent.AddAt(instance.Index, node);

		/// <inheritdoc cref="AddBefore{TValue, TNode}(IOrderedTree{TValue, TNode}, TNode)"/>
		public TNode AddBefore(TValue value) => instance.RequiredParent.AddAt(instance.Index, value);

		/// <summary>
		/// 将节点添加到自己的后面
		/// </summary>
		public TNode AddAfter(TNode newNode) => instance.RequiredParent.AddAt(instance.Index + 1, newNode);

		/// <inheritdoc cref="AddAfter{TValue, TNode}(IOrderedTree{TValue, TNode}, TNode)"/>
		public TNode AddAfter(TValue value) => instance.RequiredParent.AddAt(instance.Index + 1, value);

		/// <inheritdoc cref="IOrderedTree{TValue, TNode}.Remove(TValue)"/>
		public TNode? Remove(TValue value, Range? range = null) => range is not Range ran
			? instance.Remove(value)
			: instance[ran].Remove(value);

		/// <inheritdoc cref="IOrderedTree{TValue, TNode}.RemoveAt(int)"/>
		public TNode? RemoveAt(Index index) => instance.RemoveAt(index.GetOffset(instance.Count));

		/// <inheritdoc cref="IOrderedTree{TValue, TNode}.RemoveAll(Predicate{TNode})"/>
		public int RemoveAll(Predicate<TNode>? match = null, Range? range = null) => range is not Range ran
			? instance.RemoveAll(match)
			: instance[ran].RemoveAll(match);

		/// <inheritdoc cref="IOrderedTree{TValue, TNode}.RemoveAll(Predicate{TNode})"/>
		public int RemoveAll(Predicate<TValue>? match = null, Range? range = null) => (range, match) switch
		{
			(null, null) => instance.RemoveAll(null),
			(Range ran, null) => instance[ran].RemoveAll(null),
			(null, Predicate<TValue> m) => instance.RemoveAll(node => m(node.Value)),
			(Range ran, Predicate<TValue> m) => instance[ran].RemoveAll(node => m(node.Value))
		};

		/// <summary>
		/// 垂直添加节点 - 第一个元素添加到指定位置，后续元素递归添加为子节点
		/// </summary>
		public TNode? AddVertically(Index index, params IEnumerable<TValue> nodes)
		{
			using var e = nodes.GetEnumerator();
			if (!e.MoveNext())
			{
				return default;
			}
			var currentNode = instance.AddAt(index, e.Current);
			while (e.MoveNext())
			{
				currentNode = currentNode.Add(value: e.Current);
			}
			return currentNode;
		}

		/// <inheritdoc cref="IOrderedTree{TValue, TNode}.MoveChild(int, int)"/>
		public void MoveChild(Index fromIndex, Index toIndex) => instance.MoveChild(
			fromIndex.GetOffset(instance.Count),
			toIndex.GetOffset(instance.Count));

		/// <inheritdoc cref="IOrderedTree{TValue, TNode}.Contains(TValue)"/>
		public bool Contains(TValue value, Range? range = null) => range is not Range ran
			? instance.Contains(value)
			: instance[ran].Contains(value);

		/// <inheritdoc cref="IOrderedTree{TValue, TNode}.IndexOf(TValue)"/>
		public int IndexOf(TValue value, Range? range = null) => range is not Range ran
			? instance.IndexOf(value)
			: instance[ran].IndexOf(value);

		/// <inheritdoc cref="IOrderedTree{TValue, TNode}.LastIndexOf(TValue)"/>
		public int LastIndexOf(TValue value, Range? range = null) => range is not Range ran
			? instance.LastIndexOf(value)
			: instance[ran].LastIndexOf(value);

		/// <inheritdoc cref="IOrderedTree{TValue, TNode}.ISlice.UpdateIndex"/>
		public void UpdateIndex(Range range) => instance[range].UpdateIndex();
	}
}