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

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

using System.Diagnostics.CodeAnalysis;

using ThingsGateway.Admin.Application;
using ThingsGateway.Core.Extension;
using ThingsGateway.Foundation.OpcUa;
using ThingsGateway.Gateway.Application;
using ThingsGateway.Razor;

namespace ThingsGateway.Debug;

/// <summary>
/// 导入变量
/// </summary>
public partial class OpcUaImportVariable
{
    private List<TreeViewItem<OpcUaTagModel>> Items = new();
    private IEnumerable<OpcUaTagModel> Nodes;
    private bool ShowSkeleton = true;

    /// <summary>
    /// Opc对象
    /// </summary>
    [Parameter]
    public ThingsGateway.Foundation.OpcUa.OpcUaMaster Plc { get; set; }

    [CascadingParameter]
    private Func<Task>? OnCloseAsync { get; set; }

    [Inject]
    [NotNull]
    private IStringLocalizer<OpcUaProperty>? OpcUaPropertyLocalizer { get; set; }

    [Inject]
    [NotNull]
    private ToastService? ToastService { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Task.Factory.StartNew(async () =>
            {
                Items = BuildTreeItemList(PopulateBranch(), RenderTreeItem).ToList();
                ShowSkeleton = false;
                await InvokeAsync(StateHasChanged);
            });
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    /// <summary>
    /// 构建树节点，传入的列表已经是树结构
    /// </summary>
    private static IEnumerable<TreeViewItem<OpcUaTagModel>> BuildTreeItemList(IEnumerable<OpcUaTagModel> opcDaTagModels, Microsoft.AspNetCore.Components.RenderFragment<OpcUaTagModel> render = null, TreeViewItem<OpcUaTagModel>? parent = null)
    {
        if (opcDaTagModels == null) return Enumerable.Empty<TreeViewItem<OpcUaTagModel>>();
        var trees = new List<TreeViewItem<OpcUaTagModel>>();
        foreach (var node in opcDaTagModels)
        {
            if (node == null) continue;
            var item = new TreeViewItem<OpcUaTagModel>(node)
            {
                Text = node.Name,
                Parent = parent,
                IsExpand = false,
                Template = render,
                HasChildren = node.Children != null,
            };
            item.Items = BuildTreeItemList(node.Children, render, item).ToList();
            trees.Add(item);
        }
        return trees;
    }

    private List<OpcUaTagModel> GetAllTag(IEnumerable<OpcUaTagModel> opcDaTagModels)
    {
        List<OpcUaTagModel> result = new();
        foreach (var item in opcDaTagModels)
        {
            PopulateBranch(item);

            result.AddRange(item.GetAllTags().Where(a => a.Children == null));
        }

        return result;
    }

    private bool ModelEqualityComparer(OpcUaTagModel x, OpcUaTagModel y) => x.NodeId == y.NodeId;

    private async Task OnClickClose()
    {
        if (OnCloseAsync != null)
            await OnCloseAsync();
    }

    private async Task OnClickExport()
    {
        try
        {
            var data = GetImportVariableList(GetAllTag(Nodes));
            if (data.Item3 == null || data.Item3?.Count == 0)
            {
                await ToastService.Warning(OpcUaPropertyLocalizer["NoVariablesAvailable"], OpcUaPropertyLocalizer["NoVariablesAvailable"]);
                return;
            }

            await DownChannelExportAsync(data.Item1);
            await DownDeviceExportAsync(data.Item2, data.Item1.Name);
            await DownDeviceVariableExportAsync(data.Item3, data.Item2.Name);
            await ToastService.Default();
        }
        catch (Exception ex)
        {
            await ToastService.Warning(ex.Message);
        }
    }

    private async Task OnClickSave()
    {
        try
        {
            var data = GetImportVariableList(GetAllTag(Nodes));
            if (data.Item3 == null || data.Item3?.Count == 0)
            {
                await ToastService.Warning(OpcUaPropertyLocalizer["NoVariablesAvailable"], OpcUaPropertyLocalizer["NoVariablesAvailable"]);
                return;
            }
            await App.RootServices.GetRequiredService<IChannelService>().SaveChannelAsync(data.Item1, ItemChangedType.Add);
            await App.RootServices.GetRequiredService<IDeviceService>().SaveDeviceAsync(data.Item2, ItemChangedType.Add);
            await App.RootServices.GetRequiredService<IVariableService>().AddBatchAsync(data.Item3);
            await ToastService.Default();
        }
        catch (Exception ex)
        {
            await ToastService.Warning(ex.Message);
        }
    }

    private Task<IEnumerable<TreeViewItem<OpcUaTagModel>>> OnExpandNodeAsync(TreeViewItem<OpcUaTagModel> treeViewItem)
    {
        var data = BuildTreeItemList(PopulateBranch(treeViewItem.Value.NodeId), RenderTreeItem);
        return Task.FromResult(data);
    }

    private Task OnTreeItemChecked(List<TreeViewItem<OpcUaTagModel>> items)
    {
        Nodes = items.Select(a => a.Value);
        return Task.CompletedTask;
    }

    private void PopulateBranch(OpcUaTagModel model)
    {
        if (model.Children != null)
        {
            if (model.Children.Count == 0)
            {
                var sourceId = model.Tag.ItemName;
                model.Children = PopulateBranch(sourceId);
            }
            foreach (var item in model.Children)
            {
                PopulateBranch(item);
            }
        }
    }

    private List<OpcUaTagModel> PopulateBranch(string sourceId = null, bool isAll = false)
    {
        List<OpcUaTagModel> nodes = new()
        {
            new OpcUaTagModel() { Name = "Browsering..." }
        };
        try
        {
            var references = Plc.GetBrowseElements(sourceId);
            List<OpcUaTagModel> list = new();
            if (references != null)
            {
                for (int ii = 0; ii < references.Count; ii++)
                {
                    var target = references[ii];
                    OpcUaTagModel child = new()
                    {
                        Name = target.Name,
                        Tag = target
                    };

                    if (target.HasChildren)
                    {
                        if (isAll)
                            child.Children = PopulateBranch(target.ItemName);
                        else
                            child.Children = new();
                    }

                    list.Add(child);
                }
            }

            List<OpcUaTagModel> listNode = list;
            nodes.Clear();
            nodes.AddRange(listNode.ToArray());
            return nodes;
        }
        catch (Exception ex)
        {
            return new()
            {
                new()
                {
                    Name = ex.Message,
                    Tag = new(),
                    Children = null
                }
            };
        }
    }

#if Plugin

    private bool isDownLoading;

    /// <summary>
    /// 获取设备与变量列表
    /// </summary>
    /// <returns></returns>
    private (Channel, Device, List<Variable>) GetImportVariableList(IEnumerable<OpcUaTagModel> opcDaTagModels)
    {
        var channel = GetImportChannel();
        var device = GetImportDevice(channel.Id);

        var data = opcDaTagModels.Select(b =>
        {
            var a = b.Tag;
            if (!a.IsItem || string.IsNullOrEmpty(a.ItemName))
            {
                return null!;
            }

            ProtectTypeEnum level = ProtectTypeEnum.ReadOnly;
            try
            {
                var userAccessLevel = (accessRights)(a.Properties.FirstOrDefault(b => b.ID.Code == 5).Value);
                level = userAccessLevel == accessRights.readable ? userAccessLevel == accessRights.writable ? ProtectTypeEnum.WriteOnly : ProtectTypeEnum.ReadOnly : ProtectTypeEnum.ReadWrite;
            }
            catch
            {
            }

            var id = Yitter.IdGenerator.YitIdHelper.NextId();
            return new Variable()
            {
                Name = a.Name + "-" + id,
                RegisterAddress = a.ItemName,
                DeviceId = device.Id,
                Enable = true,
                Id = id,
                ProtectType = level,
                IntervalTime = 1000,
                RpcWriteEnable = true,
            };
        }).Where(a => a != null).ToList();
        return (channel, device, data);
    }

    private Device GetImportDevice(long channelId)
    {
        var id = Yitter.IdGenerator.YitIdHelper.NextId();
        var data = new Device()
        {
            Name = Plc.OpcUaProperty.OpcName + "-" + id,
            Id = id,
            ChannelId = channelId,
            Enable = true,
            DevicePropertys = new(),
            PluginName = "ThingsGateway.Plugin.OpcUa.OpcUaMaster",
        };
        data.DevicePropertys.Add(nameof(OpcUaMasterProperty.OpcName), Plc.OpcUaProperty.OpcName);
        data.DevicePropertys.Add(nameof(OpcUaMasterProperty.OpcIP), Plc.OpcUaProperty.OpcIP);
        data.DevicePropertys.Add(nameof(OpcUaMasterProperty.ActiveSubscribe), Plc.OpcUaProperty.ActiveSubscribe.ToString());
        data.DevicePropertys.Add(nameof(OpcUaMasterProperty.CheckRate), Plc.OpcUaProperty.CheckRate.ToString());
        data.DevicePropertys.Add(nameof(OpcUaMasterProperty.DeadBand), Plc.OpcUaProperty.DeadBand.ToString());
        data.DevicePropertys.Add(nameof(OpcUaMasterProperty.GroupSize), Plc.OpcUaProperty.GroupSize.ToString());
        data.DevicePropertys.Add(nameof(OpcUaMasterProperty.UpdateRate), Plc.OpcUaProperty.UpdateRate.ToString());
        return data;
    }

    private Channel GetImportChannel()
    {
        var id = Yitter.IdGenerator.YitIdHelper.NextId();
        var data = new Channel()
        {
            Name = Plc.OpcUaProperty.OpcName + "-" + id,
            Id = id,
            Enable = true,
            ChannelType = ChannelTypeEnum.Other,
        };
        return data;
    }

    [Inject]
    private DownloadService DownloadService { get; set; }

    /// <summary>
    /// 导出到excel
    /// </summary>
    /// <returns></returns>
    public async Task DownChannelExportAsync(Channel data)
    {
        using var memoryStream = await App.RootServices.GetRequiredService<IChannelService>().ExportMemoryStream(new List<Channel>() { data });
        await DownloadService.DownloadFromStreamAsync($"channel{DateTime.Now.ToFileDateTimeFormat()}.xlsx", memoryStream);
    }

    /// <summary>
    /// 导出到excel
    /// </summary>
    /// <returns></returns>
    public async Task DownDeviceExportAsync(Device data, string channelName)
    {
        using var memoryStream = await App.RootServices.GetRequiredService<IDeviceService>().ExportMemoryStream(new List<Device>() { data }, PluginTypeEnum.Collect, channelName);
        await DownloadService.DownloadFromStreamAsync($"device{DateTime.Now.ToFileDateTimeFormat()}.xlsx", memoryStream);
    }

    /// <summary>
    /// 导出到excel
    /// </summary>
    /// <returns></returns>
    public async Task DownDeviceVariableExportAsync(List<Variable> data, string devName)
    {
        using var memoryStream = await App.RootServices.GetRequiredService<IVariableService>().ExportMemoryStream(data, devName);
        await DownloadService.DownloadFromStreamAsync($"variable{DateTime.Now.ToFileDateTimeFormat()}.xlsx", memoryStream);
    }

#endif

    internal class OpcUaTagModel
    {
        internal List<OpcUaTagModel> Children { get; set; }
        internal string Name { get; set; }
        internal string NodeId => (Tag?.ItemName)?.ToString();
        internal BrowseElement Tag { get; set; }

        public List<OpcUaTagModel> GetAllTags()
        {
            List<OpcUaTagModel> allTags = new();
            GetAllTagsRecursive(this, allTags);
            return allTags;
        }

        private void GetAllTagsRecursive(OpcUaTagModel parentTag, List<OpcUaTagModel> allTags)
        {
            allTags.Add(parentTag);
            if (parentTag.Children != null)
                foreach (OpcUaTagModel childTag in parentTag.Children)
                {
                    GetAllTagsRecursive(childTag, allTags);
                }
        }
    }
}