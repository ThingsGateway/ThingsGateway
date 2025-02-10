//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Admin.Application;

namespace ThingsGateway.RulesEngine;

/// <summary>
/// 资源表种子数据
/// </summary>
public class SysResourceSeedData : ISqlSugarEntitySeedData<SysResource>
{
    /// <inheritdoc/>
    public IEnumerable<SysResource> SeedData()
    {
        var data1 = SeedDataUtil.GetSeedData<SysResource>(PathExtensions.CombinePathWithOs("SeedData", "RulesEngine", "seed_sys_resource.json"));
        var data2 = SeedDataUtil.GetSeedData<SysResource>(PathExtensions.CombinePathWithOs("SeedData", "RulesEngine", "seed_sys_resourcebutton.json"));

        var assembly = GetType().Assembly;
        return SeedDataUtil.GetSeedDataByJson<SysResource>(SeedDataUtil.GetManifestResourceStream(assembly, "SeedData.RulesEngine.seed_sys_resource.json")).Concat(SeedDataUtil.GetSeedDataByJson<SysResource>(SeedDataUtil.GetManifestResourceStream(assembly, "SeedData.RulesEngine.seed_sys_resourcebutton.json"))).Concat(data1).Concat(data2);

    }
}
