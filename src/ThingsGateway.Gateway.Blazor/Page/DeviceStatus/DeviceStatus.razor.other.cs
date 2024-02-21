//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Masa.Blazor;

using SqlSugar;

using ThingsGateway.Foundation;
using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.Blazor;

public partial class DeviceStatus
{
    private async Task RunTimerAsync()
    {
        while (await _periodicTimer.WaitForNextTickAsync())
        {
            try
            {
                await Task.Delay(2000);
                await Execute();
            }
            catch
            {
            }
        }
    }

    private List<Channel> _queryChannels = new();
    private List<Device> _queryDevices = new();
    private List<PluginOutput> _queryPluginOutputs = new();

    private void PluginQuerySelections(string v)
    {
        if (string.IsNullOrEmpty(v))
        {
            return;
        }
        _queryPluginOutputs = PluginOutputs.SelectMany(a => a.Children).Where(e => e.Name.ToLowerInvariant().IndexOf(v.ToLowerInvariant()) > -1)
        .OrderBy(a => a.Name.Length).Take(20).ToList();
    }

    private void DeviceQuerySelections(string v)
    {
        if (string.IsNullOrEmpty(v))
        {
            return;
        }
        _queryDevices = Devices.Where(e => e.Name.ToLowerInvariant().IndexOf(v.ToLowerInvariant()) > -1).OrderBy(a => a.Name.Length).Take(20).ToList();
    }

    private void ChannelQuerySelections(string v)
    {
        if (string.IsNullOrEmpty(v))
        {
            return;
        }
        _queryChannels = Channels.Where(e => e.Name.ToLowerInvariant().IndexOf(v.ToLowerInvariant()) > -1).OrderBy(a => a.Name.Length).Take(20).ToList();
    }

    private DeviceInput deviceInput { get; set; } = new();
    private DriverBase _driverBaseItem;
    private BootstrapDynamicComponent _driverComponent;
    private RenderFragment _driverRender;
    private object _importRef;

    private async void DeviceInfoOnClick(DriverBase item)
    {
        if (_driverBaseItem != item)
        {
            _driverBaseItem = item;
            await Execute();
        }
    }

    private async Task ShowDriverUI()
    {
        var driver = _driverBaseItem?.DriverUIType;
        if (driver == null)
        {
            return;
        }
        _driverComponent = new BootstrapDynamicComponent(driver, new Dictionary<string, object?>() { { nameof(IDriverUIBase.Driver), _driverBaseItem } });
        _driverRender = _driverComponent.Render(a => _importRef = a);
        var result = (bool?)await PopupService.OpenAsync(typeof(DriverUIPage), new Dictionary<string, object?>()
        {
            {nameof(DriverUIPage.RenderFragment),_driverRender},
        });
    }

    private async Task DeviceRedundantThreadAsync(long devId)
    {
        var confirm = await PopupService.OpenConfirmDialogAsync("切换冗余通道", $"确定切换?");
        if (confirm)
        {
            await DeviceWorker.DeviceRedundantThreadAsync(devId);
            await DeviceQuery.InvokeAsync();
            _driverBaseItem = null;
        }
    }

    private async Task PasueThread(long devId, bool? isStart)
    {
        var str = isStart == true ? "启动" : "暂停";
        var confirm = await PopupService.OpenConfirmDialogAsync(str, $"确定{str}?");
        if (confirm)
        {
            DeviceWorker.PasueThread(devId, isStart == true);
        }
    }

    private bool _restartLoading;

    private async Task RestartAsync(long devId)
    {
        try
        {
            var confirm = await PopupService.OpenConfirmDialogAsync("重启", "确定重启?");
            if (confirm)
            {
                _restartLoading = true;
                StateHasChanged();
                await DeviceWorker.RestartChannelThreadAsync(devId);
                await DeviceQuery.InvokeAsync();
                _driverBaseItem = null;
            }
        }
        catch (Exception ex)
        {
            await PopupService.EnqueueSnackbarAsync(ex);
        }
        finally
        {
            _restartLoading = false;
        }
    }

    [Inject]
    private IPlatformService PlatformService { get; set; }

    private async Task OnExportClick()
    {
        if (!string.IsNullOrEmpty(_driverBaseItem?.LogPath))
            await PlatformService.OnLogExport(_driverBaseItem.LogPath);
    }

    private List<(int, string)> Messages { get; set; } = new List<(int, string)>();

    protected async Task Execute()
    {
        try
        {
            if (_driverBaseItem?.LogPath != null)
            {
                var files = TextFileReader.GetFile(_driverBaseItem.LogPath);
                if (files == null || files.FirstOrDefault() == null || !files.FirstOrDefault().IsSuccess)
                {
                    Messages = new List<(int, string)>();
                }
                else
                {
                    await Task.Delay(1000);
                    await Task.Factory.StartNew(() =>
                    {
                        var result = TextFileReader.LastLog(files.FirstOrDefault().FullName, 0);
                        if (result.IsSuccess)
                        {
                            Messages = result.Content.Select(a => ((int)a.LogLevel, $"{a.LogTime} - {a.Message}")).ToList();
                        }
                        else
                        {
                            Messages = new List<(int, string)>();
                        }
                    });
                }
            }
            else
            {
                Messages = new List<(int, string)>();
            }
        }
        catch
        {
        }
    }
}