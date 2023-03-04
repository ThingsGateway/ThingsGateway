namespace ThingsGateway.Core
{
    /// <summary>
    /// 仓储模式对象
    /// </summary>
    [SuppressSniffer]
    public partial class DbRepository<T> : SimpleClient<T> where T : class, new()
    {
        public ITenant itenant = null;//多租户事务、GetConnection、IsAnyConnection等功能

        public DbRepository(ISqlSugarClient context = null) : base(context)//注意这里要有默认值等于null
        {
            Context = DbContext.Db.GetConnectionScopeWithAttr<T>();//ioc注入的对象
            itenant = DbContext.Db;
        }

        #region 仓储方法拓展

        #region 列表

        /// <summary>
        /// 获取列表指定多个字段
        /// </summary>
        /// <param name="whereExpression">查询条件</param>
        /// <param name="selectExpression">查询字段</param>
        /// <returns></returns>
        public virtual Task<List<T>> GetListAsync(Expression<Func<T, bool>> whereExpression, Expression<Func<T, T>> selectExpression)
        {
            return Context.Queryable<T>().Where(whereExpression).Select(selectExpression).ToListAsync();
        }

        /// <summary>
        /// 获取列表指定单个字段
        /// </summary>
        /// <param name="whereExpression">查询条件</param>
        /// <param name="selectExpression">查询字段</param>
        /// <returns></returns>
        public virtual Task<List<string>> GetListAsync(Expression<Func<T, bool>> whereExpression, Expression<Func<T, string>> selectExpression)
        {
            return Context.Queryable<T>().Where(whereExpression).Select(selectExpression).ToListAsync();
        }

        /// <summary>
        /// 获取列表指定单个字段
        /// </summary>
        /// <param name="whereExpression">查询条件</param>
        /// <param name="selectExpression">查询字段</param>
        /// <returns></returns>
        public virtual Task<List<long>> GetListAsync(Expression<Func<T, bool>> whereExpression, Expression<Func<T, long>> selectExpression)
        {
            return Context.Queryable<T>().Where(whereExpression).Select(selectExpression).ToListAsync();
        }

        #endregion 列表

        #region 单查

        /// <summary>
        /// 获取指定表的单个字段
        /// </summary>
        /// <param name="whereExpression">查询条件</param>
        /// <param name="selectExpression">查询字段</param>
        /// <returns></returns>
        public virtual Task<string> GetFirstAsync(Expression<Func<T, bool>> whereExpression, Expression<Func<T, string>> selectExpression)
        {
            return Context.Queryable<T>().Where(whereExpression).Select(selectExpression).FirstAsync();
        }

        /// <summary>
        /// 获取指定表的单个字段
        /// </summary>
        /// <param name="whereExpression">查询条件</param>
        /// <param name="selectExpression">查询字段</param>
        /// <returns></returns>
        public virtual Task<long> GetFirstAsync(Expression<Func<T, bool>> whereExpression, Expression<Func<T, long>> selectExpression)
        {
            return Context.Queryable<T>().Where(whereExpression).Select(selectExpression).FirstAsync();
        }

        #endregion 单查

        #endregion 仓储方法拓展
    }
}