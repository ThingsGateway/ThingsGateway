#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Microsoft.AspNetCore.Components.Forms;

using System.ComponentModel.DataAnnotations;

using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 插件添加DTO
/// </summary>
public class DriverPluginAddInput
{
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
}


/// <summary>
/// 插件分组
/// </summary>
public class DriverPluginCategory
{
    /// <summary>
    /// 插件子组
    /// </summary>
    public List<DriverPluginCategory> Children { get; set; }

    /// <summary>
    /// 插件ID
    /// </summary>
    public long Id { get; set; }
    /// <summary>
    /// 插件名称
    /// </summary>
    public string Name { get; set; }
}