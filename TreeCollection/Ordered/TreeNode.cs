using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using zms9110750.TreeCollection.Abstract;
using zms9110750.TreeCollection.Ordered;

namespace zms9110750.TreeCollection.Ordered;
[JsonConverter(typeof(TreeListNodeConverterFactory))]
public class TreeNode<T>(T value) : RootNode<T, TreeNode<T>>(value), IList<TreeNode<T>>, IOrderedTree<T, TreeNode<T>>
{
	protected List<TreeNode<T>> Children { get; } = new();
	public int Index { get; protected set; } = -1;

	public int Count => Children.Count;

	public bool IsReadOnly => ((ICollection<TreeNode<T>>)Children).IsReadOnly;

	protected int Version { get; set; }
	protected override IEnumerable<TreeNode<T>> ChildrenNode => Children;

	public TreeNode<T> this[int index]
	{
		get => Children[index]; set
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
					Replace(Children[index], value);
				}
			}
		}
	}
	public TreeNode<T> AddAt(int index, TreeNode<T> node)
	{
		ArgumentNullException.ThrowIfNull(node);
		if (this.CommonParent(node) == node)
		{
			throw new ArgumentException("Cannot add ancestor node to itself");
		}
		node.Parent?.Remove(node);
		node.Parent = this;
		Children.Insert(index, node);
		this[index..].UpdateIndex();
		return node;
	}

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
		Children.InsertRange(index, list);
		foreach (var node in list)
		{
			node.Parent?.Remove(node);
			node.Parent = this;
		}
		this[index..].UpdateIndex();
	}

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
		Children.InsertRange(index, nodes);
		foreach (var node in nodes)
		{
			node.Parent?.Remove(node);
			node.Parent = this;
		}
		this[index..].UpdateIndex();
	}
	public TreeNode<T> AddAt(int index, T value)
	{
		var node = new TreeNode<T>(value);
		node.Parent = this;
		Children.Insert(index, node);
		this[index..].UpdateIndex();
		return node;
	}

	public void AddAt(int index, params IEnumerable<T> values)
	{
		var nodes = values.Select(value => new TreeNode<T>(value)).ToArray();
		Children.InsertRange(index, nodes);
		foreach (var node in nodes)
		{
			node.Parent = this;
		}
		this[index..].UpdateIndex();
	}

	public static TreeNode<T> Create(T value)
	{
		return new TreeNode<T>(value);
	}

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
		Children[index] = newNode;
		newNode.Parent = this;
		newNode.Index = index;
		oldNode.Index = -1;
		oldNode.Parent = null;
		IncrementVersion();
		return true;
	}

	public TreeNode<T>? Remove(TreeNode<T> item)
	{
		if (!this.Contains(item))
		{
			return null;
		}
		item.Parent = null;
		Children.Remove(item);
		this[item.Index..].UpdateIndex();
		item.Index = -1;
		return item;
	}

	public void CopyTo(TreeNode<T>[] array, int arrayIndex)
	{
		((ICollection<TreeNode<T>>)Children).CopyTo(array, arrayIndex);
	}

	public IEnumerator<TreeNode<T>> GetEnumerator()
	{
		return ((IEnumerable<TreeNode<T>>)Children).GetEnumerator();
	}

	public TreeNode<T>? RemoveAt(int index)
	{
		return Remove(Children[index]);
	}

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
	public NodeListSlice Slice(int start, int length)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(start);
		ArgumentOutOfRangeException.ThrowIfNegative(length);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(start + length, Count);
		return new NodeListSlice(this, start, length);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)Children).GetEnumerator();
	}

	public struct NodeListSlice(TreeNode<T> listNode, int start, int length) : IOrderedTree<T, TreeNode<T>>.ISlice
	{
		private readonly Span<TreeNode<T>> Span => CollectionsMarshal.AsSpan(listNode.Children).Slice(start, length);
		private readonly int _version = listNode.Version;

		public TreeNode<T> this[int index]
		{
			get
			{
				ValidateVersion();
				return Span[index];
			}
		}

		public int Count
		{
			get
			{
				ValidateVersion();
				return Span.Length;
			}
		}

		public bool IsValid => listNode.Version != _version;

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

		public Span<TreeNode<T>>.Enumerator GetEnumerator()
		{
			ValidateVersion();
			return Span.GetEnumerator();
		}

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

		public TreeNode<T>? Remove(T value)
		{
			ValidateVersion();
			if (IndexOf(value) < 0)
			{
				return default;
			}
			return listNode.RemoveAt(IndexOf(value));
		}

		public int RemoveAll(Predicate<TreeNode<T>>? match = null)
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

		public void RotateBackward()
		{
			ValidateVersion();
			if (Span.Length < 2)
				return;

			var last = Span[^1];
			Span[1..].CopyTo(Span[..^1]);
			Span[0] = last;
			UpdateIndex();
		}

		public void UpdateIndex()
		{
			ValidateVersion();
			for (int i = 0; i < Span.Length; i++)
			{
				Span[i].Index = start + i;
			}
			listNode.IncrementVersion();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ValidateVersion()
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
