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

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 采集设备服务
/// </summary>
public class CollectDeviceHostedService : DeviceHostedService
{
    public CollectDeviceHostedService(ILogger<CollectDeviceHostedService> logger, IStringLocalizer<CollectDeviceHostedService> localizer)
    {
        _logger = logger;
        Localizer = localizer;
    }

    private IStringLocalizer Localizer { get; }

    #region worker服务

    /// <inheritdoc/>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        using var stoppingToken = new CancellationTokenSource();
        _stoppingToken = stoppingToken.Token;
        stoppingToken.Cancel();
        await StopThreadAsync(true).ConfigureAwait(false);
        await base.StopAsync(cancellationToken);
    }

    #endregion worker服务

    #region 重写

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //重启采集线程，会启动其他后台服务
        await HostedServiceUtil.ManagementHostedService.StartLock.WaitAsync().ConfigureAwait(false);
        //await RestartAsync();
        await WhileExecuteAsync(stoppingToken);
    }

    protected override async Task<IEnumerable<DeviceRunTime>> GetDeviceRunTimeAsync(long deviceId)
    {
        return await DeviceService.GetCollectDeviceRuntimeAsync(deviceId).ConfigureAwait(false);
    }

    /// <summary>
    /// 读取数据库，创建全部设备
    /// </summary>
    /// <returns></returns>
    protected override async Task CreatAllChannelThreadsAsync()
    {
        if (!_stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation(Localizer["DeviceRuntimeGeting"]);
            var collectDeviceRunTimes = (await DeviceService.GetCollectDeviceRuntimeAsync().ConfigureAwait(false));
            _logger.LogInformation(Localizer["DeviceRuntimeGeted"]);
            var idSet = collectDeviceRunTimes.Where(a => a.RedundantEnable && a.RedundantDeviceId != null).Select(a => a.RedundantDeviceId ?? 0).ToHashSet().ToDictionary(a => a);
            var result = collectDeviceRunTimes.Where(a => !idSet.ContainsKey(a.Id));

            var scripts = collectDeviceRunTimes.SelectMany(a => a.VariableRunTimes.Where(a=>!a.Value.ReadExpressions.IsNullOrWhiteSpace()).Select(b => b.Value.ReadExpressions)).Concat(collectDeviceRunTimes.SelectMany(a => a.VariableRunTimes.Where(a => !a.Value.WriteExpressions.IsNullOrWhiteSpace()).Select(b => b.Value.WriteExpressions))).Distinct().ToList();
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

            scripts.ParallelForEach(script =>
            {
                if (!_stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        ExpressionEvaluatorExtension.AddScript(script);
                    }
                    catch
                    {
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

    #endregion 重写
}
