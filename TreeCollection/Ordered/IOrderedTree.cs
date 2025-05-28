using zms9110750.TreeCollection.Abstract;
namespace zms9110750.TreeCollection.Ordered;

public interface IOrderedTree<TValue, TNode> : IValue<TValue>, INode<TNode>, IList<TNode> where TNode : IOrderedTree<TValue, TNode>
{
	static EqualityComparer<TValue> Comparer => EqualityComparer<TValue>.Default;
	int Index { get; }
	void MoveChild(int fromIndex, int toIndex)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(fromIndex);
		ArgumentOutOfRangeException.ThrowIfNegative(toIndex);
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(fromIndex, Count);
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(toIndex, Count);
		if (fromIndex < toIndex)
		{
			this[fromIndex..toIndex].RotateForward();
		}
		else
		{
			this[toIndex..fromIndex].RotateBackward();
		}
	}
	TNode AddAt(int index, TNode node);
	void AddAt(int index, IEnumerable<TNode> node);
	void AddAt(int index, params ReadOnlySpan<TNode> node);
	TNode AddAt(int index, TValue node);
	void AddAt(int index, params IEnumerable<TValue> node);
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
	bool Replace(TNode oldNode, TNode newNode);
	new TNode? Remove(TNode value);
	bool ICollection<TNode>.Remove(TNode item)
	{
		return Remove(item)?.Equals(item) == true;
	}
	new TNode? RemoveAt(int index)
	{
		return Remove(this[index]);
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
	public ISlice Slice(int start, int length);
	int RemoveAll(Predicate<TNode>? match = null);
	int RemoveAll(Predicate<TValue>? match = null)
	{
		return match switch
		{
			null => RemoveAll((Predicate<TNode>?)null),
			_ => RemoveAll(node => match(node.Value))
		};
	}
	void ICollection<TNode>.Clear()
	{
		RemoveAll((Predicate<TNode>?)null);
	}
	int IList<TNode>.IndexOf(TNode item)
	{
		return Equals(item.Parent) ? item.Index : -1;
	}
	bool ICollection<TNode>.Contains(TNode item)
	{
		return Equals(item.Parent);
	}
	bool Contains(TValue value)
	{
		foreach (var node in this)
		{
			if (Comparer.Equals(node.Value, value))
			{
				return true;
			}
		}
		return false;
	}
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
	void IncrementVersion();
	public interface ISlice : IReadOnlyList<TNode>
	{
		bool IsValid { get; }
		TNode? Remove(TValue value);
		int RemoveAll(Predicate<TNode>? match = null);
		bool Contains(TValue value);
		int IndexOf(TValue value);
		/// <summary>
		/// 将集合的第一个元素移动到末尾，其余元素向前移动一位
		/// </summary>
		void RotateForward();
		/// <summary>
		/// 将集合的最后一个元素移动到首位，其余元素向后移动一位
		/// </summary>
		void RotateBackward();
		void UpdateIndex();
	}
}
