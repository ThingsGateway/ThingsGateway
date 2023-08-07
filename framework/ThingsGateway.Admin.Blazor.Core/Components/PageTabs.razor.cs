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

using Masa.Blazor.Presets;

using Microsoft.AspNetCore.Components;

namespace ThingsGateway.Admin.Blazor.Core;
/// <summary>
/// PageTabs
/// </summary>
public partial class PageTabs
{
    private TabOptions TabOptions(PageTabPathValue value)
    {
        var item = UserResoures.PageTabItems.FirstOrDefault(u => value.IsMatch(u.Href));
        var title = item?.Title;
        var icon = item?.Icon;
        var titleClass = $"mx-2 text-capitalize {(value.Selected ? "primary--text" : "")}";
        var op = new TabOptions(title, icon, titleClass)
        {
            TitleStyle = "min-width:46px;",
            Class = "systemTab",
        };
        return op;
    }
    /// <summary>
    /// Tabs实例
    /// </summary>
    public PPageTabs PPageTabs { get; private set; }

    [Inject]
    UserResoures UserResoures { get; set; }
    /// <summary>
    /// SelfPatterns
    /// </summary>
    [Parameter]
    public IEnumerable<string> SelfPatterns { get; set; }
    /// <summary>
    /// 子组件
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; }
}