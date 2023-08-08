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

using Mapster;

using Masa.Blazor;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using SqlSugar;

using System.IO;
using System.Threading;

using ThingsGateway.Admin.Blazor;
using ThingsGateway.Admin.Blazor.Core;
using ThingsGateway.Admin.Core;
using ThingsGateway.Application;

namespace ThingsGateway.Blazor;

/// <summary>
/// 设备状态页面
/// </summary>
public partial class DeviceStatusPage : IDisposable
{
    readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromSeconds(1));
    List<CollectDeviceCore> _collectDeviceCores = new();
    private string _collectDeviceGroup;
    List<string> _collectDeviceGroups = new();
    string _collectDeviceGroupSearchName;
    List<UploadDeviceCore> _uploadDeviceCores = new();
    private string _uploadDeviceGroup;
    List<string> _uploadDeviceGroups = new();
    string _uploadDeviceGroupSearchName;
    CollectDeviceCore collectDeviceInfoItem;
    List<string> CurMessages = new();
    bool isAllRestart;
    private bool isDownExport;
    bool isRestart;
    bool pauseMessage;
    StringNumber tab;
    UploadDeviceCore uploadDeviceInfoItem;
    AlarmWorker AlarmHostService { get; set; }
    CollectDeviceWorker CollectDeviceHostService { get; set; }
    [Inject]
    GlobalDeviceData GlobalDeviceData { get; set; }

    IJSObjectReference Helper { get; set; }
    HistoryValueWorker HistoryValueHostService { get; set; }

    [Inject]
    InitTimezone InitTimezone { get; set; }
    [Inject]
    IJSRuntime JS { get; set; }
    [CascadingParameter]
    MainLayout MainLayout { get; set; }

    MemoryVariableWorker MemoryVariableWorker { get; set; }
    StringNumber Panel { get; set; }

    UploadDeviceWorker UploadDeviceHostService { get; set; }
    StringNumber Uppanel { get; set; }
    [Inject]
    IVariableService VariableService { get; set; }

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
        CollectDeviceHostService = ServiceHelper.GetBackgroundService<CollectDeviceWorker>();
        UploadDeviceHostService = ServiceHelper.GetBackgroundService<UploadDeviceWorker>();
        AlarmHostService = ServiceHelper.GetBackgroundService<AlarmWorker>();
        HistoryValueHostService = ServiceHelper.GetBackgroundService<HistoryValueWorker>();
        MemoryVariableWorker = ServiceHelper.GetBackgroundService<MemoryVariableWorker>();

        _ = RunTimerAsync();
        base.OnInitialized();
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnParametersSet()
    {
        CollectDeviceQuery();
        UploadDeviceQuery();
        base.OnParametersSet();
    }
    async Task AllRestartAsync()
    {
        try
        {
            var confirm = await PopupService.OpenConfirmDialogAsync("重启", "确定重启?");
            if (confirm)
            {
                isAllRestart = true;
                StateHasChanged();
                PopupService.ShowProgressLinear();
                await Task.Run(async () => await CollectDeviceHostService.RestartDeviceThreadAsync());
                CollectDeviceQuery();
                UploadDeviceQuery();
            }
        }
        finally
        {
            collectDeviceInfoItem = null;
            uploadDeviceInfoItem = null;
            isAllRestart = false;
            PopupService.HideProgressLinear();
            await MainLayout.StateHasChangedAsync();
        }
    }
    void CollectDeviceInfo(CollectDeviceCore item)
    {
        collectDeviceInfoItem = item;
        CurMessages = new();
    }
    void CollectDeviceQuery()
    {
        _collectDeviceGroups = GlobalDeviceData.CollectDevices.Adapt<List<CollectDevice>>()?.Select(a => a.DeviceGroup)?.Where(a => a != null).Distinct()?.ToList() ?? new();
        _collectDeviceCores = CollectDeviceHostService?.CollectDeviceCores?.WhereIF(!_collectDeviceGroup.IsNullOrEmpty(), a => a.Device?.DeviceGroup == _collectDeviceGroup).ToList() ?? new();
    }
    async Task ConfigAsync(long devId, bool? isStart)
    {
        var str = isStart == true ? "启动" : "暂停";
        var confirm = await PopupService.OpenConfirmDialogAsync(str, $"确定{str}?");
        if (confirm)
        {
            CollectDeviceHostService.ConfigDeviceThread(devId, isStart == true);
        }
    }

    async Task DownDeviceMessageExportAsync(List<string> values)
    {
        try
        {
            isDownExport = true;
            StateHasChanged();
            using var memoryStream = new MemoryStream();
            StreamWriter writer = new(memoryStream);
            foreach (var item in values)
            {
                writer.WriteLine(item);
            }

            writer.Flush();
            memoryStream.Seek(0, SeekOrigin.Begin);
            using var streamRef = new DotNetStreamReference(stream: memoryStream);
            Helper ??= await JS.InvokeAsync<IJSObjectReference>("import", $"/_content/ThingsGateway.Admin.Blazor.Core/js/downloadFileFromStream.js");
            await Helper.InvokeVoidAsync("downloadFileFromStream", $"报文导出{SysDateTimeExtensions.CurrentDateTime.ToFileDateTimeFormat()}.txt", streamRef);
        }
        finally
        {
            isDownExport = false;
        }
    }
    //去除单个采集重启
    //async Task RestartAsync(long devId)
    //{
    //    try
    //    {
    //        var confirm = await PopupService.OpenConfirmDialogAsync("重启", "确定重启?");
    //        if (confirm)
    //        {
    //            isRestart = true;
    //            StateHasChanged();
    //            await Task.Run(async () => await CollectDeviceHostService.UpDeviceThreadAsync(devId));
    //            collectDeviceInfoItem = null;
    //            CollectDeviceQuery();
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        await PopupService.EnqueueSnackbarAsync(ex.Message, AlertTypes.Warning);
    //    }
    //    finally
    //    {
    //        isRestart = false;
    //        await MainLayout.StateHasChangedAsync();
    //    }
    //}
    bool timerR;
    private async Task RunTimerAsync()
    {
        while (await _periodicTimer.WaitForNextTickAsync())
        {
            try
            {
                timerR = true;
                {
                    _collectDeviceGroups = GlobalDeviceData.CollectDevices.Adapt<List<CollectDevice>>()?.Select(a => a.DeviceGroup)?.Where(a => a != null).Distinct()?.ToList() ?? new();
                    _collectDeviceCores = CollectDeviceHostService?.CollectDeviceCores?.WhereIF(!_collectDeviceGroup.IsNullOrEmpty(), a => a.Device?.DeviceGroup == _collectDeviceGroup).ToList() ?? new();
                }
                if (_collectDeviceCores?.FirstOrDefault()?.Device == null || CollectDeviceHostService?.CollectDeviceCores.Count != _collectDeviceCores.Count)
                {
                    CollectDeviceQuery();
                }

                if (_uploadDeviceCores?.FirstOrDefault()?.Device == null || UploadDeviceHostService?.UploadDeviceCores.Count != _uploadDeviceCores.Count)
                {
                    UploadDeviceQuery();
                }

                await InvokeAsync(StateHasChanged);
            }
            catch
            {
            }
            timerR = false;

        }
    }


    async Task UpConfigAsync(long devId, bool? isStart)
    {
        var str = isStart == true ? "启动" : "暂停";
        var confirm = await PopupService.OpenConfirmDialogAsync(str, $"确定{str}?");
        if (confirm)
        {
            UploadDeviceHostService.ConfigDeviceThread(devId, isStart == true);
        }
    }

    void UploadDeviceInfo(UploadDeviceCore item)
    {
        uploadDeviceInfoItem = item;
    }

    void UploadDeviceQuery()
    {
        _uploadDeviceGroups = UploadDeviceHostService.UploadDeviceCores.Select(a => a.Device)?.Select(a => a.DeviceGroup)?.Where(a => a != null).Distinct()?.ToList() ?? new();
        _uploadDeviceCores = UploadDeviceHostService?.UploadDeviceCores?.WhereIF(!_uploadDeviceGroup.IsNullOrEmpty(), a => a.Device?.DeviceGroup == _uploadDeviceGroup).ToList() ?? new();
    }
    async Task UpRestartAsync(long devId)
    {
        try
        {
            var confirm = await PopupService.OpenConfirmDialogAsync("重启", "确定重启?");
            if (confirm)
            {
                isRestart = true;
                StateHasChanged();
                await Task.Run(async () => await UploadDeviceHostService.UpDeviceThreadAsync(devId));
                uploadDeviceInfoItem = null;
                UploadDeviceQuery();
            }
        }
        catch (Exception ex)
        {
            await PopupService.EnqueueSnackbarAsync(ex.Message, AlertTypes.Warning);
        }
        finally
        {
            isRestart = false;
            await MainLayout.StateHasChangedAsync();
        }
    }
}