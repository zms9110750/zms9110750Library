using System.Collections;
using System.Collections.Concurrent;

namespace zms9110750.Utils.Primitives;

/// <summary>
/// 延迟操作作用域，用于批量管理释放操作和延迟执行的动作
/// </summary>
public sealed class DeferredActionScope : ICollection<IDisposable>, IDisposable
{
    private readonly HashSet<IDisposable> _disposables = new();
    private readonly ConcurrentStack<Action> _actions = new();

    public int Count => ((ICollection<IDisposable>)_disposables).Count;

    public bool IsReadOnly => ((ICollection<IDisposable>)_disposables).IsReadOnly;

    /// <summary>
    /// 添加一个释放资源
    /// </summary>
    public void Add(IDisposable item)
    {
        ((ICollection<IDisposable>)_disposables).Add(item);
    }

    /// <summary>
    /// 添加一个延迟执行的动作
    /// </summary>
    public void Add(Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));
        _actions.Push(action);
    }

    public void Clear()
    {
        ((ICollection<IDisposable>)_disposables).Clear();
        _actions.Clear();
    }

    public bool Contains(IDisposable item)
    {
        return ((ICollection<IDisposable>)_disposables).Contains(item);
    }

    public void CopyTo(IDisposable[] array, int arrayIndex)
    {
        ((ICollection<IDisposable>)_disposables).CopyTo(array, arrayIndex);
    }

    public void Dispose()
    {
        // 执行委托（后进先出，由 Stack 本身保证）
        while (_actions.TryPop(out var action))
        {
            action();
        }

        // 释放资源
        foreach (var item in _disposables)
        {
            item.Dispose();
        }
        _disposables.Clear();
    }

    public IEnumerator<IDisposable> GetEnumerator()
    {
        return ((IEnumerable<IDisposable>)_disposables).GetEnumerator();
    }

    public bool Remove(IDisposable item)
    {
        return ((ICollection<IDisposable>)_disposables).Remove(item);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_disposables).GetEnumerator();
    }
}