namespace ThingsGateway.Application
{
    /// <summary>
    /// 系统配置种子数据
    /// </summary>
    public class DevConfigSeedData : ISqlSugarEntitySeedData<DevConfig>
    {
        /// <inheritdoc/>
        public IEnumerable<DevConfig> SeedData()
        {
            return SeedDataUtil.GetSeedData<DevConfig>("dev_config.json");
        }
    }
}