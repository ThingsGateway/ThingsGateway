namespace ThingsGateway.Application
{
    /// <summary>
    /// 资源表种子数据
    /// </summary>
    public class SysResourceSeedData : ISqlSugarEntitySeedData<SysResource>
    {
        /// <inheritdoc/>
        public IEnumerable<SysResource> SeedData()
        {
            return SeedDataUtil.GetSeedData<SysResource>("sys_resource.json");
        }
    }
}