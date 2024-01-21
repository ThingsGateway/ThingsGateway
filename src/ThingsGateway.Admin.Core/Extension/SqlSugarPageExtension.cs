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

using System.Linq.Expressions;
using System.Reflection;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// Sqlsugar分页拓展类
/// </summary>
public static class SqlSugarPageExtension
{
    public static ISugarQueryable<TEntity> ExportIgnoreColumns<TEntity>(this ISugarQueryable<TEntity> queryable)
    {
        return queryable.IgnoreColumns(
               new string[]
                {
           nameof(BaseEntity.Id),
           nameof(BaseEntity.CreateTime),
           nameof(BaseEntity.CreateUser),
           nameof(BaseEntity.CreateUserId),
           nameof(BaseEntity.SortCode),
           nameof(BaseEntity.ExtJson),
           nameof(BaseEntity.IsDelete),
           nameof(BaseEntity.UpdateTime),
           nameof(BaseEntity.UpdateUser),
           nameof(BaseEntity.UpdateUserId),
                }
          );
    }

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
    /// <param name="current"></param>
    /// <param name="size"></param>
    /// <returns></returns>
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
    /// SqlSugar分页扩展
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="queryable"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="expression"></param>
    /// <returns></returns>
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
    /// SqlSugar分页扩展
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="queryable"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="expression"></param>
    /// <returns></returns>
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

    public static IEnumerable<T> Sort<T>(this IEnumerable<T> list, BasePageInput basePageInput)
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