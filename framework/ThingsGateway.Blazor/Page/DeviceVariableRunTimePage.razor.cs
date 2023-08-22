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

using SqlSugar;

using System.Threading;

using ThingsGateway.Admin.Blazor.Core;
using ThingsGateway.Admin.Core;
using ThingsGateway.Application;

namespace ThingsGateway.Blazor;
/// <summary>
/// 实时数据页
/// </summary>
public partial class DeviceVariableRunTimePage
{
    readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromSeconds(1));
    private IAppDataTable _datatable;
    List<DeviceTree> _deviceGroups = new();
    string _searchName;
    /// <summary>
    /// 设备名称
    /// </summary>
    [Parameter]
    [SupplyParameterFromQuery]
    public string DeviceName { get; set; }
    /// <summary>
    /// 上传设备名称
    /// </summary>
    [Parameter]
    [SupplyParameterFromQuery]
    public string UploadDeviceName { get; set; }
    [Inject]
    GlobalDeviceData GlobalDeviceData { get; set; }

    VariablePageInput SearchModel { get; set; } = new();
    [Inject]
    IUploadDeviceService UploadDeviceService { get; set; }

    CollectDeviceWorker CollectDeviceHostService { get; set; }
    [Inject]
    RpcSingletonService RpcCore { get; set; }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override void Dispose()
    {
        _periodicTimer?.Dispose();
        base.Dispose();
    }
    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        _ = RunTimerAsync();
        base.OnInitialized();
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    protected override async Task OnParametersSetAsync()
    {
        if (SearchModel.DeviceName != DeviceName && !DeviceName.IsNullOrEmpty())
        {
            SearchModel.DeviceName = DeviceName;
            await DatatableQuery();
        }
        if (SearchModel.UploadDeviceName != UploadDeviceName && !UploadDeviceName.IsNullOrEmpty())
        {
            SearchModel.UploadDeviceName = UploadDeviceName;
            await DatatableQuery();
        }

        CollectDeviceHostService = ServiceHelper.GetBackgroundService<CollectDeviceWorker>();
        _deviceGroups = GlobalDeviceData.CollectDevices.Adapt<List<CollectDevice>>().GetTree();
        await base.OnParametersSetAsync();
    }

    private async Task DatatableQuery()
    {
        if (_datatable != null)
            await _datatable?.QueryClickAsync();
    }

    private void FilterHeaders(List<DataTableHeader<DeviceVariableRunTime>> datas)
    {
        datas.RemoveWhere(it => it.Value == nameof(DeviceVariableRunTime.DeviceId));
    }
    private void Filters(List<Filters> datas)
    {
        foreach (var item in datas)
        {
            switch (item.Key)
            {
                case nameof(DeviceVariableRunTime.WriteExpressions):
                case nameof(DeviceVariableRunTime.ReadExpressions):
                    item.Value = false;
                    break;
            }
        }
    }


    private Task<SqlSugarPagedList<DeviceVariableRunTime>> QueryCallAsync(VariablePageInput input)
    {
        var uploadDevId = UploadDeviceService.GetIdByName(input.UploadDeviceName);
        var data = GlobalDeviceData.AllVariables
            .WhereIF(!input.DeviceName.IsNullOrEmpty(), a => a.DeviceName == input.DeviceName)
            .WhereIF(!input.Name.IsNullOrEmpty(), a => a.Name.Contains(input.Name))
            .WhereIF(!input.VariableAddress.IsNullOrEmpty(), a => a.VariableAddress?.Contains(input.VariableAddress) == true)
            .WhereIF(!input.UploadDeviceName.IsNullOrEmpty(), a => a.VariablePropertys.ContainsKey(uploadDevId ?? 0))
            .ToList().ToPagedList(input);
        return Task.FromResult(data);
    }

    private async Task RunTimerAsync()
    {
        while (await _periodicTimer.WaitForNextTickAsync())
        {
            try
            {
                await InvokeAsync(StateHasChanged);
            }
            catch
            {
            }

        }
    }

    private EventCallback<string> WriteValueAsync;
    private async Task OnWriteValueAsync(DeviceVariableRunTime tag, string value)
    {
        var data = await RpcCore?.InvokeDeviceMethodAsync($"BLAZOR-{UserResoures.CurrentUser.Account}-{App.HttpContext.Connection.RemoteIpAddress.MapToIPv4()}",

            new Dictionary<string, string>()
            {

                { tag.Name, value}
            }

            , true);
        if (data.Count > 0 && !data.FirstOrDefault().Value.IsSuccess)
        {
            throw new(data.FirstOrDefault().Value.Message);
        }
    }
    private async Task WriteAsync(DeviceVariableRunTime collectVariableRunTime)
    {
        // 将异步方法添加到事件回调上
        WriteValueAsync = EventCallback.Factory.Create<string>(this, value => OnWriteValueAsync(collectVariableRunTime, value));
        await PopupService.OpenAsync(typeof(WriteValue), new Dictionary<string, object>()
        {
            { nameof(WriteValue.OnSaveAsync), WriteValueAsync }
        });

    }
}