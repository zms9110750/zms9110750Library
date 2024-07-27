using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace zms9110750Library.Obsolete.Complete;


public sealed class TreeNode<TValue>(TValue? value) : IEquatable<TValue>, IList<TreeNode<TValue>>
{
    #region 字段
    private List<TreeNode<TValue>> _children = [];
    private TreeNode<TValue>? _parent;
    private TreeNode<TValue>? _root;
    private int _level;
    private int _index;
    #endregion
    #region MyRegion

    #endregion
    /// <summary>
    /// 此节点保存的值
    /// </summary>
    public TValue? Value { get; set; } = value;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public TreeNode<TValue> this[int index]
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
    public int Count => _children.Count;
    public int Level
    {
        get => _level;
        set
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
    public int Index
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
    [AllowNull]
    public TreeNode<TValue> Root
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
    public TreeNode<TValue>[] AddAt(Index index, params TreeNode<TValue>[] node)
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
            item.RemoveInList();
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
    public TreeNode<TValue> RemoveSelf()
    {
        RemoveInList();
        _parent = null;
        _index = 0;
        Root = null;
        Level = 0;
        return this;
    }
    private void RemoveInList()
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

    #region IEnumerator 
    public IEnumerator<TreeNode<TValue>> GetEnumerator()
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
    #region 常用节点
    public TreeNode<TValue> RequiredParent => Parent ?? throw new InvalidOperationException("根节点不能执行需要父节点的操作");
    public TreeNode<TValue>? Previous => Parent != null && Index > 0 ? Parent[Index - 1] : null;
    public TreeNode<TValue>? Next => Parent != null && Index < Parent.Count - 1 ? Parent[Index + 1] : null;
    public TreeNode<TValue>? First => Count == 0 ? null : this[0];
    public TreeNode<TValue>? Last => Count == 0 ? null : this[^1];
    bool ICollection<TreeNode<TValue>>.IsReadOnly { get; }
    #endregion
    #region 添加节点  
    public TreeNode<TValue>[] AddFirst(params TreeNode<TValue>[] node) => AddAt(0, node);
    public TreeNode<TValue>[] AddLast(params TreeNode<TValue>[] node) => AddAt(^0, node);
    public TreeNode<TValue>[] AddAfter(params TreeNode<TValue>[] node) => RequiredParent.AddAt(Index + 1, node);
    public TreeNode<TValue>[] AddBefore(params TreeNode<TValue>[] node) => RequiredParent.AddAt(Index, node);
    #endregion
    #region 删除节点 
    public TreeNode<TValue> RemoveAt(Index index) => this[index].RemoveSelf();
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
    public TreeNode<TValue> RemoveBefore<T>() => Previous!.RemoveSelf();
    public TreeNode<TValue> RemoveAfter<T>() => Next!.RemoveSelf();
    public TreeNode<TValue>? RemoveFirst<T>(T value, Range? range = default)
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
    public TreeNode<TValue>? RemoveLast<T>(T value, Range? range = default)
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
    public Span<TreeNode<TValue>> Slice(int start, int length) => default;
    public TreeNode<TValue>? RemoveBefore<T>(T value) => RequiredParent.RemoveLast(value, ..(Index - 1));
    public TreeNode<TValue>? RemoveAfter<T>(T value) => RequiredParent.RemoveFirst(value, (Index + 1)..);
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
    public int IndexOf(TreeNode<TValue> item) => Contains(item) ? -1 : item.Index;
    public bool Contains(TreeNode<TValue> item) => item?.Parent == this;
    public bool Contains(TValue item, Range? range = default) => IndexOf(item, range) > -1;
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
    #region 接口
    void IList<TreeNode<TValue>>.Insert(int index, TreeNode<TValue> item) => AddAt(index, item);
    void IList<TreeNode<TValue>>.RemoveAt(int index) => RemoveAt(index);
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
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion
}