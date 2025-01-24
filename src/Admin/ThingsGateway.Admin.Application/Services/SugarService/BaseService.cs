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

using Microsoft.Extensions.Localization;

using SqlSugar;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 通用服务
/// </summary>
/// <typeparam name="T"></typeparam>
public class BaseService<T> : IDataService<T>, IDisposable where T : class, new()
{
    /// <summary>
    /// 通用服务
    /// </summary>
    public BaseService()
    {
        Localizer = App.CreateLocalizerByType(typeof(T))!;
    }

    /// <summary>
    /// 是否已释放资源
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// 语言本地化资源
    /// </summary>
    protected IStringLocalizer Localizer { get; }

    /// <inheritdoc/>
    public Task<bool> AddAsync(T model)
    {
        return SaveAsync(model, ItemChangedType.Add);
    }

    /// <inheritdoc/>
    public Task<bool> DeleteAsync(IEnumerable<T> models)
    {
        if (models.FirstOrDefault() is IPrimaryIdEntity)
            return DeleteAsync(models.Select(a => ((IPrimaryIdEntity)a).Id));
        else
            return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public virtual async Task<bool> DeleteAsync(IEnumerable<long> models)
    {
        using var db = GetDB();
        return await db.Deleteable<T>().In(models.ToList()).ExecuteCommandHasChangeAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public Task<QueryData<T>> QueryAsync(QueryPageOptions option)
    {
        return QueryAsync(option, null, null);
    }

    /// <inheritdoc/>
    public virtual async Task<QueryData<T>> QueryAsync(QueryPageOptions option, Func<ISugarQueryable<T>, ISugarQueryable<T>>? queryFunc = null, FilterKeyValueAction where = null)
    {
        var ret = new QueryData<T>()
        {
            IsSorted = option.SortOrder != SortOrder.Unset,
            IsFiltered = option.Filters.Count > 0,
            IsAdvanceSearch = option.AdvanceSearches.Count > 0 || option.CustomerSearches.Count > 0,
            IsSearch = option.Searches.Count > 0
        };

        using var db = GetDB();
        var query = db.Queryable<T>();
        if (queryFunc != null)
            query = queryFunc(query);
        query = db.GetQuery<T>(option, query, where);

        if (option.IsPage)
        {
            RefAsync<int> totalCount = 0;

            var items = await query.ToPageListAsync(option.PageIndex, option.PageItems, totalCount).ConfigureAwait(false);

            ret.TotalCount = totalCount;
            ret.Items = items;
        }
        else if (option.IsVirtualScroll)
        {
            RefAsync<int> totalCount = 0;

            var items = await query.ToPageListAsync(option.StartIndex, option.PageItems, totalCount).ConfigureAwait(false);

            ret.TotalCount = totalCount;
            ret.Items = items;
        }
        else
        {
            var items = await query.ToListAsync().ConfigureAwait(false);
            ret.TotalCount = items.Count;
            ret.Items = items;
        }
        return ret;
    }

    /// <inheritdoc/>
    public virtual async Task<bool> SaveAsync(T model, ItemChangedType changedType)
    {
        using var db = GetDB();
        if (changedType == ItemChangedType.Add)
        {
            return (await db.Insertable(model).ExecuteCommandAsync().ConfigureAwait(false)) > 0;
        }
        else
        {
            return (await db.Updateable(model).ExecuteCommandAsync().ConfigureAwait(false)) > 0;
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
    }

    /// <summary>
    /// 获取数据库连接
    /// </summary>
    /// <returns></returns>
    protected SqlSugarClient GetDB()
    {
        return DbContext.Db.GetConnectionScopeWithAttr<T>().CopyNew();
    }
}
