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

using Masa.Blazor;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

using ThingsGateway.Core;
using ThingsGateway.Foundation.Dmtp;
using ThingsGateway.Foundation.Dmtp.Rpc;

namespace ThingsGateway.UpgradeManger;

/// <summary>
/// UpgradeManger
/// </summary>
public partial class UpgradeMangerPage
{
    readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromSeconds(5));
    IBrowserFile _importCollectDevicesFile;

    IBrowserFile _importDeviceVariablesFile;
    IBrowserFile _importMemoryVariablesFile;

    IBrowserFile _importUploadDevicesFile;

    private bool IsCollectDevicesFullUp;

    private bool IsDeviceVariablesFullUp;
    private bool IsMemoryVariablesFullUp;

    private bool isUploadLoading;


    private bool IsUploadDevicesFullUp;

    private IJSObjectReference JSObjectReference;

    [Inject]
    IJSRuntime JSRuntime { get; set; }

    List<TcpDmtpSocketClient> TcpDmtpSocketClients { get; set; }

    [Inject]
    UpgradeManger UpgradeManger { get; set; }
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
        _ = RunTimerAsync();
        base.OnInitialized();
    }
    bool disabled => TcpDmtpSocketClient?.CanSend != true;
    GatewayInfo GatewayInfo;
    TcpDmtpSocketClient TcpDmtpSocketClient;
    GatewayExcel GatewayExcel;


    async Task ExcelUpload()
    {
        try
        {
            isUploadLoading = true;

            GatewayExcel = await TcpDmtpSocketClient.GetDmtpRpcActor().InvokeTAsync<GatewayExcel>("GetGatewayExcelAsync", InvokeOption.WaitInvoke);

            if (GatewayExcel.CollectDevice?.Length > 0)
            {
                try
                {
                    GatewayExcel.CollectDevice.Seek(0, SeekOrigin.Begin);
                    using var streamRef = new DotNetStreamReference(stream: GatewayExcel.CollectDevice);
                    JSObjectReference ??= await JSRuntime.LoadModuleAsync("js/downloadFileFromStream");
                    await JSObjectReference.InvokeVoidAsync("downloadFileFromStream", $"{TcpDmtpSocketClient.GetIPPort()}采集设备表导出{DateTimeExtensions.CurrentDateTime.ToString("yyyy-MM-dd HH-mm-ss-fff zz")}.xlsx", streamRef);

                }
                catch (Exception ex)
                {
                    UpgradeManger.LogMessage.LogWarning(ex, "采集设备表导出失败");
                }

            }
            if (GatewayExcel.UploadDevice?.Length > 0)
            {
                try
                {
                    GatewayExcel.UploadDevice.Seek(0, SeekOrigin.Begin);
                    using var streamRef = new DotNetStreamReference(stream: GatewayExcel.UploadDevice);
                    JSObjectReference ??= await JSRuntime.LoadModuleAsync("js/downloadFileFromStream");
                    await JSObjectReference.InvokeVoidAsync("downloadFileFromStream", $"{TcpDmtpSocketClient.GetIPPort()}上传设备表导出{DateTimeExtensions.CurrentDateTime.ToString("yyyy-MM-dd HH-mm-ss-fff zz")}.xlsx", streamRef);

                }
                catch (Exception ex)
                {
                    UpgradeManger.LogMessage.LogWarning(ex, "上传设备表导出失败");
                }

            }
            if (GatewayExcel.MemoryVariable?.Length > 0)
            {
                try
                {
                    GatewayExcel.MemoryVariable.Seek(0, SeekOrigin.Begin);
                    using var streamRef = new DotNetStreamReference(stream: GatewayExcel.MemoryVariable);
                    JSObjectReference ??= await JSRuntime.LoadModuleAsync("js/downloadFileFromStream");
                    await JSObjectReference.InvokeVoidAsync("downloadFileFromStream", $"{TcpDmtpSocketClient.GetIPPort()}内存变量表导出{DateTimeExtensions.CurrentDateTime.ToString("yyyy-MM-dd HH-mm-ss-fff zz")}.xlsx", streamRef);

                }
                catch (Exception ex)
                {
                    UpgradeManger.LogMessage.LogWarning(ex, "内存变量表导出失败");
                }

            }
            if (GatewayExcel.DeviceVariable?.Length > 0)
            {
                try
                {
                    GatewayExcel.DeviceVariable.Seek(0, SeekOrigin.Begin);
                    using var streamRef = new DotNetStreamReference(stream: GatewayExcel.DeviceVariable);
                    JSObjectReference ??= await JSRuntime.LoadModuleAsync("js/downloadFileFromStream");
                    await JSObjectReference.InvokeVoidAsync("downloadFileFromStream", $"{TcpDmtpSocketClient.GetIPPort()}采集变量表导出{DateTimeExtensions.CurrentDateTime.ToString("yyyy-MM-dd HH-mm-ss-fff zz")}.xlsx", streamRef);

                }
                catch (Exception ex)
                {
                    UpgradeManger.LogMessage.LogWarning(ex, "采集变量表导出失败");
                }

            }


            await PopupService.EnqueueSnackbarAsync("上传成功", AlertTypes.Success);
        }
        finally { isUploadLoading = false; }

    }

    async Task DBUpload()
    {
        try
        {
            isUploadLoading = true;
            await UpgradeManger.DBUpload(TcpDmtpSocketClient);

        }
        finally { isUploadLoading = false; }
    }

    /// <summary>
    /// 下发子网关配置
    /// </summary>
    /// <returns></returns>
    private async Task ExcelDown()
    {
        try
        {
            isUploadLoading = true;
            GatewayExcel gatewayExcel = new();
            if (_importCollectDevicesFile != null)
            {
                using var fs1 = new MemoryStream();
                using var stream1 = _importCollectDevicesFile.OpenReadStream(512000000);
                await stream1.CopyToAsync(fs1);
                fs1.Seek(0, SeekOrigin.Begin);
                gatewayExcel.CollectDevice = fs1;
            }
            if (_importUploadDevicesFile != null)
            {

                using var fs2 = new MemoryStream();
                using var stream2 = _importUploadDevicesFile.OpenReadStream(512000000);
                await stream2.CopyToAsync(fs2);
                fs2.Seek(0, SeekOrigin.Begin);
                gatewayExcel.UploadDevice = fs2;
            }
            if (_importDeviceVariablesFile != null)
            {
                using var fs3 = new MemoryStream();
                using var stream3 = _importDeviceVariablesFile.OpenReadStream(512000000);
                await stream3.CopyToAsync(fs3);
                fs3.Seek(0, SeekOrigin.Begin);
                gatewayExcel.DeviceVariable = fs3;
            }
            if (_importMemoryVariablesFile != null)
            {
                using var fs4 = new MemoryStream();
                using var stream4 = _importMemoryVariablesFile.OpenReadStream(512000000);
                await stream4.CopyToAsync(fs4);
                fs4.Seek(0, SeekOrigin.Begin);
                gatewayExcel.MemoryVariable = fs4;
            }

            var result = await TcpDmtpSocketClient.GetDmtpRpcActor().InvokeTAsync<OperResult>("SetGatewayExcelAsync", new InvokeOption(30000), gatewayExcel);
            if (result.IsSuccess)
            {
                await PopupService.EnqueueSnackbarAsync("更新成功", AlertTypes.Success);
            }
            else
            {
                await PopupService.EnqueueSnackbarAsync(result.Message, AlertTypes.Error);
            }
        }
        catch (Exception ex)
        {

            await PopupService.EnqueueSnackbarAsync(ex.Message, AlertTypes.Error);
        }
        finally
        {
            isUploadLoading = false;
        }

    }
    private async Task FileDown()
    {
        try
        {
            isUploadLoading = true;
            var data = Program.app.MainWindow.ShowOpenFolder("选择更新文件夹", AppContext.BaseDirectory);
            if (data.Length > 0)
            {
                await Task.Run(async () =>
                {
                    await UpgradeManger.FileDown(TcpDmtpSocketClient, data.FirstOrDefault());
                }
                );
                await PopupService.EnqueueSnackbarAsync("推送成功", AlertTypes.Success);
            }

        }
        finally { isUploadLoading = false; }

    }

    private async Task FileRestart()
    {
        try
        {
            isUploadLoading = true;
            var confirm = await PopupService.ConfirmAsync("重启", "网关重启，会暂时断开连接，会在约1分钟后重新连接 ?", AlertTypes.Warning);
            if (confirm)
            {
                await TcpDmtpSocketClient.GetDmtpRpcActor().InvokeTAsync<OperResult>("FileRestart", InvokeOption.WaitSend);
            }
        }
        finally
        {
            isUploadLoading = false;
        }

    }



    /// <summary>
    /// DBRestart
    /// </summary>
    /// <returns></returns>
    private async Task DBRestart()
    {
        try
        {
            isUploadLoading = true;
            await TcpDmtpSocketClient.GetDmtpRpcActor().InvokeTAsync<OperResult>("DBRestartAsync", new InvokeOption(30000));
            await PopupService.EnqueueSnackbarAsync("重启成功", AlertTypes.Success);
        }
        finally
        {
            isUploadLoading = false;
        }
    }


    async Task ChoiceTcpDmtpSocketClient(TcpDmtpSocketClient item)
    {
        TcpDmtpSocketClient = item;
        GatewayInfo = await item.GetDmtpRpcActor().InvokeTAsync<GatewayInfo>("GetGatewayInfo", InvokeOption.WaitInvoke);
    }

    private async Task RefreshAsync()
    {
        await Task.CompletedTask;
        TcpDmtpSocketClients = UpgradeManger.TcpDmtpService.GetClients().ToList();

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