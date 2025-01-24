//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

using ThingsGateway.Admin.Application;

namespace ThingsGateway.Admin.Razor;

/// <inheritdoc/>
[ThingsGateway.DependencyInjection.SuppressSniffer]
public class ResourceUtil
{

    /// <summary>
    /// 构造选择项，ID/TITLE
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<SelectedItem> BuildMenuSelectList(IEnumerable<SysResource> items)
    {
        var data = items.Where(a => a.Category == ResourceCategoryEnum.Menu)
        .Select((item, index) =>
            new SelectedItem(item.Id.ToString(), item.Title)
            {
            }
        ).ToList();
        return data;
    }

    /// <summary>
    /// 构造树形菜单
    /// </summary>
    /// <param name="items">资源列表</param>
    /// <param name="parentId">父ID</param>
    /// <returns></returns>
    public static IEnumerable<MenuItem> BuildMenuTrees(IEnumerable<SysResource> items, long parentId = 0)
    {
        return items
        .Where(it => it.ParentId == parentId)
        .Select((item, index) =>
            new MenuItem()
            {
                Match = item.NavLinkMatch ?? Microsoft.AspNetCore.Components.Routing.NavLinkMatch.All,
                Text = item.Title,
                Icon = item.Icon,
                Url = item.Href,
                Target = item.Target.ToString(),
                Items = BuildMenuTrees(items, item.Id).ToList()
            }
        );
    }

    /// <summary>
    /// 构造选择项，ID/TITLE
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<SelectedItem> BuildModuleSelectList(IEnumerable<SysResource> items)
    {
        var data = items.Where(a => a.Category == ResourceCategoryEnum.Module)
        .Select((item, index) =>
            new SelectedItem(item.Id.ToString(), item.Title)
            {
                GroupName = items.FirstOrDefault(a => a.Id == item.ParentId)?.Title!
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
    public static IEnumerable<TableTreeNode<SysResource>> BuildTableTrees(IEnumerable<SysResource> items, long parentId = 0)
    {
        return items
        .Where(it => it.ParentId == parentId)
        .Select((item, index) =>
            new TableTreeNode<SysResource>(item)
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
    public static List<TreeViewItem<SysResource>> BuildTreeItemList(IEnumerable<SysResource> sysresources, List<long> selectedItems, Microsoft.AspNetCore.Components.RenderFragment<SysResource> render, long parentId = 0, TreeViewItem<SysResource>? parent = null, Func<SysResource, bool> disableFunc = null)
    {
        if (sysresources == null) return null;
        var trees = new List<TreeViewItem<SysResource>>();
        var roots = sysresources.Where(i => i.ParentId == parentId).OrderBy(a => a.Module).ThenBy(i => i.SortCode);
        foreach (var node in roots)
        {
            var item = new TreeViewItem<SysResource>(node)
            {
                Text = node.Title,
                Icon = node.Icon,
                IsDisabled = disableFunc == null ? false : disableFunc(node),
                IsActive = selectedItems.Any(v => node.Id == v),
                IsExpand = true,
                Parent = parent,
                Template = render,
                CheckedState = selectedItems.Any(i => i == node.Id) ? CheckboxState.Checked : CheckboxState.UnChecked
            };
            item.Items = BuildTreeItemList(sysresources, selectedItems, render, node.Id, item, disableFunc) ?? new();
            trees.Add(item);
        }
        return trees;
    }

    /// <summary>
    /// 构建树节点
    /// </summary>
    public static List<TreeViewItem<T>> BuildTreeItemList<T>(IEnumerable<SysResource> sysresources, List<long> selectedItems, Microsoft.AspNetCore.Components.RenderFragment<T> render, long parentId = 0, TreeViewItem<T>? parent = null, Func<SysResource, bool> disableFunc = null) where T : class
    {
        if (sysresources == null) return null;
        var trees = new List<TreeViewItem<T>>();
        var roots = sysresources.Where(i => i.ParentId == parentId).OrderBy(i => i.SortCode);
        foreach (var node in roots)
        {
            var item = new TreeViewItem<T>(node.Adapt<T>())
            {
                Text = node.Title,
                Icon = node.Icon,
                IsDisabled = disableFunc == null ? false : disableFunc(node),
                IsActive = selectedItems.Any(v => node.Id == v),
                IsExpand = true,
                Parent = parent,
                Template = render,
                CheckedState = selectedItems.Any(i => i == node.Id) ? CheckboxState.Checked : CheckboxState.UnChecked
            };
            item.Items = BuildTreeItemList(sysresources, selectedItems, render, node.Id, item, disableFunc) ?? new();
            trees.Add(item);
        }
        return trees;
    }

}
