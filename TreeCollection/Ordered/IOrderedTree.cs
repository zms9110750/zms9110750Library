using zms9110750.TreeCollection.Abstract;
namespace zms9110750.TreeCollection.Ordered;

/// <summary>
/// 有序节点接口
/// </summary>
/// <typeparam name="TValue">值类型</typeparam>
/// <typeparam name="TNode">自我约束</typeparam>
/// <remarks>节点提供<see cref="Index"/>属性，并允许<see cref="MoveChild(int, int)"/>改变索引</remarks>
[InterfaceImplAsExtensionGenerator.Config.InterfaceImplAsExtension]
public interface IOrderedTree<TValue, TNode> : IValue<TValue>, INode<TNode>, IList<TNode> where TNode : IOrderedTree<TValue, TNode>, INode<TNode>
{
	/// <summary>
	/// 默认相等比较器
	/// </summary>
	private static EqualityComparer<TValue> Comparer => EqualityComparer<TValue>.Default;

	/// <summary>
	/// 节点在父节点中的索引
	/// </summary>
	/// <remarks>派生类实现时，根节点应该为-1</remarks>
	int Index { get; }

	/// <summary>
	/// 移动子节点的位置
	/// </summary>
	/// <param name="fromIndex">要移动的子节点的索引</param>
	/// <param name="toIndex">要放到的位置的索引</param>
	void MoveChild(int fromIndex, int toIndex)
	{
#if NET8_0_OR_GREATER
    ArgumentOutOfRangeException.ThrowIfNegative(fromIndex);
    ArgumentOutOfRangeException.ThrowIfNegative(toIndex);
    ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(fromIndex, Count);
    ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(toIndex, Count);
#else
		if (fromIndex < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(fromIndex), "Value must not be negative.");
		}

		if (toIndex < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(toIndex), "Value must not be negative.");
		}

		if (fromIndex >= Count)
		{
			throw new ArgumentOutOfRangeException(nameof(fromIndex), $"Value must be less than {Count}.");
		}

		if (toIndex >= Count)
		{
			throw new ArgumentOutOfRangeException(nameof(toIndex), $"Value must be less than {Count}.");
		}
#endif
		if (fromIndex < toIndex)
		{
			this[fromIndex..toIndex].RotateForward();
		}
		else
		{
			this[toIndex..fromIndex].RotateBackward();
		}
	}

	/// <summary>
	/// 添加节点到指定位置
	/// </summary>
	/// <param name="index">位置索引</param>
	/// <param name="node">要添加的节点</param>
	/// <returns>被添加的节点，提供链式调用</returns>
	/// <remarks>实现类应当自动把节点从所在树解除<br/>应当检查添加节点是否是当前节点的祖先<br/>所有节点合法时再添加</remarks>
	TNode AddAt(int index, TNode node);

	/// <inheritdoc cref="AddAt(int, TNode)" />
	void AddAt(int index, IEnumerable<TNode> node);

	/// <inheritdoc cref="AddAt(int, TNode)" />
	void AddAt(int index, params ReadOnlySpan<TNode> node);

	/// <remarks>添加值时应当跳过检查步骤。</remarks>
	/// <inheritdoc cref="AddAt(int, TNode)" />
	TNode AddAt(int index, TValue node);

	/// <inheritdoc cref="AddAt(int, TValue)" />
	void AddAt(int index, params IEnumerable<TValue> node);

	/// <summary>
	/// 替换节点
	/// </summary>
	/// <param name="oldNode">原节点</param>
	/// <param name="newNode">新节点</param>
	/// <returns>旧节点是子节点。</returns>
	/// <remarks>等效于移除旧节点，再添加新节点。</remarks>
	bool Replace(TNode oldNode, TNode newNode);

	/// <summary>
	/// 查找并移除具有指定值的节点
	/// </summary>
	/// <param name="value">查找值</param>
	/// <returns>被移除的节点以提供链式调用</returns>
	TNode? Remove(TValue value)
	{
		foreach (var node in this)
		{
			if (Comparer.Equals(node.Value, value))
			{
				return Remove(node);
			}
		}
		return default;
	}

	/// <summary>
	/// 移除节点
	/// </summary>
	/// <param name="value">子节点</param>
	/// <returns><inheritdoc cref="Remove(TValue)" path="/returns" /></returns>
	/// <remarks>如果参数不是子节点，则返回null。</remarks>
	new TNode? Remove(TNode value);

	bool ICollection<TNode>.Remove(TNode item)
	{
		return Remove(item)?.Equals(item) == true;
	}

	/// <summary>
	/// 移除指定索引处的节点
	/// </summary>
	/// <param name="index">索引</param>
	/// <returns><inheritdoc cref="Remove(TValue)" path="/returns" /></returns>
	new TNode RemoveAt(int index)
	{
		return Remove(this[index])!;
	}

	void IList<TNode>.Insert(int index, TNode item)
	{
		AddAt(index, item);
	}

	void IList<TNode>.RemoveAt(int index)
	{
		RemoveAt(index);
	}

	void ICollection<TNode>.Add(TNode item)
	{
		AddAt(Count, item);
	}

	/// <summary>
	/// 获取子节点的切片
	/// </summary>
	/// <param name="start">开始位置</param>
	/// <param name="length">切片长度</param>
	/// <returns>切片</returns>
	public ISlice Slice(int start, int length);

	/// <summary>
	/// 移除所有符合条件的节点
	/// </summary>
	/// <param name="match">若为null则视为总是true</param>
	/// <returns>成功移除的节点数量</returns>
	int RemoveAll(Predicate<TNode>? match = null);

	void ICollection<TNode>.Clear()
	{
		RemoveAll(null);
	}
	int IList<TNode>.IndexOf(TNode item)
	{
		return Equals(item.Parent) ? item.Index : -1;
	}
	bool ICollection<TNode>.Contains(TNode item)
	{
		return Equals(item.Parent);
	}

	/// <summary>
	/// 查询是否具有包含指定值的子节点
	/// </summary>
	/// <param name="value">查找值</param>
	/// <returns>若存在则返回true，否则返回false</returns>
	bool Contains(TValue value)
	{
		return IndexOf(value) != -1;
	}

	/// <summary>
	/// 查询包含指定值的子节点索引
	/// </summary>
	/// <returns>找到的第一个节点的索引，找不到则返回-1</returns>
	/// <inheritdoc cref="Contains(TValue)"/>
	int IndexOf(TValue value)
	{
		foreach (var node in this)
		{
			if (Comparer.Equals(node.Value, value))
			{
				return node.Index;
			}
		}
		return -1;
	}

	/// <summary>
	/// 查询包含指定值的子节点索引
	/// </summary>
	/// <returns>找到的最后一个节点的索引，找不到则返回-1</returns>
	/// <inheritdoc cref="Contains(TValue)"/>
	int LastIndexOf(TValue value)
	{
		foreach (var node in this.Reverse())
		{
			if (Comparer.Equals(node.Value, value))
			{
				return node.Index;
			}
		}
		return -1;
	}

	/// <summary>
	/// 改变版本
	/// </summary>
	/// <remarks>切片改变集合时应当调用此方法。<br/>版本改变后切片应当失效。</remarks>
	internal void IncrementVersion();


	/// <summary>
	/// 切片接口
	/// </summary>
	/// <remarks>对于一些遍历节点查找值的方法，此接口提供相同功能并仅在范围内遍历。</remarks>
	[InterfaceImplAsExtensionGenerator.Config.InterfaceImplAsExtension]
	public interface ISlice : IReadOnlyList<TNode>
	{
		/// <summary>
		/// 是否有效
		/// </summary>
		/// <remarks>在切片生成后，若集合改变，切片应当失效。</remarks>
		bool IsValid { get; }

		/// <inheritdoc cref="IOrderedTree{TValue, TNode}.Remove(TValue)"/>
		TNode? Remove(TValue value);

		/// <inheritdoc cref="IOrderedTree{TValue, TNode}.RemoveAll(Predicate{TNode}?)"/>
		int RemoveAll(Predicate<TNode>? match = null);

		/// <inheritdoc cref="IOrderedTree{TValue, TNode}.Contains(TValue)"/>
		bool Contains(TValue value);

		/// <inheritdoc cref="IOrderedTree{TValue, TNode}.IndexOf(TValue)"/>
		int IndexOf(TValue value);

		/// <inheritdoc cref="IOrderedTree{TValue, TNode}.LastIndexOf(TValue)"/>
		int LastIndexOf(TValue value);

		/// <summary>
		/// 将集合的第一个元素移动到末尾，其余元素向前移动一位
		/// </summary>
		void RotateForward();

		/// <summary>
		/// 将集合的最后一个元素移动到首位，其余元素向后移动一位
		/// </summary>
		void RotateBackward();

		/// <summary>
		/// 更新索引
		/// </summary>
		void UpdateIndex();

	}
}
