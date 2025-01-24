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
public class OrgUtil
{
    /// <summary>
    /// 构造选择项，ID/TITLE
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<SelectedItem> BuildOrgSelectList(IEnumerable<SysOrg> items)
    {
        var data = items
        .Select((item, index) =>
            new SelectedItem(item.Id.ToString(), item.Name)
            {
            }
        ).ToList();
        return data;
    }
    /// <summary>
    /// 构造树形数据
    /// </summary>
    /// <param name="items">资源列表</param>
    /// <param name="parentId">父ID</param>
    /// <returns></returns>
    public static IEnumerable<TableTreeNode<SysOrg>> BuildTableTrees(IEnumerable<SysOrg> items, long parentId = 0)
    {
        return items
        .Where(it => it.ParentId == parentId)
        .Select((item, index) =>
            new TableTreeNode<SysOrg>(item)
            {
                HasChildren = items.Any(i => i.ParentId == item.Id),
                IsExpand = items.Any(i => i.ParentId == item.Id),
                Items = BuildTableTrees(items, item.Id).ToList()
            }
        );
    }

    /// <summary>
    /// 构建树节点
    /// </summary>
    public static List<TreeViewItem<SysOrg>> BuildTreeItemList(IEnumerable<SysOrg> sysresources, List<long> selectedItems, Microsoft.AspNetCore.Components.RenderFragment<SysOrg> render = null, long parentId = 0, TreeViewItem<SysOrg>? parent = null)
    {
        if (sysresources == null) return null;
        var trees = new List<TreeViewItem<SysOrg>>();
        var roots = sysresources.Where(i => i.ParentId == parentId).OrderBy(i => i.SortCode);
        foreach (var node in roots)
        {
            var item = new TreeViewItem<SysOrg>(node)
            {
                Text = node.Name,
                IsActive = selectedItems.Contains(node.Id),
                IsExpand = true,
                Parent = parent,
                Template = render,
                CheckedState = selectedItems.Contains(node.Id) ? CheckboxState.Checked : CheckboxState.UnChecked
            };
            item.Items = BuildTreeItemList(sysresources, selectedItems, render, node.Id, item) ?? new();
            trees.Add(item);
        }
        return trees;
    }

    /// <summary>
    /// 构建树节点
    /// </summary>
    public static List<TreeViewItem<long>> BuildTreeIdItemList(IEnumerable<SysOrg> sysresources, List<long> selectedItems, Microsoft.AspNetCore.Components.RenderFragment<long> render = null, long parentId = 0, TreeViewItem<long>? parent = null)
    {
        if (sysresources == null) return null;
        var trees = new List<TreeViewItem<long>>();
        var roots = sysresources.Where(i => i.ParentId == parentId).OrderBy(i => i.SortCode);
        foreach (var node in roots)
        {
            var item = new TreeViewItem<long>(node.Id)
            {
                Text = node.Name,
                IsActive = selectedItems.Contains(node.Id),
                IsExpand = true,
                Parent = parent,
                Template = render,
                CheckedState = selectedItems.Contains(node.Id) ? CheckboxState.Checked : CheckboxState.UnChecked
            };
            item.Items = BuildTreeIdItemList(sysresources, selectedItems, render, node.Id, item) ?? new();
            trees.Add(item);
        }
        return trees;
    }

}
