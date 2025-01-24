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
public class OpenApiUtil
{
    /// <summary>
    /// 构建树节点，传入的列表已经是树结构
    /// </summary>
    public static List<TreeViewItem<OpenApiPermissionTreeSelector>> BuildTreeItemList(IEnumerable<OpenApiPermissionTreeSelector> openApiPermissionTreeSelectors, List<string> selectedItems, Microsoft.AspNetCore.Components.RenderFragment<OpenApiPermissionTreeSelector> render, TreeViewItem<OpenApiPermissionTreeSelector>? parent = null)
    {
        if (openApiPermissionTreeSelectors == null) return null;
        var trees = new List<TreeViewItem<OpenApiPermissionTreeSelector>>();
        foreach (var node in openApiPermissionTreeSelectors)
        {
            var item = new TreeViewItem<OpenApiPermissionTreeSelector>(node)
            {
                Text = node.ApiRoute,
                IsActive = selectedItems.Any(v => node.ApiRoute == v),
                Parent = parent,
                IsExpand = true,
                Template = render,
                CheckedState = selectedItems.Any(i => i == node.ApiRoute) ? CheckboxState.Checked : CheckboxState.UnChecked
            };
            item.Items = BuildTreeItemList(node.Children, selectedItems, render, item) ?? new();
            trees.Add(item);
        }
        return trees;
    }


}
