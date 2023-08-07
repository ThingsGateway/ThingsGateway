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

using Furion.DependencyInjection;

using System.Collections.Concurrent;
using System.Reflection;

namespace ThingsGateway.Admin.Core;

/// <summary>
/// 对象拓展
/// </summary>
[SuppressSniffer]
public static class CollectionExtensions
{



    /// <summary>
    /// 从并发字典中删除
    /// </summary>
    public static bool Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key)
    {
        return dict.TryRemove(key, out TValue _);
    }
    /// <summary>
    /// 分页拓展
    /// </summary>
    /// <returns></returns>
    public static SqlSugarPagedList<TEntity> ToPagedList<TEntity>(this IEnumerable<TEntity> entity, BasePageInput basePageInput = null, bool isAll = false)
        where TEntity : new()
    {
        if (isAll)
        {
            entity = Sort(basePageInput, entity);
            var data = new SqlSugarPagedList<TEntity>
            {
                Current = 1,
                Size = entity?.Count() ?? 0,
                Records = entity,
                Total = entity?.Count() ?? 0,
                Pages = 1,
                HasNextPages = false,
                HasPrevPages = false
            };
            return data;
        }

        int _PageIndex = basePageInput.Current;
        int _PageSize = basePageInput.Size;
        var num = entity.Count();
        var pageConut = (double)num / _PageSize;
        int PageConut = (int)Math.Ceiling(pageConut);
        IEnumerable<TEntity> list = new List<TEntity>();
        entity = Sort(basePageInput, entity);
        if (PageConut >= _PageIndex)
        {
            list = entity.Skip((_PageIndex - 1) * _PageSize).Take(_PageSize);
        }
        return new SqlSugarPagedList<TEntity>
        {
            Current = _PageIndex,
            Size = _PageSize,
            Records = list,
            Total = entity?.Count() ?? 0,
            Pages = PageConut,
            HasNextPages = _PageIndex < PageConut,
            HasPrevPages = _PageIndex - 1 > 0
        };

        static IEnumerable<TEntity> Sort(BasePageInput basePageInput, IEnumerable<TEntity> list)
        {
            if (basePageInput != null && basePageInput.SortField != null)
            {
                for (int i = 0; i < basePageInput.SortField.Count; i++)
                {
                    var pro = typeof(TEntity).GetRuntimeProperty(basePageInput.SortField[i]);
                    if (!basePageInput.SortDesc[i])
                        list = list.OrderBy(a => pro.GetValue(a));
                    else
                        list = list.OrderByDescending(a => pro.GetValue(a));
                }
            }
            return list;
        }
    }


    /// <summary>
    /// 移除符合条件的元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="where"></param>
    public static void RemoveWhere<T>(this ICollection<T> @this, Func<T, bool> @where)
    {
        foreach (var obj in @this.Where(where).ToList())
        {
            @this.Remove(obj);
        }
    }

    /// <summary>
    /// 添加多个元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="values"></param>
    public static void AddRange<T>(this ICollection<T> @this, IEnumerable<T> values)
    {
        foreach (var obj in values)
        {
            @this.Add(obj);
        }
    }

}