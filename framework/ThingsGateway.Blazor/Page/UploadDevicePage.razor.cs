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
    [Inject]
    IDriverPluginService DriverPluginService { get; set; }

    [Inject]
    InitTimezone InitTimezone { get; set; }

    [CascadingParameter]
    MainLayout MainLayout { get; set; }
    [Inject]
    IUploadDeviceService UploadDeviceService { get; set; }

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
        await UploadDeviceService.AddAsync(input);
        _deviceGroups = UploadDeviceService.GetCacheList()?.Select(a => a.DeviceGroup)?.Where(a => a != null).Distinct()?.ToList();
        await MainLayout.StateHasChangedAsync();
    }

    async Task CopyDevice(IEnumerable<UploadDevice> data)
    {
        if (!data.Any())
        {
            await PopupService.EnqueueSnackbarAsync("需选择一项或多项", AlertTypes.Warning);
            return;
        }

        await UploadDeviceService.CopyDevAsync(data);
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
        await UploadDeviceService.DeleteAsync(input.Select(a => a.Id).ToArray());
        _deviceGroups = UploadDeviceService.GetCacheList()?.Select(a => a.DeviceGroup)?.Where(a => a != null).Distinct()?.ToList();
        await MainLayout.StateHasChangedAsync();
    }

    Task<Dictionary<string, ImportPreviewOutputBase>> DeviceImportAsync(IBrowserFile file)
    {
        return UploadDeviceService.PreviewAsync(file);
    }
    async Task DownExportAsync(UploadDevicePageInput input = null)
    {
        await AjaxService.DownFileAsync("gatewayFile/uploadDevice", SysDateTimeExtensions.CurrentDateTime.ToFileDateTimeFormat(), input.Adapt<CollectDeviceInput>());
    }
    private async Task DriverValueChangedAsync(UploadDeviceEditInput context, long pluginId)
    {
        if (pluginId <= 0)
            return;
        if (context.DevicePropertys == null || context.DevicePropertys?.Count == 0)
        {
            try
            {
                context.PluginId = pluginId;
                context.DevicePropertys = GetDriverProperties(pluginId, context.Id);
                await PopupService.EnqueueSnackbarAsync("插件附加属性已更新", AlertTypes.Success);

            }
            catch (Exception ex)
            {
                await PopupService.EnqueueSnackbarAsync(ex.Message, AlertTypes.Error);
            }
        }

    }
    private async Task EditCallAsync(UploadDeviceEditInput input)
    {
        await UploadDeviceService.EditAsync(input);
        _deviceGroups = UploadDeviceService.GetCacheList()?.Select(a => a.DeviceGroup)?.Where(a => a != null).Distinct()?.ToList();
        await MainLayout.StateHasChangedAsync();
    }


    List<DependencyProperty> GetDriverProperties(long driverId, long devId)
    {
        return ServiceHelper.GetBackgroundService<UploadDeviceWorker>().GetDevicePropertys(driverId, devId);
    }

    private async Task<SqlSugarPagedList<UploadDevice>> QueryCallAsync(UploadDevicePageInput input)
    {
        var data = await UploadDeviceService.PageAsync(input);
        return data;
    }

    async Task SaveDeviceImportAsync(Dictionary<string, ImportPreviewOutputBase> data)
    {
        await UploadDeviceService.ImportAsync(data);
        await DatatableQuery();
        ImportExcel.IsShowImport = false;
        await MainLayout.StateHasChangedAsync();
    }
}