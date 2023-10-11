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

using Masa.Blazor;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

using MQTTnet.Server;

namespace ThingsGateway.Gateway.Blazor;

/// <summary>
/// ManageGatewayPage
/// </summary>
public partial class ManageGatewayPage
{
    readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromSeconds(3));
    IBrowserFile _importCollectDevicesFile;
    IBrowserFile _importDeviceVariablesFile;
    IBrowserFile _importUploadDevicesFile;
    private bool IsCollectDevicesFullUp;
    private bool IsDeviceVariablesFullUp;
    private bool isDownExport;
    private bool IsRestart;
    StringNumber tab;
    private bool IsUploadDevicesFullUp;
    private IJSObjectReference JSObjectReference;
    /// <inheritdoc/>
    [Inject]
    protected IJSRuntime JSRuntime { get; set; }

    List<MqttClientStatus> CurClients { get; set; }
    ManageGatewayWorker ManageGatewayWorker { get; set; }
    List<MqttClientStatus> MqttClientStatuses { get; set; } = new();
    List<StringNumber> Panel { get; set; } = new();
    /// <inheritdoc/>
    public override void Dispose()
    {
        _periodicTimer?.Dispose();
        base.Dispose();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnInitialized()
    {
        ManageGatewayWorker = BackgroundServiceUtil.GetBackgroundService<ManageGatewayWorker>();
        Panel.Add("2");
        _ = RunTimerAsync();
        base.OnInitialized();
    }
    /// <summary>
    /// 下发子网关配置
    /// </summary>
    /// <returns></returns>
    private async Task DBDown(MqttClientStatus mqttClientStatus)
    {
        MqttDBDownRpc rpc = new()
        {
            IsCollectDevicesFullUp = IsCollectDevicesFullUp,
            IsDeviceVariablesFullUp = IsDeviceVariablesFullUp,
            IsUploadDevicesFullUp = IsUploadDevicesFullUp,
            IsRestart = IsRestart
        };

        if (_importCollectDevicesFile != null)
        {
            using var fs1 = new MemoryStream();
            using var stream1 = _importCollectDevicesFile.OpenReadStream(512000000);
            await stream1.CopyToAsync(fs1);
            rpc.CollectDevices = fs1.ToArray();
        }
        if (_importUploadDevicesFile != null)
        {

            using var fs2 = new MemoryStream();
            using var stream2 = _importUploadDevicesFile.OpenReadStream(512000000);
            await stream2.CopyToAsync(fs2);
            rpc.UploadDevices = fs2.ToArray();
        }
        if (_importDeviceVariablesFile != null)
        {
            using var fs3 = new MemoryStream();
            using var stream3 = _importDeviceVariablesFile.OpenReadStream(512000000);
            await stream3.CopyToAsync(fs3);
            rpc.DeviceVariables = fs3.ToArray();
        }




        var data = await ManageGatewayWorker.SetClientGatewayDBAsync(mqttClientStatus.Id, rpc);
        if (data.IsSuccess)
        {
            await PopupService.EnqueueSnackbarAsync("下发成功", AlertTypes.Success);
        }
        else
        {
            await PopupService.EnqueueSnackbarAsync(data.Message, AlertTypes.Error);
        }
    }

    /// <summary>
    /// 获取子网关配置，导出excel
    /// </summary>
    /// <param name="mqttClientStatus"></param>
    /// <returns></returns>
    private async Task DBUpload(MqttClientStatus mqttClientStatus)
    {

        var data = await ManageGatewayWorker.GetClientGatewayDBAsync(mqttClientStatus.Id);
        if (data.IsSuccess)
        {
            isDownExport = true;
            await InvokeAsync(StateHasChanged);
            if (data.Content.CollectDevices.Count > 0)
            {
                using var devices = await App.GetService<CollectDeviceService>().ExportFileAsync(data.Content.CollectDevices);
                using var streamRef = new DotNetStreamReference(stream: devices);
                JSObjectReference ??= await JSRuntime.LoadModuleAsync("js/downloadFileFromStream");
                await JSObjectReference.InvokeVoidAsync("downloadFileFromStream", $"子网关{mqttClientStatus.Id}采集设备导出{DateTimeExtensions.CurrentDateTime.ToFileDateTimeFormat()}.xlsx", streamRef);

            }
            else
            {
                await PopupService.EnqueueSnackbarAsync("无采集设备", AlertTypes.None);

            }
            if (data.Content.UploadDevices.Count > 0)
            {
                using var devices = await App.GetService<UploadDeviceService>().ExportFileAsync(data.Content.UploadDevices);
                using var streamRef = new DotNetStreamReference(stream: devices);
                JSObjectReference ??= await JSRuntime.LoadModuleAsync("js/downloadFileFromStream");
                await JSObjectReference.InvokeVoidAsync("downloadFileFromStream", $"子网关{mqttClientStatus.Id}上传设备导出{DateTimeExtensions.CurrentDateTime.ToFileDateTimeFormat()}.xlsx", streamRef);

            }
            else
            {
                await PopupService.EnqueueSnackbarAsync("无上传设备", AlertTypes.None);

            }
            if (data.Content.DeviceVariables.Count > 0)
            {
                using var devices = await App.GetService<VariableService>().ExportFileAsync(data.Content.DeviceVariables);
                using var streamRef = new DotNetStreamReference(stream: devices);
                JSObjectReference ??= await JSRuntime.LoadModuleAsync("js/downloadFileFromStream");
                await JSObjectReference.InvokeVoidAsync("downloadFileFromStream", $"子网关{mqttClientStatus.Id}变量导出{DateTimeExtensions.CurrentDateTime.ToFileDateTimeFormat()}.xlsx", streamRef);

            }
            else
            {
                await PopupService.EnqueueSnackbarAsync("无采集变量", AlertTypes.None);

            }
            await PopupService.EnqueueSnackbarAsync("上传成功", AlertTypes.Success);

        }
        else
        {
            await PopupService.EnqueueSnackbarAsync(data.Message, AlertTypes.Error);
        }
        isDownExport = false;
    }

    private async Task RefreshAsync()
    {
        MqttClientStatuses = await ManageGatewayWorker.GetClientGatewayAsync();
    }

    private async Task RunTimerAsync()
    {
        await RefreshAsync();
        while (await _periodicTimer.WaitForNextTickAsync())
        {
            try
            {
                await RefreshAsync();
                await InvokeAsync(StateHasChanged);
            }
            catch
            {
            }

        }
    }
}