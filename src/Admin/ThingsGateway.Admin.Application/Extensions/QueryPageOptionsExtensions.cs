//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using SqlSugar;

namespace ThingsGateway.Admin.Application;

/// <inheritdoc/>
[ThingsGateway.DependencyInjection.SuppressSniffer]
public static class QueryPageOptionsExtensions
{

    public static IEnumerable<T> GetData<T>(this IEnumerable<T> datas, QueryPageOptions option, FilterKeyValueAction where = null)
    {
        where ??= option.ToFilter();
        if (where.HasFilters())
        {
            datas = datas.Where(where.GetFilterFunc<T>());//name asc模式
        }

        if (option.SortList.Count > 0)
        {
            datas = datas.Sort(option.SortList);//name asc模式
        }
        if (option.AdvancedSortList.Count > 0)
        {
            datas = datas.Sort(option.AdvancedSortList);//name asc模式
        }
        if (option.SortOrder != SortOrder.Unset && !option.SortName.IsNullOrWhiteSpace())
        {
            datas = datas.Sort(option.SortName, option.SortOrder);
        }
        if (option.IsPage)
        {
            datas = datas.Skip((option.PageIndex - 1) * option.PageItems).Take(option.PageItems);
        }
        else if (option.IsVirtualScroll)
        {
            datas = datas.Skip((option.StartIndex) * option.PageItems).Take(option.PageItems);
        }
        return datas;
    }

    /// <summary>
    /// 根据查询条件返回sqlsugar ISugarQueryable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static ISugarQueryable<T> GetQuery<T>(this SqlSugarClient db, QueryPageOptions option, ISugarQueryable<T>? query = null, FilterKeyValueAction where = null)
    {
        query ??= db.Queryable<T>();
        where ??= option.ToFilter();

        if (where.HasFilters())
        {
            query = query.Where(where.GetFilterLambda<T>());//name asc模式
        }

        foreach (var item in option.SortList)
        {
            query = query.OrderByIF(!string.IsNullOrEmpty(item), $"{item}");//name asc模式
        }
        foreach (var item in option.AdvancedSortList)
        {
            query = query.OrderByIF(!string.IsNullOrEmpty(item), $"{item}");//name asc模式
        }
        query = query.OrderByIF(option.SortOrder != SortOrder.Unset, $"{option.SortName} {option.SortOrder}");
        return query;
    }

    /// <summary>
    /// 根据查询条件返回QueryData
    /// </summary>
    public static QueryData<T> GetQueryData<T>(this IEnumerable<T> datas, QueryPageOptions option, FilterKeyValueAction where = null)
    {
        var ret = new QueryData<T>()
        {
            IsSorted = option.SortOrder != SortOrder.Unset,
            IsFiltered = option.Filters.Count > 0,
            IsAdvanceSearch = option.AdvanceSearches.Count > 0 || option.CustomerSearches.Count > 0,
            IsSearch = option.Searches.Count > 0
        };
        var items = datas.GetData(option, where);
        ret.TotalCount = datas.Count();
        ret.Items = items.ToList();
        return ret;
    }
}
