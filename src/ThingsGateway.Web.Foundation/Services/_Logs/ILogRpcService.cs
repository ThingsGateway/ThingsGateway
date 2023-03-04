using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation
{
    public interface IRpcLogService : ITransient
    {
        Task Delete();
        Task<SqlSugarPagedList<RpcLog>> Page(RpcLogPageInput input);
    }
}