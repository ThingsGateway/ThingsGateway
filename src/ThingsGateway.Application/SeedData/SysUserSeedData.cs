namespace ThingsGateway.Application
{
    /// <summary>
    /// 用户表种子数据
    /// </summary>
    public class SysUserSeedData : ISqlSugarEntitySeedData<SysUser>
    {
        /// <inheritdoc/>
        public IEnumerable<SysUser> SeedData()
        {
            return SeedDataUtil.GetSeedData<SysUser>("sys_user.json");
        }
    }
}