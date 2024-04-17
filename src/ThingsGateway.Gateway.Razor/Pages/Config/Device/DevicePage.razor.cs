
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------


using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

using NewLife.Extension;

using System.Data;

using ThingsGateway.Core.Extension;
using ThingsGateway.Gateway.Application;
using ThingsGateway.Razor;

namespace ThingsGateway.Gateway.Razor;

public abstract partial class DevicePage
{
    protected IEnumerable<SelectedItem> PluginNames;
    protected abstract PluginTypeEnum PluginType { get; }
    protected abstract string RolePrex { get; }

    [Inject]
    [NotNull]
    private IPluginService? PluginService { get; set; }

    [Inject]
    [NotNull]
    private IDeviceService? DeviceService { get; set; }

    [Inject]
    [NotNull]
    private IChannelService? ChannelService { get; set; }

    private Device? SearchModel { get; set; } = new();

    protected override void OnInitialized()
    {
        SearchModel.PluginType = PluginType;
        base.OnInitialized();
    }

    private Dictionary<long, Channel> ChannelDict { get; set; } = new();

    protected override Task OnParametersSetAsync()
    {
        ChannelDict = ChannelService.GetAll().ToDictionary(a => a.Id);
        PluginNames = PluginService.GetList(PluginType).BuildPluginSelectList();
        return base.OnParametersSetAsync();
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
        var items = DeviceService.GetAll().WhereIF(!option.SearchText.IsNullOrWhiteSpace(), a => a.Name.Contains(option.SearchText))
            .Where(a => a.PluginName == device.PluginName && a.Id != device.Id).BuildDeviceSelectList();
        ret.TotalCount = items.Count();
        ret.Items = items;
        return Task.FromResult(ret);
    }

    #region 查询

    private async Task<QueryData<Device>> OnQueryAsync(QueryPageOptions options)
    {
        var data = await DeviceService.PageAsync(options, SearchModel.PluginType);
        return data;
    }

    #endregion 查询

    #region 修改

    private async Task DeleteAllAsync()
    {
        try
        {
            await DeviceService.ClearDeviceAsync(PluginType);
        }
        catch (Exception ex)
        {
            await ToastService.Warning(null, $"{ex.Message}");
        }
    }

    private async Task<bool> Save(Device device, ItemChangedType itemChangedType)
    {
        try
        {
            var result = device.PluginPropertyModel.ValidateForm?.Validate();
            if (result == false)
            {
                return false;
            }
            device.PluginType = PluginType;
            device.DevicePropertys = PluginServiceUtil.SetDict(device.PluginPropertyModel.Value);
            return await DeviceService.SaveDeviceAsync(device, itemChangedType);
        }
        catch (Exception ex)
        {
            await ToastService.Warning(null, $"{ex.Message}");
            return false;
        }
    }

    private async Task<bool> Delete(IEnumerable<Device> devices)
    {
        try
        {
            return await DeviceService.DeleteDeviceAsync(devices.Select(a => a.Id));
        }
        catch (Exception ex)
        {
            await ToastService.Warning(null, $"{ex.Message}");
            return false;
        }
    }

    #endregion 修改

    #region 导出

    [Inject]
    [NotNull]
    private ITableExport? TableExport { get; set; }

    [Inject]
    private IJSRuntime JSRuntime { get; set; }

    private async Task ExcelExportAsync(ITableExportContext<Device> tableExportContext)
    {
        await using var ajaxJS = await JSRuntime.InvokeAsync<IJSObjectReference>("import", $"{WebsiteConst.DefaultResourceUrl}js/downloadFile.js");
        string url = PluginType == PluginTypeEnum.Collect ? "api/gatewayExport/collectdevice" : "api/gatewayExport/businessdevice";
        string fileName = DateTime.Now.ToFileDateTimeFormat();
        var dtoObject = tableExportContext.BuildQueryPageOptions();
        await ajaxJS.InvokeVoidAsync("blazor_downloadFile", url, fileName, dtoObject);

        // 返回 true 时自动弹出提示框
        await ToastService.Default();
    }

    private async Task ExcelImportAsync(ITableExportContext<Device> tableExportContext)
    {
        var op = new DialogOption()
        {
            Title = Localizer["ImportExcel"],
            ShowFooter = false,
            ShowCloseButton = false,
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

        await table.QueryAsync();
    }

    #endregion 导出
}