//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.Blazor;

public partial class DeviceStatusPage
{
    private CollectDeviceWorker CollectDeviceWorker { get; set; }

    private BusinessDeviceWorker BusinessDeviceWorker { get; set; }

    private void CollectDeviceQuery()
    {
        CollectBases = CollectDeviceWorker?.DriverBases.Select(a => (CollectBase)a);
    }

    private void BusinessDeviceQuery()
    {
        BusinessBases = BusinessDeviceWorker?.DriverBases.Select(a => (BusinessBase)a);
    }

    private async Task RunTimerAsync()
    {
        while (await _periodicTimer.WaitForNextTickAsync())
        {
            try
            {
                await Task.Delay(1000);

                if (CollectBases?.FirstOrDefault()?.CurrentDevice == null || CollectDeviceWorker?.DriverBases.Count() != CollectBases.Count())
                {
                    CollectDeviceQuery();
                }

                if (BusinessBases?.FirstOrDefault()?.CurrentDevice == null || BusinessDeviceWorker?.DriverBases.Count() != BusinessBases.Count())
                {
                    BusinessDeviceQuery();
                }

                await InvokeAsync(StateHasChanged);
            }
            catch
            {
            }
        }
    }

    private IEnumerable<DriverBase> CollectBases = new List<DriverBase>();
    private IEnumerable<DriverBase> BusinessBases = new List<DriverBase>();
}