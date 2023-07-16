#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

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
            Context = DbContext.Db.GetConnectionScopeWithAttr<T>().CopyNew();//ioc注入的对象
            itenant = base.Context.AsTenant();
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