//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using SqlSugar;

using System.Linq.Expressions;

using ThingsGateway.Core;

namespace ThingsGateway.Admin.Application;

public static class SqlSugarExtensions
{
    public static async Task<bool> UpdateRangeAsync<T>(this SqlSugarClient db, List<T> updateObjs) where T : class, new()
    {
        return await db.Updateable(updateObjs).ExecuteCommandAsync() > 0;
    }

    public static async Task<bool> UpdateSetColumnsTrueAsync<T>(this SqlSugarClient db, Expression<Func<T, T>> columns, Expression<Func<T, bool>> whereExpression) where T : class, new()
    {
        return await db.Updateable<T>().SetColumns(columns, appendColumnsByDataFilter: true).Where(whereExpression)
            .ExecuteCommandAsync() > 0;
    }

    public static ISugarQueryable<TEntity> ExportIgnoreColumns<TEntity>(this ISugarQueryable<TEntity> queryable)
    {
        return queryable.IgnoreColumns(
               new string[]
                {
           nameof(BaseEntity.Id),
           nameof(BaseEntity.CreateTime),
           nameof(BaseEntity.CreateUser),
           nameof(BaseEntity.CreateUserId),
           nameof(BaseEntity.ExtJson),
           nameof(BaseEntity.IsDelete),
           nameof(BaseEntity.UpdateTime),
           nameof(BaseEntity.UpdateUser),
           nameof(BaseEntity.UpdateUserId),
                }
          );
    }

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

    public static async Task<SqlSugarPagedList<TEntity>> ToPagedListAsync<TEntity>(this ISugarQueryable<TEntity> queryable,
        int current, int size)
    {
        RefAsync<int> totalCount = 0;
        var records = await queryable.ToPageListAsync(current, size, totalCount);
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
        var records = await queryable.ToPageListAsync(current, size, totalCount);
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
    /// SqlSugar分页扩展，查询前扩展转换实体类
    /// </summary>

    public static async Task<SqlSugarPagedList<TResult>> ToPagedListAsync<TEntity, TResult>(
        this ISugarQueryable<TEntity> queryable, int pageIndex, int pageSize, Expression<Func<TEntity, TResult>> expression)
    {
        RefAsync<int> totalCount = 0;
        var items = await queryable.ToPageListAsync(pageIndex, pageSize, totalCount, expression);
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
}