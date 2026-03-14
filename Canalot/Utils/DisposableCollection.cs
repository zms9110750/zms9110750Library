using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Canalot.Utils;

/// <summary>
/// 可释放对象集合
/// </summary>
public sealed class DisposableCollection : ICollection<IDisposable>, IDisposable
{
    public int Count => ((ICollection<IDisposable>)Disposables).Count;

    public bool IsReadOnly => ((ICollection<IDisposable>)Disposables).IsReadOnly;

    HashSet<IDisposable> Disposables { get; } = new HashSet<IDisposable>();

    public void Add(IDisposable item)
    {
        ((ICollection<IDisposable>)Disposables).Add(item);
    }

    public void Clear()
    {
        ((ICollection<IDisposable>)Disposables).Clear();
    }

    public bool Contains(IDisposable item)
    {
        return ((ICollection<IDisposable>)Disposables).Contains(item);
    }

    public void CopyTo(IDisposable[] array, int arrayIndex)
    {
        ((ICollection<IDisposable>)Disposables).CopyTo(array, arrayIndex);
    }

    public void Dispose()
    {
        foreach (var item in Disposables)
        {
            item.Dispose();
        }
        Disposables.Clear();
    }

    public IEnumerator<IDisposable> GetEnumerator()
    {
        return ((IEnumerable<IDisposable>)Disposables).GetEnumerator();
    }

    public bool Remove(IDisposable item)
    {
        return ((ICollection<IDisposable>)Disposables).Remove(item);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Disposables).GetEnumerator();
    }
}