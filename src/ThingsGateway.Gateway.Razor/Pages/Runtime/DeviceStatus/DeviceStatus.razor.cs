//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.Razor;

public partial class DeviceStatus
{
    private DriverBase? _driverBaseItem { get; set; }
    private Device DeviceInput { get; set; } = new();

    private async Task DeleteCacheAsync(DeviceRunTime deviceRunTime)
    {
        await DeviceHostedService.RestartChannelThreadAsync(deviceRunTime.Id, false, true);
        await DeviceQuery.InvokeAsync();
    }

    private async Task DeviceRedundantThreadAsync(long deviceId)
    {
        await DeviceHostedService.DeviceRedundantThreadAsync(deviceId);
        await DeviceQuery.InvokeAsync();
    }

    private void DriverBaseOnClick(DriverBase item)
    {
        if (_driverBaseItem != item)
        {
            _driverBaseItem = item;
        }
    }

    private void PasueThread(long devId, bool? isStart)
    {
        DeviceHostedService.PasueThread(devId, isStart == true);
    }

    private async Task RestartAsync(long deviceId)
    {
        await DeviceHostedService.RestartChannelThreadAsync(deviceId, true);
        await DeviceQuery.InvokeAsync();
        _driverBaseItem = null;
    }

    private void SetLogEnable()
    {
        _driverBaseItem.ChannelThread.LogEnable = !_driverBaseItem.ChannelThread.LogEnable;
    }

    private async Task ShowDriverUI()
    {
        var driver = _driverBaseItem?.DriverUIType;
        if (driver == null)
        {
            return;
        }
        await DialogService.Show(new DialogOption()
        {
            IsScrolling = false,
            Title = _driverBaseItem.DeviceName,
            Component = BootstrapDynamicComponent.CreateComponent(driver, new Dictionary<string, object?>()
        {
            {nameof(IDriverUIBase.Driver),_driverBaseItem},
        })
        });
    }
}
