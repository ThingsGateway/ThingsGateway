//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Furion.Logging.Extensions;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 采集设备服务
/// </summary>
public class CollectDeviceWorker : DeviceWorker
{
    public CollectDeviceWorker(IServiceScopeFactory serviceScopeFactory, IHostApplicationLifetime appLifetime) : base(serviceScopeFactory, appLifetime)
    {
        _logger = _serviceScope.ServiceProvider.GetService<ILoggerFactory>().CreateLogger("采集设备服务");
    }

    #region public 设备创建更新结束

    private EasyLock publicRestartLock = new();

    public async Task RestartAsync()
    {
        try
        {
            await publicRestartLock.WaitAsync();

            await StopAsync();
            await StartAsync();
        }
        finally
        {
            publicRestartLock.Release();
        }
    }

    /// <summary>
    /// 启动全部设备，如果没有找到设备会创建
    /// </summary>
    public async Task StartAsync()
    {
        try
        {
            await restartLock.WaitAsync();
            await singleRestartLock.WaitAsync();
            if (ChannelThreads.Count == 0)
            {
                await CreatAllChannelThreadsAsync();
                await ProtectedStarting();
            }
            await StartAllChannelThreadsAsync();
            await ProtectedStarted();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "启动发生错误");
        }
        finally
        {
            singleRestartLock.Release();
            restartLock.Release();
        }
    }

    /// <summary>
    /// 初始化，如果没有找到设备会创建
    /// </summary>
    public async Task CreatThreadsAsync()
    {
        try
        {
            await restartLock.WaitAsync();
            await singleRestartLock.WaitAsync();
            if (ChannelThreads.Count == 0)
            {
                await CreatAllChannelThreadsAsync();
                await ProtectedStarting();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "启动发生错误");
        }
        finally
        {
            singleRestartLock.Release();
            restartLock.Release();
        }
    }

    /// <summary>
    /// 停止
    /// </summary>
    public async Task StopAsync()
    {
        try
        {
            await restartLock.WaitAsync();
            await singleRestartLock.WaitAsync();
            await StopThreadAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "停止错误");
        }
        finally
        {
            singleRestartLock.Release();
            restartLock.Release();
        }
    }

    internal async Task StopThreadAsync()
    {
        //取消全部采集线程
        await BeforeRemoveAllChannelThreadAsync();
        //取消其他后台服务
        await ProtectedStoping();
        //停止全部采集线程
        await RemoveAllChannelThreadAsync();
        //停止其他后台服务
        await ProtectedStoped();
        //清空内存列表
        GlobalData.CollectDevices.Clear();
    }

    /// <summary>
    /// 读取数据库，创建全部设备
    /// </summary>
    /// <returns></returns>
    protected virtual async Task CreatAllChannelThreadsAsync()
    {
        if (!_stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("正在获取采集设备组态信息");
            var collectDeviceRunTimes = (await _serviceScope.ServiceProvider.GetService<IDeviceService>().GetCollectDeviceRuntimeAsync());
            _logger.LogInformation("获取采集设备组态信息完成");
            var idSet = collectDeviceRunTimes.ToDictionary(a => a.Id);
            var result = collectDeviceRunTimes.Where(a => !idSet.ContainsKey(a.RedundantDeviceId) && !a.IsRedundant).ToList();
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

    #endregion public 设备创建更新结束

    #region worker服务

    /// <inheritdoc/>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        using var stoppingToken = new CancellationTokenSource();
        _stoppingToken = stoppingToken.Token;
        stoppingToken.Cancel();
        await StopThreadAsync();
        await base.StopAsync(cancellationToken);
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _easyLock?.WaitAsync();
        PluginService = _serviceScope.ServiceProvider.GetService<IPluginService>();
        GlobalData = _serviceScope.ServiceProvider.GetService<GlobalData>();
        //重启采集线程，会启动其他后台服务
        await RestartAsync();
        await WhileExecuteAsync(stoppingToken);
    }

    protected override async Task<IEnumerable<DeviceRunTime>> GetDeviceRunTimeAsync(long deviceId)
    {
        return await _serviceScope.ServiceProvider.GetService<DeviceService>().GetCollectDeviceRuntimeAsync(deviceId);
    }

    #endregion worker服务
}