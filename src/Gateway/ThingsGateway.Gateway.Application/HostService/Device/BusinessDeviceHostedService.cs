//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 业务设备服务
/// </summary>
internal sealed class BusinessDeviceHostedService : DeviceHostedService, IBusinessDeviceHostedService
{
    /// <summary>
    /// 线程检查时间，10分钟
    /// </summary>
    public const int CheckIntervalTime = 600;

    private WaitLock _easyLock = new(false);

    /// <summary>
    /// 已执行CreatThreads
    /// </summary>
    private volatile bool started = false;

    private IStringLocalizer BusinessDeviceHostedServiceLocalizer { get; }

    /// <inheritdoc/>
    public bool StartBusinessDeviceEnable { get; set; } = true;

    private WaitLock publicRestartLock = new();

    public BusinessDeviceHostedService(ILogger<BusinessDeviceHostedService> logger, IStringLocalizer<BusinessDeviceHostedService> localizer)
    {
        _logger = logger;
        BusinessDeviceHostedServiceLocalizer = localizer;
    }

    #region 服务

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                //每5分钟检测一次
                await Task.Delay(300000, stoppingToken).ConfigureAwait(false);

                //检测设备线程假死
                var data = DriverBases.ToList();
                var num = data.Count;
                for (int i = 0; i < num; i++)
                {
                    DriverBase driverBase = data[i];
                    try
                    {
                        if (driverBase.CurrentDevice != null)
                        {
                            //线程卡死/初始化失败检测
                            if (((driverBase.CurrentDevice.ActiveTime != null && driverBase.CurrentDevice.ActiveTime != DateTime.UnixEpoch.ToLocalTime() && driverBase.CurrentDevice.ActiveTime.Value.AddMinutes(CheckIntervalTime) <= DateTime.Now)
                                || (driverBase.IsInitSuccess == false)) && !driverBase.DisposedValue)
                            {
                                //如果线程处于暂停状态，跳过
                                if (driverBase.CurrentDevice.DeviceStatus == DeviceStatusEnum.Pause)
                                    continue;
                                //如果初始化失败
                                if (!driverBase.IsInitSuccess)
                                    _logger?.LogWarning(Localizer["DeviceInitFail", driverBase.CurrentDevice.Name]);
                                else
                                    _logger?.LogWarning(Localizer["DeviceTaskDeath", driverBase.CurrentDevice.Name]);
                                //重启线程
                                await RestartChannelThreadAsync(driverBase.CurrentDevice.Id, false).ConfigureAwait(false);
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "WhileExecute");
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BusinessDeviceHostedService WhileExecute");
            }
        }

    }

    /// <inheritdoc/>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        using var stoppingToken = new CancellationTokenSource();
        _stoppingToken = stoppingToken.Token;
        stoppingToken.Cancel();
        //取消全部采集线程
        await BeforeRemoveAllChannelThreadAsync().ConfigureAwait(false);
        //停止全部采集线程
        await RemoveAllChannelThreadAsync(true).ConfigureAwait(false);
        DriverBases.RemoveBusinessDeviceRuntime();

        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        GlobalData.CollectDeviceHostedService.Starting += CollectDeviceHostedService_Starting;
        GlobalData.CollectDeviceHostedService.Started += CollectDeviceHostedService_Started;
        GlobalData.CollectDeviceHostedService.Stoping += CollectDeviceHostedService_Stoping;
        return base.StartAsync(cancellationToken);
    }

    #endregion

    /// <inheritdoc/>
    public override async Task RestartChannelThreadAsync(long deviceId, bool isChanged, bool deleteCache = false)
    {
        try
        {
            // 等待单个重启锁
            await singleRestartLock.WaitAsync().ConfigureAwait(false);

            // 如果没有收到停止请求
            if (!_stoppingToken.IsCancellationRequested)
            {

                // 获取包含指定设备ID的通道线程，如果找不到则抛出异常
                var channelThread = ChannelThreads.FirstOrDefault(it => it.Has(deviceId))
                    ?? throw new Exception(Localizer["UpadteDeviceIdNotFound", deviceId]);

                // 获取设备运行时信息或者使用通道线程中当前设备的信息
                var dev = isChanged ? (await GetDeviceRunTimeAsync(deviceId).ConfigureAwait(false)).FirstOrDefault() : channelThread.GetDriver(deviceId).CurrentDevice;

                // 先移除设备驱动，此操作会取消线程，需要重新启动线程
                await channelThread.RemoveDriverAsync(deviceId).ConfigureAwait(false);

                if (deleteCache)
                {
                    Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
                    await Task.Delay(2000).ConfigureAwait(false);
                    var dir = CacheDBUtil.GetFileBasePath();
                    var dirs = Directory.GetDirectories(dir).FirstOrDefault(a => Path.GetFileName(a) == deviceId.ToString());
                    if (dirs != null)
                    {
                        //删除文件夹
                        try
                        {
                            Directory.Delete(dirs, true);
                        }
                        catch { }
                    }
                }

                // 如果设备信息不为空
                if (dev != null)
                {
                    // 创建新的设备驱动并获取对应的通道线程
                    DriverBase newDriverBase = dev.CreateDriver(PluginService);
                    var newChannelThread = GetChannelThread(newDriverBase);

                    // 如果找到了对应的通道线程
                    if (newChannelThread != null)
                    {
                        // 启动新的通道线程
                        await StartChannelThreadAsync(newChannelThread).ConfigureAwait(false);
                    }

                }
                else
                {
                }
            }

            _ = Task.Run(() =>
            {
                DispatchService.Dispatch(new());
            });
        }
        finally
        {
            // 释放单个重启锁
            singleRestartLock.Release();
        }
    }

    /// <inheritdoc/>
    public async Task RestartAsync(bool removeDevice = true)
    {
        try
        {
            await publicRestartLock.WaitAsync().ConfigureAwait(false);
            await StopAsync(removeDevice).ConfigureAwait(false);
            await StartAsync().ConfigureAwait(false);
        }
        finally
        {
            publicRestartLock.Release();
        }
    }

    /// <summary>
    /// 启动/创建全部设备，如果没有找到设备会创建
    /// </summary>
    public async Task StartAsync()
    {
        try
        {
            await restartLock.WaitAsync().ConfigureAwait(false);
            await singleRestartLock.WaitAsync().ConfigureAwait(false);
            if (!started)
            {
                ChannelThreads.Clear();
                DriverBases.RemoveBusinessDeviceRuntime();
                await CreatAllChannelThreadsAsync().ConfigureAwait(false);
                _ = Task.Run(() =>
                {
                    DispatchService.Dispatch(new());
                });
            }
            await StartAllChannelThreadsAsync().ConfigureAwait(false);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Start");
        }
        finally
        {
            started = true;
            singleRestartLock.Release();
            restartLock.Release();
        }
    }

    /// <inheritdoc/>
    public async Task StopAsync(bool removeDevice)
    {
        try
        {
            await restartLock.WaitAsync().ConfigureAwait(false);
            await singleRestartLock.WaitAsync().ConfigureAwait(false);
            if (started)
            {
                //取消全部采集线程
                await BeforeRemoveAllChannelThreadAsync().ConfigureAwait(false);
                //停止全部采集线程
                await RemoveAllChannelThreadAsync(removeDevice).ConfigureAwait(false);
                DriverBases.RemoveBusinessDeviceRuntime();


                for (int i = 0; i < 3; i++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stop");
        }
        finally
        {
            started = false;
            singleRestartLock.Release();
            restartLock.Release();
        }
    }

    /// <summary>
    /// 读取数据库，创建全部设备
    /// </summary>
    /// <returns></returns>
    private async Task CreatAllChannelThreadsAsync()
    {
        if (!_stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation(BusinessDeviceHostedServiceLocalizer["DeviceRuntimeGeting"]);
            var deviceRunTimes = await DeviceService.GetBusinessDeviceRuntimeAsync().ConfigureAwait(false);
            _logger.LogInformation(BusinessDeviceHostedServiceLocalizer["DeviceRuntimeGeted"]);
            var idSet = deviceRunTimes.Where(a => a.RedundantEnable && a.RedundantDeviceId != null).Select(a => a.RedundantDeviceId ?? 0).ToHashSet().ToDictionary(a => a);
            var result = deviceRunTimes.Where(a => !idSet.ContainsKey(a.Id));
            result.ParallelForEach(businessDeviceRunTime =>
            {
                if (!_stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        DriverBase driverBase = businessDeviceRunTime.CreateDriver(PluginService);
                        GetChannelThread(driverBase);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, Localizer["InitError", businessDeviceRunTime.Name]);
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

    #region 事件通知

    private async Task CollectDeviceHostedService_Started()
    {
        if (GlobalData.CollectDeviceHostedService.StartCollectDeviceEnable || GlobalData.BusinessDeviceHostedService.StartBusinessDeviceEnable)
        {
            await StartAsync().ConfigureAwait(false);
        }
    }

    private async Task CollectDeviceHostedService_Starting()
    {
        if (started)
        {
            await StopAsync(true).ConfigureAwait(false);
        }
        try
        {
            await restartLock.WaitAsync().ConfigureAwait(false);
            await singleRestartLock.WaitAsync().ConfigureAwait(false);
            if (!started)
            {
                ChannelThreads.Clear();
                DriverBases.RemoveBusinessDeviceRuntime();
                await CreatAllChannelThreadsAsync().ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreatThreads");
        }
        finally
        {
            started = true;
            singleRestartLock.Release();
            restartLock.Release();
        }
    }

    private async Task CollectDeviceHostedService_Stoping()
    {
        if (!GlobalData.BusinessDeviceHostedService.StartBusinessDeviceEnable)
            await StopAsync(true).ConfigureAwait(false);
    }

    #endregion
}
