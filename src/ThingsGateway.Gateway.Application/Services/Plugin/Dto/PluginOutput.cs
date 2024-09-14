//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 插件分组
/// </summary>
public class PluginOutput
{
    /// <summary>
    /// 插件文件名称.插件类型名称
    /// </summary>
    [AutoGenerateColumn(Ignore = true)]
    public List<PluginOutput> Children { get; set; } = new();

    /// <summary>
    /// 插件文件名称.插件类型名称
    /// </summary>
    [AutoGenerateColumn(Ignore = true)]
    public string FullName => PluginServiceUtil.GetFullName(FileName, Name);

    /// <summary>
    /// 插件文件名称
    /// </summary>
    [AutoGenerateColumn(Filterable = true, Sortable = true)]
    public string FileName { get; set; }

    /// <summary>
    /// 插件类型
    /// </summary>
    [AutoGenerateColumn(Filterable = true, Sortable = true)]
    public PluginTypeEnum PluginType { get; set; }

    /// <summary>
    /// 插件名称
    /// </summary>
    [AutoGenerateColumn(Filterable = true, Sortable = true)]
    public string Name { get; set; }

    /// <summary>
    /// 插件版本
    /// </summary>
    [AutoGenerateColumn(Filterable = true, Sortable = true)]
    public string Version { get; set; }

    /// <summary>
    /// 插件编译时间
    /// </summary>
    [AutoGenerateColumn(Filterable = true, Sortable = true)]
    public DateTime LastWriteTime { get; set; }
}
