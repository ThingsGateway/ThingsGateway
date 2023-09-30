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

namespace ThingsGateway.Gateway.Blazor;

/// <summary>
/// 上传设备页
/// </summary>
public partial class UploadDevicePage
{
    private readonly UploadDevicePageInput search = new();
    private IAppDataTable _datatable;
    List<string> _deviceGroups = new();
    string _searchName;
    List<DriverPluginCategory> DriverPlugins;
    ImportExcel ImportExcel;
    StringNumber tab;
    [Inject]
    AjaxService AjaxService { get; set; }


    [CascadingParameter]
    MainLayout MainLayout { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    protected override async Task OnParametersSetAsync()
    {
        DriverPlugins = App.GetService<IDriverPluginService>().GetDriverPluginChildrenList(DriverEnum.Upload);
        _deviceGroups = App.GetService<IUploadDeviceService>().GetCacheList()?.Select(a => a.DeviceGroup)?.Where(a => a != null).Distinct()?.ToList();
        await base.OnParametersSetAsync();
    }
    private async Task AddCallAsync(UploadDeviceAddInput input)
    {
        await App.GetService<UploadDeviceService>().AddAsync(input);
        _deviceGroups = App.GetService<UploadDeviceService>().GetCacheList()?.Select(a => a.DeviceGroup)?.Where(a => a != null).Distinct()?.ToList();
        await MainLayout.StateHasChangedAsync();
    }

    async Task CopyDevice(IEnumerable<UploadDevice> data)
    {
        if (!data.Any())
        {
            await PopupService.EnqueueSnackbarAsync("需选择一项或多项", AlertTypes.Warning);
            return;
        }

        await App.GetService<UploadDeviceService>().CopyDevAsync(data);
        await DatatableQuery();
        await PopupService.EnqueueSnackbarAsync("复制成功", AlertTypes.Success);
        await MainLayout.StateHasChangedAsync();
    }

    private async Task DatatableQuery()
    {
        await _datatable?.QueryClickAsync();
    }

    private async Task DeleteCallAsync(IEnumerable<UploadDevice> input)
    {
        await App.GetService<UploadDeviceService>().DeleteAsync(input.Select(a => a.Id).ToArray());
        _deviceGroups = App.GetService<UploadDeviceService>().GetCacheList()?.Select(a => a.DeviceGroup)?.Where(a => a != null).Distinct()?.ToList();
        await MainLayout.StateHasChangedAsync();
    }

    Task<Dictionary<string, ImportPreviewOutputBase>> DeviceImportAsync(IBrowserFile file)
    {
        return App.GetService<UploadDeviceService>().PreviewAsync(file);
    }
    async Task DownExportAsync(UploadDevicePageInput input = null)
    {
        await AjaxService.DownFileAsync("gatewayFile/uploadDevice", DateTimeExtensions.CurrentDateTime.ToFileDateTimeFormat(), input.Adapt<CollectDeviceInput>());
    }
    private async Task DriverValueChangedAsync(UploadDeviceEditInput context, long pluginId)
    {
        if (pluginId <= 0)
            return;
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
    private async Task EditCallAsync(UploadDeviceEditInput input)
    {
        await App.GetService<UploadDeviceService>().EditAsync(input);
        _deviceGroups = App.GetService<UploadDeviceService>().GetCacheList()?.Select(a => a.DeviceGroup)?.Where(a => a != null).Distinct()?.ToList();
        await MainLayout.StateHasChangedAsync();
    }


    List<DependencyProperty> GetDriverProperties(long driverId, long devId)
    {
        return BackgroundServiceUtil.GetBackgroundService<UploadDeviceWorker>().GetDevicePropertys(driverId, devId);
    }

    private async Task<ISqlSugarPagedList<UploadDevice>> QueryCallAsync(UploadDevicePageInput input)
    {
        var data = await App.GetService<UploadDeviceService>().PageAsync(input);
        return data;
    }

    async Task SaveDeviceImportAsync(Dictionary<string, ImportPreviewOutputBase> data)
    {
        await App.GetService<UploadDeviceService>().ImportAsync(data);
        await DatatableQuery();
        ImportExcel.IsShowImport = false;
        await MainLayout.StateHasChangedAsync();
    }
}