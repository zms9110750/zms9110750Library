using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using zms9110750.TreeCollection.Abstract;
using zms9110750.TreeCollection.Ordered;

namespace zms9110750.TreeCollection.Ordered;

/// <summary>
/// 有序树节点
/// </summary>
/// <typeparam name="T">储存值类型</typeparam>
/// <param name="value">初始值</param>
[JsonConverter(typeof(TreeListNodeConverterFactory))]
public class TreeNode<T>(T value) : RootNode<T, TreeNode<T>>(value), IList<TreeNode<T>>, IOrderedTree<T, TreeNode<T>>
{
	/// <inheritdoc/>
	protected override List<TreeNode<T>> ChildrenNode { get; } = new();

	/// <inheritdoc/>
	public int Index { get; protected set; } = -1;

	/// <inheritdoc/>
	public int Count => ChildrenNode.Count;

	/// <inheritdoc/>
	public bool IsReadOnly => ((ICollection<TreeNode<T>>)ChildrenNode).IsReadOnly;

	/// <summary>
	/// 在修改集合时的版本。用于令<see cref="NodeListSlice"/>失效
	/// </summary>
	protected int Version { get; set; }

	/// <summary>
	/// 索引器
	/// </summary>
	/// <param name="index">索引</param>
	/// <returns></returns>
	/// <exception cref="ArgumentOutOfRangeException">索引非法</exception>
	/// <remarks>
	/// <list type="bullet">
	/// <item>如果有值且索引为Count，则添加到末尾</item>
	/// <item>如果有值且索引合法，调用<see cref="Replace(TreeNode{T}, TreeNode{T})"/></item>
	/// <item>如果为null且索引合法，调用<see cref="RemoveAt(int)"/></item>
	/// <item>如果值为子节点，则调用<see cref="IOrderedTree{TValue, TNode}.MoveChild(int, int)"/></item>
	/// </list>
	/// </remarks>
	public TreeNode<T> this[int index]
	{
		get => ChildrenNode[index]; set
		{
			if (index < 0 || index > Count)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}
			else if (index == Count)
			{
				if (value == null)
				{
					throw new ArgumentOutOfRangeException(nameof(index));
				}
				else
				{
					AddAt(index, value);
				}
			}
			else if (value == null)
			{
				RemoveAt(index); // 移除指定索引的节点   
			}
			else
			{
				if (this.Contains(value))
				{
					this.MoveChild(value.Index, index);
				}
				else
				{
					Replace(ChildrenNode[index], value);
				}
			}
		}
	}

	/// <inheritdoc/>
	public TreeNode<T> AddAt(int index, TreeNode<T> node)
	{
		ArgumentNullException.ThrowIfNull(node);
		if (this.CommonParent(node) == node)
		{
			throw new ArgumentException("Cannot add ancestor node to itself");
		}
		node.Parent?.Remove(node);
		node.Parent = this;
		ChildrenNode.Insert(index, node);
		this[index..].UpdateIndex();
		return node;
	} 
	/// <inheritdoc/>

	public void AddAt(int index, IEnumerable<TreeNode<T>> nodes)
	{
		ArgumentNullException.ThrowIfNull(nodes);
		var list = nodes.ToList();
		foreach (var node in list)
		{
			ArgumentNullException.ThrowIfNull(node);
			if (this.CommonParent(node) == node)
			{
				throw new ArgumentException("Cannot add ancestor node to itself");
			}
		}
		ChildrenNode.InsertRange(index, list);
		foreach (var node in list)
		{
			node.Parent?.Remove(node);
			node.Parent = this;
		}
		this[index..].UpdateIndex();
	}

	/// <inheritdoc/>
	public void AddAt(int index, params scoped ReadOnlySpan<TreeNode<T>> nodes)
	{
		foreach (var node in nodes)
		{
			ArgumentNullException.ThrowIfNull(node);
			if (this.CommonParent(node) == node)
			{
				throw new ArgumentException("Cannot add ancestor node to itself");
			}
		}
		ChildrenNode.InsertRange(index, nodes);
		foreach (var node in nodes)
		{
			node.Parent?.Remove(node);
			node.Parent = this;
		}
		this[index..].UpdateIndex();
	}

	/// <inheritdoc/>
	public TreeNode<T> AddAt(int index, T value)
	{
		var node = new TreeNode<T>(value);
		node.Parent = this;
		ChildrenNode.Insert(index, node);
		this[index..].UpdateIndex();
		return node;
	}

	/// <inheritdoc/>
	public void AddAt(int index, params IEnumerable<T> values)
	{
		var nodes = values.Select(value => new TreeNode<T>(value)).ToArray();
		ChildrenNode.InsertRange(index, nodes);
		foreach (var node in nodes)
		{
			node.Parent = this;
		}
		this[index..].UpdateIndex();
	}


	/// <inheritdoc/>
	public bool Replace(TreeNode<T> oldNode, TreeNode<T> newNode)
	{
		if (!this.Contains(oldNode))
		{
			return false;
		}
		if (this.Contains(newNode))
		{
			throw new ArgumentException("new node already exists");
		}
		var index = oldNode.Index;
		newNode.Parent?.Remove(newNode);
		ChildrenNode[index] = newNode;
		newNode.Parent = this;
		newNode.Index = index;
		oldNode.Index = -1;
		oldNode.Parent = null;
		IncrementVersion();
		return true;
	}

	/// <inheritdoc/>
	public TreeNode<T>? Remove(TreeNode<T> item)
	{
		if (!this.Contains(item))
		{
			return null;
		}
		item.Parent = null;
		ChildrenNode.Remove(item);
		this[item.Index..].UpdateIndex();
		item.Index = -1;
		return item;
	}

	/// <inheritdoc/>
	public void CopyTo(TreeNode<T>[] array, int arrayIndex)
	{
		((ICollection<TreeNode<T>>)ChildrenNode).CopyTo(array, arrayIndex);
	}

	/// <inheritdoc/>
	public IEnumerator<TreeNode<T>> GetEnumerator()
	{
		return ((IEnumerable<TreeNode<T>>)ChildrenNode).GetEnumerator();
	}

	/// <inheritdoc/>
#pragma warning disable CS8766 // 返回类型中引用类型的为 Null 性与隐式实现的成员不匹配(可能是由于为 Null 性特性)。
	public TreeNode<T>? RemoveAt(int index)
#pragma warning restore CS8766 // 返回类型中引用类型的为 Null 性与隐式实现的成员不匹配(可能是由于为 Null 性特性)。
	{
		return Remove(ChildrenNode[index]);
	}

	/// <inheritdoc/>
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
		string[]? nodeLines = null;
		string? nodetext = Value?.ToString();
		if (string.IsNullOrEmpty(nodetext))
		{
			sb.AppendJoin(null, stack.Reverse()).AppendLine();
		}
		else
		{
			nodeLines = nodetext.Split('\n');
			sb.AppendJoin(null, stack.Reverse()).AppendLine(nodeLines.First());
			if (nodeLines.Length == 1)
			{
				nodeLines = null;
			}
		}
		stack.Push(stack.Pop() switch
		{
			V0 => V2,
			V1 => V3,
			_ => ""
		});
		if (nodeLines != null)
		{
			for (int i = 1; i < nodeLines.Length; i++)
			{
				sb.AppendJoin(null, stack.Reverse()).AppendLine(nodeLines[i]);
			}
		}
		foreach (var node in this)
		{
			stack.Push(node.GetNextSibling() != null ? V1 : V0);
			node.Append(sb, stack);
		}
		stack.Pop();
	}

	/// <inheritdoc/>
	public int RemoveAll(Predicate<TreeNode<T>>? match = null)
	{
		int removed = 0;
		foreach (var node in this)
		{
			if (match == null || match(node))
			{
				node.Parent = null;
				node.Index = -1;
				removed++;
			}
		}
		if (removed > 0)
		{
			this[0..].UpdateIndex();
		}
		return removed;
	}

	/// <summary>
	/// 遍历树
	/// </summary>
	/// <returns></returns>
	public IEnumerable<TreeNode<T>> EnumTree()
	{
		var parent = Parent;
		var index = Index;
		yield return this;
		foreach (var item in this.GetFirstChild()?.EnumTree() ?? [])
		{
			if (parent != Parent)
			{
				break;
			}
			index = Index;
			yield return item;
		}
		if (parent != null && parent.Count > index + 1)
		{
			foreach (var item in parent[index + 1].EnumTree() ?? [])
			{
				yield return item;
			}
		}
	}
	void IOrderedTree<T, TreeNode<T>>.IncrementVersion()
	{
		IncrementVersion();
	}
	private void IncrementVersion()
	{
		Version++;
	}

	IOrderedTree<T, TreeNode<T>>.ISlice IOrderedTree<T, TreeNode<T>>.Slice(int start, int length)
	{
		return Slice(start, length);
	}

	/// <inheritdoc cref="IOrderedTree{TValue, TNode}.Slice(int, int)"/>
	public NodeListSlice Slice(int start, int length)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(start);
		ArgumentOutOfRangeException.ThrowIfNegative(length);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(start + length, Count);
		return new NodeListSlice(this, start, length);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)ChildrenNode).GetEnumerator();
	}

	/// <summary>
	/// 节点列表切片
	/// </summary>
	/// <param name="listNode">切片的节点</param>
	/// <param name="start">开始位置</param>
	/// <param name="length">长度</param>
	/// <remarks>调用任何修改集合的方法都会使切片失效</remarks>
	public readonly struct NodeListSlice(TreeNode<T> listNode, int start, int length) : IOrderedTree<T, TreeNode<T>>.ISlice
	{
		private readonly Span<TreeNode<T>> Span => CollectionsMarshal.AsSpan(listNode.ChildrenNode).Slice(start, length);
		private readonly int _version = listNode.Version;

		/// <summary>
		/// 索引器
		/// </summary>
		/// <param name="index">索引</param>
		/// <returns></returns>
		public TreeNode<T> this[int index]
		{
			get
			{
				ValidateVersion();
				return Span[index];
			}
		}

		/// <inheritdoc/>
		public int Count
		{
			get
			{
				ValidateVersion();
				return Span.Length;
			}
		}


		/// <inheritdoc/>
		public bool IsValid => listNode.Version != _version;


		/// <inheritdoc/>
		public bool Contains(T value)
		{
			ValidateVersion();
			foreach (ref readonly var node in Span)
			{
				if (EqualityComparer<T>.Default.Equals(node.Value, value))
					return true;
			}
			return false;
		}

		/// <inheritdoc/>
		public Span<TreeNode<T>>.Enumerator GetEnumerator()
		{
			ValidateVersion();
			return Span.GetEnumerator();
		}

		/// <inheritdoc/>
		public int IndexOf(T value)
		{
			ValidateVersion();
			foreach (var item in Span)
			{
				if (EqualityComparer<T>.Default.Equals(item.Value, value))
					return item.Index;
			}
			return -1;
		}


		/// <inheritdoc/>
		public int LastIndexOf(T value)
		{
			ValidateVersion();
			var span = Span;
			for (int i = Count - 1; i >= 0; i--)
			{
				if (EqualityComparer<T>.Default.Equals(span[i].Value, value))
					return span[i].Index;
			}
			return -1;
		}

		/// <inheritdoc/>
		public TreeNode<T>? Remove(T value)
		{
			ValidateVersion();
			if (IndexOf(value) < 0)
			{
				return default;
			}
			return listNode.RemoveAt(IndexOf(value));
		}

		/// <inheritdoc/>
		public  int RemoveAll(Predicate<TreeNode<T>>? match = null)
		{
			ValidateVersion();
			int removed = 0;
			for (int i = Span.Length - 1; i >= 0; i--)
			{
				if (match == null || match(Span[i]))
				{
					listNode.RemoveAt(start + i);
					removed++;
				}
			}
			if (removed > 0)
				listNode.IncrementVersion();
			return removed;
		}

		/// <inheritdoc/>
		public void RotateForward()
		{
			ValidateVersion();
			if (Span.Length < 2)
				return;

			var first = Span[0];
			Span[..^1].CopyTo(Span[1..]);
			Span[^1] = first;
			UpdateIndex();
		}

		/// <inheritdoc/>
		public  void RotateBackward()
		{
			ValidateVersion();
			if (Span.Length < 2)
				return;

			var last = Span[^1];
			Span[1..].CopyTo(Span[..^1]);
			Span[0] = last;
			UpdateIndex();
		}

		/// <inheritdoc/>
		public  void UpdateIndex()
		{
			ValidateVersion();
			for (int i = 0; i < Span.Length; i++)
			{
				Span[i].Index = start + i;
			}
			listNode.IncrementVersion();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private  void ValidateVersion()
		{
			if (listNode.Version != _version)
				throw new InvalidOperationException("Collection was modified after slice creation");
		}

		IEnumerator<TreeNode<T>> IEnumerable<TreeNode<T>>.GetEnumerator()
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}
	}
}
