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
using Microsoft.Extensions.Localization;

using System.Diagnostics.CodeAnalysis;

using ThingsGateway.Foundation.OpcDa.Rcw;

#if Plugin


#endif

namespace ThingsGateway.Debug;

/// <summary>
/// 导入变量
/// </summary>
public partial class OpcDaImportVariable
{
    private List<BrowseElement> actived = new();

    private List<OpcDaTagModel> Nodes = new();
    private List<TreeViewItem<OpcDaTagModel>> Items = new();
    private bool ModelEqualityComparer(OpcDaTagModel x, OpcDaTagModel y) => x.NodeId == y.NodeId;
    private Task<IEnumerable<TreeViewItem<OpcDaTagModel>>> OnExpandNodeAsync(TreeViewItem<OpcDaTagModel> treeViewItem)
    {

    }
    [Inject]
    [NotNull]
    private IStringLocalizer<OpcDaImportVariable>? Localizer { get; set; }

    [Inject]
    [NotNull]
    private ToastService? ToastService { get; set; }

    /// <summary>
    /// Opc对象
    /// </summary>
    [Parameter]
    public ThingsGateway.Foundation.OpcDa.OpcDaMaster Plc { get; set; }
    private bool ShowSkeleton = true;
    private List<BrowseElement> Selected { get; set; } = new();

#if Plugin1

    private bool isDownLoading;

    /// <summary>
    /// 获取设备与变量列表
    /// </summary>
    /// <returns></returns>
    public (Channel, Device, List<Variable>) GetImportVariableList()
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
            Name = Plc.OpcDaConfig.OpcName + "-" + id,
            Id = id,
            ChannelId = channelId,
            Enable = true,
            DevicePropertys = new(),
            PluginName = "ThingsGateway.Plugin.OpcDa.OpcDaMaster",
        };
        data.DevicePropertys.Add(nameof(OpcDaMasterProperty.OpcName), Plc.OpcDaConfig.OpcName);
        data.DevicePropertys.Add(nameof(OpcDaMasterProperty.OpcIP), Plc.OpcDaConfig.OpcIP);
        data.DevicePropertys.Add(nameof(OpcDaMasterProperty.ActiveSubscribe), Plc.OpcDaConfig.ActiveSubscribe.ToString());
        data.DevicePropertys.Add(nameof(OpcDaMasterProperty.CheckRate), Plc.OpcDaConfig.CheckRate.ToString());
        data.DevicePropertys.Add(nameof(OpcDaMasterProperty.DeadBand), Plc.OpcDaConfig.DeadBand.ToString());
        data.DevicePropertys.Add(nameof(OpcDaMasterProperty.GroupSize), Plc.OpcDaConfig.GroupSize.ToString());
        data.DevicePropertys.Add(nameof(OpcDaMasterProperty.UpdateRate), Plc.OpcDaConfig.UpdateRate.ToString());
        return data;
    }

    private Channel GetImportChannel()
    {
        var id = Yitter.IdGenerator.YitIdHelper.NextId();
        var data = new Channel()
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
                await ToastService.Warning(Localizer["NoVariablesAvailable"], Localizer["NoVariablesAvailable"]);
                return;
            }
            await App.RootServices.GetRequiredService<IChannelService>().SaveChannelAsync(data.Item1, ItemChangedType.Add);
            await App.RootServices.GetRequiredService<IDeviceService>().SaveDeviceAsync(data.Item2, ItemChangedType.Add);
            await App.RootServices.GetRequiredService<IVariableService>().AddBatchAsync(data.Item3);
            await ToastService.Success(Localizer["Success"], Localizer["Success"]);
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
                await ToastService.Warning(Localizer["NoVariablesAvailable"], Localizer["NoVariablesAvailable"]);
                return;
            }

            await DownChannelExportAsync(data.Item1);
            await DownDeviceExportAsync(data.Item2, data.Item1.Name);
            await DownDeviceVariableExportAsync(data.Item3, data.Item2.Name);
            await ToastService.Success(Localizer["Success"], Localizer["Success"]);
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
        using var memoryStream = await App.RootServices.GetRequiredService<IChannelService>().ExportMemoryStream(new List<Channel>() { data });
        //await AppService.DownXlsxAsync(memoryStream, "通道导出");
    }

    /// <summary>
    /// 导出到excel
    /// </summary>
    /// <returns></returns>
    public async Task DownDeviceExportAsync(Device data, string channelName)
    {
        using var memoryStream = await App.RootServices.GetRequiredService<IDeviceService>().ExportMemoryStream(new List<Device>() { data }, PluginTypeEnum.Collect, channelName);
        //await AppService.DownXlsxAsync(memoryStream, "设备导出");
    }

    /// <summary>
    /// 导出到excel
    /// </summary>
    /// <returns></returns>
    public async Task DownDeviceVariableExportAsync(List<Variable> data, string devName)
    {
        using var memoryStream = await App.RootServices.GetRequiredService<IVariableService>().ExportMemoryStream(data, devName);
        //await AppService.DownXlsxAsync(memoryStream, "变量导出");
    }

#endif

    /// <inheritdoc/>
    protected override async Task OnInitializedAsync()
    {
        await Task.Factory.StartNew(async () =>
        {
            Items = PopulateBranch("");
            ShowSkeleton = false;
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
                            child.Items = PopulateBranch(target.ItemName);
                        else
                            child.Items = new();
                    }
                    else
                    {
                        child.Items = null;
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
                    Items = null
                }
            };
        }
    }

    private async Task PopulateBranchAsync(OpcDaTagModel model)
    {
        await Task.Run(() =>
       {
           var sourceId = model.Tag.ItemName;
           model.Items = PopulateBranch(sourceId);
       });
    }

    internal class OpcDaTagModel
    {
        internal string Name { get; set; }
        internal string NodeId => (Tag?.ItemName)?.ToString();
        internal List<OpcDaTagModel> Items { get; set; } = new();
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
            if (parentTag.Items != null)
                foreach (OpcDaTagModel childTag in parentTag.Items)
                {
                    GetAllTagsRecursive(childTag, allTags);
                }
        }
    }
}