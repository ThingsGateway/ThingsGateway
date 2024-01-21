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

using BlazorComponent;

using Masa.Blazor;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using Opc.Ua;

using System.Reflection;

using ThingsGateway.Foundation.OpcUa;

#if Plugin

using ThingsGateway.Gateway.Application;
using ThingsGateway.Plugin.OpcUa;

#endif

namespace ThingsGateway.Demo;

/// <summary>
/// 导入变量
/// </summary>
public partial class OpcUaImportVariable : BasePopupComponentBase
{
    private List<ReferenceDescription> actived = new();

    private OPCNodeAttribute[] nodeAttributes;

    private List<OpcUaTagModel> Nodes = new();

    private bool overlay = true;

    /// <summary>
    /// Opc对象
    /// </summary>
    [Parameter]
    public ThingsGateway.Foundation.OpcUa.OpcUaMaster Plc { get; set; }

    /// <summary>
    /// 是否显示子变量
    /// </summary>
    [Parameter]
    public bool IsShowSubvariable { get; set; }

    private List<ReferenceDescription> Actived
    {
        get => actived;
        set
        {
            if (actived?.FirstOrDefault() != value?.FirstOrDefault() && value?.Count > 0)
            {
                actived = value;
                if (actived.FirstOrDefault().NodeId != null)
                    nodeAttributes = Plc.ReadNoteAttributes(actived.FirstOrDefault().NodeId.ToString());
            }
        }
    }

    private List<ReferenceDescription> Selected { get; set; } = new();

#if Plugin

    private bool isDownLoading;

    /// <summary>
    /// 获取设备与变量列表
    /// </summary>
    /// <returns></returns>
    public async Task<(ChannelAddInput, DeviceAddInput, List<VariableAddInput>)> GetImportVariableList()
    {
        var channel = GetImportChannel();
        var device = GetImportDevice(channel.Id);
        foreach (var node in Selected.ToList())
        {
            List<OpcUaTagModel> nodes = await PopulateBranchAsync((NodeId)node.NodeId, true, IsShowSubvariable);
            if (nodes.Count > 0)
            {
                Selected.AddRange(nodes.SelectMany(a => a.GetAllTags()).Select(a => a.Tag).Where(a => a != null).ToList());
            }
        }
        var data = (await SelectAsync(Selected, async a =>
        {
            var nodeClass = (await Plc.ReadNoteAttributeAsync(a.NodeId.ToString(), Opc.Ua.Attributes.NodeClass)).FirstOrDefault().Value.ToString();
            if (nodeClass == nameof(NodeClass.Variable))
            {
                ProtectTypeEnum level = ProtectTypeEnum.ReadOnly;
                DataTypeEnum dataTypeEnum = DataTypeEnum.Object;
                try
                {
                    var userAccessLevel = (AccessLevelType)(await Plc.ReadNoteAttributeAsync(a.NodeId.ToString(), Opc.Ua.Attributes.UserAccessLevel)).FirstOrDefault().Value;
                    level = (userAccessLevel.HasFlag(AccessLevelType.CurrentRead)) ?
        userAccessLevel.HasFlag(AccessLevelType.CurrentWrite) ?
        ProtectTypeEnum.ReadWrite : ProtectTypeEnum.ReadOnly : ProtectTypeEnum.WriteOnly;

                    var dataTypeId = (Opc.Ua.NodeId)(await Plc.ReadNoteAttributeAsync(a.NodeId.ToString(), Opc.Ua.Attributes.DataType)).FirstOrDefault().Value;
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

                var id = Yitter.IdGenerator.YitIdHelper.NextId();

                return new VariableAddInput()
                {
                    Name = a.DisplayName.Text + "-" + id,
                    RegisterAddress = a.NodeId.ToString(),
                    DeviceId = device.Id,
                    DataType = dataTypeEnum,
                    Enable = true,
                    Id = id,
                    ProtectType = level,
                    IntervalTime = 1000,
                    RpcWriteEnable = true,
                };
            }
            else
            {
                return null;
            }
        })).Where(a => a != null).ToList();
        return (channel, device, data);
    }

    private DeviceAddInput GetImportDevice(long channelId)
    {
        var id = Yitter.IdGenerator.YitIdHelper.NextId();

        var data = new DeviceAddInput()
        {
            Name = Plc.OpcUaConfig.OpcUrl + "-" + id,
            Id = id,
            ChannelId = channelId,
            Enable = true,
            DevicePropertys = new(),
            PluginName = "ThingsGateway.Plugin.OpcUa.OpcUaMaster",
        };
        data.DevicePropertys.Add(new() { Name = nameof(OpcUaMasterProperty.OpcUrl), Value = Plc.OpcUaConfig.OpcUrl, Description = typeof(OpcUaMasterProperty).GetProperty(nameof(OpcUaMasterProperty.OpcUrl)).GetCustomAttribute<DynamicPropertyAttribute>().Description });
        data.DevicePropertys.Add(new() { Name = nameof(OpcUaMasterProperty.UserName), Value = Plc.OpcUaConfig.UserName, Description = typeof(OpcUaMasterProperty).GetProperty(nameof(OpcUaMasterProperty.UserName)).GetCustomAttribute<DynamicPropertyAttribute>().Description });
        data.DevicePropertys.Add(new() { Name = nameof(OpcUaMasterProperty.Password), Value = Plc.OpcUaConfig.Password, Description = typeof(OpcUaMasterProperty).GetProperty(nameof(OpcUaMasterProperty.Password)).GetCustomAttribute<DynamicPropertyAttribute>().Description });
        data.DevicePropertys.Add(new() { Name = nameof(OpcUaMasterProperty.CheckDomain), Value = Plc.OpcUaConfig.CheckDomain.ToString(), Description = typeof(OpcUaMasterProperty).GetProperty(nameof(OpcUaMasterProperty.CheckDomain)).GetCustomAttribute<DynamicPropertyAttribute>().Description });
        data.DevicePropertys.Add(new() { Name = nameof(OpcUaMasterProperty.LoadType), Value = Plc.OpcUaConfig.LoadType.ToString(), Description = typeof(OpcUaMasterProperty).GetProperty(nameof(OpcUaMasterProperty.LoadType)).GetCustomAttribute<DynamicPropertyAttribute>().Description });
        data.DevicePropertys.Add(new() { Name = nameof(OpcUaMasterProperty.IsUseSecurity), Value = Plc.OpcUaConfig.IsUseSecurity.ToString(), Description = typeof(OpcUaMasterProperty).GetProperty(nameof(OpcUaMasterProperty.IsUseSecurity)).GetCustomAttribute<DynamicPropertyAttribute>().Description });
        data.DevicePropertys.Add(new() { Name = nameof(OpcUaMasterProperty.ActiveSubscribe), Value = true.ToString(), Description = typeof(OpcUaMasterProperty).GetProperty(nameof(OpcUaMasterProperty.ActiveSubscribe)).GetCustomAttribute<DynamicPropertyAttribute>().Description });
        data.DevicePropertys.Add(new() { Name = nameof(OpcUaMasterProperty.DeadBand), Value = Plc.OpcUaConfig.DeadBand.ToString(), Description = typeof(OpcUaMasterProperty).GetProperty(nameof(OpcUaMasterProperty.DeadBand)).GetCustomAttribute<DynamicPropertyAttribute>().Description });
        data.DevicePropertys.Add(new() { Name = nameof(OpcUaMasterProperty.GroupSize), Value = Plc.OpcUaConfig.GroupSize.ToString(), Description = typeof(OpcUaMasterProperty).GetProperty(nameof(OpcUaMasterProperty.GroupSize)).GetCustomAttribute<DynamicPropertyAttribute>().Description });
        data.DevicePropertys.Add(new() { Name = nameof(OpcUaMasterProperty.UpdateRate), Value = Plc.OpcUaConfig.UpdateRate.ToString(), Description = typeof(OpcUaMasterProperty).GetProperty(nameof(OpcUaMasterProperty.UpdateRate)).GetCustomAttribute<DynamicPropertyAttribute>().Description });
        data.DevicePropertys.Add(new() { Name = nameof(OpcUaMasterProperty.KeepAliveInterval), Value = Plc.OpcUaConfig.KeepAliveInterval.ToString(), Description = typeof(OpcUaMasterProperty).GetProperty(nameof(OpcUaMasterProperty.KeepAliveInterval)).GetCustomAttribute<DynamicPropertyAttribute>().Description });
        return data;
    }

    private ChannelAddInput GetImportChannel()
    {
        var id = Yitter.IdGenerator.YitIdHelper.NextId();
        var data = new ChannelAddInput()
        {
            Name = Plc.OpcUaConfig.OpcUrl + "-" + id,
            Id = id,
            Enable = true,
            ChannelType = ChannelTypeEnum.Other,
        };
        return data;
    }

    private async Task DeviceImport()
    {
        isDownLoading = true;
        await InvokeStateHasChangedAsync();
        try
        {
            var data = await GetImportVariableList();
            if (data.Item3 == null || data.Item3?.Count == 0)
            {
                await PopupService.EnqueueSnackbarAsync("无可用变量", AlertTypes.Warning);
                return;
            }
            await _serviceScope.ServiceProvider.GetService<IChannelService>().AddAsync(data.Item1);
            await _serviceScope.ServiceProvider.GetService<IDeviceService>().AddAsync(data.Item2);
            await _serviceScope.ServiceProvider.GetService<IVariableService>().AddBatchAsync(data.Item3);
            await PopupService.EnqueueSnackbarAsync("成功", AlertTypes.Success);
        }
        finally
        {
            isDownLoading = false;
        }
    }

    private async Task DownDeviceExport()
    {
        isDownLoading = true;
        await InvokeAsync(StateHasChanged);
        try
        {
            var data = await GetImportVariableList();
            if (data.Item3 == null || data.Item3?.Count == 0)
            {
                await PopupService.EnqueueSnackbarAsync("无可用变量", AlertTypes.Warning);
                return;
            }

            await DownChannelExportAsync(data.Item1);
            await DownDeviceExportAsync(data.Item2, data.Item1.Name);
            await DownDeviceVariableExportAsync(data.Item3, data.Item2.Name);
            await PopupService.EnqueueSnackbarAsync("成功", AlertTypes.Success);
        }
        finally
        {
            isDownLoading = false;
        }
    }

    /// <summary>
    /// 导出到excel
    /// </summary>
    /// <returns></returns>
    public async Task DownChannelExportAsync(Channel data)
    {
        using var memoryStream = await _serviceScope.ServiceProvider.GetService<IChannelService>().ExportMemoryStream(new List<Channel>() { data });
        await AppService.DownXlsxAsync(memoryStream, "通道导出");
    }

    /// <summary>
    /// 导出到excel
    /// </summary>
    /// <returns></returns>
    public async Task DownDeviceExportAsync(Device data, string channelName)
    {
        using var memoryStream = await _serviceScope.ServiceProvider.GetService<IDeviceService>().ExportMemoryStream(new List<Device>() { data }, PluginTypeEnum.Collect, channelName);
        await AppService.DownXlsxAsync(memoryStream, "设备导出");
    }

    /// <summary>
    /// 导出到excel
    /// </summary>
    /// <returns></returns>
    public async Task DownDeviceVariableExportAsync(List<VariableAddInput> data, string devName)
    {
        using var memoryStream = await _serviceScope.ServiceProvider.GetService<IVariableService>().ExportMemoryStream(data, devName);
        await AppService.DownXlsxAsync(memoryStream, "变量导出");
    }

#endif

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnInitialized()
    {
        Task.Run(async () =>
        {
            Nodes = await PopulateBranchAsync(ObjectIds.ObjectsFolder, isShowSubvariable: IsShowSubvariable);
            overlay = false;
            await InvokeAsync(StateHasChanged);
        });
        base.OnInitialized();
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

        ReferenceDescriptionCollection references = await FormUtils.BrowseAsync(Plc.Session, nodesToBrowse, false);
        return references;
    }

    private async Task PopulateBranchAsync(OpcUaTagModel model)
    {
        var sourceId = (NodeId)model.Tag.NodeId;
        model.Nodes = await PopulateBranchAsync(sourceId, isShowSubvariable: IsShowSubvariable);
    }

    private async Task<List<OpcUaTagModel>> PopulateBranchAsync(NodeId sourceId, bool isAll = false, bool isShowSubvariable = false)
    {
        if (!Plc.Connected)
        {
            return new() { new() { Name = "未完成连接", Tag = new(), Nodes = null } };
        }
        List<OpcUaTagModel> nodes = new()
        {
            new OpcUaTagModel() { Name = "Browsering..." }
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
                if (isShowSubvariable || target.NodeClass != NodeClass.Variable)
                {
                    var data = await GetReferenceDescriptionCollectionAsync((NodeId)target.NodeId);
                    if (data != null && data.Count > 0)
                    {
                        if (isAll)
                            child.Nodes = await PopulateBranchAsync((NodeId)target.NodeId, isShowSubvariable: IsShowSubvariable);
                        else
                            child.Nodes = new();
                    }
                    else
                    {
                        child.Nodes = null;
                    }
                }
                else
                {
                    child.Nodes = null;
                }

                ////if (target.NodeClass != NodeClass.Variable)  //这个判断注释后会让子节点也是变量的情况下无法加载
                //{
                //    var data = await GetReferenceDescriptionCollectionAsync((NodeId)target.NodeId);
                //    if (data != null && data.Count > 0)
                //    {
                //        if (isAll)
                //            child.Nodes = await PopulateBranchAsync((NodeId)target.NodeId);
                //        else
                //            child.Nodes = new();
                //    }
                //    else
                //    {
                //        child.Nodes = null;
                //    }
                //}
                ////else
                ////{
                ////    child.Nodes = null;
                ////}
                list.Add(child);
            }
        }

        List<OpcUaTagModel> listNode = list;

        nodes.Clear();
        nodes.AddRange(listNode.ToArray());
        return nodes;
    }

    private async Task<TResult[]> SelectAsync<T, TResult>(IEnumerable<T> source, Func<T, Task<TResult>> selector)
    {
        return await Task.WhenAll(source.Select(selector));
    }

    internal class OpcUaTagModel
    {
        internal string Name { get; set; }
        internal string NodeId => (Tag.NodeId).ToString();
        internal List<OpcUaTagModel> Nodes { get; set; } = new();
        internal ReferenceDescription Tag { get; set; }

        public List<OpcUaTagModel> GetAllTags()
        {
            List<OpcUaTagModel> allTags = new();
            GetAllTagsRecursive(this, allTags);
            return allTags;
        }

        private void GetAllTagsRecursive(OpcUaTagModel parentTag, List<OpcUaTagModel> allTags)
        {
            allTags.Add(parentTag);

            if (parentTag.Nodes != null)
                foreach (OpcUaTagModel childTag in parentTag.Nodes)
                {
                    GetAllTagsRecursive(childTag, allTags);
                }
        }
    }
}