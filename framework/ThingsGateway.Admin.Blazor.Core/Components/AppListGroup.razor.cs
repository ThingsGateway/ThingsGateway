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
/// AppListGroup
/// </summary>
/// <typeparam name="TItem"></typeparam>
public partial class AppListGroup<TItem>
{
    /// <summary>
    /// icon
    /// </summary>
    [Parameter]
    public string Icon { get; set; }

    /// <summary>
    /// item
    /// </summary>
    [Parameter, EditorRequired]
    public TItem Item { get; set; }

    /// <summary>
    /// sub
    /// </summary>
    [Parameter]
    public bool SubGroup { get; set; }

    private List<string> _group = new();
    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        _group = GenGroup(Item.Children);
    }

    List<string> GenGroup(List<TItem> items)
    {
        if (items == null || !items.Any())
            return new List<string>();
        var groups = new List<string>();
        foreach (var item in items)
        {
            groups.AddRange(GenGroup(item.Children));
            if (item.HasChildren())
                continue;
            groups.Add(item.Href);
        }

        return groups;
    }
}