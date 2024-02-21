//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BlazorComponent;

using Masa.Blazor;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

using ThingsGateway.Foundation.OpcDa.Rcw;

#if Plugin

using ThingsGateway.Gateway.Application;
using ThingsGateway.Plugin.OpcDa;

#endif

namespace ThingsGateway.Demo;

/// <summary>
/// 导入变量
/// </summary>
public partial class OpcDaImportVariable : BasePopupComponentBase
{
    private List<BrowseElement> actived = new();

    private ItemProperty[] nodeAttributes;

    private List<OpcDaTagModel> Nodes = new();

    private bool overlay = true;

    /// <summary>
    /// Opc对象
    /// </summary>
    [Parameter]
    public ThingsGateway.Foundation.OpcDa.OpcDaMaster Plc { get; set; }

    private List<BrowseElement> Actived
    {
        get => actived;
        set
        {
            if (actived?.FirstOrDefault() != value?.FirstOrDefault() && value?.Count > 0)
            {
                actived = value;
                nodeAttributes = actived.FirstOrDefault().Properties;
            }
        }
    }

    private List<BrowseElement> Selected { get; set; } = new();

#if Plugin

    private bool isDownLoading;

    /// <summary>
    /// 获取设备与变量列表
    /// </summary>
    /// <returns></returns>
    public (ChannelAddInput, DeviceAddInput, List<VariableAddInput>) GetImportVariableList()
    {
        var channel = GetImportChannel();
        var device = GetImportDevice(channel.Id);
        //动态加载子项时，导出内容需要添加手动加载代码
        foreach (var node in Selected.ToList())
        {
            var nodes = PopulateBranch(node.ItemName, true);
            if (nodes.Count > 0)
            {
                Selected.AddRange(nodes.SelectMany(a => a.GetAllTags()).Select(a => a.Tag).Where(a => a != null).ToList());
            }
        }
        var data = Selected.Select(a =>
        {
            if (!a.IsItem || string.IsNullOrEmpty(a.ItemName))
            {
                return null;
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
            return new VariableAddInput()
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

    private DeviceAddInput GetImportDevice(long channelId)
    {
        var id = Yitter.IdGenerator.YitIdHelper.NextId();
        var data = new DeviceAddInput()
        {
            Name = Plc.OpcDaConfig.OpcName + "-" + id,
            Id = id,
            ChannelId = channelId,
            Enable = true,
            DevicePropertys = new(),
            PluginName = "ThingsGateway.Plugin.OpcDa.OpcDaMaster",
        };
        data.DevicePropertys.Add(new() { Name = nameof(OpcDaMasterProperty.OpcName), Value = Plc.OpcDaConfig.OpcName, Description = typeof(OpcDaMasterProperty).GetProperty(nameof(OpcDaMasterProperty.OpcName)).GetCustomAttribute<DynamicPropertyAttribute>().Description });
        data.DevicePropertys.Add(new() { Name = nameof(OpcDaMasterProperty.OpcIP), Value = Plc.OpcDaConfig.OpcIP, Description = typeof(OpcDaMasterProperty).GetProperty(nameof(OpcDaMasterProperty.OpcIP)).GetCustomAttribute<DynamicPropertyAttribute>().Description });
        data.DevicePropertys.Add(new() { Name = nameof(OpcDaMasterProperty.ActiveSubscribe), Value = Plc.OpcDaConfig.ActiveSubscribe.ToString(), Description = typeof(OpcDaMasterProperty).GetProperty(nameof(OpcDaMasterProperty.ActiveSubscribe)).GetCustomAttribute<DynamicPropertyAttribute>().Description });
        data.DevicePropertys.Add(new() { Name = nameof(OpcDaMasterProperty.CheckRate), Value = Plc.OpcDaConfig.CheckRate.ToString(), Description = typeof(OpcDaMasterProperty).GetProperty(nameof(OpcDaMasterProperty.CheckRate)).GetCustomAttribute<DynamicPropertyAttribute>().Description });
        data.DevicePropertys.Add(new() { Name = nameof(OpcDaMasterProperty.DeadBand), Value = Plc.OpcDaConfig.DeadBand.ToString(), Description = typeof(OpcDaMasterProperty).GetProperty(nameof(OpcDaMasterProperty.DeadBand)).GetCustomAttribute<DynamicPropertyAttribute>().Description });
        data.DevicePropertys.Add(new() { Name = nameof(OpcDaMasterProperty.GroupSize), Value = Plc.OpcDaConfig.GroupSize.ToString(), Description = typeof(OpcDaMasterProperty).GetProperty(nameof(OpcDaMasterProperty.GroupSize)).GetCustomAttribute<DynamicPropertyAttribute>().Description });
        data.DevicePropertys.Add(new() { Name = nameof(OpcDaMasterProperty.UpdateRate), Value = Plc.OpcDaConfig.UpdateRate.ToString(), Description = typeof(OpcDaMasterProperty).GetProperty(nameof(OpcDaMasterProperty.UpdateRate)).GetCustomAttribute<DynamicPropertyAttribute>().Description });
        return data;
    }

    private ChannelAddInput GetImportChannel()
    {
        var id = Yitter.IdGenerator.YitIdHelper.NextId();
        var data = new ChannelAddInput()
        {
            Name = Plc.OpcDaConfig.OpcName + "-" + id,
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
            var data = GetImportVariableList();
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
            var data = GetImportVariableList();
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

    /// <inheritdoc/>
    protected override async Task OnInitializedAsync()
    {
        await Task.Factory.StartNew(async () =>
        {
            Nodes = PopulateBranch("");
            overlay = false;
            await InvokeAsync(StateHasChanged);
        });
        await base.OnInitializedAsync();
    }

    private List<OpcDaTagModel> PopulateBranch(string sourceId, bool isAll = false)
    {
        List<OpcDaTagModel> nodes = new()
        {
            new OpcDaTagModel() { Name = "Browsering..." }
        };
        try
        {
            var references = Plc.GetBrowseElements(sourceId);
            List<OpcDaTagModel> list = new();
            if (references != null)
            {
                for (int ii = 0; ii < references.Count; ii++)
                {
                    var target = references[ii];
                    OpcDaTagModel child = new()
                    {
                        Name = target.Name,
                        Tag = target
                    };

                    if (target.HasChildren)
                    {
                        if (isAll)
                            child.Nodes = PopulateBranch(target.ItemName);
                        else
                            child.Nodes = new();
                    }
                    else
                    {
                        child.Nodes = null;
                    }

                    list.Add(child);
                }
            }

            List<OpcDaTagModel> listNode = list;
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
                    Nodes = null
                }
            };
        }
    }

    private async Task PopulateBranchAsync(OpcDaTagModel model)
    {
        await Task.Run(() =>
       {
           var sourceId = model.Tag.ItemName;
           model.Nodes = PopulateBranch(sourceId);
       });
    }

    internal class OpcDaTagModel
    {
        internal string Name { get; set; }
        internal string NodeId => (Tag?.ItemName)?.ToString();
        internal List<OpcDaTagModel> Nodes { get; set; } = new();
        internal BrowseElement Tag { get; set; }

        public List<OpcDaTagModel> GetAllTags()
        {
            List<OpcDaTagModel> allTags = new();
            GetAllTagsRecursive(this, allTags);
            return allTags;
        }

        private void GetAllTagsRecursive(OpcDaTagModel parentTag, List<OpcDaTagModel> allTags)
        {
            allTags.Add(parentTag);
            if (parentTag.Nodes != null)
                foreach (OpcDaTagModel childTag in parentTag.Nodes)
                {
                    GetAllTagsRecursive(childTag, allTags);
                }
        }
    }
}