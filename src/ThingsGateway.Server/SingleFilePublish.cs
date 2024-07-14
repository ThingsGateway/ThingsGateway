//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using SqlSugar;

using System.Reflection;

namespace ThingsGateway.Server;

/// <summary>
/// 解决单文件发布程序集扫描问题
/// </summary>
public class SingleFilePublish : ISingleFilePublish
{
    /// <summary>
    /// 解决单文件不能扫描的程序集
    /// </summary>
    /// <remarks>和 <see cref="IncludeAssemblyNames"/> 可同时配置</remarks>
    /// <returns></returns>
    public Assembly[] IncludeAssemblies()
    {
        // 需要 Furion 框架扫描哪些程序集就写上去即可
        return Array.Empty<Assembly>();
    }

    /// <summary>
    /// 解决单文件不能扫描的程序集名称
    /// </summary>
    /// <remarks>和 <see cref="IncludeAssemblies"/> 可同时配置</remarks>
    /// <returns></returns>
    public string[] IncludeAssemblyNames()
    {
#if !Admin
        InstanceFactory.CustomAssemblies =
    [typeof(SqlSugar.TDengine.TDengineProvider).Assembly];
#endif
        return
        [
            "ThingsGateway.Foundation",
            "ThingsGateway.Core",
            "ThingsGateway.NewLife.X",
            "ThingsGateway.Startup",
            "ThingsGateway.CSScript",
            "ThingsGateway.Razor",
            "ThingsGateway.Gateway.Application",
            "ThingsGateway.Gateway.Razor"   ,
            "ThingsGateway.Admin.Razor"   ,
            "ThingsGateway.Admin.Application"   ,
            "SqlSugar.TDengineCore",
        ];
    }
}
