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

using System.IO;
using System.Threading;

using ThingsGateway.Admin.Core;
using ThingsGateway.Application;

namespace ThingsGateway.Blazor;

/// <summary>
/// ManageGatewayPage
/// </summary>
public partial class ManageGatewayPage
{
    List<StringNumber> panel { get; set; } = new();

    readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromSeconds(5));
    ManageGatewayWorker ManageGatewayWorker { get; set; }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnInitialized()
    {
        ManageGatewayWorker = ServiceHelper.GetBackgroundService<ManageGatewayWorker>();
        panel.Add("2");
        _ = RunTimerAsync();
        base.OnInitialized();
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
    List<MqttClientStatus> CurClients { get; set; }
    List<MqttClientStatus> MqttClientStatuses { get; set; } = new();
    private async Task RefreshAsync()
    {
        MqttClientStatuses = await ManageGatewayWorker.GetClientGatewayAsync();
    }
    /// <inheritdoc/>
    public override void Dispose()
    {
        _periodicTimer?.Dispose();
        base.Dispose();
    }
    IJSObjectReference _helper;
    [Inject]
    IJSRuntime JS { get; set; }

    private bool isDownExport;

    private bool IsCollectDevicesFullUp;
    private bool IsUploadDevicesFullUp;
    private bool IsDeviceVariablesFullUp;
    private bool IsRestart;

    IBrowserFile _importCollectDevicesFile;
    IBrowserFile _importUploadDevicesFile;
    IBrowserFile _importDeviceVariablesFile;

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
                _helper ??= await JS.InvokeAsync<IJSObjectReference>("import", $"/_content/ThingsGateway.Admin.Blazor.Core/js/downloadFileFromStream.js");
                await _helper.InvokeVoidAsync("downloadFileFromStream", $"子网关{mqttClientStatus.Id}采集设备导出{SysDateTimeExtensions.CurrentDateTime.ToFileDateTimeFormat()}.xlsx", streamRef);
            }
            else
            {
                await PopupService.EnqueueSnackbarAsync("无采集设备", AlertTypes.None);

            }
            if (data.Content.UploadDevices.Count > 0)
            {
                using var devices = await App.GetService<UploadDeviceService>().ExportFileAsync(data.Content.UploadDevices);
                using var streamRef = new DotNetStreamReference(stream: devices);
                _helper ??= await JS.InvokeAsync<IJSObjectReference>("import", $"/_content/ThingsGateway.Admin.Blazor.Core/js/downloadFileFromStream.js");
                await _helper.InvokeVoidAsync("downloadFileFromStream", $"子网关{mqttClientStatus.Id}上传设备导出{SysDateTimeExtensions.CurrentDateTime.ToFileDateTimeFormat()}.xlsx", streamRef);

            }
            else
            {
                await PopupService.EnqueueSnackbarAsync("无上传设备", AlertTypes.None);

            }
            if (data.Content.DeviceVariables.Count > 0)
            {
                using var devices = await App.GetService<VariableService>().ExportFileAsync(data.Content.DeviceVariables);
                using var streamRef = new DotNetStreamReference(stream: devices);
                _helper ??= await JS.InvokeAsync<IJSObjectReference>("import", $"/_content/ThingsGateway.Admin.Blazor.Core/js/downloadFileFromStream.js");
                await _helper.InvokeVoidAsync("downloadFileFromStream", $"子网关{mqttClientStatus.Id}变量导出{SysDateTimeExtensions.CurrentDateTime.ToFileDateTimeFormat()}.xlsx", streamRef);

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

    /// <summary>
    /// 下发子网关配置
    /// </summary>
    /// <returns></returns>
    private async Task DBDown(MqttClientStatus mqttClientStatus)
    {
        MqttDBDownRpc rpc = new MqttDBDownRpc();
        rpc.IsCollectDevicesFullUp = IsCollectDevicesFullUp;
        rpc.IsDeviceVariablesFullUp = IsDeviceVariablesFullUp;
        rpc.IsUploadDevicesFullUp = IsUploadDevicesFullUp;
        rpc.IsRestart = IsRestart;

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
            if (data.Content.IsSuccess)
                await PopupService.EnqueueSnackbarAsync("下发成功", AlertTypes.Success);
            else
                await PopupService.EnqueueSnackbarAsync(data.Content.Message, AlertTypes.Error);

        }
        else
        {
            await PopupService.EnqueueSnackbarAsync(data.Message, AlertTypes.Error);
        }
    }




}