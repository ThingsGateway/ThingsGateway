//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Concurrent;

namespace NewLife.Collections;

/// <summary>并行哈希集合</summary>
/// <remarks>
/// 主要用于频繁添加删除而又要遍历的场合
/// </remarks>
public class ConcurrentHashSet<T> : IEnumerable<T> where T : notnull
{
    private readonly ConcurrentDictionary<T, Byte> _dic = new();

    /// <summary>是否空集合</summary>
    public Boolean IsEmpty => _dic.IsEmpty;

    /// <summary>元素个数</summary>
    public Int32 Count => _dic.Count;

    /// <summary>是否包含元素</summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public Boolean Contain(T item) => _dic.ContainsKey(item);

    /// <summary>尝试添加</summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public Boolean TryAdd(T item) => _dic.TryAdd(item, 0);

    /// <summary>尝试删除</summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public Boolean TryRemove(T item) => _dic.TryRemove(item, out _);

    #region IEnumerable<T> 成员

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => _dic.Keys.GetEnumerator();

    #endregion IEnumerable<T> 成员

    #region IEnumerable 成员

    IEnumerator IEnumerable.GetEnumerator() => _dic.Keys.GetEnumerator();

    #endregion IEnumerable 成员
}
