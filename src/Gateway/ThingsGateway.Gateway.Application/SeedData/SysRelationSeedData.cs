//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 关系表种子数据
/// </summary>
public class SysRelationSeedData : ISqlSugarEntitySeedData<SysRelation>
{
    /// <inheritdoc/>
    public IEnumerable<SysRelation> SeedData()
    {
        var data = SeedDataUtil.GetSeedData<SysRelation>(PathExtensions.CombinePathWithOs("SeedData", "Gateway", "seed_gateway_relation.json"));
        var assembly = GetType().Assembly;
        return SeedDataUtil.GetSeedDataByJson<SysRelation>(SeedDataUtil.GetManifestResourceStream(assembly, "SeedData.Gateway.seed_gateway_relation.json")).Concat(data);
    }
}
