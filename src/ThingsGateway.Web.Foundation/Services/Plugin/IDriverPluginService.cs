using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation
{
    public interface IDriverPluginService : ITransient
    {
        Task Add(DriverPluginAddInput input);
        List<DriverPlugin> GetCacheListAsync();
        DriverPlugin GetDriverPluginById(long Id);
        List<DriverPluginCategory> GetDriverPluginChildrenList(DriverEnum driverTypeEnum);
        long? GetIdByName(string name);
        string GetNameById(long id);
        Task<SqlSugarPagedList<DriverPlugin>> Page(DriverPluginPageInput input);
    }
}