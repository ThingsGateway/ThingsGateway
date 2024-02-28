//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 业务设备服务
/// </summary>
public class BusinessDeviceWorker : DeviceWorker
{
    public BusinessDeviceWorker(IServiceScopeFactory serviceScopeFactory, IHostApplicationLifetime appLifetime) : base(serviceScopeFactory, appLifetime)
    {
        _logger = _serviceScope.ServiceProvider.GetService<ILoggerFactory>().CreateLogger("业务设备服务");
    }
    private async Task CollectDeviceWorker_Starting()
    {
        if (started)
        {
            await StopAsync(true);
        }
        await CreatThreadsAsync();

    }

    private async Task CollectDeviceWorker_Started()
    {
        await Task.Delay(1000);
        await StartAsync();
    }

    private async Task CollectDeviceWorker_Stoping()
    {
        await StopAsync(true);
    }

    #region 设备信息获取

    /// <summary>
    /// 获取变量业务属性
    /// </summary>
    /// <param name="driverId"></param>
    /// <param name="dependencyProperties"></param>
    /// <returns></returns>
    public List<DependencyProperty> GetVariablePropertys(string pluginName, List<DependencyProperty> dependencyProperties = null)
    {
        var propertys = PluginService.GetVariablePropertyTypes(pluginName).Values.ToList().Adapt<List<DependencyProperty>>();
        dependencyProperties?.ForEach(it =>
        {
            var dependencyProperty = propertys.FirstOrDefault(a => a.Name == it.Name);
            if (dependencyProperty != null && !string.IsNullOrEmpty(it.Value))
            {
                dependencyProperty.Value = it.Value;
            }
        });

        return propertys.ToList();
    }

    #endregion 设备信息获取

    #region worker服务

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        using var stoppingToken = new CancellationTokenSource();
        _stoppingToken = stoppingToken.Token;
        stoppingToken.Cancel();
        await base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _easyLock?.WaitAsync();
        ManagementWoker = WorkerUtil.GetWoker<ManagementWoker>();
        var collectDeviceWorker = WorkerUtil.GetWoker<CollectDeviceWorker>();
        collectDeviceWorker.Starting += CollectDeviceWorker_Starting;
        collectDeviceWorker.Started += CollectDeviceWorker_Started;
        collectDeviceWorker.Stoping += CollectDeviceWorker_Stoping;
        PluginService = _serviceScope.ServiceProvider.GetService<IPluginService>();
        GlobalData = _serviceScope.ServiceProvider.GetService<GlobalData>();
        await WhileExecuteAsync(stoppingToken);
    }

    #endregion worker服务

    #region 重写

    protected override async Task<IEnumerable<DeviceRunTime>> GetDeviceRunTimeAsync(long deviceId)
    {
        return await _serviceScope.ServiceProvider.GetService<IDeviceService>().GetBusinessDeviceRuntimeAsync(deviceId);
    }

    /// <summary>
    /// 读取数据库，创建全部设备
    /// </summary>
    /// <returns></returns>
    protected override async Task CreatAllChannelThreadsAsync()
    {
        if (!_stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("正在获取业务设备组态信息");
            var deviceRunTimes = await _serviceScope.ServiceProvider.GetService<IDeviceService>().GetBusinessDeviceRuntimeAsync();
            _logger.LogInformation("获取业务设备组态信息完成");
            var idSet = deviceRunTimes.ToDictionary(a => a.Id);
            var result = deviceRunTimes.Where(a => !idSet.ContainsKey(a.RedundantDeviceId) && !a.IsRedundant).ToList();
            result.ForEach(collectDeviceRunTime =>
            {
                if (!_stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        DriverBase driverBase = collectDeviceRunTime.CreatDriver(PluginService);
                        GetChannelThread(driverBase);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"{collectDeviceRunTime.Name}初始化错误！");
                    }
                }
            });
        }
    }

    #endregion 重写
}