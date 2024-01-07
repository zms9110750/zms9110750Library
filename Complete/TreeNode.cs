using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace zms9110750Library.Complete;
public sealed class TreeNode<T>(T? value) : IEquatable<T>, IList<TreeNode<T>>
{
    #region 储存值
    public T? Value { get; set; } = value;
    #endregion
    #region 层级和索引
    int level;
    int index;
    public int Level
    {
        get => level;
        private set
        {
            if (level != (level = value))
            {
                foreach (var item in children)
                {
                    item.Level = level + 1;
                }
            }
        }
    }
    public int Index
    {
        get => index;
        set
        {
            var parent = RequiredParent;
            if (parent.children.Count <= value)
            {
                throw new ArgumentException("索引越界");
            }
            parent.children.RemoveAt(index);
            parent.children.Insert(value, this);
            var end = Math.Max(index, value);
            for (int i = Math.Min(index, value); i <= end; i++)
            {
                parent.children[i].index = i;
            }
        }
    }
    #endregion
    #region 父节点和根节点 
    TreeNode<T>? parent;
    TreeNode<T>? root;
    public TreeNode<T>? Parent
    {
        get => parent;
        set
        {
            if (parent != value)
            {
                RemoveSelf();
                value?.AddLast(this);
                parent = value;
            }
        }
    }
    public TreeNode<T> RequiredParent => Parent ?? throw new InvalidOperationException("根节点不能执行需要父节点的操作");
    [AllowNull]
    public TreeNode<T> Root
    {
        get => root ?? this;
        private set
        {
            if (Root != (value ?? this))
            {
                root = value;
                foreach (var item in children)
                {
                    item.Root = Root;
                }
            }
        }
    }
    #endregion
    #region 子节点集合
    List<TreeNode<T>> children = [];
    public IReadOnlyList<TreeNode<T>> Children => children;
    public int Count => children.Count;
    public TreeNode<T> this[int index]
    {
        get => children[index];
        set
        {
            if (value == null)
            {
                this.RemoveAt(index);
            }
            else if (value.Parent == this)
            {
                value.Index = index;
            }
            else
            {
                this.RemoveAt(index);
                AddAt(index, value);
            }
        }
    }
    #endregion
    #region 常用节点
    public TreeNode<T>? Previous => parent != null && index > 0 ? parent.children[index - 1] : null;
    public TreeNode<T>? Next => parent != null && index < parent.children.Count - 1 ? parent.children[index + 1] : null;
    public TreeNode<T>? First => children.Count == 0 ? null : children[0];
    public TreeNode<T>? Last => children.Count == 0 ? null : children[^1];
    #endregion
    #region 添加节点 
    public TreeNode<T>[] AddAt(Index index, params TreeNode<T>[] node)
    {
        if (node == null)
        {
            throw new ArgumentNullException(nameof(node), "node为null");
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
        var offset = index.GetOffset(children.Count);
        children.InsertRange(offset, node);
        foreach (var item in node)
        {
            item.RemoveSelf();
            item.parent = this;
            item.Root = Root;
            item.Level = Level + 1;
        }
        for (int i = offset; i < children.Count; i++)
        {
            children[i].index = i;
        }
        return node;
    }
    #endregion
    #region 删除节点
    public void RemoveSelf()
    {
        if (Parent != null)
        {
            Parent.children.RemoveAt(index);
            for (int i = index; i < Parent.children.Count; i++)
            {
                Parent.children[i].index = i;
            }
        }
        parent = null;
        Root = null;
        Level = 0;
        index = 0;
    }

    #endregion
    #region 查询节点
    public bool Equals(T? other)
    {
        return other?.Equals(Value) ?? Value == null;
    }
    public int IndexOf(T item, Range? range = default)
    {
        var (sta, len) = range.GetValueOrDefault(..).GetOffsetAndLength(Count);
        for (int i = sta; i < len; i++)
        {
            if (children[i].Equals(item))
            {
                return i;
            }
        }
        return -1;
    }
    #endregion
    #region IList
    bool ICollection<TreeNode<T>>.IsReadOnly => false;
    int IList<TreeNode<T>>.IndexOf(TreeNode<T> item) => item.Parent == this ? item.Index : -1;
    void IList<TreeNode<T>>.Insert(int index, TreeNode<T> item) => AddAt(index, item);
    void IList<TreeNode<T>>.RemoveAt(int index) => this.RemoveAt(index);
    void ICollection<TreeNode<T>>.Add(TreeNode<T> item) => this.AddLast(item);
    bool ICollection<TreeNode<T>>.Contains(TreeNode<T> item) => item.Parent == this;
    void ICollection<TreeNode<T>>.CopyTo(TreeNode<T>[] array, int arrayIndex) => children.CopyTo(array, arrayIndex);
    bool ICollection<TreeNode<T>>.Remove(TreeNode<T> item)
    {
        if (item.Parent == this)
        {
            item.RemoveSelf();
            return true;
        }
        else
        {
            return false;
        }
    }
    public void Clear() => this.RemoveAll();
    #endregion
    #region IEnumerator 
    public IEnumerator<TreeNode<T>> GetEnumerator()
    {
        var parent = Parent;
        var index = Index;
        yield return this;
        foreach (var item in First ?? Enumerable.Empty<TreeNode<T>>())
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
            foreach (var item in parent[index] ?? Enumerable.Empty<TreeNode<T>>())
            {
                yield return item;
            }
        }
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion
    #region operator 
    /// <summary>
    /// 获取两个节点的最近公共节点
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static TreeNode<T>? operator &(TreeNode<T>? left, TreeNode<T>? right)
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
    public static IEnumerable<TreeNode<T>> operator |(TreeNode<T>? left, TreeNode<T>? right)
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
    public static implicit operator TreeNode<T>(T value) => new TreeNode<T>(value);
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
        foreach (var node in children)
        {
            stack.Push(node.Next != null ? V1 : V0);
            node.Append(sb, stack);
        }
        stack.Pop();
    }
    #endregion
}
[SuppressMessage("Design", "CA1062:验证公共方法的参数", Justification = "<挂起>")]
public static class TreeNodeExtensions
{
    public static TreeNode<T>[] AddFirst<T>(this TreeNode<T> tree, params TreeNode<T>[] node) => tree.AddAt(0, node);
    public static TreeNode<T>[] AddLast<T>(this TreeNode<T> tree, params TreeNode<T>[] node) => tree.AddAt(^0, node);
    public static TreeNode<T>[] AddAfter<T>(this TreeNode<T> tree, params TreeNode<T>[] node) => tree.RequiredParent.AddAt(tree.Index + 1, node);
    public static TreeNode<T>[] AddBefore<T>(this TreeNode<T> tree, params TreeNode<T>[] node) => tree.RequiredParent.AddAt(tree.Index, node);
    public static TreeNode<T> RemoveAt<T>(this TreeNode<T> tree, Index index)
    {
        var node = tree[index];
        node.RemoveSelf();
        return node;
    }
    public static int RemoveAll<T>(this TreeNode<T> tree, Predicate<TreeNode<T>>? predicate = null)
    {
        var count = 0;
        for (int i = tree.Count - 1; i >= 0; i--)
        {
            if (predicate?.Invoke(tree[i]) ?? true)
            {
                tree[i].RemoveSelf();
            }
        }
        return count;
    }
    public static TreeNode<T>? RemoveBefore<T>(this TreeNode<T> tree)
    {
        var node = tree.Previous;
        node?.RemoveSelf();
        return node;
    }
    public static TreeNode<T>? RemoveAfter<T>(this TreeNode<T> tree)
    {
        var node = tree.Next;
        node?.RemoveSelf();
        return node;
    }
    public static TreeNode<T>? RemoveFirst<T>(this TreeNode<T> tree, T value, Range? range = default)
    {
        var (sta, len) = range.GetValueOrDefault(..).GetOffsetAndLength(tree.Count);
        for (int i = sta; i < sta + len; i++)
        {
            if (tree[i].Equals(value))
            {
                var node = tree[i];
                node.RemoveSelf();
                return node;
            }
        }
        return null;
    }
    public static TreeNode<T>? RemoveLast<T>(this TreeNode<T> tree, T value, Range? range = default)
    {
        var (sta, len) = range.GetValueOrDefault(..).GetOffsetAndLength(tree.Count);
        for (int i = sta + len - 1; i >= sta; i--)
        {
            if (tree[i].Equals(value))
            {
                var node = tree[i];
                node.RemoveSelf();
                return node;
            }
        }
        return null;
    }
    public static TreeNode<T>? RemoveBefore<T>(this TreeNode<T> tree, T value) => tree.RequiredParent.RemoveLast(value, ..(tree.Index - 1));
    public static TreeNode<T>? RemoveAfter<T>(this TreeNode<T> tree, T value) => tree.RequiredParent.RemoveFirst(value, (tree.Index + 1)..);
    public static bool Contains<T>(this TreeNode<T> tree, T item, Range? range = default) => tree.IndexOf(item, range) > -1;

}