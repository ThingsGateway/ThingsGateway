#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Furion.DatabaseAccessor;

using Microsoft.AspNetCore.Mvc.Filters;

namespace ThingsGateway.Core
{
    /// <summary>
    /// SqlSugar 事务和工作单元
    /// </summary>
    public sealed class SqlSugarUnitOfWork : IUnitOfWork
    {
        /// <summary>
        /// SqlSugar 对象
        /// </summary>
        private readonly ISqlSugarClient _sqlSugarClient;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="sqlSugarClient"></param>
        public SqlSugarUnitOfWork(ISqlSugarClient sqlSugarClient)
        {
            _sqlSugarClient = sqlSugarClient;
        }

        /// <summary>
        /// 开启工作单元处理
        /// </summary>
        /// <param name="context"></param>
        /// <param name="unitOfWork"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void BeginTransaction(FilterContext context, UnitOfWorkAttribute unitOfWork)
        {
            _sqlSugarClient.AsTenant().BeginTran();
        }

        /// <summary>
        /// 提交工作单元处理
        /// </summary>
        /// <param name="resultContext"></param>
        /// <param name="unitOfWork"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void CommitTransaction(FilterContext resultContext, UnitOfWorkAttribute unitOfWork)
        {
            _sqlSugarClient.AsTenant().CommitTran();
        }

        /// <summary>
        /// 执行完毕（无论成功失败）
        /// </summary>
        /// <param name="context"></param>
        /// <param name="resultContext"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void OnCompleted(FilterContext context, FilterContext resultContext)
        {
            _sqlSugarClient.Dispose();
        }

        /// <summary>
        /// 回滚工作单元处理
        /// </summary>
        /// <param name="resultContext"></param>
        /// <param name="unitOfWork"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void RollbackTransaction(FilterContext resultContext, UnitOfWorkAttribute unitOfWork)
        {
            _sqlSugarClient.AsTenant().RollbackTran();
        }
    }
}