
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------



using BootstrapBlazor.Components;

namespace ThingsGateway.Gateway.Application;

public static class ResourceExtensions
{
    /// <summary>
    /// 构造选择项，ID/Name
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<SelectedItem> BuildChannelSelectList(this IEnumerable<Channel> items)
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
    /// 构造选择项，ID/Name
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<SelectedItem> BuildDeviceSelectList(this IEnumerable<Device> items)
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
    /// 构造选择项，ID/Name
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<SelectedItem> BuildPluginSelectList(this IEnumerable<PluginOutput> items)
    {
        var data = items
        .Select((item, index) =>
            new SelectedItem(item.FullName, item.Name)
            {
                GroupName = item.FileName,
            }
        ).ToList();
        return data;
    }

    /// <summary>
    /// 构建树节点，传入的列表已经是树结构
    /// </summary>
    public static List<TreeViewItem<PluginOutput>> BuildTreeItemList(this IEnumerable<PluginOutput> pluginOutputs, Microsoft.AspNetCore.Components.RenderFragment<PluginOutput> render = null, TreeViewItem<PluginOutput>? parent = null)
    {
        if (pluginOutputs == null) return null;
        var trees = new List<TreeViewItem<PluginOutput>>();
        foreach (var node in pluginOutputs)
        {
            var item = new TreeViewItem<PluginOutput>(node)
            {
                Text = node.Name,
                Parent = parent,
                IsExpand = false,
                Template = render,
            };
            item.Items = BuildTreeItemList(node.Children, render, item) ?? new();
            trees.Add(item);
        }
        return trees;
    }
}
