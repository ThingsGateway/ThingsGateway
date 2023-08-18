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

using Microsoft.AspNetCore.Components;

namespace ThingsGateway.Admin.Blazor.Core;
/// <summary>
/// 启用/停用 文本提示
/// </summary>
public partial class EnableChip
{
    /// <summary>
    /// Class
    /// </summary>
    [Parameter]
    public string Class { get; set; } = "";
    /// <summary>
    /// Style
    /// </summary>
    [Parameter]
    public string Style { get; set; } = "";
    /// <summary>
    /// Value
    /// </summary>
    [Parameter]
    public bool Value { get; set; }
    /// <summary>
    /// DisabledLabel
    /// </summary>
    [Parameter]
    public string DisabledLabel { get; set; }
    /// <summary>
    /// EnabledLabel
    /// </summary>
    [Parameter]
    public string EnabledLabel { get; set; }

    private string TextColor => Value ? "green" : "error";
    private string Color => Value ? "green-lighten" : "warning-lighten";
    private string Label => Value ? EnabledLabel ?? "启用" : DisabledLabel ?? "停用";
}