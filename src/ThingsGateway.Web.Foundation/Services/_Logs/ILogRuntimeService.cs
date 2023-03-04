using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation
{
    public interface IRuntimeLogService : ITransient
    {
        Task Delete();
        Task<SqlSugarPagedList<RuntimeLog>> Page(RuntimeLogPageInput input);
    }
}