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

using SqlSugar;

using System.Linq.Expressions;

namespace ThingsGateway.Admin.Core;

/// <summary>
/// Sqlsugar分页拓展类
/// </summary>
public static class SqlSugarPageExtension
{
    /// <summary>
    /// SqlSugar分页扩展
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="queryable"></param>
    /// <param name="current"></param>
    /// <param name="size"></param>
    /// <returns></returns>
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
    /// SqlSugar分页扩展
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="queryable"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="whereExpression"></param>
    /// <returns></returns>
    public static async Task<SqlSugarPagedList<TEntity>> ToPagedListAsync<TEntity>(this ISugarQueryable<TEntity> queryable,
        int pageIndex, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null)
    {
        RefAsync<int> totalCount = 0;
        if (whereExpression != null)
            queryable = queryable.Where(whereExpression);
        var records = await queryable.ToPageListAsync(pageIndex, pageSize, totalCount);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return new SqlSugarPagedList<TEntity>
        {
            Current = pageIndex,
            Size = pageSize,
            Records = records,
            Total = (int)totalCount,
            Pages = totalPages,
            HasNextPages = pageIndex < totalPages,
            HasPrevPages = pageIndex - 1 > 0
        };
    }
}