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
/// 职位表种子数据
/// </summary>
public class SysPositionSeedData : ISqlSugarEntitySeedData<SysPosition>
{
    /// <inheritdoc/>
    public IEnumerable<SysPosition> SeedData()
    {
        var data = SeedDataUtil.GetSeedData<SysPosition>(PathExtensions.CombinePathWithOs("SeedData", "Admin", "seed_sys_position.json"));

        var assembly = GetType().Assembly;
        return new List<SysPosition>()
        {
            new SysPosition()
            {
                  Id=RoleConst.DefaultPositionId,
                  Status=true,
                  IsDelete=false,
                  Name="管理员",
                  OrgId=RoleConst.DefaultTenantId,
                  Code="ThingsGateway",
                  Category=PositionCategoryEnum.HIGH,
                  CreateUserId=RoleConst.SuperAdminId,
                  SortCode=0
            }
        }.Concat(SeedDataUtil.GetSeedDataByJson<SysPosition>(SeedDataUtil.GetManifestResourceStream(assembly, "SeedData.Admin.seed_sys_position.json")).Concat(data));

    }
}
