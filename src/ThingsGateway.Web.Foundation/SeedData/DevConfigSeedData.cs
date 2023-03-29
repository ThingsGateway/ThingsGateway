using ThingsGateway.Core;
using ThingsGateway.Core.Utils;

namespace ThingsGateway.Web.Foundation
{
    /// <summary>
    /// 系统配置种子数据
    /// </summary>
    public class DevConfigSeedData : ISqlSugarEntitySeedData<DriverPlugin>
    {
        /// <inheritdoc/>
        public IEnumerable<DriverPlugin> SeedData()
        {
            return SeedDataUtil.GetSeedData<DriverPlugin>("driver_plugin.json");
        }
    }
}