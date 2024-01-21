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

using System.ComponentModel;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 插件分组
/// </summary>
public class PluginOutput : PrimaryIdEntity
{
    /// <summary>
    /// 插件子组
    /// </summary>
    public List<PluginOutput> Children { get; set; }

    /// <summary>
    /// 插件文件名称.插件类型名称
    /// </summary>
    [DataTable(Order = 2, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    [Description("插件全称")]
    public string FullName => string.IsNullOrEmpty(FileName) ? Name : FileName + "." + Name;

    /// <summary>
    /// 插件文件名称
    /// </summary>
    [Description("文件名称")]
    [DataTable(Order = 1, IsShow = true, Sortable = true)]
    public string FileName { get; set; }

    /// <summary>
    /// 插件类型
    /// </summary>
    [Description("插件类型")]
    [DataTable(Order = 3, IsShow = true, Sortable = true)]
    public PluginTypeEnum PluginType { get; set; }

    /// <summary>
    /// 插件名称
    /// </summary>
    [Description("插件名称")]
    [DataTable(Order = 0, IsShow = true, Sortable = true)]
    public string Name { get; set; }

    /// <summary>
    /// 插件版本
    /// </summary>
    [Description("插件版本")]
    [DataTable(Order = 3, IsShow = true, Sortable = true)]
    public string Version { get; set; }

    /// <summary>
    /// 插件编译时间
    /// </summary>
    [Description("插件编译时间")]
    [DataTable(Order = 3, IsShow = true, Sortable = true)]
    public DateTime LastWriteTime { get; set; }
}