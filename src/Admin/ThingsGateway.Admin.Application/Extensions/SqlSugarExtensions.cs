//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using SqlSugar;

using System.Linq.Expressions;
using System.Reflection;

namespace ThingsGateway.Admin.Application;

/// <inheritdoc/>
[ThingsGateway.DependencyInjection.SuppressSniffer]
public static class SqlSugarExtensions
{
    /// <inheritdoc/>
    public static ISugarQueryable<TEntity> ExportIgnoreColumns<TEntity>(this ISugarQueryable<TEntity> queryable)
    {
        return queryable.IgnoreColumns(
               [
           nameof(BaseEntity.Id),
           nameof(BaseEntity.CreateTime),
           nameof(BaseEntity.CreateUser),
           nameof(BaseEntity.CreateUserId),
           nameof(BaseDataEntity.CreateOrgId),
           nameof(BaseEntity.ExtJson),
           nameof(BaseEntity.IsDelete),
           nameof(BaseEntity.UpdateTime),
           nameof(BaseEntity.UpdateUser),
           nameof(BaseEntity.UpdateUserId),
                ]
          );
    }

    /// <inheritdoc/>
    public static SqlSugarPagedList<TEntity> ToPagedList<TEntity>(this ISugarQueryable<TEntity> queryable, int current,
        int size)
    {
        var total = 0;
        var records = queryable.ToPageList(current, size, ref total);
        var pages = (int)Math.Ceiling(total / (double)size);
        return new SqlSugarPagedList<TEntity>
        {
            Current = current,
            Size = size,
            Records = records,
            Total = total,
            Pages = pages,
            HasNextPages = current < pages,
            HasPrevPages = current - 1 > 0
        };
    }

    /// <summary>
    /// SqlSugar分页扩展,查询出结果后再转换实体类
    /// </summary>
    public static SqlSugarPagedList<TResult> ToPagedList<TEntity, TResult>(this ISugarQueryable<TEntity> queryable,
        int current, int size)
    {
        var totalCount = 0;
        var records = queryable.ToPageList(current, size, ref totalCount);
        var totalPages = (int)Math.Ceiling(totalCount / (double)size);
        return new SqlSugarPagedList<TResult>
        {
            Current = current,
            Size = size,
            Records = records.Cast<TResult>(),
            Total = totalCount,
            Pages = totalPages,
            HasNextPages = current < totalPages,
            HasPrevPages = current - 1 > 0
        };
    }

    /// <summary>
    /// SqlSugar分页扩展，查询前扩展转换实体类
    /// </summary>
    public static SqlSugarPagedList<TResult> ToPagedList<TEntity, TResult>(this ISugarQueryable<TEntity> queryable, int pageIndex,
        int pageSize, Expression<Func<TEntity, TResult>> expression)
    {
        var totalCount = 0;
        var items = queryable.ToPageList(pageIndex, pageSize, ref totalCount, expression);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return new SqlSugarPagedList<TResult>
        {
            Current = pageIndex,
            Size = pageSize,
            Records = items,
            Total = totalCount,
            Pages = totalPages,
            HasNextPages = pageIndex < totalPages,
            HasPrevPages = pageIndex - 1 > 0
        };
    }

    /// <summary>
    /// 分页查询
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">数据列表</param>
    /// <param name="basePageInput">参数</param>
    /// <param name="isAll">不分页</param>
    /// <returns>分页集合</returns>
    public static SqlSugarPagedList<T> ToPagedList<T>(this IEnumerable<T> list, BasePageInput basePageInput = null, bool isAll = false)
    {
        if (isAll)
        {
            list = Sort(list, basePageInput);
            var data = new SqlSugarPagedList<T>
            {
                Current = 1,
                Size = list?.Count() ?? 0,
                Records = list,
                Total = list?.Count() ?? 0,
                Pages = 1,
                HasNextPages = false,
                HasPrevPages = false
            };
            return data;
        }

        int _PageIndex = basePageInput.Current;
        int _PageSize = basePageInput.Size;
        var num = list.Count();
        var pageConut = (double)num / _PageSize;
        int PageConut = (int)Math.Ceiling(pageConut);
        list = Sort(list, basePageInput);
        if (PageConut >= _PageIndex)
        {
            list = list.Skip((_PageIndex - 1) * _PageSize).Take(_PageSize);
        }
        return new SqlSugarPagedList<T>
        {
            Current = _PageIndex,
            Size = _PageSize,
            Records = list,
            Total = num,
            Pages = PageConut,
            HasNextPages = _PageIndex < PageConut,
            HasPrevPages = _PageIndex - 1 > 0
        };
    }

    /// <summary>
    /// SqlSugar分页扩展，查询前扩展转换实体类
    /// </summary>
    public static async Task<SqlSugarPagedList<TEntity>> ToPagedListAsync<TEntity>(this ISugarQueryable<TEntity> queryable,
        int current, int size)
    {
        RefAsync<int> totalCount = 0;
        var records = await queryable.ToPageListAsync(current, size, totalCount).ConfigureAwait(false);
        var totalPages = (int)Math.Ceiling(totalCount / (double)size);
        return new SqlSugarPagedList<TEntity>
        {
            Current = current,
            Size = size,
            Records = records,
            Total = (int)totalCount,
            Pages = totalPages,
            HasNextPages = current < totalPages,
            HasPrevPages = current - 1 > 0
        };
    }

    /// <summary>
    /// SqlSugar分页扩展,查询出结果后再转换实体类
    /// </summary>
    public static async Task<SqlSugarPagedList<TResult>> ToPagedListAsync<TEntity, TResult>(this ISugarQueryable<TEntity> queryable,
        int current, int size)
    {
        RefAsync<int> totalCount = 0;
        var records = await queryable.ToPageListAsync(current, size, totalCount).ConfigureAwait(false);
        var totalPages = (int)Math.Ceiling(totalCount / (double)size);
        return new SqlSugarPagedList<TResult>
        {
            Current = current,
            Size = size,
            Records = records.Cast<TResult>(),
            Total = (int)totalCount,
            Pages = totalPages,
            HasNextPages = current < totalPages,
            HasPrevPages = current - 1 > 0
        };
    }

    /// <summary>
    /// SqlSugar分页扩展，查询前扩展转换实体类
    /// </summary>
    public static async Task<SqlSugarPagedList<TResult>> ToPagedListAsync<TEntity, TResult>(
        this ISugarQueryable<TEntity> queryable, int pageIndex, int pageSize, Expression<Func<TEntity, TResult>> expression)
    {
        RefAsync<int> totalCount = 0;
        var items = await queryable.ToPageListAsync(pageIndex, pageSize, totalCount, expression).ConfigureAwait(false);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return new SqlSugarPagedList<TResult>
        {
            Current = pageIndex,
            Size = pageSize,
            Records = items,
            Total = (int)totalCount,
            Pages = totalPages,
            HasNextPages = pageIndex < totalPages,
            HasPrevPages = pageIndex - 1 > 0
        };
    }

    /// <inheritdoc/>
    public static async Task<bool> UpdateRangeAsync<T>(this SqlSugarClient db, List<T> updateObjs) where T : class, new()
    {
        return await db.Updateable(updateObjs).ExecuteCommandAsync().ConfigureAwait(false) > 0;
    }

    /// <inheritdoc/>
    public static async Task<bool> UpdateSetColumnsTrueAsync<T>(this SqlSugarClient db, Expression<Func<T, T>> columns, Expression<Func<T, bool>> whereExpression) where T : class, new()
    {
        return await db.Updateable<T>().SetColumns(columns, appendColumnsByDataFilter: true).Where(whereExpression)
            .ExecuteCommandAsync().ConfigureAwait(false) > 0;
    }

    private static IEnumerable<T> Sort<T>(this IEnumerable<T> list, BasePageInput basePageInput)
    {
        if (basePageInput != null && basePageInput.SortField != null)
        {
            for (int i = 0; i < basePageInput.SortField.Count; i++)
            {
                var pro = typeof(T).GetRuntimeProperty(basePageInput.SortField[i]);
                if (pro != null)
                {
                    if (!basePageInput.SortDesc[i])
                        list = list.OrderBy(a => pro.GetValue(a));
                    else
                        list = list.OrderByDescending(a => pro.GetValue(a));
                }
            }
        }
        return list;
    }
}
