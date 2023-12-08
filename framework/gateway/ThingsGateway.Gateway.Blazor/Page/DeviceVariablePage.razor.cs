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

using Mapster;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace ThingsGateway.Gateway.Blazor;

/// <summary>
/// 采集变量页面
/// </summary>
public partial class DeviceVariablePage
{
    private readonly DeviceVariablePageInput _search = new();
    private long _choiceUploadDeviceId;
    private List<CollectDevice> _collectDevices = new();
    private IAppDataTable _datatable;

    private List<DeviceTree> _deviceGroups = new();

    private ImportExcel _importExcel;
    private Dictionary<long, List<string>> _otherMethods = new();
    //string _searchName;

    private List<Device> _uploadDevices = new();

    [Inject]
    private AjaxService _ajaxService { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    protected override async Task OnParametersSetAsync()
    {
        _collectDevices = _serviceScope.ServiceProvider.GetService<ICollectDeviceService>().GetCacheList(true);
        _uploadDevices = _serviceScope.ServiceProvider.GetService<IUploadDeviceService>().GetCacheList(true);
        _deviceGroups = _serviceScope.ServiceProvider.GetService<ICollectDeviceService>().GetTree();
        await base.OnParametersSetAsync();
    }

    private async Task AddCallAsync(DeviceVariableAddInput input)
    {
        await _serviceScope.ServiceProvider.GetService<VariableService>().AddAsync(input);
    }

    private async Task ClearAsync()
    {
        var confirm = await PopupService.OpenConfirmDialogAsync("确认", "清空?");
        if (confirm)
        {
            await _serviceScope.ServiceProvider.GetService<VariableService>().ClearDeviceVariableAsync();
        }
        await DatatableQueryAsync();
    }

    private async Task DatatableQueryAsync()
    {
        await _datatable.QueryClickAsync();
    }

    private async Task DeleteCallAsync(IEnumerable<DeviceVariable> input)
    {
        await _serviceScope.ServiceProvider.GetService<VariableService>().DeleteAsync(input.Select(a => a.Id).ToArray());
    }

    private void DeviceChanged(long devId)
    {
        if (devId > 0)
        {
            var data = BackgroundServiceUtil.GetBackgroundService<CollectDeviceWorker>().GetDeviceMethods(devId);
            _otherMethods.AddOrUpdate(devId, data);
        }
        else
            _otherMethods = new();
    }

    private Task<Dictionary<string, ImportPreviewOutputBase>> DeviceImportAsync(IBrowserFile file)
    {
        return _serviceScope.ServiceProvider.GetService<VariableService>().PreviewAsync(file);
    }

    private async Task DownExportAsync(DeviceVariablePageInput input = null)
    {
        await _ajaxService.DownFileAsync("gatewayFile/deviceVariable", DateTimeExtensions.CurrentDateTime.ToFileDateTimeFormat(), input.Adapt<DeviceVariableInput>());
    }

    private async Task EditCallAsync(VariableEditInput input)
    {
        await _serviceScope.ServiceProvider.GetService<VariableService>().EditAsync(input);
    }

    private List<DependencyProperty> GetDriverProperties(string pluginName, List<DependencyProperty> dependencyProperties)
    {
        return BackgroundServiceUtil.GetBackgroundService<UploadDeviceWorker>().GetVariablePropertys(pluginName, dependencyProperties);
    }

    private async Task<ISqlSugarPagedList<DeviceVariable>> QueryCallAsync(DeviceVariablePageInput input)
    {
        var data = await _serviceScope.ServiceProvider.GetService<VariableService>().PageAsync(input);
        return data;
    }

    private async Task SaveDeviceImportAsync(Dictionary<string, ImportPreviewOutputBase> data)
    {
        await _serviceScope.ServiceProvider.GetService<VariableService>().ImportAsync(data);
        await DatatableQueryAsync();
        _importExcel.IsShowImport = false;
    }
}