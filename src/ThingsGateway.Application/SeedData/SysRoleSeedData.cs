namespace ThingsGateway.Application
{
    /// <summary>
    /// 角色种子数据
    /// </summary>
    public class SysRoleSeedData : ISqlSugarEntitySeedData<SysRole>
    {
        /// <inheritdoc/>
        public IEnumerable<SysRole> SeedData()
        {
            return SeedDataUtil.GetSeedData<SysRole>("sys_role.json");
        }
    }
}