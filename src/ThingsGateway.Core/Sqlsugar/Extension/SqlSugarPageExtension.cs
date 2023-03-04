namespace ThingsGateway.Core
{
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

    /// <summary>
    /// SqlSugar 分页泛型集合
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class SqlSugarPagedList<TEntity>
    {
        /// <summary>
        /// 页码
        /// </summary>
        public int Current { get; set; }

        /// <summary>
        /// 是否有下一页
        /// </summary>
        public bool HasNextPages { get; set; }

        /// <summary>
        /// 是否有上一页
        /// </summary>
        public bool HasPrevPages { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int Pages { get; set; }

        /// <summary>
        /// 当前页集合
        /// </summary>
        public IEnumerable<TEntity> Records { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// 总条数
        /// </summary>
        public int Total { get; set; }
    }
}