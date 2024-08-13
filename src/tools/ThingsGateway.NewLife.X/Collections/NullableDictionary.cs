//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.NewLife.X.Collections;

/// <summary>可空字典。获取数据时如果指定键不存在可返回空而不是抛出异常</summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public class NullableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IDictionary<TKey, TValue> where TKey : notnull
{
    /// <summary>实例化一个可空字典</summary>
    public NullableDictionary()
    { }

    /// <summary>指定比较器实例化一个可空字典</summary>
    /// <param name="comparer"></param>
    public NullableDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { }

    /// <summary>实例化一个可空字典</summary>
    /// <param name="dic"></param>
    public NullableDictionary(IDictionary<TKey, TValue> dic) : base(dic) { }

    /// <summary>实例化一个可空字典</summary>
    /// <param name="dic"></param>
    /// <param name="comparer"></param>
    public NullableDictionary(IDictionary<TKey, TValue> dic, IEqualityComparer<TKey> comparer) : base(dic, comparer) { }

    /// <summary>获取 或 设置 数据</summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public new TValue this[TKey item]
    {
        get
        {
            if (TryGetValue(item, out var v)) return v;

            return default!;
        }
        set
        {
            base[item] = value;
        }
    }
}
