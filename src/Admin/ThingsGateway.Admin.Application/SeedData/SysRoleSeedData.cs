//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 角色种子数据
/// </summary>
public class SysRoleSeedData : ISqlSugarEntitySeedData<SysRole>
{
    /// <inheritdoc/>
    public IEnumerable<SysRole> SeedData()
    {
        var data = SeedDataUtil.GetSeedData<SysRole>(PathExtensions.CombinePathWithOs("SeedData", "Admin", "seed_sys_role.json"));
        var assembly = GetType().Assembly;
        return new List<SysRole>()
        {
            new SysRole()
            {
                  Id=RoleConst.SuperAdminRoleId,
                  Code=RoleConst.SuperAdmin,
                  Name="超级管理员",
                  Category=RoleCategoryEnum.Global,
                  DefaultDataScope=new(),
                  IsDelete=false,
                  SortCode=0
            }
        }.Concat(SeedDataUtil.GetSeedDataByJson<SysRole>(SeedDataUtil.GetManifestResourceStream(assembly, "SeedData.Admin.seed_sys_role.json")).Concat(data));
    }
}
