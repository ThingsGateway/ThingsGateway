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

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

using ThingsGateway.Admin.Blazor.Core;
using ThingsGateway.Admin.Core;
using ThingsGateway.Application;

using TouchSocket.Core;

namespace ThingsGateway.Blazor;
/// <summary>
/// 采集变量页面
/// </summary>
public partial class DeviceVariablePage
{
    private readonly DeviceVariablePageInput search = new();
    private IAppDataTable _datatable;


    List<DeviceTree> _deviceGroups = new();

    string _searchName;

    long choiceUploadDeviceId;

    List<CollectDevice> CollectDevices = new();

    ImportExcel ImportExcel;

    Dictionary<long, List<string>> OtherMethods = new();
    StringNumber tab;

    List<UploadDevice> UploadDevices = new();

    [Inject]
    AjaxService AjaxService { get; set; }

    [Inject]
    IVariableService VariableService { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    protected override async Task OnParametersSetAsync()
    {
        CollectDevices = App.GetService<ICollectDeviceService>().GetCacheList();
        UploadDevices = App.GetService<IUploadDeviceService>().GetCacheList();
        _deviceGroups = App.GetService<ICollectDeviceService>().GetTree();

        await base.OnParametersSetAsync();
    }
    private async Task AddCallAsync(DeviceVariableAddInput input)
    {
        await VariableService.AddAsync(input);
    }

    private async Task ClearAsync()
    {
        var confirm = await PopupService.OpenConfirmDialogAsync("确认", "清空?");
        if (confirm)
        {
            await VariableService.ClearDeviceVariableAsync();
        }
        await DatatableQueryAsync();

    }

    private async Task DatatableQueryAsync()
    {
        await _datatable.QueryClickAsync();
    }

    private async Task DeleteCallAsync(IEnumerable<DeviceVariable> input)
    {
        await VariableService.DeleteAsync(input.Select(a => a.Id).ToArray());
    }

    void DeviceChanged(long devId)
    {
        if (devId > 0)
        {
            var data = ServiceHelper.GetBackgroundService<CollectDeviceWorker>().GetDeviceMethods(devId);
            OtherMethods.AddOrUpdate(devId, data);
        }
        else
            OtherMethods = new();
    }

    Task<Dictionary<string, ImportPreviewOutputBase>> DeviceImportAsync(IBrowserFile file)
    {
        return VariableService.PreviewAsync(file);
    }
    async Task DownExportAsync(DeviceVariablePageInput input = null)
    {
        await AjaxService.DownFileAsync("gatewayFile/deviceVariable", SysDateTimeExtensions.CurrentDateTime.ToFileDateTimeFormat(), input.Adapt<DeviceVariableInput>());
    }

    private async Task EditCallAsync(VariableEditInput input)
    {
        await VariableService.EditAsync(input);
    }

    List<DependencyProperty> GetDriverProperties(long driverId, List<DependencyProperty> dependencyProperties)
    {
        return ServiceHelper.GetBackgroundService<UploadDeviceWorker>().GetVariablePropertys(driverId, dependencyProperties);
    }

    private async Task<SqlSugarPagedList<DeviceVariable>> QueryCallAsync(DeviceVariablePageInput input)
    {
        var data = await VariableService.PageAsync(input);
        return data;
    }

    async Task SaveDeviceImportAsync(Dictionary<string, ImportPreviewOutputBase> data)
    {
        await VariableService.ImportAsync(data);
        await DatatableQueryAsync();
        ImportExcel.IsShowImport = false;
    }
}