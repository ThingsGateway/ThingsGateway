//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.Razor;

[ThingsGateway.DependencyInjection.SuppressSniffer]
public static class ResourceUtil
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
    public static IEnumerable<SelectedItem> BuildPluginSelectList(this IEnumerable<PluginInfo> items)
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
    public static List<TreeViewItem<PluginInfo>> BuildTreeItemList(this IEnumerable<PluginInfo> pluginInfos, Microsoft.AspNetCore.Components.RenderFragment<PluginInfo> render = null, TreeViewItem<PluginInfo>? parent = null)
    {
        if (pluginInfos == null) return null;
        var trees = new List<TreeViewItem<PluginInfo>>();
        foreach (var node in pluginInfos)
        {
            var item = new TreeViewItem<PluginInfo>(node)
            {
                Text = node.Name,
                Parent = parent,
                IsExpand = true,
                Template = render,
            };
            item.Items = BuildTreeItemList(node.Children, render, item) ?? new();
            trees.Add(item);
        }
        return trees;
    }


    /// <summary>
    /// 构建树节点，传入的列表已经是树结构
    /// </summary>
    public static List<TreeViewItem<ChannelDeviceTreeItem>> BuildTreeItemList(this IEnumerable<ChannelRuntime> channelRuntimes, List<ChannelDeviceTreeItem> selectedItems, Microsoft.AspNetCore.Components.RenderFragment<ChannelDeviceTreeItem> render = null, TreeViewItem<ChannelDeviceTreeItem>? parent = null, List<TreeViewItem<ChannelDeviceTreeItem>> items = null)
    {
        if (channelRuntimes == null) return null;
        items ??= new();
        var trees = new List<TreeViewItem<ChannelDeviceTreeItem>>();



        //筛选插件名称
        foreach (var pluginName in channelRuntimes.Select(a => a.PluginName).ToHashSet())
        {
            var pluginItem = new TreeViewItem<ChannelDeviceTreeItem>(new ChannelDeviceTreeItem() { ChannelDevicePluginType = ChannelDevicePluginTypeEnum.PluginName, PluginName = pluginName })
            {
                Text = PluginServiceUtil.GetFileNameAndTypeName(pluginName).TypeName,
                IsExpand = true,
                Parent = parent,
            };
            var samePluginItem = items.FirstOrDefault(a => a.Value.Equals(pluginItem.Value));

            if (samePluginItem != null)
            {
                pluginItem = samePluginItem;
            }

            var channelOldItems = pluginItem.Items.ToList();
            pluginItem.Items.Clear();
            foreach (var channelRuntime in channelRuntimes.Where(a => a.PluginName == pluginName))
            {


                var channelRuntimeTreeItem = new ChannelDeviceTreeItem() { ChannelDevicePluginType = ChannelDevicePluginTypeEnum.Channel, ChannelRuntime = channelRuntime };
                var channelTreeItemItem = new TreeViewItem<ChannelDeviceTreeItem>(channelRuntimeTreeItem)
                {
                    Text = channelRuntime.ToString(),
                    Parent = pluginItem,
                    IsExpand = true,
                    IsActive = selectedItems.Contains(channelRuntimeTreeItem),
                    Template = render,
                };

                var sameChannelItem = channelOldItems.FirstOrDefault(a => a.Value.Equals(channelTreeItemItem.Value));

                if (sameChannelItem != null)
                {
                    channelTreeItemItem = sameChannelItem;
                }



                var deviceOldItems = channelTreeItemItem.Items.ToList();
                channelTreeItemItem.Items.Clear();

                foreach (var keyValue in channelRuntime.ReadDeviceRuntimes.OrderBy(a => a.Value.DeviceStatus))
                {
                    var deviceRuntime = keyValue.Value;
                    var deviceRuntimeTreeItem = new ChannelDeviceTreeItem() { ChannelDevicePluginType = ChannelDevicePluginTypeEnum.Device, DeviceRuntime = deviceRuntime };
                    var deviceTreeItemItem = new TreeViewItem<ChannelDeviceTreeItem>(deviceRuntimeTreeItem)
                    {
                        Text = keyValue.Value.Name,
                        Parent = pluginItem,
                        IsExpand = false,
                        IsActive = selectedItems.Contains(deviceRuntimeTreeItem),
                        Template = render,
                    };

                    var sameDeviceItem = deviceOldItems.FirstOrDefault(a => a.Value.Equals(deviceTreeItemItem.Value));

                    if (sameDeviceItem != null)
                    {
                        deviceTreeItemItem = sameDeviceItem;
                    }

                    channelTreeItemItem.Items.Add(deviceTreeItemItem);
                }


                pluginItem.Items.Add(channelTreeItemItem);
            }

            trees.Add(pluginItem);
        }

        return trees;
    }


    /// <summary>
    /// 构建树节点，传入的列表已经是树结构
    /// </summary>
    public static List<TreeViewItem<ChannelDeviceTreeItem>> BuildTreeItemList(this Dictionary<ChannelRuntime, List<DeviceRuntime>> dict, List<ChannelDeviceTreeItem> selectedItems, Microsoft.AspNetCore.Components.RenderFragment<ChannelDeviceTreeItem> render = null, TreeViewItem<ChannelDeviceTreeItem>? parent = null, List<TreeViewItem<ChannelDeviceTreeItem>> items = null)
    {
        if (dict == null) return null;
        items ??= new();
        var trees = new List<TreeViewItem<ChannelDeviceTreeItem>>();

        //筛选插件名称

        foreach (var pluginName in dict.Select(a => a.Key.PluginName).ToHashSet())
        {

            var pluginItem = new TreeViewItem<ChannelDeviceTreeItem>(new ChannelDeviceTreeItem() { ChannelDevicePluginType = ChannelDevicePluginTypeEnum.PluginName, PluginName = pluginName })
            {
                Text = PluginServiceUtil.GetFileNameAndTypeName(pluginName).TypeName,
                IsExpand = true,
                Parent = parent,
            };

            var samePluginItem = items.FirstOrDefault(a => a.Value.Equals(pluginItem.Value));

            if (samePluginItem != null)
            {
                pluginItem = samePluginItem;
            }
            var channelOldItems = pluginItem.Items.ToList();
            pluginItem.Items.Clear();

            foreach (var channelRuntime in dict.Where(a => a.Key.PluginName == pluginName))
            {
                var channelRuntimeTreeItem = new ChannelDeviceTreeItem() { ChannelDevicePluginType = ChannelDevicePluginTypeEnum.Channel, ChannelRuntime = channelRuntime.Key };
                var channelTreeItemItem = new TreeViewItem<ChannelDeviceTreeItem>(channelRuntimeTreeItem)
                {
                    Text = channelRuntime.ToString(),
                    Parent = pluginItem,
                    IsExpand = true,
                    IsActive = selectedItems.Contains(channelRuntimeTreeItem),
                    Template = render,
                };

                var sameChannelItem = channelOldItems.FirstOrDefault(a => a.Value.Equals(channelTreeItemItem.Value));

                if (sameChannelItem != null)
                {
                    channelTreeItemItem = sameChannelItem;
                }

                var deviceOldItems = channelTreeItemItem.Items.ToList();
                channelTreeItemItem.Items.Clear();

                foreach (var deviceRuntime in channelRuntime.Value.OrderBy(a => a.DeviceStatus))
                {
                    var deviceRuntimeTreeItem = new ChannelDeviceTreeItem() { ChannelDevicePluginType = ChannelDevicePluginTypeEnum.Device, DeviceRuntime = deviceRuntime };
                    var deviceTreeItemItem = new TreeViewItem<ChannelDeviceTreeItem>(deviceRuntimeTreeItem)
                    {
                        Text = deviceRuntime.Name,
                        Parent = pluginItem,
                        IsExpand = false,
                        IsActive = selectedItems.Contains(deviceRuntimeTreeItem),
                        Template = render,
                    };


                    var sameDeviceItem = deviceOldItems.FirstOrDefault(a => a.Value.Equals(deviceTreeItemItem.Value));

                    if (sameDeviceItem != null)
                    {
                        deviceTreeItemItem = sameDeviceItem;
                    }

                    channelTreeItemItem.Items.Add(deviceTreeItemItem);
                }


                pluginItem.Items.Add(channelTreeItemItem);
            }

            trees.Add(pluginItem);
        }

        return trees;
    }

}
