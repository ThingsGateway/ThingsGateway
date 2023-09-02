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

namespace ThingsGateway.Blazor;

/// <summary>
/// 内存变量页面
/// </summary>
public partial class MemoryVariablePage
{
    private IAppDataTable _datatable;

    long choiceUploadDeviceId;

    ImportExcel ImportExcel;

    private readonly MemoryVariablePageInput search = new();

    StringNumber tab;

    List<UploadDevice> UploadDevices = new();

    [Inject]
    AjaxService AjaxService { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    protected override async Task OnParametersSetAsync()
    {
        UploadDevices = App.GetService<IUploadDeviceService>().GetCacheList();
        await base.OnParametersSetAsync();
    }

    private void FilterHeaders(List<DataTableHeader<DeviceVariable>> datas)
    {
        datas.RemoveWhere(it => it.Value == nameof(DeviceVariable.DeviceId));
        datas.RemoveWhere(it => it.Value == nameof(DeviceVariable.VariableAddress));
        datas.RemoveWhere(it => it.Value == nameof(DeviceVariable.OtherMethod));
    }


    private async Task AddCallAsync(MemoryVariableAddInput input)
    {
        await App.GetService<VariableService>().AddAsync(input);
    }

    private async Task ClearAsync()
    {
        var confirm = await PopupService.OpenConfirmDialogAsync("确认", "清空?");
        if (confirm)
        {
            await App.GetService<VariableService>().ClearMemoryVariableAsync();
        }
        await DatatableQueryAsync();

    }

    private async Task DatatableQueryAsync()
    {
        await _datatable.QueryClickAsync();
    }

    private async Task DeleteCallAsync(IEnumerable<DeviceVariable> input)
    {
        await App.GetService<VariableService>().DeleteAsync(input.Select(a => a.Id).ToArray());
    }

    Task<Dictionary<string, ImportPreviewOutputBase>> DeviceImportAsync(IBrowserFile file)
    {
        return App.GetService<VariableService>().MemoryVariablePreviewAsync(file);
    }



    async Task DownExportAsync(MemoryVariablePageInput input = null)
    {
        await AjaxService.DownFileAsync("gatewayFile/memoryVariable", SysDateTimeExtensions.CurrentDateTime.ToFileDateTimeFormat(), input.Adapt<MemoryVariableInput>());
    }

    private async Task EditCallAsync(MemoryVariableAddInput input)
    {
        await App.GetService<VariableService>().EditAsync(input);
    }


    List<DependencyProperty> GetDriverProperties(long driverId, List<DependencyProperty> dependencyProperties)
    {
        return ServiceHelper.GetBackgroundService<UploadDeviceWorker>().GetVariablePropertys(driverId, dependencyProperties);
    }

    private async Task<SqlSugarPagedList<DeviceVariable>> QueryCallAsync(MemoryVariablePageInput input)
    {
        var data = await App.GetService<VariableService>().PageAsync(input);
        return data;
    }
    async Task SaveDeviceImportAsync(Dictionary<string, ImportPreviewOutputBase> data)
    {
        await App.GetService<VariableService>().ImportAsync(data);
        await DatatableQueryAsync();
        ImportExcel.IsShowImport = false;
    }
}