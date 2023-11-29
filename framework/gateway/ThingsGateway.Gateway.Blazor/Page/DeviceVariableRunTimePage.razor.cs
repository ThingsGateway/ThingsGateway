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

using SqlSugar;

using ThingsGateway.Admin.Core;

namespace ThingsGateway.Gateway.Blazor;
/// <summary>
/// 实时数据页
/// </summary>
public partial class DeviceVariableRunTimePage
{
    private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromSeconds(1));
    private IAppDataTable _datatable;
    private List<DeviceTree> _deviceGroups = new();
    private EventCallback<string> _onWrite;
    //private string _searchName;
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
    private CollectDeviceWorker _collectDeviceWorker { get; set; }

    [Inject]
    private GlobalDeviceData _globalDeviceData { get; set; }

    [Inject]
    RpcSingletonService _rpcSingletonService { get; set; }
    private VariablePageInput _search { get; set; } = new();
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
        if (_search.DeviceName != DeviceName && !DeviceName.IsNullOrEmpty())
        {
            _search.DeviceName = DeviceName;
            await DatatableQuery();
        }
        if (_search.UploadDeviceName != UploadDeviceName && !UploadDeviceName.IsNullOrEmpty())
        {
            _search.UploadDeviceName = UploadDeviceName;
            await DatatableQuery();
        }

        _collectDeviceWorker = BackgroundServiceUtil.GetBackgroundService<CollectDeviceWorker>();
        _deviceGroups = _globalDeviceData.CollectDevices.Adapt<List<CollectDevice>>().GetTree();
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


    private async Task OnWriteValueAsync(DeviceVariableRunTime tag, string value)
    {
        var data = await _rpcSingletonService?.InvokeDeviceMethodAsync($"BLAZOR-{UserResoures.CurrentUser.Account}-{App.HttpContext.Connection.RemoteIpAddress.MapToIPv4()}",

            new Dictionary<string, string>()
            {

                { tag.Name, value}
            }

            , true);
        if (data.Count > 0 && !data.FirstOrDefault().Value.IsSuccess)
        {
            throw new(data.FirstOrDefault().Value.Message);
        }
        else
        {
            await PopupService.EnqueueSnackbarAsync(data.ToJsonString(true), AlertTypes.Info);

        }
    }

    private async Task<ISqlSugarPagedList<DeviceVariableRunTime>> QueryCallAsync(VariablePageInput input)
    {
        var uploadDevId = _serviceScope.ServiceProvider.GetService<IUploadDeviceService>().GetIdByName(input.UploadDeviceName);
        var data = _globalDeviceData.AllVariables
            .WhereIF(!input.DeviceName.IsNullOrEmpty(), a => a.DeviceName == input.DeviceName)
            .WhereIF(!input.Name.IsNullOrEmpty(), a => a.Name.Contains(input.Name))
            .WhereIF(!input.Address.IsNullOrEmpty(), a => a.Address?.Contains(input.Address) == true)
            .WhereIF(!input.UploadDeviceName.IsNullOrEmpty(), a => a.VariablePropertys.ContainsKey(uploadDevId ?? 0))
            .ToList().ToPagedList(input);
        await Task.CompletedTask;
        return data;
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
    private async Task WriteAsync(DeviceVariableRunTime collectVariableRunTime)
    {
        // 将异步方法添加到事件回调上
        _onWrite = EventCallback.Factory.Create<string>(this, value => OnWriteValueAsync(collectVariableRunTime, value));
        await PopupService.OpenAsync(typeof(WriteValue), new Dictionary<string, object>()
        {
            { nameof(WriteValue.OnSaveAsync), _onWrite }
        });

    }
}