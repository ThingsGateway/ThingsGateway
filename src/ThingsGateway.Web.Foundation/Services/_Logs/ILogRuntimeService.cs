using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation
{
    /// <summary>
    /// 运行日志服务
    /// </summary>
    public interface IRuntimeLogService : ITransient
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
        Task<SqlSugarPagedList<RuntimeLog>> Page(RuntimeLogPageInput input);
    }
}