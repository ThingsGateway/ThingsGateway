namespace ThingsGateway.Application
{
    /// <summary>
    /// 关系表种子数据
    /// </summary>
    public class SysRelationSeedData : ISqlSugarEntitySeedData<SysRelation>
    {
        /// <inheritdoc/>
        public IEnumerable<SysRelation> SeedData()
        {
            return SeedDataUtil.GetSeedData<SysRelation>("sys_relation.json");
        }
    }
}