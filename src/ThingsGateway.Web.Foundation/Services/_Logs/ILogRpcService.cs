using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation
{
    /// <summary>
    /// RPC日志服务
    /// </summary>
    public interface IRpcLogService : ITransient
    {
        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        Task Delete();
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<SqlSugarPagedList<RpcLog>> Page(RpcLogPageInput input);
    }
}