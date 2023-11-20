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

using Microsoft.AspNetCore.Components.Forms;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 插件添加DTO
/// </summary>
public class DriverPluginAddInput
{
    /// <summary>
    /// 文件名称
    /// </summary>
    [Description("文件名称")]
    public string FileName { get; set; }
    /// <summary>
    /// 主程序集
    /// </summary>
    [Description("主程序集")]
    [Required(ErrorMessage = "主程序集不能为空")]
    public IBrowserFile MainFile { get; set; }
    /// <summary>
    /// 附属程序集
    /// </summary>
    [Description("附属程序集")]
    public List<IBrowserFile> OtherFiles { get; set; } = new();

}

/// <summary>
/// 插件分页
/// </summary>
public class DriverPluginPageInput : BasePageInput
{
    /// <summary>
    /// 插件名称
    /// </summary>
    [Description("插件名称")]
    public string Name { get; set; }
    /// <summary>
    /// 文件名称
    /// </summary>
    [Description("文件名称")]
    public string FileName { get; set; }
}


/// <summary>
/// 插件分组
/// </summary>
public class DriverPlugin : PrimaryIdEntity
{
    /// <summary>
    /// 插件子组
    /// </summary>
    public List<DriverPlugin> Children { get; set; }
    /// <summary>
    /// 插件文件名称.插件类型名称
    /// </summary>
    [DataTable(Order = 2, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    [Description("插件全称")]
    public string FullName => FileName.IsNullOrEmpty() ? Name : FileName + "." + Name;
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
    public DriverEnum DriverEnum { get; set; }
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
