
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------




namespace ThingsGateway.Admin.Application;

/// <summary>
/// 用户表种子数据
/// </summary>
public class SysUserSeedData : ISqlSugarEntitySeedData<SysUser>
{
    /// <inheritdoc/>
    public IEnumerable<SysUser> SeedData()
    {
        return new List<SysUser>()
        {
            new SysUser()
            {
                  Id=RoleConst.SuperAdminId,
                  Account= "SuperAdmin",
                  Password="7DA385A25A98388E",
                  Status=true,
                  DefaultModule=6,
                  IsDelete=false,
                  SortCode=0
            }
        }.Concat(SeedDataUtil.GetSeedData<SysUser>("seed_sys_user.json"));
    }
}
