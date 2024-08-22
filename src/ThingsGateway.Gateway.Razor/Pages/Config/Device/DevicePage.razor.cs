//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

using Microsoft.AspNetCore.Components.Forms;

using System.Data;

using ThingsGateway.Core.Extension;
using ThingsGateway.Gateway.Application;
using ThingsGateway.NewLife.X.Extension;
using ThingsGateway.Razor;

namespace ThingsGateway.Gateway.Razor;

public abstract partial class DevicePage : IDisposable
{
    protected IEnumerable<SelectedItem> PluginNames;
    protected abstract PluginTypeEnum PluginType { get; }
    protected abstract string RolePrex { get; }

    private Dictionary<long, Channel> ChannelDict { get; set; } = new();

    [Inject]
    [NotNull]
    private IDispatchService<Channel>? ChannelDispatchService { get; set; }

    [Inject]
    [NotNull]
    private IChannelService? ChannelService { get; set; }

    private Dictionary<long, string> DeviceDict { get; set; } = new();

    [Inject]
    [NotNull]
    private IDeviceService? DeviceService { get; set; }

    [Inject]
    [NotNull]
    private IDispatchService<bool>? DispatchService { get; set; }

    [Inject]
    [NotNull]
    private IDispatchService<PluginOutput>? PluginDispatchService { get; set; }

    [Inject]
    [NotNull]
    private IPluginService? PluginService { get; set; }

    private Device? SearchModel { get; set; } = new();

    public void Dispose()
    {
        ChannelDispatchService.UnSubscribe(Notify);
        DispatchService.UnSubscribe(Notify);
        PluginDispatchService.UnSubscribe(Notify);
    }

    protected override void OnInitialized()
    {
        SearchModel.PluginType = PluginType;
        base.OnInitialized();
    }

    protected override Task OnInitializedAsync()
    {
        ChannelDispatchService.Subscribe(Notify);
        PluginDispatchService.Subscribe(Notify);
        DispatchService.Subscribe(Notify);
        return base.OnInitializedAsync();
    }

    protected override Task OnParametersSetAsync()
    {
        ChannelDict = ChannelService.GetAll().ToDictionary(a => a.Id);
        DeviceDict = DeviceService.GetAll().ToDictionary(a => a.Id, a => a.Name);
        PluginNames = PluginService.GetList(PluginType).BuildPluginSelectList();
        return base.OnParametersSetAsync();
    }

    private async Task Notify(DispatchEntry<PluginOutput> entry)
    {
        await OnParametersSetAsync();
        await InvokeAsync(table.QueryAsync);
        await InvokeAsync(StateHasChanged);
    }

    private async Task Notify(DispatchEntry<Channel> entry)
    {
        await OnParametersSetAsync();
        await InvokeAsync(table.QueryAsync);
        await InvokeAsync(StateHasChanged);
    }

    private async Task Notify(DispatchEntry<bool> entry)
    {
        await OnParametersSetAsync();
        await InvokeAsync(table.QueryAsync);
        await InvokeAsync(StateHasChanged);
    }

    private Task<QueryData<SelectedItem>> OnRedundantDevicesQuery(VirtualizeQueryOption option, Device device)
    {
        var ret = new QueryData<SelectedItem>()
        {
            IsSorted = false,
            IsFiltered = false,
            IsAdvanceSearch = false,
            IsSearch = !option.SearchText.IsNullOrWhiteSpace()
        };
        var items = new List<SelectedItem>() { new SelectedItem(string.Empty, "none") }.Concat(DeviceService.GetAll().WhereIF(!option.SearchText.IsNullOrWhiteSpace(), a => a.Name.Contains(option.SearchText))
            .Where(a => a.PluginName == device.PluginName && a.Id != device.Id).BuildDeviceSelectList()
            );

        ret.TotalCount = items.Count();
        ret.Items = items;
        return Task.FromResult(ret);
    }

    #region 查询

    private async Task<QueryData<Device>> OnQueryAsync(QueryPageOptions options)
    {
        return await Task.Run(async () =>
        {
            var data = await DeviceService.PageAsync(options, SearchModel.PluginType);
            return data;
        });
    }

    #endregion 查询

    #region 修改

    private async Task BatchEdit(IEnumerable<Device> devices)
    {
        var op = new DialogOption()
        {
            IsScrolling = true,
            Title = DefaultLocalizer["BatchEdit"],
            ShowFooter = false,
            ShowCloseButton = false,
            Size = Size.ExtraLarge
        };
        var oldmodel = devices.FirstOrDefault();//默认值显示第一个
        if (oldmodel == null)
        {
            await ToastService.Warning(null, DefaultLocalizer["PleaseSelect"]);
            return;
        }
        var model = oldmodel.Adapt<Device>();//默认值显示第一个
        op.Component = BootstrapDynamicComponent.CreateComponent<DeviceEditComponent>(new Dictionary<string, object?>
        {
             {nameof(DeviceEditComponent.OnValidSubmit), async () =>
            {
                await DeviceService.BatchEditAsync(devices,oldmodel,model);

                await InvokeAsync(async ()=>
                {
        await InvokeAsync(table.QueryAsync);
        await Change();
                });
            }},
            {nameof(DeviceEditComponent.Model),model },
            {nameof(DeviceEditComponent.ValidateEnable),true },
            {nameof(DeviceEditComponent.BatchEditEnable),true },
        });
        await DialogService.Show(op);
    }

    private async Task Change()
    {
        await OnParametersSetAsync();
    }

    private async Task<bool> Delete(IEnumerable<Device> devices)
    {
        try
        {
            return await Task.Run(async () =>
            {
                var result = await DeviceService.DeleteDeviceAsync(devices.Select(a => a.Id));
                await Change();
                return result;
            });

        }
        catch (Exception ex)
        {
            await InvokeAsync(async () =>
            {
                await ToastService.Warning(null, $"{ex.Message}");
            });
            return false;
        }
    }



    private async Task<bool> Save(Device device, ItemChangedType itemChangedType)
    {
        try
        {
            var result = (!PluginServiceUtil.HasDynamicProperty(device.PluginPropertyModel.Value)) || (device.PluginPropertyModel.ValidateForm?.Validate() != false);

            if (result == false)
            {
                return false;
            }
            device.PluginType = PluginType;
            device.DevicePropertys = PluginServiceUtil.SetDict(device.PluginPropertyModel.Value);
            var saveResult = await DeviceService.SaveDeviceAsync(device, itemChangedType);
            await Change();
            return saveResult;
        }
        catch (Exception ex)
        {
            await InvokeAsync(async () =>
            {
                await ToastService.Warning(null, $"{ex.Message}");
            });
            return false;
        }
    }

    #endregion 修改

    #region 导出

    [Inject]
    [NotNull]
    private IGatewayExportService? GatewayExportService { get; set; }

    private async Task ExcelExportAsync(ITableExportContext<Device> tableExportContext)
    {
        await GatewayExportService.OnDeviceExport(tableExportContext.BuildQueryPageOptions(), PluginType == PluginTypeEnum.Collect);

        // 返回 true 时自动弹出提示框
        await ToastService.Default();
    }

    private async Task ExcelImportAsync(ITableExportContext<Device> tableExportContext)
    {
        var op = new DialogOption()
        {
            IsScrolling = true,
            Title = Localizer["ImportExcel"],
            ShowFooter = false,
            ShowCloseButton = false,
            OnCloseAsync = async () =>
            {
                await InvokeAsync(table.QueryAsync);
                await Change();
            },
            Size = Size.ExtraLarge
        };

        Func<IBrowserFile, Task<Dictionary<string, ImportPreviewOutputBase>>> preview = (a => DeviceService.PreviewAsync(a));
        Func<Dictionary<string, ImportPreviewOutputBase>, Task> import = (value => DeviceService.ImportDeviceAsync(value));
        op.Component = BootstrapDynamicComponent.CreateComponent<ImportExcel>(new Dictionary<string, object?>
        {
             {nameof(ImportExcel.Import),import },
            {nameof(ImportExcel.Preview),preview },
        });
        await DialogService.Show(op);
    }

    #endregion 导出

    #region 清空

    private async Task ClearDeviceAsync()
    {
        try
        {
            await Task.Run(async () =>
            {

                await DeviceService.ClearDeviceAsync(PluginType);
                await InvokeAsync(async () =>
                {
                    await ToastService.Default();
                    await InvokeAsync(table.QueryAsync);
                });
            });
        }
        catch (Exception ex)
        {
            await InvokeAsync(async () =>
            {
                await ToastService.Warning(null, $"{ex.Message}");
            });
        }

    }
    #endregion
}
