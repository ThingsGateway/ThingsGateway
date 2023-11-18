#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 资源表种子数据
/// </summary>
public class SysResourceSeedData : ISqlSugarEntitySeedData<SysResource>
{
    /// <inheritdoc/>
    public IEnumerable<SysResource> SeedData()
    {
        List<SysResource> configList = new List<SysResource>
{
            //配置
            new SysResource
            {
                Id = 200001,
                Title = "网关配置",
                Icon = "mdi-cog",
                Category = ResourceCategoryEnum.MENU,
                Code = ResourceConst.System,
                ParentId = 0,
                SortCode = 2,
                TargetType = TargetTypeEnum.None,
            },
            new SysResource
            {
                Id = 200001002,
                Title = "插件管理",
                Icon = "mdi-database-cog-outline",
                Component = "/gatewayconfig/plugin",
                Category = ResourceCategoryEnum.MENU,
                Code = ResourceConst.System,
                ParentId = 200001,
                SortCode = 1,
                TargetType = TargetTypeEnum.SELF,
            },
            new SysResource
            {
                Id = 200001011,
                Title = "驱动调试",
                Icon = "mdi-database-cog-outline",
                Component = "/gatewayconfig/driverdebug",
                Category = ResourceCategoryEnum.MENU,
                Code = ResourceConst.System,
                ParentId = 200001,
                SortCode = 1,
                TargetType = TargetTypeEnum.SELF,
            },
            new SysResource
            {
                Id = 200001001,
                Title = "采集设备",
                Icon = "mdi-database-cog-outline",
                Component = "/gatewayconfig/collectdevice",
                Category = ResourceCategoryEnum.MENU,
                Code = ResourceConst.System,
                ParentId = 200001,
                SortCode = 2,
                TargetType = TargetTypeEnum.SELF,
            },
            new SysResource
            {
                Id = 200001101,
                Title = "上传设备",
                Icon = "mdi-database-cog-outline",
                Component = "/gatewayconfig/uploaddevice",
                Category = ResourceCategoryEnum.MENU,
                Code = ResourceConst.System,
                ParentId = 200001,
                SortCode = 2,
                TargetType = TargetTypeEnum.SELF,
            },
            new SysResource
            {
                Id = 200001003,
                Title = "变量管理",
                Icon = "mdi-database-cog-outline",
                Component = "/gatewayconfig/devicevariable",
                Category = ResourceCategoryEnum.MENU,
                Code = ResourceConst.System,
                ParentId = 200001,
                SortCode = 3,
                TargetType = TargetTypeEnum.SELF,
            },

            //状态
            new SysResource
            {
                Id = 389850957095173,
                Title = "网关状态",
                Icon = "mdi-transit-connection-variant",
                Category = ResourceCategoryEnum.MENU,
                Code = ResourceConst.System,
                ParentId = 0,
                SortCode = 3,
                TargetType = TargetTypeEnum.None,
            },
            new SysResource
            {
                Id = 200001004,
                Title = "运行状态",
                Icon = "mdi-transit-connection-horizontal",
                Component = "/gatewayruntime/devicestatus",
                Category = ResourceCategoryEnum.MENU,
                Code = ResourceConst.System,
                ParentId = 389850957095173,
                SortCode = 1,
                TargetType = TargetTypeEnum.SELF,
            },
            new SysResource
            {
                Id = 200001005,
                Title = "实时数据",
                Icon = "mdi-database-refresh-outline",
                Component = "/gatewayruntime/devicevariable",
                Category = ResourceCategoryEnum.MENU,
                Code = "system",
                ParentId = 389850957095173,
                SortCode = 2,
                TargetType = TargetTypeEnum.SELF,
            },
            new SysResource
            {
                Id = 389854423286021,
                Title = "硬件信息",
                Icon = "mdi-memory",
                Component = "/gatewayruntime/hardwareinfo",
                Category = ResourceCategoryEnum.MENU,
                ParentId = 389850957095173,
                SortCode = 4,
                TargetType = TargetTypeEnum.SELF,
            },
            new SysResource
            {
                Id = 389854579716357,
                Title = "实时报警",
                Icon = "mdi-alarm-light-outline",
                Component = "/gatewayruntime/realalarm",
                Category = ResourceCategoryEnum.MENU,
                ParentId = 389850957095173,
                SortCode = 3,
                TargetType = TargetTypeEnum.SELF,
            },
            //日志
            new SysResource
            {
                Id = 390107241025797,
                Title = "网关日志",
                Icon = "mdi-database-search-outline",
                Category = ResourceCategoryEnum.MENU,
                ParentId = 0,
                SortCode = 3,
                TargetType = TargetTypeEnum.None,
            },
            new SysResource
            {
                Id = 390107473895685,
                Title = "Rpc日志",
                Icon = "mdi-database-search-outline",
                Component = "/gatewaylog/rpclog",
                Category = ResourceCategoryEnum.MENU,
                ParentId = 390107241025797,
                SortCode = 1,
                TargetType = TargetTypeEnum.SELF,
            },
            new SysResource
            {
                Id = 390107521245445,
                Title = "后台日志",
                Icon = "mdi-database-search-outline",
                Component = "/gatewaylog/backendlog",
                Category = ResourceCategoryEnum.MENU,
                ParentId = 390107241025797,
                SortCode = 1,
                TargetType = TargetTypeEnum.SELF,
            },


};
        return configList.Concat(SeedDataUtil.GetSeedData<SysResource>("gateway_resource.json"));
    }
}