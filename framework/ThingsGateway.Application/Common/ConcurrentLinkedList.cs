#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using System.Collections;

namespace ThingsGateway.Application;

/// <summary>
/// 线程安全的LinkedList
/// </summary>
/// <typeparam name="T"></typeparam>
public class ConcurrentLinkedList<T> : ICollection<T>
{
    private readonly LinkedList<T> _list = new();
    /// <inheritdoc cref="LinkedList{T}.Count"/>
    public int Count
    {
        get
        {
            lock (((ICollection)_list).SyncRoot)
            {
                return _list.Count;
            }
        }
    }

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    public void Add(T item)
    {
        AddFirst(item);
    }

    /// <inheritdoc cref="LinkedList{T}.AddLast(T)"/>
    public void AddLast(T value)
    {
        lock (((ICollection)_list).SyncRoot)
        {
            _list.AddLast(value);
        }
    }

    /// <summary>
    /// <inheritdoc cref="LinkedList{T}.Clear"/>
    /// </summary>
    public void Clear()
    {
        lock (((ICollection)_list).SyncRoot)
        {
            _list.Clear();
        }
    }

    /// <inheritdoc/>
    public bool Contains(T item)
    {
        lock (((ICollection)_list).SyncRoot)
        {
            return _list.Contains(item);
        }
    }

    /// <inheritdoc/>
    public void CopyTo(T[] array, int arrayIndex)
    {
        lock (((ICollection)_list).SyncRoot)
        {
            _list.CopyTo(array, arrayIndex);
        }
    }

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
        lock (((ICollection)_list).SyncRoot)
        {
            return _list.ToList().GetEnumerator();
        }
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        lock (((ICollection)_list).SyncRoot)
        {
            return GetEnumerator();
        }
    }

    /// <inheritdoc/>
    public bool Remove(T item)
    {
        lock (((ICollection)_list).SyncRoot)
        {
            return _list.Remove(item);
        }
    }

    /// <inheritdoc cref="LinkedList{T}.AddFirst(T)"/>
    private void AddFirst(T value)
    {
        lock (((ICollection)_list).SyncRoot)
        {
            _list.AddFirst(value);
        }
    }
}
