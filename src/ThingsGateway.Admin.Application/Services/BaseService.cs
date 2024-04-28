
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------




using BootstrapBlazor.Components;

using Microsoft.Extensions.Localization;

using SqlSugar;

namespace ThingsGateway.Admin.Application;

public abstract class BaseService<T> : IDisposable where T : class, new()
{
    protected IStringLocalizer Localizer { get; }

    protected BaseService()
    {
        this.Localizer = App.CreateLocalizerByType(typeof(T))!;
    }

    protected SqlSugarClient GetDB()
    {
        return DbContext.Db.GetConnectionScopeWithAttr<T>().CopyNew();
    }

    protected virtual async Task<bool> DeleteAsync(IEnumerable<long> models)
    {
        using var db = GetDB();
        return await db.Deleteable<T>().In(models.ToList()).ExecuteCommandHasChangeAsync();
    }

    protected virtual async Task<bool> SaveAsync(T model, ItemChangedType changedType)
    {
        using var db = GetDB();
        if (changedType == ItemChangedType.Add)
        {
            return (await db.Insertable(model).ExecuteCommandAsync()) > 0;
        }
        else
        {
            return (await db.Updateable(model).ExecuteCommandAsync()) > 0;
        }
    }

    protected async Task<QueryData<T>> QueryAsync(QueryPageOptions option, Func<ISugarQueryable<T>, ISugarQueryable<T>>? queryFunc = null)
    {
        var ret = new QueryData<T>()
        {
            IsSorted = option.SortOrder != SortOrder.Unset,
            IsFiltered = option.Filters.Any(),
            IsAdvanceSearch = option.AdvanceSearches.Any(),
            IsSearch = option.Searches.Any() || option.CustomerSearches.Any()
        };

        using var db = GetDB();
        var query = db.GetQuery<T>(option);
        if (queryFunc != null)
            query = queryFunc(query);
        if (option.IsPage)
        {
            RefAsync<int> totalCount = 0;

            var items = await query
                .ToPageListAsync(option.PageIndex, option.PageItems, totalCount);

            ret.TotalCount = totalCount;
            ret.Items = items;
        }
        else if (option.IsVirtualScroll)
        {
            RefAsync<int> totalCount = 0;

            var items = await query
                .ToPageListAsync(option.StartIndex, option.PageItems, totalCount);

            ret.TotalCount = totalCount;
            ret.Items = items;
        }
        else
        {
            var items = await query
                .ToListAsync();
            ret.TotalCount = items.Count;
            ret.Items = items;
        }
        return ret;
    }

    public bool IsDisposed { get; private set; }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
    }
}