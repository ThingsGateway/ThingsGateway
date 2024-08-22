//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 业务设备服务
/// </summary>
public class BusinessDeviceHostedService : DeviceHostedService
{
    public BusinessDeviceHostedService(ILogger<BusinessDeviceHostedService> logger, IStringLocalizer<BusinessDeviceHostedService> localizer)
    {
        _logger = logger;
        Localizer = localizer;
    }

    private IStringLocalizer Localizer { get; }

    private async Task CollectDeviceHostedService_Started()
    {
        await Task.Delay(1000).ConfigureAwait(false);
        await StartAsync().ConfigureAwait(false);
    }

    private async Task CollectDeviceHostedService_Starting()
    {
        if (started)
        {
            await StopAsync(true).ConfigureAwait(false);
        }
        await CreatThreadsAsync().ConfigureAwait(false);
    }

    private async Task CollectDeviceHostedService_Stoping()
    {
        await StopAsync(true).ConfigureAwait(false);
    }

    #region worker服务

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        HostedServiceUtil.CollectDeviceHostedService.Starting += CollectDeviceHostedService_Starting;
        HostedServiceUtil.CollectDeviceHostedService.Started += CollectDeviceHostedService_Started;
        HostedServiceUtil.CollectDeviceHostedService.Stoping += CollectDeviceHostedService_Stoping;
        return base.StartAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        using var stoppingToken = new CancellationTokenSource();
        _stoppingToken = stoppingToken.Token;
        stoppingToken.Cancel();
        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await WhileExecuteAsync(stoppingToken).ConfigureAwait(false);
    }

    #endregion worker服务

    #region 重写

    /// <summary>
    /// 读取数据库，创建全部设备
    /// </summary>
    /// <returns></returns>
    protected override async Task CreatAllChannelThreadsAsync()
    {
        if (!_stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation(Localizer["DeviceRuntimeGeting"]);
            var deviceRunTimes = await DeviceService.GetBusinessDeviceRuntimeAsync().ConfigureAwait(false);
            _logger.LogInformation(Localizer["DeviceRuntimeGeted"]);
            var idSet = deviceRunTimes.Where(a => a.RedundantEnable && a.RedundantDeviceId != null).Select(a => a.RedundantDeviceId ?? 0).ToHashSet().ToDictionary(a => a);
            var result = deviceRunTimes.Where(a => !idSet.ContainsKey(a.Id));
            result.ParallelForEach(collectDeviceRunTime =>
            {
                if (!_stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        DriverBase driverBase = collectDeviceRunTime.CreateDriver(PluginService);
                        GetChannelThread(driverBase);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, Localizer["InitError", collectDeviceRunTime.Name]);
                    }
                }
            });
        }
        for (int i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    protected override async Task<IEnumerable<DeviceRunTime>> GetDeviceRunTimeAsync(long deviceId)
    {
        return await DeviceService.GetBusinessDeviceRuntimeAsync(deviceId).ConfigureAwait(false);
    }

    #endregion 重写
}
