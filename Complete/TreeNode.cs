using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace zms9110750Library.Complete;
public abstract class TreeNodeBase<TValue, TDerived> : IEquatable<TValue>, IList<TDerived> where TDerived : TreeNodeBase<TValue, TDerived>
{
	#region 储存值
	public abstract TValue? Value { get; set; }
	#endregion
	#region 层级和索引 
	public abstract int Level { get; protected set; }
	public abstract int Index { get; set; }
	#endregion
	#region 父节点和根节点  
	public abstract TDerived? Parent { get; set; }
	public TDerived RequiredParent => Parent ?? throw new InvalidOperationException("根节点不能执行需要父节点的操作");
	[AllowNull]
	public abstract TDerived Root { get; protected set; }
	#endregion
	#region 子节点集合  
	public abstract int Count { get; }
	public abstract TDerived this[int index] { get; set; }
	public TDerived this[Index index] { get => this[index.GetOffset(Count)]; set => this[index.GetOffset(Count)] = value; }
	#endregion
	#region 常用节点
	public TDerived? Previous => Parent != null && Index > 0 ? Parent[Index - 1] : null;
	public TDerived? Next => Parent != null && Index < Parent.Count - 1 ? Parent[Index + 1] : null;
	public TDerived? First => Count == 0 ? null : this[0];
	public TDerived? Last => Count == 0 ? null : this[^1];
	#endregion
	#region 添加节点 
	public abstract TDerived[] AddAt(Index index, params TDerived[] node);
	public TDerived[] AddFirst(params TDerived[] node) => AddAt(0, node);
	public TDerived[] AddLast(params TDerived[] node) => AddAt(^0, node);
	public TDerived[] AddAfter(params TDerived[] node) => RequiredParent.AddAt(Index + 1, node);
	public TDerived[] AddBefore(params TDerived[] node) => RequiredParent.AddAt(Index, node);
	#endregion
	#region 删除节点
	public abstract TDerived RemoveSelf();
	public TDerived RemoveAt(Index index) => this[index].RemoveSelf();
	public int RemoveAll(Predicate<TDerived>? predicate = null)
	{
		var count = 0;
		for (int i = Count - 1; i >= 0; i--)
		{
			if (predicate?.Invoke(this[i]) ?? true)
			{
				this[i].RemoveSelf();
				count++;
			}
		}
		return count;
	}
	public TDerived RemoveBefore<T>() => Previous!.RemoveSelf();
	public TDerived RemoveAfter<T>() => Next!.RemoveSelf();
	public TDerived? RemoveFirst<T>(T value, Range? range = default)
	{
		var (sta, len) = range.GetValueOrDefault(..).GetOffsetAndLength(Count);
		for (int i = sta; i < sta + len; i++)
		{
			if (this[i].Equals(value))
			{
				return this[i].RemoveSelf();
			}
		}
		return null;
	}
	public TDerived? RemoveLast<T>(T value, Range? range = default)
	{
		var (sta, len) = range.GetValueOrDefault(..).GetOffsetAndLength(Count);
		for (int i = sta + len - 1; i >= sta; i--)
		{
			if (this[i].Equals(value))
			{
				return this[i].RemoveSelf();
			}
		}
		return null;
	}
	public TDerived? RemoveBefore<T>(T value) => RequiredParent.RemoveLast(value, ..(Index - 1));
	public TDerived? RemoveAfter<T>(T value) => RequiredParent.RemoveFirst(value, (Index + 1)..);
	#endregion
	#region 查询节点
	public bool Equals(TValue? other) => other?.Equals(Value) ?? Value == null;
	public int IndexOf(TValue item, Range? range = default)
	{
		var (sta, len) = range.GetValueOrDefault(..).GetOffsetAndLength(Count);
		for (int i = sta; i < len; i++)
		{
			if (this[i].Equals(item))
			{
				return i;
			}
		}
		return -1;
	}
	public int IndexOf(TDerived item) => Contains(item) ? -1 : item.Index;
	public bool Contains(TDerived item) => item?.Parent == this;
	public bool Contains(TValue item, Range? range = default) => IndexOf(item, range) > -1;
	#endregion
	#region IList  
	protected bool IsReadOnly => false;
	bool ICollection<TDerived>.IsReadOnly => IsReadOnly;
	public abstract IEnumerator<TDerived> GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	protected void Insert(int index, TDerived item) => AddAt(index, item);
	protected void Add(TDerived item) => AddLast(item);
	protected void Clear() => RemoveAll();
	protected abstract void CopyTo(TDerived[] array, int arrayIndex);
	protected bool Remove(TDerived item)
	{
		if (item?.Parent == this)
		{
			item.RemoveSelf();
			return true;
		}
		return false;
	}
	void IList<TDerived>.Insert(int index, TDerived item) => Insert(index, item);
	void IList<TDerived>.RemoveAt(int index) => RemoveAt(index);
	void ICollection<TDerived>.Add(TDerived item) => Add(item);
	void ICollection<TDerived>.Clear() => Clear();
	void ICollection<TDerived>.CopyTo(TDerived[] array, int arrayIndex) => CopyTo(array, arrayIndex);
	bool ICollection<TDerived>.Remove(TDerived item) => Remove(item);
	#endregion
}
public sealed class TreeNode<TValue>(TValue? value) : TreeNodeBase<TValue, TreeNode<TValue>>
{
	List<TreeNode<TValue>> _children = [];
	TreeNode<TValue>? _parent;
	TreeNode<TValue>? _root;
	int _level;
	int _index;
	public override TValue? Value { get; set; } = value;
	public override TreeNode<TValue> this[int index]
	{
		get => _children[index];
		set
		{
			if (value == null)
			{
				RemoveAt(index);
			}
			else if (value.Parent == this)
			{
				value.Index = index;
			}
			else if ((this & value) == value)
			{
				throw new ArgumentException("不能把自己的祖先节点设置为自己的子节点");
			}
			else
			{
				this[index]._parent = null;
				this[index]._index = 0;
				this[index].Root = null;
				this[index].Level = 0;
				_children[index] = value;
				value.RemoveInList();
				value._parent = this;
				value._index = index;
				value.Root = Root;
				value.Level = Level + 1;
			}
		}
	}
	public override int Count => _children.Count;
	public override int Level
	{
		get => _level;
		protected set
		{
			if (_level != (_level = value))
			{
				foreach (var item in _children)
				{
					item.Level = _level + 1;
				}
			}
		}
	}
	public override int Index
	{
		get => _index;
		set
		{
			var parent = RequiredParent;
			if (parent.Count <= value)
			{
				throw new ArgumentException("索引越界");
			}
			var span = CollectionsMarshal.AsSpan(RequiredParent._children);
			if (value < Index)
			{
				span = span[value..(Index + 1)];
				span[..^1].CopyTo(span[1..]);
				span[0] = this;
			}
			else
			{
				span = span[Index..(value + 1)];
				span[1..].CopyTo(span[..^1]);
				span[^1] = this;
			}
		}
	}
	public override TreeNode<TValue>? Parent
	{
		get => _parent;
		set
		{
			if (_parent == value)
			{
				return;
			}
			if (value == null)
			{
				RemoveSelf();
			}
			else
			{
				value.AddLast(this);
			}
		}
	}
	[AllowNull]
	public override TreeNode<TValue> Root
	{
		get => _root ?? this;
		protected set
		{
			if (Root != (value ?? this))
			{
				_root = value;
				foreach (var item in _children)
				{
					item.Root = Root;
				}
			}
		}
	}
	public override TreeNode<TValue>[] AddAt(Index index, params TreeNode<TValue>[] node)
	{
		if (node == null)
		{
			return [];
		}
		foreach (var item in node)
		{
			if (item == null)
			{
				throw new ArgumentException("添加的节点中有Null值");
			}
			if ((this & item) == item)
			{
				throw new ArgumentException("不能把自己的祖先节点设置为自己的子节点");
			}
		}
		var offset = index.GetOffset(_children.Count);
		_children.InsertRange(offset, node);
		foreach (var item in node)
		{
			RemoveInList();
			item._parent = this;
			item.Root = Root;
			item.Level = Level + 1;
		}
		for (int i = offset; i < _children.Count; i++)
		{
			_children[i]._index = i;
		}
		return node;
	}
	public override TreeNode<TValue> RemoveSelf()
	{
		RemoveInList();
		_parent = null;
		_index = 0;
		Root = null;
		Level = 0;
		return this;
	}
	void RemoveInList()
	{
		if (Parent != null)
		{
			Parent._children.RemoveAt(Index);
			for (int i = Index; i < Parent.Count; i++)
			{
				Parent[i]._index = i;
			}
		}
	}
	protected override void CopyTo(TreeNode<TValue>[] array, int arrayIndex) => _children.CopyTo(array, arrayIndex);
	#region IEnumerator 
	public override IEnumerator<TreeNode<TValue>> GetEnumerator()
	{
		var parent = Parent;
		var index = Index;
		yield return this;
		foreach (var item in First ?? Enumerable.Empty<TreeNode<TValue>>())
		{
			if (parent != Parent)
			{
				break;
			}
			index = Index;
			yield return item;
		}
		if (parent != null && parent.Count > index)
		{
			foreach (var item in parent[index] ?? Enumerable.Empty<TreeNode<TValue>>())
			{
				yield return item;
			}
		}
	}
	#endregion
	#region operator 
	/// <summary>
	/// 获取两个节点的最近公共节点
	/// </summary>
	/// <param name="left"></param>
	/// <param name="right"></param>
	/// <returns></returns>
	public static TreeNode<TValue>? operator &(TreeNode<TValue>? left, TreeNode<TValue>? right)
	{
		if (left == null || right == null || left.Root != right.Root)
		{
			return null;
		}
		while (left!.Level > right.Level)
		{
			left = left.Parent;
		}
		while (left.Level < right!.Level)
		{
			right = right.Parent;
		}
		while (left != right)
		{
			left = left!.Parent;
			right = right!.Parent;
		}
		return left;
	}
	public static IEnumerable<TreeNode<TValue>> operator |(TreeNode<TValue>? left, TreeNode<TValue>? right)
	{
		if (left == right)
		{
			yield break;
		}
		var ancestor = left & right;
		if (ancestor == right)
		{
			while (left != right)
			{
				yield return left!;
				left = left!.Parent;
			}
		}
		else if (ancestor == left)
		{
			foreach (var item in (right | left).Reverse())
			{
				yield return item;
			}
		}
		else
		{
			foreach (var item in left | ancestor)
			{
				yield return item;
			}
			foreach (var item in (right | ancestor).Reverse())
			{
				yield return item;
			}
		}
	}
	public static implicit operator TreeNode<TValue>(TValue value) => new TreeNode<TValue>(value);
	#endregion
	#region ToString
	public override string ToString()
	{
		StringBuilder sb = new StringBuilder();
		Stack<string> stack = new Stack<string>();
		stack.Push("");
		Append(sb, stack);
		return sb.ToString();
	}
	void Append(StringBuilder sb, Stack<string> stack)
	{
		const string V0 = "└─ ";
		const string V1 = "├─ ";
		const string V2 = "   ";
		const string V3 = "│  ";
		sb.AppendJoin(null, stack.Reverse()).AppendLine(Value?.ToString());
		if (stack.Peek() == V0)
		{
			stack.Pop();
			stack.Push(V2);
		}
		else if (stack.Peek() == V1)
		{
			stack.Pop();
			stack.Push(V3);
		}
		foreach (var node in _children)
		{
			stack.Push(node.Next != null ? V1 : V0);
			node.Append(sb, stack);
		}
		stack.Pop();
	}
	#endregion
}