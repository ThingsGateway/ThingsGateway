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

using SqlSugar;

using ThingsGateway.Admin.Blazor;

namespace ThingsGateway.Gateway.Blazor;

/// <summary>
/// 设备状态页面
/// </summary>
public partial class DeviceStatusPage : IDisposable
{
    readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromSeconds(3));
    private bool _isShowDetailUI;
    //List<string> _collectDeviceGroups = new();
    //string _collectDeviceGroupSearchName;
    //private string _collectDeviceSearchName;
    string _collectDeviceSearchName;
    List<CollectBase> _collectDriverBases = new();
    CollectBase _collectDriverItem;
    BootstrapDynamicComponent _driverComponent;
    private RenderFragment _driverRender;
    private object _importRef;
    bool _isAllRestart;
    bool _isRestart;
    StringNumber _tabNumber;
    //private string _uploadDeviceSearchName;
    //List<string> _uploadDeviceGroups = new();
    //string _uploadDeviceGroupSearchName;
    string _uploadDeviceSearchName;
    List<DriverBase> _uploadDriverBases = new();
    DriverBase _uploadDriverItem;
    AlarmWorker _alarmWorker { get; set; }
    CollectDeviceWorker _collectDeviceWorker { get; set; }
    [Inject]
    GlobalDeviceData _globalDeviceData { get; set; }

    [Inject]
    InitTimezone _initTimezone { get; set; }

    [CascadingParameter]
    MainLayout _mainLayout { get; set; }

    UploadDeviceWorker _uploadDeviceWorker { get; set; }
    StringNumber _upTabNumber { get; set; }
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
        _collectDeviceWorker = BackgroundServiceUtil.GetBackgroundService<CollectDeviceWorker>();
        _uploadDeviceWorker = BackgroundServiceUtil.GetBackgroundService<UploadDeviceWorker>();
        _alarmWorker = BackgroundServiceUtil.GetBackgroundService<AlarmWorker>();
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
                _isAllRestart = true;
                StateHasChanged();
                PopupService.ShowProgressLinear();
                await Task.Run(async () => await _collectDeviceWorker.RestartDeviceThreadAsync());
                CollectDeviceQuery();
                UploadDeviceQuery();
            }
        }
        finally
        {
            _collectDriverItem = null;
            _uploadDriverItem = null;
            _isAllRestart = false;
            PopupService.HideProgressLinear();
            await _mainLayout.StateHasChangedAsync();
        }
    }

    void CollectDeviceInfo(CollectBase item)
    {
        _collectDriverItem = item;
    }

    void CollectDeviceQuery()
    {
        //_collectDeviceGroups = _globalDeviceData.CollectDevices?.Select(a => a.DeviceGroup)?.Where(a => !a.IsNullOrEmpty()).Distinct()?.ToList() ?? new();
        _collectDriverBases = _collectDeviceWorker?.DriverBases?.WhereIF(!_collectDeviceSearchName.IsNullOrEmpty(), a => a.CurrentDevice?.Name.Contains(_collectDeviceSearchName) == true).Select(a => (CollectBase)a).ToList() ?? new();
    }
    async Task DeviceRedundantThreadAsync(long devId)
    {
        var confirm = await PopupService.OpenConfirmDialogAsync("切换冗余通道", $"确定切换?");
        if (confirm)
        {
            await _collectDeviceWorker.DeviceRedundantThreadAsync(devId);
            CollectDeviceQuery();
            _collectDriverItem = null;
        }
    }
    async Task UpDeviceRedundantThreadAsync(long devId)
    {
        var confirm = await PopupService.OpenConfirmDialogAsync("切换冗余通道", $"确定切换?");
        if (confirm)
        {
            await _uploadDeviceWorker.DeviceRedundantThreadAsync(devId);
            UploadDeviceQuery();
            _uploadDriverItem = null;
        }
    }

    async Task ConfigAsync(long devId, bool? isStart)
    {
        var str = isStart == true ? "启动" : "暂停";
        var confirm = await PopupService.OpenConfirmDialogAsync(str, $"确定{str}?");
        if (confirm)
        {
            _collectDeviceWorker.PasueThread(devId, isStart == true);
        }
    }

    void GetCollectDriverUI()
    {
        var driver = _collectDriverItem.DriverUIType;
        if (driver == null)
        {
            return;
        }
        _driverComponent = new BootstrapDynamicComponent(driver);
        _driverRender = _driverComponent.Render(a => _importRef = a);
        _isShowDetailUI = true;
    }
    void GetUploadDriverUI()
    {
        var driver = _uploadDriverItem.DriverUIType;
        if (driver == null)
        {
            return;
        }
        _driverComponent = new BootstrapDynamicComponent(driver);
        _driverRender = _driverComponent.Render(a => _importRef = a);
    }

    async Task RestartAsync(long devId)
    {
        try
        {
            var confirm = await PopupService.OpenConfirmDialogAsync("重启", "确定重启?");
            if (confirm)
            {
                _isRestart = true;
                StateHasChanged();
                await Task.Run(async () => await _collectDeviceWorker.UpDeviceThreadAsync(devId));
                _collectDriverItem = null;
                CollectDeviceQuery();
            }
        }
        catch (Exception ex)
        {
            await PopupService.EnqueueSnackbarAsync(ex);
        }
        finally
        {
            _isRestart = false;
            await _mainLayout.StateHasChangedAsync();
        }
    }
    private async Task RunTimerAsync()
    {
        while (await _periodicTimer.WaitForNextTickAsync())
        {
            try
            {
                if (_collectDriverBases?.FirstOrDefault()?.CurrentDevice == null || _collectDeviceWorker?.DriverBases.Count != _collectDriverBases.Count)
                {
                    CollectDeviceQuery();
                }

                if (_uploadDriverBases?.FirstOrDefault()?.CurrentDevice == null || _uploadDeviceWorker?.DriverBases.Count != _uploadDriverBases.Count)
                {
                    UploadDeviceQuery();
                }

                await InvokeAsync(StateHasChanged);
            }
            catch
            {
            }

        }
    }


    async Task UpConfigAsync(long devId, bool? isStart)
    {
        var str = isStart == true ? "启动" : "暂停";
        var confirm = await PopupService.OpenConfirmDialogAsync(str, $"确定{str}?");
        if (confirm)
        {
            _uploadDeviceWorker.PasueThread(devId, isStart == true);
        }
    }

    void UploadDeviceInfo(DriverBase item)
    {
        _uploadDriverItem = item;
    }

    void UploadDeviceQuery()
    {
        //_uploadDeviceGroups = _uploadDeviceWorker.DriverBases.Select(a => a.CurrentDevice)?.Select(a => a.DeviceGroup)?.Where(a => a != null).Distinct()?.ToList() ?? new();
        _uploadDriverBases = _uploadDeviceWorker?.DriverBases?.WhereIF(!_uploadDeviceSearchName.IsNullOrEmpty(), a => a.CurrentDevice?.Name.Contains(_uploadDeviceSearchName) == true).ToList() ?? new();
    }
    async Task UpRestartAsync(long devId)
    {
        try
        {
            var confirm = await PopupService.OpenConfirmDialogAsync("重启", "确定重启?");
            if (confirm)
            {
                _isRestart = true;
                StateHasChanged();
                await Task.Run(async () => await _uploadDeviceWorker.UpDeviceThreadAsync(devId));
                _uploadDriverItem = null;
                UploadDeviceQuery();
            }
        }
        catch (Exception ex)
        {
            await PopupService.EnqueueSnackbarAsync(ex);
        }
        finally
        {
            _isRestart = false;
            await _mainLayout.StateHasChangedAsync();
        }
    }
}