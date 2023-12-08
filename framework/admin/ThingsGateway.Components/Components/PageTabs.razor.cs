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

using System.Text.RegularExpressions;

namespace ThingsGateway.Components;
/// <summary>
/// Tab表示类
/// </summary>
/// <param name="Title">标题</param>
/// <param name="Href">跳转类型</param>
/// <param name="Icon">图标</param>
public record PageTabItem(string Title, string Href, string Icon);

/// <summary>
/// PageTabs
/// </summary>
public partial class PageTabs
{
    /// <summary>
    /// 子组件
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; }

    /// <summary>
    /// PageTabItems
    /// </summary>
    [Parameter]
    public List<PageTabItem> PageTabItems { get; set; }

    /// <summary>
    /// Tabs实例
    /// </summary>
    public PPageTabs PPageTabs { get; private set; }

    /// <summary>
    /// SelfPatterns
    /// </summary>
    [Parameter]
    public IEnumerable<string> SelfPatterns { get; set; }

    private TabOptions TabOptions(PageTabPathValue value)
    {
        var item = PageTabItems.FirstOrDefault(u => IsMatch(u.Href, value.AbsolutePath));
        var title = item?.Title;
        var icon = item?.Icon;
        var titleClass = $"mx-2 text-capitalize {(value.Selected ? "primary--text" : "")}";
        var op = new TabOptions(title, icon, titleClass)
        {
            TitleStyle = "min-width:40px;",
        };
        return op;
    }

    private bool IsMatch(string input, string absolutePath)
    {
        if (!input.StartsWith("/"))
        {
            input = "/" + input;
        }
        string pattern = $@"^{input}(/.*)?$";
        bool isMatch = Regex.IsMatch(absolutePath, pattern, RegexOptions.IgnoreCase);
        return isMatch;
    }
}