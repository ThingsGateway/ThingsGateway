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

using ThingsGateway.Gateway.Application.Extensions;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 采集设备服务
/// </summary>
public class CollectDeviceHostedService : DeviceHostedService
{
    /// <summary>
    /// 线程检查时间，10分钟
    /// </summary>
    public const int CheckIntervalTime = 600;

    private EasyLock _easyLock = new(false);

    /// <summary>
    /// 已执行CreatThreads
    /// </summary>
    private volatile bool started = false;

    private EasyLock publicRestartLock = new();
    private IStringLocalizer Localizer { get; }

    public CollectDeviceHostedService(ILogger<CollectDeviceHostedService> logger, IStringLocalizer<CollectDeviceHostedService> localizer)
    {
        _logger = logger;
        Localizer = localizer;
    }

    #region 服务

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await HostedServiceUtil.ManagementHostedService.StartLock.WaitAsync().ConfigureAwait(false);
        GlobalData.DeviceStatusChangeEvent += DeviceRedundantThread;
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
                            if ((driverBase.CurrentDevice.ActiveTime != null && driverBase.CurrentDevice.ActiveTime != DateTime.UnixEpoch.ToLocalTime() && driverBase.CurrentDevice.ActiveTime.Value.AddMinutes(CheckIntervalTime) <= DateTime.Now)
                                || (driverBase.IsInitSuccess == false))
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
                _logger.LogError(ex, "CollectDeviceHostedService WhileExecute");
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
        //取消其他后台服务
        await OnCollectDeviceStoping().ConfigureAwait(false);
        //停止全部采集线程
        await RemoveAllChannelThreadAsync(true).ConfigureAwait(false);
        //停止其他后台服务
        await OnCollectDeviceStoped().ConfigureAwait(false);
        DriverBases.RemoveCollectDeviceRuntime();

        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }

    #endregion


    public event RestartEventHandler Started;

    public event RestartEventHandler Starting;

    public event RestartEventHandler Stoped;

    public event RestartEventHandler Stoping;


    /// <summary>
    /// 更新设备线程
    /// </summary>
    public override async Task RestartChannelThreadAsync(long deviceId, bool isChanged, bool deleteCache = false)
    {
        try
        {
            // 等待单个重启锁
            await singleRestartLock.WaitAsync().ConfigureAwait(false);

            // 如果没有收到停止请求
            if (!_stoppingToken.IsCancellationRequested)
            {
                // 如果设备已更改，则停止
                if (isChanged)
                    await OnCollectDeviceStoping().ConfigureAwait(false);

                // 获取包含指定设备ID的通道线程，如果找不到则抛出异常
                var channelThread = ChannelThreads.FirstOrDefault(it => it.Has(deviceId))
                    ?? throw new Exception(Localizer["UpadteDeviceIdNotFound", deviceId]);

                // 获取设备运行时信息或者使用通道线程中当前设备的信息
                var dev = isChanged ? (await GetDeviceRunTimeAsync(deviceId).ConfigureAwait(false)).FirstOrDefault() : channelThread.GetDriver(deviceId).CurrentDevice;

                // 先移除设备驱动，此操作会取消线程，需要重新启动线程
                await channelThread.RemoveDriverAsync(deviceId).ConfigureAwait(false);

                if (isChanged)
                    await OnCollectDeviceStoped().ConfigureAwait(false);

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
                        // 如果设备已更改，则执行启动前的操作
                        if (isChanged)
                            await OnCollectDeviceStarting().ConfigureAwait(false);

                        try
                        {
                            // 启动新的通道线程
                            await StartChannelThreadAsync(newChannelThread).ConfigureAwait(false);
                        }
                        finally
                        {
                            if (isChanged)
                                await OnCollectDeviceStarted().ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        // 如果找不到对应的通道线程，则执行启动前后的操作
                        if (isChanged)
                        {
                            await OnCollectDeviceStarting().ConfigureAwait(false);
                            await OnCollectDeviceStarted().ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    // 如果设备信息为空，则执行启动前后的操作
                    if (isChanged)
                    {
                        await OnCollectDeviceStarting().ConfigureAwait(false);
                        await OnCollectDeviceStarted().ConfigureAwait(false);
                    }
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
    internal async Task StartAsync()
    {
        try
        {
            await restartLock.WaitAsync().ConfigureAwait(false);
            await singleRestartLock.WaitAsync().ConfigureAwait(false);
            if (!started)
            {
                ChannelThreads.Clear();
                DriverBases.RemoveCollectDeviceRuntime();

                await CreatAllChannelThreadsAsync().ConfigureAwait(false);
                await OnCollectDeviceStarting().ConfigureAwait(false);
            }
            await StartAllChannelThreadsAsync().ConfigureAwait(false);
            await OnCollectDeviceStarted().ConfigureAwait(false);
            _ = Task.Run(() =>
            {
                DispatchService.Dispatch(new());
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CollectDeviceHostedService Start error");
        }
        finally
        {
            started = true;
            singleRestartLock.Release();
            restartLock.Release();
        }
    }

    /// <summary>
    /// 停止
    /// </summary>
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
                //取消其他后台服务
                await OnCollectDeviceStoping().ConfigureAwait(false);
                //停止全部采集线程
                await RemoveAllChannelThreadAsync(removeDevice).ConfigureAwait(false);
                //停止其他后台服务
                await OnCollectDeviceStoped().ConfigureAwait(false);
                DriverBases.RemoveCollectDeviceRuntime();


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
    protected async Task CreatAllChannelThreadsAsync()
    {
        if (!_stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation(Localizer["DeviceRuntimeGeting"]);
            var collectDeviceRunTimes = (await DeviceService.GetCollectDeviceRuntimeAsync().ConfigureAwait(false));
            var idSet = collectDeviceRunTimes.Where(a => a.RedundantEnable && a.RedundantDeviceId != null).Select(a => a.RedundantDeviceId ?? 0).ToHashSet().ToDictionary(a => a);
            var result = collectDeviceRunTimes.Where(a => !idSet.ContainsKey(a.Id));

            var scripts = collectDeviceRunTimes.SelectMany(a => a.VariableRunTimes.Where(a => !a.Value.ReadExpressions.IsNullOrWhiteSpace()).Select(b => b.Value.ReadExpressions)).Concat(collectDeviceRunTimes.SelectMany(a => a.VariableRunTimes.Where(a => !a.Value.WriteExpressions.IsNullOrWhiteSpace()).Select(b => b.Value.WriteExpressions))).Distinct().ToList();
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
            _logger.LogInformation(Localizer["DeviceRuntimeGeted"]);
        }
        for (int i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }



    protected override async Task<IEnumerable<DeviceRunTime>> GetDeviceRunTimeAsync(long deviceId)
    {
        return await DeviceService.GetCollectDeviceRuntimeAsync(deviceId).ConfigureAwait(false);
    }

    #region 事件通知
    protected virtual async Task OnCollectDeviceStarted()
    {
        try
        {
            if (Started != null)
                await Started.Invoke().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "OnCollectDeviceStarted warn");
        }
        finally
        {
            started = true;
        }
    }

    protected virtual async Task OnCollectDeviceStarting()
    {
        try
        {
            if (Starting != null)
                await Starting.Invoke().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "OnCollectDeviceStarting warn");
        }

    }

    protected virtual async Task OnCollectDeviceStoped()
    {
        try
        {
            if (Stoped != null)
                await Stoped.Invoke().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "OnCollectDeviceStoped warn");
        }
        finally
        {
            started = false;
        }
    }

    protected virtual async Task OnCollectDeviceStoping()
    {
        try
        {
            if (Stoping != null)
                await Stoping.Invoke().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "OnCollectDeviceStoping warn");
        }

    }

    #endregion




}
