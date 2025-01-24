//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------
#pragma warning disable CA2007 // 考虑对等待的任务调用 ConfigureAwait

using BootstrapBlazor.Components;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

using Opc.Ua;

using System.Diagnostics.CodeAnalysis;

using ThingsGateway.Extension;
using ThingsGateway.Foundation.OpcUa;

#if Plugin

using ThingsGateway.Gateway.Application;
using ThingsGateway.Plugin.OpcUa;

#endif

using ThingsGateway.Razor;

using TouchSocket.Core;

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
            await Task.Run(async () =>
            {
                Items = BuildTreeItemList(await PopulateBranchAsync(ObjectIds.ObjectsFolder), RenderTreeItem).ToList();
                ShowSkeleton = false;
                await InvokeAsync(StateHasChanged);
            });
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    /// <summary>
    /// 构建树节点，传入的列表已经是树结构
    /// </summary>
    private static IEnumerable<TreeViewItem<OpcUaTagModel>> BuildTreeItemList(IEnumerable<OpcUaTagModel> opcUaTagModels, Microsoft.AspNetCore.Components.RenderFragment<OpcUaTagModel> render = null, TreeViewItem<OpcUaTagModel>? parent = null)
    {
        if (opcUaTagModels == null) return Enumerable.Empty<TreeViewItem<OpcUaTagModel>>();
        var trees = new List<TreeViewItem<OpcUaTagModel>>();
        foreach (var node in opcUaTagModels)
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

    private static bool ModelEqualityComparer(OpcUaTagModel x, OpcUaTagModel y) => x.NodeId == y.NodeId;

    private async Task<IEnumerable<TreeViewItem<OpcUaTagModel>>> OnExpandNodeAsync(TreeViewItem<OpcUaTagModel> treeViewItem)
    {
        var data = BuildTreeItemList(await PopulateBranchAsync(treeViewItem.Value.NodeId), RenderTreeItem);
        return data;
    }

    private Task OnTreeItemChecked(List<TreeViewItem<OpcUaTagModel>> items)
    {
        Nodes = items.Select(a => a.Value);
        return Task.CompletedTask;
    }

    private async Task PopulateBranch(OpcUaTagModel model)
    {
        if (model.Children != null)
        {
            if (model.Children.Count == 0)
            {
                model.Children = await PopulateBranchAsync((NodeId)model.Tag.NodeId);
            }
            foreach (var item in model.Children)
            {
                await PopulateBranch(item);
            }
        }
    }

    private async Task<ReferenceDescriptionCollection> GetReferenceDescriptionCollectionAsync(NodeId sourceId)
    {
        BrowseDescription nodeToBrowse1 = new()
        {
            NodeId = sourceId,
            BrowseDirection = BrowseDirection.Forward,
            ReferenceTypeId = ReferenceTypeIds.Aggregates,
            IncludeSubtypes = true,
            NodeClassMask = (uint)(NodeClass.Object | NodeClass.Variable | NodeClass.Method | NodeClass.ReferenceType | NodeClass.ObjectType | NodeClass.View | NodeClass.VariableType | NodeClass.DataType),
            ResultMask = (uint)BrowseResultMask.All
        };

        BrowseDescription nodeToBrowse2 = new()
        {
            NodeId = sourceId,
            BrowseDirection = BrowseDirection.Forward,
            ReferenceTypeId = ReferenceTypeIds.Organizes,
            IncludeSubtypes = true,
            NodeClassMask = (uint)(NodeClass.Object | NodeClass.Variable | NodeClass.Method | NodeClass.View | NodeClass.ReferenceType | NodeClass.ObjectType | NodeClass.VariableType | NodeClass.DataType),
            ResultMask = (uint)BrowseResultMask.All
        };

        BrowseDescriptionCollection nodesToBrowse = new()
        {
            nodeToBrowse1,
            nodeToBrowse2
        };

        ReferenceDescriptionCollection references = await OpcUaUtils.BrowseAsync(Plc.Session, nodesToBrowse, false);
        return references;
    }

    [Parameter]
    public bool ShowSubvariable { get; set; }

    private async Task<List<OpcUaTagModel>> PopulateBranchAsync(NodeId sourceId, bool isAll = false)
    {
        if (!Plc.Connected)
        {
            return new() { };
        }
        List<OpcUaTagModel> nodes = new()
        {
        };

        ReferenceDescriptionCollection references = await GetReferenceDescriptionCollectionAsync(sourceId);
        List<OpcUaTagModel> list = new();
        if (references != null)
        {
            for (int ii = 0; ii < references.Count; ii++)
            {
                ReferenceDescription target = references[ii];
                OpcUaTagModel child = new()
                {
                    Name = Utils.Format("{0}", target),
                    Tag = target
                };
                if (ShowSubvariable || target.NodeClass != NodeClass.Variable)
                {
                    var data = await GetReferenceDescriptionCollectionAsync((NodeId)target.NodeId);
                    if (data != null && data.Count > 0)
                    {
                        if (isAll)
                            child.Children = await PopulateBranchAsync((NodeId)target.NodeId);
                        else
                            child.Children = new();
                    }
                }

                list.Add(child);
            }
        }

        nodes.Clear();
        nodes.AddRange(list.ToArray());
        return nodes;
    }

#if Plugin

    private async Task<List<OpcUaTagModel>> GetAllTag(IEnumerable<OpcUaTagModel> opcUaTagModels)
    {
        List<OpcUaTagModel> result = new();
        foreach (var item in opcUaTagModels)
        {
            await PopulateBranch(item);

            result.AddRange(item.GetAllTags().Where(a => a.Children == null).ToList());
        }

        return result;
    }

    private async Task OnClickClose()
    {
        if (OnCloseAsync != null)
            await OnCloseAsync();
    }

    private async Task OnClickExport()
    {
        try
        {
            if (Nodes == null) return;
            var data = await GetImportVariableList((await GetAllTag(Nodes)).DistinctBy(a => a.NodeId));
            if (data.Item3 == null || data.Item3?.Count == 0)
            {
                await ToastService.Warning(OpcUaPropertyLocalizer["NoVariablesAvailable"], OpcUaPropertyLocalizer["NoVariablesAvailable"]);
                return;
            }

            await DownChannelExportAsync(data.Item1);
            await DownDeviceExportAsync(data.Item2, data.Item1.Name);
            await DownDeviceVariableExportAsync(data.Item3.ToList(), data.Item2.Name);
            await ToastService.Default();
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
        }
    }

    private async Task OnClickSave()
    {
        try
        {
            if (Nodes == null) return;
            var data = await GetImportVariableList((await GetAllTag(Nodes)).DistinctBy(a => a.NodeId));
            if (data.Item3 == null || data.Item3?.Count == 0)
            {
                await ToastService.Warning(OpcUaPropertyLocalizer["NoVariablesAvailable"], OpcUaPropertyLocalizer["NoVariablesAvailable"]);
                return;
            }
            await App.RootServices.GetRequiredService<IChannelRuntimeService>().SaveChannelAsync(data.Item1, ItemChangedType.Add);
            await App.RootServices.GetRequiredService<IDeviceRuntimeService>().SaveDeviceAsync(data.Item2, ItemChangedType.Add);
            await App.RootServices.GetRequiredService<IVariableRuntimeService>().AddBatchAsync(data.Item3.ToList());
            await ToastService.Default();
        }
        catch (Exception ex)
        {
            await ToastService.Warning(ex.Message);
        }
    }

    /// <summary>
    /// 获取设备与变量列表
    /// </summary>
    /// <returns></returns>
    private async Task<(Channel, Device, IList<Variable>)> GetImportVariableList(IEnumerable<OpcUaTagModel> opcUaTagModels)
    {
        var channel = GetImportChannel();
        var device = GetImportDevice(channel.Id);
        var variables = new ConcurrentList<Variable>();
        await opcUaTagModels.ParallelForEachAsync(async (b, token) =>
         {
             var a = b.Tag;
             var nodeClass = (await Plc.ReadNoteAttributeAsync(a.NodeId.ToString(), Opc.Ua.Attributes.NodeClass, token)).FirstOrDefault().Value.ToString();
             if (nodeClass == nameof(NodeClass.Variable))
             {
                 ProtectTypeEnum level = ProtectTypeEnum.ReadOnly;
                 DataTypeEnum dataTypeEnum = DataTypeEnum.Object;
                 try
                 {
                     var userAccessLevel = (AccessLevelType)(await Plc.ReadNoteAttributeAsync(a.NodeId.ToString(), Opc.Ua.Attributes.UserAccessLevel, token)).FirstOrDefault().Value;
                     level = (userAccessLevel.HasFlag(AccessLevelType.CurrentRead)) ?
         userAccessLevel.HasFlag(AccessLevelType.CurrentWrite) ?
         ProtectTypeEnum.ReadWrite : ProtectTypeEnum.ReadOnly : ProtectTypeEnum.WriteOnly;

                     var dataTypeId = (Opc.Ua.NodeId)(await Plc.ReadNoteAttributeAsync(a.NodeId.ToString(), Opc.Ua.Attributes.DataType, token)).FirstOrDefault().Value;
                     var dataType = Opc.Ua.TypeInfo.GetSystemType(dataTypeId, Plc.Session.Factory);
                     var result = dataType != null && Enum.TryParse<DataTypeEnum>(dataType.Name, out dataTypeEnum);
                     if (!result)
                     {
                         dataTypeEnum = DataTypeEnum.Object;
                     }
                 }
                 catch
                 {
                 }

                 var id = Admin.Application.CommonUtils.GetSingleId();

                 variables.Add(new Variable()
                 {
                     Name = a.DisplayName.Text + "-" + id,
                     RegisterAddress = a.NodeId.ToString(),
                     DeviceId = device.Id,
                     DataType = dataTypeEnum,
                     Enable = true,
                     Id = id,
                     ProtectType = level,
                     IntervalTime = "1000",
                     RpcWriteEnable = true,
                 });
             }
         }, Environment.ProcessorCount / 2);

        return (channel, device, variables);
    }

    private Device GetImportDevice(long channelId)
    {
        var id = Admin.Application.CommonUtils.GetSingleId();
        var data = new Device()
        {
            Name = Plc.OpcUaProperty.OpcUrl + "-" + id,
            Id = id,
            ChannelId = channelId,
            Enable = true,
            DevicePropertys = new(),
        };
        data.DevicePropertys.Add(nameof(OpcUaMasterProperty.OpcUrl), Plc.OpcUaProperty.OpcUrl);
        data.DevicePropertys.Add(nameof(OpcUaMasterProperty.UserName), Plc.OpcUaProperty.UserName);
        data.DevicePropertys.Add(nameof(OpcUaMasterProperty.Password), Plc.OpcUaProperty.Password);
        data.DevicePropertys.Add(nameof(OpcUaMasterProperty.CheckDomain), Plc.OpcUaProperty.CheckDomain.ToString());
        data.DevicePropertys.Add(nameof(OpcUaMasterProperty.LoadType), Plc.OpcUaProperty.LoadType.ToString());
        data.DevicePropertys.Add(nameof(OpcUaMasterProperty.UseSecurity), Plc.OpcUaProperty.UseSecurity.ToString());
        data.DevicePropertys.Add(nameof(OpcUaMasterProperty.ActiveSubscribe), true.ToString());
        data.DevicePropertys.Add(nameof(OpcUaMasterProperty.DeadBand), Plc.OpcUaProperty.DeadBand.ToString());
        data.DevicePropertys.Add(nameof(OpcUaMasterProperty.GroupSize), Plc.OpcUaProperty.GroupSize.ToString());
        data.DevicePropertys.Add(nameof(OpcUaMasterProperty.UpdateRate), Plc.OpcUaProperty.UpdateRate.ToString());
        data.DevicePropertys.Add(nameof(OpcUaMasterProperty.KeepAliveInterval), Plc.OpcUaProperty.KeepAliveInterval.ToString());
        return data;
    }

    private Channel GetImportChannel()
    {
        var id = Admin.Application.CommonUtils.GetSingleId();
        var data = new Channel()
        {
            Name = Plc.OpcUaProperty.OpcUrl + "-" + id,
            Id = id,
            Enable = true,
            ChannelType = ChannelTypeEnum.Other,
            PluginName = "ThingsGateway.Plugin.OpcUa.OpcUaMaster",
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
        using var memoryStream = await App.RootServices.GetRequiredService<IChannelRuntimeService>().ExportMemoryStream(new List<Channel>() { data });
        await DownloadService.DownloadFromStreamAsync($"channel{DateTime.Now.ToFileDateTimeFormat()}.xlsx", memoryStream);
    }

    /// <summary>
    /// 导出到excel
    /// </summary>
    /// <returns></returns>
    public async Task DownDeviceExportAsync(Device data, string channelName)
    {
        using var memoryStream = await App.RootServices.GetRequiredService<IDeviceRuntimeService>().ExportMemoryStream(new List<Device>() { data }, channelName);
        await DownloadService.DownloadFromStreamAsync($"device{DateTime.Now.ToFileDateTimeFormat()}.xlsx", memoryStream);
    }

    /// <summary>
    /// 导出到excel
    /// </summary>
    /// <returns></returns>
    public async Task DownDeviceVariableExportAsync(List<Variable> data, string devName)
    {
        using var memoryStream = await App.RootServices.GetRequiredService<IVariableRuntimeService>().ExportMemoryStream(data, devName);
        await DownloadService.DownloadFromStreamAsync($"variable{DateTime.Now.ToFileDateTimeFormat()}.xlsx", memoryStream);
    }

#endif

    internal sealed class OpcUaTagModel
    {
        internal List<OpcUaTagModel> Children { get; set; }
        internal string Name { get; set; }
        internal string NodeId => (Tag?.NodeId)?.ToString();
        internal ReferenceDescription Tag { get; set; }

        public List<OpcUaTagModel> GetAllTags()
        {
            List<OpcUaTagModel> allTags = new();
            OpcUaTagModel.GetAllTagsRecursive(this, allTags);
            return allTags;
        }

        private static void GetAllTagsRecursive(OpcUaTagModel parentTag, List<OpcUaTagModel> allTags)
        {
            allTags.Add(parentTag);
            if (parentTag.Children != null)
                foreach (OpcUaTagModel childTag in parentTag.Children)
                {
                    OpcUaTagModel.GetAllTagsRecursive(childTag, allTags);
                }
        }
    }
}
