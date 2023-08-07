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

using Furion;

using Mapster;

using Masa.Blazor;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

using SqlSugar;

using ThingsGateway.Admin.Blazor;
using ThingsGateway.Admin.Blazor.Core;
using ThingsGateway.Admin.Core;
using ThingsGateway.Application;

namespace ThingsGateway.Blazor;
/// <summary>
/// 采集设备页面
/// </summary>
public partial class CollectDevicePage
{
    readonly CollectDevicePageInput search = new();
    IAppDataTable _datatable;
    List<string> _deviceGroups = new();
    string _searchName;
    List<CollectDevice> CollectDevices = new();
    List<DriverPluginCategory> DriverPlugins;
    ImportExcel ImportExcel;
    StringNumber tab;
    [Inject]
    AjaxService AjaxService { get; set; }
    [CascadingParameter]
    MainLayout MainLayout { get; set; }
    /// <inheritdoc/>
    protected override async Task OnParametersSetAsync()
    {
        CollectDevices = App.GetService<ICollectDeviceService>().GetCacheList();
        DriverPlugins = App.GetService<IDriverPluginService>().GetDriverPluginChildrenList(DriverEnum.Collect);
        _deviceGroups = CollectDevices?.Select(a => a.DeviceGroup)?.Where(a => a != null).Distinct()?.ToList();
        await base.OnParametersSetAsync();
    }

    private async Task AddCallAsync(CollectDeviceAddInput input)
    {
        await CollectDeviceService.AddAsync(input);
        CollectDevices = CollectDeviceService.GetCacheList();
        _deviceGroups = CollectDevices?.Select(a => a.DeviceGroup)?.Where(a => a != null).Distinct()?.ToList();
        await MainLayout.StateHasChangedAsync();
    }

    async Task CopyDevAndVarAsync(IEnumerable<CollectDevice> data)
    {
        if (!data.Any())
        {
            await PopupService.EnqueueSnackbarAsync("需选择一项或多项", AlertTypes.Warning);
            return;
        }

        await CollectDeviceService.CopyDevAndVarAsync(data);
        await DatatableQueryAsync();
        await PopupService.EnqueueSnackbarAsync("复制成功", AlertTypes.Success);
        await MainLayout.StateHasChangedAsync();
    }

    async Task CopyDeviceAsync(IEnumerable<CollectDevice> data)
    {
        if (!data.Any())
        {
            await PopupService.EnqueueSnackbarAsync("需选择一项或多项", AlertTypes.Warning);
            return;
        }

        await CollectDeviceService.CopyDevAsync(data);
        await DatatableQueryAsync();
        await PopupService.EnqueueSnackbarAsync("复制成功", AlertTypes.Success);
        await MainLayout.StateHasChangedAsync();
    }

    private async Task DatatableQueryAsync()
    {
        await _datatable?.QueryClickAsync();
    }

    private async Task DeleteCallAsync(IEnumerable<CollectDevice> input)
    {
        await CollectDeviceService.DeleteAsync(input.Select(a => a.Id).ToArray());
        CollectDevices = CollectDeviceService.GetCacheList();
        _deviceGroups = CollectDevices?.Select(a => a.DeviceGroup)?.Where(a => a != null).Distinct()?.ToList();
        await MainLayout.StateHasChangedAsync();
    }

    Task<Dictionary<string, ImportPreviewOutputBase>> DeviceImportAsync(IBrowserFile file)
    {
        return CollectDeviceService.PreviewAsync(file);
    }
    async Task DownExportAsync(CollectDevicePageInput input = null)
    {
        await AjaxService.DownFileAsync("gatewayFile/collectDevice", SysDateTimeExtensions.CurrentDateTime.ToFileDateTimeFormat(), input.Adapt<CollectDeviceInput>());
    }
    private async Task DriverValueChangedAsync(CollectDeviceAddInput context, long pluginId)
    {
        if (pluginId <= 0) return;

        if (context.DevicePropertys == null || context.DevicePropertys?.Count == 0 || context.PluginId != pluginId)
        {
            try
            {
                context.DevicePropertys = GetDriverProperties(pluginId, context.Id);
                await PopupService.EnqueueSnackbarAsync("插件附加属性已更新", AlertTypes.Success);
            }
            catch (Exception ex)
            {
                await PopupService.EnqueueSnackbarAsync(ex.Message, AlertTypes.Error);
            }
        }
        context.PluginId = pluginId;
    }

    private async Task EditCallAsync(CollectDeviceEditInput input)
    {
        await CollectDeviceService.EditAsync(input);
        CollectDevices = CollectDeviceService.GetCacheList();
        _deviceGroups = CollectDevices?.Select(a => a.DeviceGroup)?.Where(a => a != null).Distinct()?.ToList();
        await MainLayout.StateHasChangedAsync();
    }


    List<DependencyProperty> GetDriverProperties(long driverId, long devId)
    {
        return ServiceHelper.GetBackgroundService<CollectDeviceWorker>().GetDevicePropertys(driverId, devId);
    }

    private async Task<SqlSugarPagedList<CollectDevice>> QueryCallAsync(CollectDevicePageInput input)
    {
        var data = await CollectDeviceService.PageAsync(input);
        return data;
    }

    async Task SaveDeviceImportAsync(Dictionary<string, ImportPreviewOutputBase> data)
    {
        await CollectDeviceService.ImportAsync(data);
        await DatatableQueryAsync();
        ImportExcel.IsShowImport = false;
        await MainLayout.StateHasChangedAsync();
    }
}