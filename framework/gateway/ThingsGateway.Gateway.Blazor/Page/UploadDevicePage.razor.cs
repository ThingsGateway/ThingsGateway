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

using Mapster;

using Masa.Blazor;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

using SqlSugar;

using ThingsGateway.Admin.Blazor;

namespace ThingsGateway.Gateway.Blazor;
/// <summary>
/// 采集设备页面
/// </summary>
public partial class UploadDevicePage
{
    readonly DevicePageInput _search = new();
    IAppDataTable _datatable;
    List<string> _deviceGroups = new();
    List<Device> _devices = new();
    List<DriverPlugin> _driverPlugins;
    ImportExcel _importExcel;
    //string _searchName;
    [Inject]
    AjaxService _ajaxService { get; set; }
    [Inject]
    DriverPluginService _driverPluginService { get; set; }

    [CascadingParameter]
    MainLayout _mainLayout { get; set; }

    /// <inheritdoc/>
    protected override async Task OnParametersSetAsync()
    {
        Refresh();
        _driverPlugins = _driverPluginService.GetAllDriverPlugin(DriverEnum.Upload);
        await base.OnParametersSetAsync();
    }

    private async Task AddCallAsync(DeviceAddInput input)
    {
        await _serviceScope.ServiceProvider.GetService<IUploadDeviceService>().AddAsync(input);
        Refresh();
        await _mainLayout.StateHasChangedAsync();
    }
    private async Task GetDriverProperties(List<Device> data)
    {
        if (data != null)
        {
            if (!data.Any())
            {
                await PopupService.EnqueueSnackbarAsync("需选择一项或多项", AlertTypes.Warning);
                return;
            }
        }
        data ??= _serviceScope.ServiceProvider.GetService<IUploadDeviceService>().GetCacheList(true);
        foreach (var device in data)
        {
            device.DevicePropertys = GetDriverProperties(device.PluginName, device.Id);
        }
        await _serviceScope.ServiceProvider.GetService<IUploadDeviceService>().EditsAsync(data);
        await PopupService.EnqueueSnackbarAsync("刷新成功", AlertTypes.Success);

    }
    async Task CopyDeviceAsync(IEnumerable<Device> data)
    {
        if (!data.Any())
        {
            await PopupService.EnqueueSnackbarAsync("需选择一项或多项", AlertTypes.Warning);
            return;
        }

        await _serviceScope.ServiceProvider.GetService<IUploadDeviceService>().CopyDevAsync(data);
        await DatatableQueryAsync();
        await PopupService.EnqueueSnackbarAsync("复制成功", AlertTypes.Success);
        await _mainLayout.StateHasChangedAsync();
    }

    private async Task DatatableQueryAsync()
    {
        await _datatable?.QueryClickAsync();
    }

    private async Task DeleteCallAsync(IEnumerable<Device> input)
    {
        await _serviceScope.ServiceProvider.GetService<IUploadDeviceService>().DeleteAsync(input.Select(a => a.Id).ToArray());
        Refresh();
        await _mainLayout.StateHasChangedAsync();
    }

    Task<Dictionary<string, ImportPreviewOutputBase>> DeviceImportAsync(IBrowserFile file)
    {
        return _serviceScope.ServiceProvider.GetService<IUploadDeviceService>().PreviewAsync(file);
    }

    async Task DownExportAsync(DevicePageInput input = null)
    {
        await _ajaxService.DownFileAsync("gatewayFile/uploadDevice", DateTimeExtensions.CurrentDateTime.ToFileDateTimeFormat(), input.Adapt<DeviceInput>());
    }
    private async Task DriverValueChangedAsync(DeviceAddInput context, string pluginName)
    {
        if (pluginName.IsNullOrEmpty()) return;

        if (context.DevicePropertys == null || context.DevicePropertys?.Count == 0 || context.PluginName != pluginName)
        {
            try
            {
                var currentDependencyProperty = GetDriverProperties(pluginName, context.Id);
                context.DevicePropertys = currentDependencyProperty;
                await PopupService.EnqueueSnackbarAsync("插件附加属性已更新", AlertTypes.Success);
            }
            catch (Exception ex)
            {
                await PopupService.EnqueueSnackbarAsync(ex);
            }
        }
        context.PluginName = pluginName;
    }

    private async Task EditCallAsync(DeviceEditInput input)
    {
        await _serviceScope.ServiceProvider.GetService<IUploadDeviceService>().EditAsync(input);
        _devices = _serviceScope.ServiceProvider.GetService<IUploadDeviceService>().GetCacheList(true);
        _deviceGroups = _devices?.Select(a => a.DeviceGroup)?.Where(a => a != null).Distinct()?.ToList();
        await _mainLayout.StateHasChangedAsync();
    }

    List<DependencyProperty> GetDriverProperties(string pluginName, long devId)
    {
        return BackgroundServiceUtil.GetBackgroundService<UploadDeviceWorker>().GetDevicePropertys(pluginName, devId);
    }

    private async Task<ISqlSugarPagedList<Device>> QueryCallAsync(DevicePageInput input)
    {
        var data = await _serviceScope.ServiceProvider.GetService<IUploadDeviceService>().PageAsync(input);
        return data;
    }

    private void Refresh()
    {
        _devices = _serviceScope.ServiceProvider.GetService<IUploadDeviceService>().GetCacheList(true);
        _deviceGroups = _devices?.Select(a => a.DeviceGroup)?.Where(a => a != null).Distinct()?.ToList();
    }

    async Task SaveDeviceImportAsync(Dictionary<string, ImportPreviewOutputBase> data)
    {
        await _serviceScope.ServiceProvider.GetService<IUploadDeviceService>().ImportAsync(data);
        await DatatableQueryAsync();
        _importExcel.IsShowImport = false;
        await _mainLayout.StateHasChangedAsync();
    }

}