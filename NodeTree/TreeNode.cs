using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace zms9110750Library.TreeNode;

/// <summary>
/// 节点树
/// </summary>
/// <typeparam name="TValue">值类型</typeparam>
/// <param name="value">初始值</param>

public sealed class TreeNode<TValue>(TValue? value = default) : IEquatable<TValue>, IList<TreeNode<TValue>>
{
	#region 字段
	private List<TreeNode<TValue>> _children = [];
	private TreeNode<TValue>? _parent;
	private TreeNode<TValue>? _root;
	private int _level;
	private int _index;
	#endregion
	#region 自我状态
	/// <summary>
	/// 此节点保存的值
	/// </summary>
	/// <value>替换原保存的值</value>
	public TValue? Value { get; set; } = value;
	/// <summary>
	/// 获取或替换目标索引的节点
	/// </summary>
	/// <param name="index">索引</param>
	/// <returns>指定索引的节点</returns>
	/// <value>替换原节点的新节点</value>
	/// <remarks>可能发生以下情形
	/// <list type="bullet">
	/// <item>移除节点（value为null）</item>
	/// <item>排列节点（value是自己的子节点）</item>
	/// <item>添加节点（index为count)</item>
	/// </list>
	/// </remarks>
	/// <exception cref="ArgumentException">目标节点是自己的祖先节点</exception>
	public TreeNode<TValue> this[int index]
	{
		get => _children[index];
		set
		{
			if (value == this[index])
			{
				return;
			}
			else if (value == null)
			{
				RemoveAt(index);
			}
			else if (Contains(value))
			{
				value.Index = index;
			}
			else if ((this & value) == value)
			{
				throw new ArgumentException("不能把自己的祖先节点设置为自己的子节点", nameof(value));
			}
			else if (index == Count && !Contains(this))
			{
				AddLast(value);
			}
			else if (!Contains(this))
			{
				this[index]._parent = null;
				this[index]._index = 0;
				this[index].Root = null;
				this[index].Level = 0;
				_children[index] = value;
				value.RemoveSelf(false);
				value._parent = this;
				value._index = index;
				value.Root = Root;
				value.Level = Level + 1;
			}
			else
			{
				throw new NotImplementedException("未预测到的情形");
			}
		}
	}
	/// <summary>
	/// 子节点数量
	/// </summary>
	public int Count => _children.Count;
	/// <summary>
	/// 自己所处层级
	/// </summary>
	public int Level
	{
		get => _level;
		private set
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
	/// <summary>
	/// 自己所处索引
	/// </summary>
	/// <value>排列到新索引的位置</value>
	public int Index
	{
		get => _index;
		set
		{
			var parent = RequiredParent;
			if (parent.Count <= value)
			{
				throw new IndexOutOfRangeException("索引越界");
			}
			else if (value == Index)
			{
				return;
			}
			var span = this[..];
			if (value < Index)
			{
				span = span[value..(Index + 1)];
				span[..^1].CopyTo(span[1..]);
				span[0] = this;
			}
			else if (value > Index)
			{
				span = span[Index..(value + 1)];
				span[1..].CopyTo(span[..^1]);
				span[^1] = this;
			}
		}
	}
	#endregion
	#region 临近节点
	/// <summary>
	/// 父节点
	/// </summary>
	/// <remarks>如果设置为null，自己会成为根节点</remarks>
	public TreeNode<TValue>? Parent
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
	/// <summary>
	/// 根节点
	/// </summary>
	[AllowNull]
	public TreeNode<TValue> Root
	{
		get => _root ?? this;
		private set
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
	/// <summary>
	/// 父节点
	/// </summary>
	/// <remarks>如果父节点为null会抛出异常</remarks>
	/// <exception cref="InvalidOperationException"></exception>
	public TreeNode<TValue> RequiredParent => Parent ?? throw new InvalidOperationException("根节点不能执行需要父节点的操作");
	/// <summary>
	/// 前一个节点
	/// </summary>
	public TreeNode<TValue>? Previous => Parent != null && Index > 0 ? Parent[Index - 1] : null;
	/// <summary>
	/// 后一个节点
	/// </summary>
	public TreeNode<TValue>? Next => Parent != null && Index < Parent.Count - 1 ? Parent[Index + 1] : null;
	/// <summary>
	/// 子节点第一个
	/// </summary>
	public TreeNode<TValue>? First => Count == 0 ? null : this[0];
	/// <summary>
	/// 子节点最后一个
	/// </summary>
	public TreeNode<TValue>? Last => Count == 0 ? null : this[^1];
	#endregion
	#region 添加节点  
	/// <summary>
	/// 添加节点到指定处
	/// </summary>
	/// <param name="index">要插入的位置</param>
	/// <param name="nodes">放入的节点</param>
	/// <returns>放入节点的第一个</returns>
	/// <exception cref="ArgumentException"></exception>
	public TreeNode<TValue> AddAt(Index index, params Span<TreeNode<TValue>> nodes)
	{
		if (nodes.IsEmpty)
		{
			throw new ArgumentException("参数长度为0");
		}
		foreach (var item in nodes)
		{
			if ((this & item) == item)
			{
				throw new ArgumentException("不能把自己的祖先节点设置为自己的子节点");
			}
		}
		foreach (ref var item in nodes)
		{
			item ??= new TreeNode<TValue>();
		}
		var offset = index.GetOffset(_children.Count);
		switch ((isInsert: offset != Count, isRange: nodes.Length == 1))
		{
			case { isInsert: false, isRange: false }:
				_children.Add(nodes[0]);
				break;
			case { isInsert: false, isRange: true }:
				_children.AddRange(nodes);
				break;
			case { isInsert: true, isRange: false }:
				_children.Insert(offset, nodes[0]);
				break;
			case { isInsert: true, isRange: true }:
				_children.InsertRange(offset, nodes);
				break;
		}
		foreach (var item in nodes)
		{
			item.RemoveSelf(false);
			item._parent = this;
			item.Root = Root;
			item.Level = Level + 1;
		}
		for (int i = offset; i < _children.Count; i++)
		{
			_children[i]._index = i;
		}
		return nodes[0];
	}
	/// <summary>
	/// 添加到子节点的开头
	/// </summary>
	/// <param name="nodes">放入的节点</param>
	/// <returns>放入节点的第一个</returns>
	public TreeNode<TValue> AddFirst(params Span<TreeNode<TValue>> nodes) => AddAt(0, nodes);
	/// <summary>
	/// 添加到子节点的末尾
	/// </summary>
	/// <param name="nodes">放入的节点</param>
	/// <returns>放入节点的第一个</returns>
	public TreeNode<TValue> AddLast(params Span<TreeNode<TValue>> nodes) => AddAt(^0, nodes);
	/// <summary>
	/// 添加到自己之前
	/// </summary>
	/// <param name="nodes">放入的节点</param>
	/// <returns>放入节点的第一个</returns>
	public TreeNode<TValue> AddAfter(params Span<TreeNode<TValue>> nodes) => RequiredParent.AddAt(Index + 1, nodes);
	/// <summary>
	/// 添加到自己之后
	/// </summary>
	/// <param name="nodes">放入的节点</param>
	/// <returns>放入节点的第一个</returns>
	public TreeNode<TValue> AddBefore(params Span<TreeNode<TValue>> nodes) => RequiredParent.AddAt(Index, nodes);
	#endregion
	#region 删除节点  
	/// <summary>
	/// 把自己从树上移除
	/// </summary>
	/// <param name="resetting">是否重置状态星系</param>
	/// <remarks>如果紧接着要覆写状态信息或者直接弃用，则<paramref name="resetting"/>为<c>false</c></remarks>
	/// <returns>自己</returns>
	public TreeNode<TValue> RemoveSelf(bool resetting = true)
	{
		if (Parent != null)
		{
			Parent._children.RemoveAt(Index);
			for (int i = Index; i < Parent.Count; i++)
			{
				Parent[i]._index = i;
			}
		}
		if (resetting)
		{
			_parent = null;
			_index = 0;
			Root = null;
			Level = 0;
		}
		return this;
	}
	/// <summary>
	/// 移除指定处的子节点
	/// </summary>
	/// <param name="index">指定位置</param>
	/// <returns>移除的节点</returns>
	public TreeNode<TValue> RemoveAt(Index index) => this[index].RemoveSelf();
	/// <summary>
	/// 移除所有符合条件的节点
	/// </summary>
	/// <param name="predicate">条件</param>
	/// <returns>移除的数量</returns>
	public int RemoveAll(Predicate<TreeNode<TValue>>? predicate = null)
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
	/// <summary>
	/// 移除自己之前的节点
	/// </summary> 
	/// <returns>移除的节点</returns>
	public TreeNode<TValue> RemoveBefore() => Previous!.RemoveSelf();
	/// <summary>
	/// 移除自己之后的节点
	/// </summary> 
	/// <returns>移除的节点</returns>
	public TreeNode<TValue> RemoveAfter() => Next!.RemoveSelf();
	/// <summary>
	/// 移除指定范围内找到的第一个具有指定值的节点
	/// </summary>
	/// <param name="value">搜寻的值</param>
	/// <param name="range">范围</param>
	/// <returns>移除的节点</returns>
	public TreeNode<TValue>? RemoveFirst(TValue value, Range? range = default)
	{
		var span = this[range.GetValueOrDefault(Range.All)];
		for (int i = 0; i < span.Length; i++)
		{
			if (span[i].Equals(value))
			{
				return span[i].RemoveSelf();
			}
		}
		return null;
	}
	/// <summary>
	/// 移除指定范围内找到的最后一个具有指定值的节点
	/// </summary>
	/// <param name="value">搜寻的值</param>
	/// <param name="range">范围</param>
	/// <returns>移除的节点</returns>
	public TreeNode<TValue>? RemoveLast<T>(T value, Range? range = default)
	{
		var span = this[range.GetValueOrDefault(Range.All)];
		for (int i = span.Length - 1; i >= 0; i--)
		{
			if (span[i].Equals(value))
			{
				return span[i].RemoveSelf();
			}
		}
		return null;
	}
	/// <summary>
	/// 移除自己之前的最后一个具有指定值的节点
	/// </summary>
	/// <param name="value">搜寻的值</param>
	/// <returns>移除的节点</returns>
	public TreeNode<TValue>? RemoveBefore(TValue value) => RequiredParent.RemoveLast(value, ..(Index - 1));
	/// <summary>
	/// 移除自己之后的第一个具有指定值的节点
	/// </summary>
	/// <param name="value">搜寻的值</param>
	/// <returns>移除的节点</returns>
	public TreeNode<TValue>? RemoveAfter(TValue value) => RequiredParent.RemoveFirst(value, (Index + 1)..);
	#endregion
	#region 查询节点
	/// <summary>
	/// 裁切指定范围的跨度
	/// </summary>
	/// <param name="start">开始位置</param>
	/// <param name="length">数量</param>
	/// <returns>跨度</returns>
	public Span<TreeNode<TValue>> Slice(int start, int length) => CollectionsMarshal.AsSpan(RequiredParent._children).Slice(start, length);
	/// <summary>
	/// 判断值和自己储存的值是否相等
	/// </summary>
	/// <param name="other">判断的值</param>
	/// <returns>相等</returns>
	public bool Equals(TValue? other) => other?.Equals(Value) ?? Value == null;
	/// <summary>
	/// 查询指定范围内是否具有储存结果和指定值匹配的节点
	/// </summary>
	/// <param name="item">判断值</param>
	/// <param name="range">范围</param>
	/// <returns>节点的索引，没有则为-1</returns>
	public int IndexOf(TValue item, Range? range = default)
	{
		var span = this[range.GetValueOrDefault(Range.All)];
		for (int i = span.Length - 1; i >= 0; i--)
		{
			if (span[i].Equals(item))
			{
				return span[i].Index;
			}
		}
		return -1;
	}
	/// <summary>
	/// 查询目标节点的索引
	/// </summary>
	/// <param name="item">查找节点</param>
	/// <returns>节点的索引，若不是自己的子节点则为-1</returns>
	public int IndexOf(TreeNode<TValue> item) => Contains(item) ? -1 : item.Index;
	/// <summary>
	/// 查询目标节点是否为自己的子节点
	/// </summary>
	/// <param name="item">目标节点</param>
	/// <returns>是自己的子节点</returns>
	public bool Contains(TreeNode<TValue> item) => item?.Parent == this;
	/// <summary>
	/// 查询指定范围内是否有储存了目标值的节点
	/// </summary>
	/// <param name="item">查询的值</param>
	/// <param name="range">范围</param>
	/// <returns>范围内有储存目标值的节点</returns>
	public bool Contains(TValue item, Range? range = default) => IndexOf(item, range) > -1;
	#endregion
	#region 显式实现接口
	void IList<TreeNode<TValue>>.Insert(int index, TreeNode<TValue> item) => AddAt(index, item);
	void IList<TreeNode<TValue>>.RemoveAt(int index) => RemoveAt(index);
	bool ICollection<TreeNode<TValue>>.IsReadOnly => false;
	void ICollection<TreeNode<TValue>>.Add(TreeNode<TValue> item) => AddLast(item);
	void ICollection<TreeNode<TValue>>.Clear() => RemoveAll();
	void ICollection<TreeNode<TValue>>.CopyTo(TreeNode<TValue>[] array, int arrayIndex) => _children.CopyTo(array, arrayIndex);
	bool ICollection<TreeNode<TValue>>.Remove(TreeNode<TValue> item)
	{
		if (Contains(item))
		{
			item.RemoveSelf();
			return true;
		}
		return false;
	}
	public IEnumerator<TreeNode<TValue>> GetEnumerator() => _children.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => _children.GetEnumerator();
	#endregion
	#region 迭代和转字符串 
	public IEnumerable<TreeNode<TValue>> EnumTree()
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
	public override string ToString()
	{
		StringBuilder sb = new StringBuilder();
		Stack<string> stack = new Stack<string>();
		stack.Push("");
		Append(sb, stack);
		return sb.ToString();
	}
	private void Append(StringBuilder sb, Stack<string> stack)
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
	#region 运算符 
	/// <summary>
	/// 获取两个节点的最近公共节点
	/// </summary>
	/// <param name="left">参数1</param>
	/// <param name="right">参数2</param>
	/// <returns>公共节点</returns>
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
	/// <summary>
	/// 两个节点间的路径
	/// </summary>
	/// <param name="left">参数1</param>
	/// <param name="right">参数2</param>
	/// <returns>路径的迭代</returns>
	/// <remarks>不包含公共节点。包含不是公共节点的参数。</remarks>
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
}