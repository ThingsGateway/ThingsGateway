//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 设备服务
/// </summary>
public abstract class DeviceHostedService : BackgroundService
{
    protected IDispatchService<DeviceRunTime> dispatchService;

    public DeviceHostedService()
    {
        DeviceService = App.RootServices.GetRequiredService<IDeviceService>();
        ChannelService = App.RootServices.GetRequiredService<IChannelService>();
        PluginService = App.RootServices.GetRequiredService<IPluginService>();
        Localizer = App.CreateLocalizerByType(typeof(DeviceHostedService))!;
        dispatchService = App.RootServices.GetService<IDispatchService<DeviceRunTime>>();
    }

    private IStringLocalizer Localizer { get; }
    protected ILogger _logger;

    /// <summary>
    /// 全部重启锁
    /// </summary>
    protected readonly EasyLock restartLock = new();

    /// <summary>
    /// 单个重启锁
    /// </summary>
    protected readonly EasyLock singleRestartLock = new();

    protected IChannelService ChannelService;
    protected IDeviceService DeviceService;
    protected IPluginService PluginService;

    /// <summary>
    /// 插件列表
    /// </summary>
    public IEnumerable<DriverBase> DriverBases => ChannelThreads.SelectMany(a => a.GetDriverEnumerable()).Where(a => a.CurrentDevice != null).OrderByDescending(a => a.CurrentDevice.DeviceStatus);

    /// <summary>
    /// 设备子线程列表
    /// </summary>
    protected ConcurrentList<ChannelThread> ChannelThreads { get; set; } = new();

    #region 暂停设备

    /// <summary>
    /// 控制设备线程启停
    /// </summary>
    /// <param name="deviceId">传入0时全部设备都会执行</param>
    /// <param name="isStart"></param>
    /// <returns></returns>
    public void PasueThread(long deviceId, bool isStart)
    {
        if (deviceId == 0)
            DriverBases.ForEach(a => a.PasueThread(isStart));
        else
            DriverBases.FirstOrDefault(it => it.DeviceId == deviceId)?.PasueThread(isStart);
    }

    #endregion 暂停设备

    #region protected

    /// <summary>
    /// 根据设备生成或获取通道线程管理器
    /// </summary>
    /// <param name="driverBase">驱动程序实例</param>
    /// <returns>通道线程管理器</returns>
    protected ChannelThread GetChannelThread(DriverBase driverBase)
    {
        var channelId = driverBase.CurrentDevice.ChannelId;
        lock (ChannelThreads)
        {
            // 尝试从现有的通道线程管理器列表中查找匹配的通道线程
            var channelThread = ChannelThreads.FirstOrDefault(t => t.ChannelId == channelId);
            if (channelThread != null)
            {
                // 如果找到了匹配的通道线程，则将驱动程序添加到该线程中
                channelThread.AddDriver(driverBase);
                channelThread.Channel?.Setup(channelThread.FoundataionConfig.Clone());
                return channelThread;
            }

            // 如果未找到匹配的通道线程，则创建一个新的通道线程
            return NewChannelThread(driverBase, channelId);
        }

        // 创建新的通道线程的内部方法
        ChannelThread NewChannelThread(DriverBase driverBase, long channelId)
        {
            // 根据通道ID获取通道信息
            var channel = ChannelService.GetChannelById(channelId);
            if (channel == null)
            {
                _logger.LogWarning(Localizer["ChannelNotNull", driverBase.CurrentDevice.Name, channelId]);
                return null;
            }
            // 检查通道是否启用
            if (!channel.Enable)
            {
                _logger.LogWarning(Localizer["ChannelNotEnable", driverBase.CurrentDevice.Name, channel.Name]);
                return null;
            }
            // 确保通道不为 null
            ArgumentNullException.ThrowIfNull(channel);
            if (ChannelThreads.Count > ChannelThread.MaxCount)
            {
                throw new Exception($"Exceeded maximum number of channels：{ChannelThread.MaxCount}");
            }
            if (DriverBases.Select(a => a.CurrentDevice.VariableRunTimes.Count).Sum() > ChannelThread.MaxVariableCount)
            {
                throw new Exception($"Exceeded maximum number of variables：{ChannelThread.MaxVariableCount}");
            }

            // 创建新的通道线程，并将驱动程序添加到其中
            ChannelThread channelThread = new ChannelThread(channel, (a =>
            {
                return ChannelService.GetChannel(channel, a);
            }));
            channelThread.AddDriver(driverBase);
            ChannelThreads.Add(channelThread);
            return channelThread;
        }
    }

    /// <summary>
    /// 删除所有通道线程，并释放资源（可选择同时移除相关设备）
    /// </summary>
    /// <returns>异步任务</returns>
    protected async Task RemoveAllChannelThreadAsync(bool removeDevice)
    {
        // 执行删除所有通道线程前的操作
        await BeforeRemoveAllChannelThreadAsync().ConfigureAwait(false);

        // 并行遍历通道线程列表，并停止每个通道线程
        await ChannelThreads.ParallelForEachAsync(async (channelThread, cancellationToken) =>
        {
            try
            {
                await channelThread.StopThreadAsync(removeDevice).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // 记录停止通道线程时的异常信息
                _logger?.LogError(ex, channelThread.ToString());
            }
        }, Environment.ProcessorCount / 2).ConfigureAwait(false);

        // 如果指定了同时移除相关设备，则清空通道线程列表
        if (removeDevice)
            ChannelThreads.Clear();
    }

    /// <summary>
    /// 在删除所有通道线程之前执行的操作
    /// </summary>
    /// <returns>异步任务</returns>
    protected async Task BeforeRemoveAllChannelThreadAsync()
    {
        // 遍历通道线程列表，并在每个通道线程上执行 BeforeStopThread 方法
        ChannelThreads.ParallelForEach((channelThread) =>
        {
            try
            {
                channelThread.BeforeStopThread();
            }
            catch (Exception ex)
            {
                // 记录执行 BeforeStopThread 方法时的异常信息
                _logger?.LogError(ex, channelThread.ToString());
            }
        });

        // 等待一小段时间，以确保 BeforeStopThread 方法有足够的时间执行
        await Task.Delay(100).ConfigureAwait(false);
    }

    /// <summary>
    /// 启动所有通道线程
    /// </summary>
    /// <returns>异步任务</returns>
    protected async Task StartAllChannelThreadsAsync()
    {
        // 检查是否已请求停止，如果没有则开始每个通道线程
        if (!_stoppingToken.IsCancellationRequested)
        {
            foreach (var item in ChannelThreads)
            {
                if (!_stoppingToken.IsCancellationRequested)
                {
                    await StartChannelThreadAsync(item).ConfigureAwait(false);
                }
            }
        }
    }

    /// <summary>
    /// 启动通道线程
    /// </summary>
    /// <param name="item">要启动的通道线程</param>
    /// <returns>异步任务</returns>
    private async Task StartChannelThreadAsync(ChannelThread item)
    {
        // 如果通道线程是采集通道，并且启动采集设备选项已启用
        if (item.IsCollectChannel)
        {
            // 启动通道线程
            if (HostedServiceUtil.ManagementHostedService.StartCollectDeviceEnable)
                await item.StartThreadAsync().ConfigureAwait(false);
        }
        else
        {
            // 如果启动采集设备选项未启用，但启动业务设备选项已启用
            if (HostedServiceUtil.ManagementHostedService.StartBusinessDeviceEnable)
            {
                // 启动通道线程
                await item.StartThreadAsync().ConfigureAwait(false);
            }
        }
    }

    #endregion protected

    #region 单个重启

    private void SetRedundantDevice(DeviceRunTime? dev, Device? newDev)
    {
        dev.DevicePropertys = newDev.DevicePropertys;
        dev.Description = newDev.Description;
        dev.ChannelId = newDev.ChannelId;
        dev.Enable = newDev.Enable;
        dev.IntervalTime = newDev.IntervalTime;
        dev.Name = newDev.Name;
        dev.PluginName = newDev.PluginName;
    }

    /// <summary>
    /// 更新设备线程
    /// </summary>
    public async Task RestartChannelThreadAsync(long deviceId, bool isChanged, bool deleteCache = false)
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
                    await ProtectedStoping().ConfigureAwait(false);

                // 获取包含指定设备ID的通道线程，如果找不到则抛出异常
                var channelThread = ChannelThreads.FirstOrDefault(it => it.Has(deviceId))
                    ?? throw new Exception(Localizer["UpadteDeviceIdNotFound", deviceId]);

                // 获取设备运行时信息或者使用通道线程中当前设备的信息
                var dev = isChanged ? (await GetDeviceRunTimeAsync(deviceId).ConfigureAwait(false)).FirstOrDefault() : channelThread.GetDriver(deviceId).CurrentDevice;

                // 先移除设备驱动，此操作会取消线程，需要重新启动线程
                await channelThread.RemoveDriverAsync(deviceId).ConfigureAwait(false);

                if (isChanged)
                    await ProtectedStoped().ConfigureAwait(false);

                if (deleteCache)
                {
                    Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
                    await Task.Delay(2000);
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
                            await ProtectedStarting().ConfigureAwait(false);

                        try
                        {
                            // 启动新的通道线程
                            await StartChannelThreadAsync(newChannelThread).ConfigureAwait(false);
                        }
                        finally
                        {
                            if (isChanged)
                                await ProtectedStarted().ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        // 如果找不到对应的通道线程，则执行启动前后的操作
                        if (isChanged)
                        {
                            await ProtectedStarting().ConfigureAwait(false);
                            await ProtectedStarted().ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    // 如果设备信息为空，则执行启动前后的操作
                    if (isChanged)
                    {
                        await ProtectedStarting().ConfigureAwait(false);
                        await ProtectedStarted().ConfigureAwait(false);
                    }
                }
            }

            _ = Task.Run(() =>
            {
                dispatchService.Dispatch(new());
            });
        }
        finally
        {
            // 释放单个重启锁
            singleRestartLock.Release();
        }
    }

    private void DeviceRedundantThread(DeviceRunTime deviceRunTime, DeviceData deviceData)
    {
        _ = Task.Run(async () =>
        {
            var driverBase = DriverBases.FirstOrDefault(a => a.CurrentDevice.Id == deviceData.Id);
            if (driverBase != null)
            {
                if (driverBase.CurrentDevice.DeviceStatus == DeviceStatusEnum.OffLine && (driverBase.IsInitSuccess == false || driverBase.IsBeforStarted))
                {
                    await Task.Delay(10000).ConfigureAwait(false);//10s后再次检测
                    if (driverBase.CurrentDevice.DeviceStatus == DeviceStatusEnum.OffLine && (driverBase.IsInitSuccess == false || driverBase.IsBeforStarted))
                    {
                        //冗余切换
                        if (driverBase.CurrentDevice.RedundantEnable && DeviceService.GetAll().Any(a => a.Id == driverBase.CurrentDevice.RedundantDeviceId))
                        {
                            await DeviceRedundantThreadAsync(driverBase.CurrentDevice.Id).ConfigureAwait(false);
                        }
                    }
                }
            }
        });
    }

    /// <summary>
    /// 更新设备线程,切换为冗余通道
    /// </summary>
    public async Task DeviceRedundantThreadAsync(long deviceId)
    {
        try
        {
            await singleRestartLock.WaitAsync().ConfigureAwait(false);

            if (!_stoppingToken.IsCancellationRequested)
            {
                var channelThread = ChannelThreads.FirstOrDefault(it => it.Has(deviceId))
                    ?? throw new(Localizer["UpadteDeviceIdNotFound", deviceId]);
                //这里先停止采集，操作会使线程取消，需要重新恢复线程
                var dev = channelThread.GetDriver(deviceId).CurrentDevice;
                await channelThread.RemoveDriverAsync(deviceId).ConfigureAwait(false);

                if (dev.RedundantEnable)
                {
                    if (dev.RedundantType == RedundantTypeEnum.Standby)
                    {
                        var newDev = DeviceService.GetDeviceById(deviceId);
                        if (dev == null)
                        {
                            _logger.LogWarning(Localizer["UpadteDeviceIdNotFound", deviceId]);
                        }
                        else
                        {
                            //冗余切换时，改变全部属性，但不改变变量信息
                            SetRedundantDevice(dev, newDev);
                            dev.RedundantType = RedundantTypeEnum.Primary;
                            _logger?.LogInformation(Localizer["DeviceSwtichMain", dev.Name]);
                        }
                    }
                    else
                    {
                        try
                        {
                            var newDev = DeviceService.GetDeviceById(dev.RedundantDeviceId ?? 0);
                            if (newDev == null)
                            {
                                _logger.LogWarning(Localizer["UpadteDeviceIdNotFound", deviceId]);
                            }
                            else
                            {
                                SetRedundantDevice(dev, newDev);
                                dev.RedundantType = RedundantTypeEnum.Standby;
                                _logger?.LogInformation(Localizer["DeviceSwtichBackup", dev.Name]);
                            }
                        }
                        catch
                        {
                        }
                    }
                }

                //初始化
                DriverBase newDriverBase = dev.CreateDriver(PluginService);
                var newChannelThread = GetChannelThread(newDriverBase);
                if (newChannelThread != null)
                {
                    await StartChannelThreadAsync(newChannelThread).ConfigureAwait(false);
                }
            }
        }
        finally
        {
            singleRestartLock.Release();
        }
    }

    #endregion 单个重启

    #region 设备信息获取

    /// <summary>
    /// GetDebugUI
    /// </summary>
    /// <param name="pluginName"></param>
    /// <returns></returns>
    public Type GetDebugUI(string pluginName)
    {
        var driverPlugin = PluginService.GetDriver(pluginName);
        driverPlugin?.SafeDispose();
        return driverPlugin?.DriverDebugUIType;
    }

    /// <summary>
    /// GetDriverUI
    /// </summary>
    /// <param name="pluginName"></param>
    /// <returns></returns>
    public Type GetDriverUI(string pluginName)
    {
        var driverPlugin = PluginService.GetDriver(pluginName);
        driverPlugin?.SafeDispose();
        return driverPlugin?.DriverUIType;
    }

    /// <summary>
    /// 获取设备方法
    /// </summary>
    /// <param name="deviceId"></param>
    /// <returns></returns>
    public List<DriverMethodInfo> GetDriverMethodInfo(long deviceId)
    {
        var pluginName = (DeviceService.GetDeviceById(deviceId))?.PluginName;
        if (!pluginName.IsNullOrEmpty())
        {
            var propertys = PluginService.GetDriverMethodInfos(pluginName);
            return propertys;
        }
        else
        {
            return new();
        }
    }

    #endregion 设备信息获取

    #region worker服务

    protected EasyLock _easyLock = new(false);

    /// <summary>
    /// 在软件关闭时取消
    /// </summary>
    protected CancellationToken _stoppingToken;

    /// <summary>
    /// 线程检查时间，10分钟
    /// </summary>
    public const int CheckIntervalTime = 600;

    public event RestartEventHandler Stoping;

    public event RestartEventHandler Stoped;

    public event RestartEventHandler Started;

    public event RestartEventHandler Starting;

    /// <summary>
    /// 已执行ProtectedStarted
    /// </summary>
    private volatile bool otherstarted = false;

    /// <summary>
    /// 已执行CreatThreads
    /// </summary>
    protected volatile bool started = false;

    protected virtual async Task ProtectedStarted()
    {
        try
        {
            //if (!otherstarted)
            if (HostedServiceUtil.ManagementHostedService.StartCollectDeviceEnable || HostedServiceUtil.ManagementHostedService.StartBusinessDeviceEnable)
                if (Started != null)
                    await Started.Invoke().ConfigureAwait(false);
        }
        finally
        {
            otherstarted = true;
        }
    }

    protected virtual async Task ProtectedStarting()
    {
        if (Starting != null)
            await Starting.Invoke().ConfigureAwait(false);
    }

    protected virtual async Task ProtectedStoped()
    {
        try
        {
            if (otherstarted)
                if (Stoped != null)
                    await Stoped.Invoke().ConfigureAwait(false);
        }
        finally
        {
            otherstarted = false;
        }
    }

    protected virtual async Task ProtectedStoping()
    {
        if (Stoping != null)
            await Stoping.Invoke().ConfigureAwait(false);
    }

    protected abstract Task<IEnumerable<DeviceRunTime>> GetDeviceRunTimeAsync(long deviceId);

    protected virtual async Task WhileExecuteAsync(CancellationToken stoppingToken)
    {
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
                _logger.LogError(ex, "WhileExecute");
            }
        }
    }

    #endregion worker服务

    #region 全部重启

    private EasyLock publicRestartLock = new();

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
                await CreatAllChannelThreadsAsync().ConfigureAwait(false);
                await ProtectedStarting().ConfigureAwait(false);
            }
            await StartAllChannelThreadsAsync().ConfigureAwait(false);
            await ProtectedStarted().ConfigureAwait(false);
            _ = Task.Run(() =>
            {
                dispatchService.Dispatch(new());
            });
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

    /// <summary>
    /// 初始化，如果没有找到设备会创建
    /// </summary>
    public async Task CreatThreadsAsync()
    {
        try
        {
            await restartLock.WaitAsync().ConfigureAwait(false);
            await singleRestartLock.WaitAsync().ConfigureAwait(false);
            if (!started)
            {
                ChannelThreads.Clear();
                await CreatAllChannelThreadsAsync().ConfigureAwait(false);
                await ProtectedStarting().ConfigureAwait(false);
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

    /// <summary>
    /// 停止
    /// </summary>
    public async Task StopAsync(bool removeDevice)
    {
        try
        {
            await restartLock.WaitAsync().ConfigureAwait(false);
            await singleRestartLock.WaitAsync().ConfigureAwait(false);
            await StopThreadAsync(removeDevice).ConfigureAwait(false);
            BytePool.Default.Clear(); // 清空内存池
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stop");
        }
        finally
        {
            singleRestartLock.Release();
            restartLock.Release();
        }
    }

    protected async Task StopThreadAsync(bool removeDevice)
    {
        if (started)
        {
            //取消全部采集线程
            await BeforeRemoveAllChannelThreadAsync().ConfigureAwait(false);
            if (!HostedServiceUtil.ManagementHostedService.StartBusinessDeviceEnable)
                //取消其他后台服务
                await ProtectedStoping().ConfigureAwait(false);
            //停止全部采集线程
            await RemoveAllChannelThreadAsync(removeDevice).ConfigureAwait(false);
            if (!HostedServiceUtil.ManagementHostedService.StartBusinessDeviceEnable)
                //停止其他后台服务
                await ProtectedStoped().ConfigureAwait(false);
            //清空内存列表
        }
        started = false;
        otherstarted = false;
    }

    #endregion 全部重启

    #region 读取数据库

    /// <summary>
    /// 读取数据库，创建全部设备
    /// </summary>
    /// <returns></returns>
    protected abstract Task CreatAllChannelThreadsAsync();

    #endregion 读取数据库
}

public delegate Task RestartEventHandler();
