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

namespace ThingsGateway.Admin.Application;

/// <inheritdoc/>
[ThingsGateway.DependencyInjection.SuppressSniffer]
public class PositionUtil
{

    /// <summary>
    /// 构建树节点
    /// </summary>
    public static List<TreeViewItem<PositionTreeOutput>> BuildTreeItemList(IEnumerable<PositionTreeOutput> sysresources, List<long> selectedItems, Microsoft.AspNetCore.Components.RenderFragment<PositionTreeOutput> render = null, TreeViewItem<PositionTreeOutput>? parent = null)
    {
        if (sysresources == null) return null;
        var trees = new List<TreeViewItem<PositionTreeOutput>>();
        foreach (var node in sysresources)
        {
            var item = new TreeViewItem<PositionTreeOutput>(node)
            {
                Text = node.Name,
                IsActive = selectedItems.Contains(node.Id),
                IsExpand = true,
                Parent = parent,
                Template = render,
                CheckedState = selectedItems.Contains(node.Id) ? CheckboxState.Checked : CheckboxState.UnChecked
            };
            item.Items = BuildTreeItemList(node.Children, selectedItems, render, item) ?? new();
            trees.Add(item);
        }
        return trees;
    }


    public static List<CascaderItem> BuildCascaderItemList(IEnumerable<PositionSelectorOutput> sysresources)
    {
        if (sysresources == null) return null;
        var trees = new List<CascaderItem>();
        foreach (var node in sysresources)
        {
            var item = new CascaderItem()
            {
                Text = node.Name,
                Value = node.Id.ToString(),
            };
            var data = BuildCascaderItemList(node.Children);
            foreach (var children in data)
            {
                item.AddItem(children);
            }
            trees.Add(item);
        }
        return trees;
    }


}
