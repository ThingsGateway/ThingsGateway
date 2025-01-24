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

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;

using System.Collections.Concurrent;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 设备线程管理
/// </summary>
internal sealed class DeviceThreadManage : IAsyncDisposable, IDeviceThreadManage
{
    #region 动态配置

    /// <summary>
    /// 线程最大等待间隔时间
    /// </summary>
    public static volatile ChannelThreadOptions ChannelThreadOptions = App.GetOptions<ChannelThreadOptions>();

    /// <summary>
    /// 线程等待间隔时间
    /// </summary>
    public static volatile int CycleInterval = ChannelThreadOptions.MaxCycleInterval;

    private static IDispatchService<DeviceRuntime> devicelRuntimeDispatchService;
    private static IDispatchService<DeviceRuntime> DeviceRuntimeDispatchService
    {
        get
        {
            if (devicelRuntimeDispatchService == null)
                devicelRuntimeDispatchService = App.GetService<IDispatchService<DeviceRuntime>>();

            return devicelRuntimeDispatchService;
        }
    }
    static DeviceThreadManage()
    {
        Task.Factory.StartNew(async () => await SetCycleInterval().ConfigureAwait(false), TaskCreationOptions.LongRunning);
    }

    private static async Task SetCycleInterval()
    {
        var appLifetime = App.RootServices!.GetService<IHostApplicationLifetime>()!;
        var hardwareJob = GlobalData.HardwareJob;

        List<float> cpus = new();
        while (!appLifetime.ApplicationStopping.IsCancellationRequested)
        {
            try
            {
                if (hardwareJob?.HardwareInfo?.MachineInfo?.CpuRate == null) continue;
                cpus.Add((float)(hardwareJob.HardwareInfo.MachineInfo.CpuRate * 100));
                if (cpus.Count == 1 || cpus.Count > 5)
                {
                    var avg = cpus.Average();
                    cpus.RemoveAt(0);
                    //Console.WriteLine($"CPU平均值：{avg}");
                    if (avg > 80)
                    {
                        CycleInterval = Math.Max(CycleInterval, (int)(ChannelThreadOptions.MaxCycleInterval * avg / 100));
                    }
                    else if (avg < 50)
                    {
                        CycleInterval = Math.Min(CycleInterval, ChannelThreadOptions.MinCycleInterval);
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                NewLife.Log.XTrace.WriteException(ex);
            }
            finally
            {
                await Task.Delay(30000, appLifetime?.ApplicationStopping ?? default).ConfigureAwait(false);
            }
        }
    }

    #endregion 动态配置

    /// <summary>
    /// 通道线程构造函数，用于初始化通道线程实例。
    /// </summary>
    /// <param name="channelRuntime">通道表</param>
    public DeviceThreadManage(ChannelRuntime channelRuntime)
    {
        Localizer = App.CreateLocalizerByType(typeof(DeviceThreadManage));

        var config = new TouchSocketConfig();
        LogMessage = new LoggerGroup() { LogLevel = TouchSocket.Core.LogLevel.Warning };//不显示调试日志
        // 配置容器中注册日志记录器实例
        config.ConfigureContainer(a => a.RegisterSingleton<ILog>(LogMessage));

        // 设置通道信息
        CurrentChannel = channelRuntime;

        var logger = App.RootServices.GetService<Microsoft.Extensions.Logging.ILoggerFactory>().CreateLogger($"DeviceThreadManage[{channelRuntime.Name}]");
        // 添加默认日志记录器
        LogMessage.AddLogger(new EasyLogger(logger.Log_Out) { LogLevel = TouchSocket.Core.LogLevel.Trace });

        var ichannel = config.GetChannel(channelRuntime);

        // 根据配置获取通道实例
        Channel = ichannel;

        //初始设置输出文本日志
        SetLog(CurrentChannel.LogEnable, CurrentChannel.LogLevel);

        channelRuntime.DeviceThreadManage = this;

        GlobalData.DeviceStatusChangeEvent += GlobalData_DeviceStatusChangeEvent;

        LogMessage?.LogInformation(Localizer["ChannelCreate", channelRuntime.Name]);
        _ = Task.Run(CheckThreadAsync);
        _ = Task.Run(CheckRedundantAsync);
    }

    #region 日志

    private WaitLock SetLogLock = new();
    public async Task SetLogAsync(bool enable, LogLevel? logLevel = null, bool upDataBase = true)
    {
        try
        {
            await SetLogLock.WaitAsync().ConfigureAwait(false);
            bool up = false;

            if (upDataBase && (CurrentChannel.LogEnable != enable || (logLevel != null && CurrentChannel.LogLevel != logLevel)))
            {
                up = true;
            }

            CurrentChannel.LogEnable = enable;
            if (logLevel != null)
                CurrentChannel.LogLevel = logLevel.Value;
            if (up)
            {
                //更新数据库
                await GlobalData.ChannelService.UpdateLogAsync(CurrentChannel.Id, CurrentChannel.LogEnable, CurrentChannel.LogLevel).ConfigureAwait(false);
            }

            SetLog(CurrentChannel.LogEnable, CurrentChannel.LogLevel);

        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
        }
        finally
        {
            SetLogLock.Release();
        }
    }
    private void SetLog(bool enable, LogLevel? logLevel = null)
    {
        // 如果日志使能状态为 true
        if (enable)
        {

            LogMessage.LogLevel = logLevel ?? TouchSocket.Core.LogLevel.Trace;
            // 移除旧的文件日志记录器并释放资源
            if (TextLogger != null)
            {
                LogMessage.RemoveLogger(TextLogger);
                TextLogger?.Dispose();
            }

            // 创建新的文件日志记录器，并设置日志级别为 Trace
            TextLogger = TextFileLogger.GetMultipleFileLogger(LogPath);
            TextLogger.LogLevel = logLevel ?? TouchSocket.Core.LogLevel.Trace;
            // 将文件日志记录器添加到日志消息组中
            LogMessage.AddLogger(TextLogger);
        }
        else
        {
            if (logLevel != null)
                LogMessage.LogLevel = logLevel.Value;
            //LogMessage.LogLevel = TouchSocket.Core.LogLevel.Warning;
            // 如果日志使能状态为 false，移除文件日志记录器并释放资源
            if (TextLogger != null)
            {
                LogMessage.RemoveLogger(TextLogger);
                TextLogger?.Dispose();
            }
        }
    }

    private TextFileLogger? TextLogger;

    public LoggerGroup LogMessage { get; private set; }

    public string LogPath => CurrentChannel?.LogPath;


    #endregion

    #region 属性

    /// <summary>
    /// 是否采集通道
    /// </summary>
    public bool? IsCollectChannel => CurrentChannel.IsCollect;

    public long ChannelId => CurrentChannel.Id;

    internal IChannel? Channel { get; }

    public ChannelRuntime CurrentChannel { get; }

    /// <summary>
    /// 任务
    /// </summary>
    internal ConcurrentDictionary<long, DoTask> DriverTasks { get; set; } = new();

    /// <summary>
    /// 取消令箭列表
    /// </summary>
    private ConcurrentDictionary<long, CancellationTokenSource> CancellationTokenSources { get; set; } = new();

    /// <summary>
    /// 插件列表
    /// </summary>
    private ConcurrentDictionary<long, DriverBase> Drivers { get; set; } = new();

    private IStringLocalizer Localizer { get; }
    public IChannelThreadManage ChannelThreadManage { get; internal set; }

    #endregion

    #region 设备管理

    private WaitLock NewDeviceLock = new();

    /// <summary>
    /// 向当前通道添加设备
    /// </summary>
    public async Task RestartDeviceAsync(DeviceRuntime deviceRuntime, bool deleteCache)
    {
        try
        {
            await NewDeviceLock.WaitAsync().ConfigureAwait(false);
            await PrivateRestartDeviceAsync(Enumerable.Repeat(deviceRuntime, 1), deleteCache).ConfigureAwait(false);
            DeviceRuntimeDispatchService.Dispatch(null);
        }
        finally
        {
            NewDeviceLock.Release();
        }
    }

    /// <summary>
    /// 向当前通道添加设备
    /// </summary>
    public async Task RestartDeviceAsync(IEnumerable<DeviceRuntime> deviceRuntimes, bool deleteCache)
    {

        try
        {
            await NewDeviceLock.WaitAsync().ConfigureAwait(false);
            await PrivateRestartDeviceAsync(deviceRuntimes, deleteCache).ConfigureAwait(false);
            DeviceRuntimeDispatchService.Dispatch(null);
        }
        finally
        {
            NewDeviceLock.Release();
        }
    }

    private async Task PrivateRestartDeviceAsync(IEnumerable<DeviceRuntime> deviceRuntimes, bool deleteCache)
    {
        try
        {

            await PrivateRemoveDevicesAsync(deviceRuntimes.Select(a => a.Id)).ConfigureAwait(false);

            if (Disposed)
            {
                return;
            }

            if (deleteCache)
            {
                await Task.Delay(1000).ConfigureAwait(false);
                Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
                await Task.Delay(1000).ConfigureAwait(false);
                var basePath = CacheDBUtil.GetFileBasePath();


                var strings = deviceRuntimes.Select(a => a.Id.ToString()).ToHashSet();
                var dirs = Directory.GetDirectories(basePath).Where(a => strings.Contains(Path.GetFileName(a)));
                foreach (var dir in dirs)
                {
                    //删除文件夹
                    try
                    {
                        Directory.Delete(dir, true);
                    }
                    catch { }
                }

            }


            var idSet = GlobalData.GetRedundantDeviceIds();

            await deviceRuntimes.ParallelForEachAsync(async (deviceRuntime, cancellationToken) =>
            {
                if (deviceRuntime.IsCollect == true)
                {
                    if (!GlobalData.StartCollectChannelEnable)
                    {
                        return;
                    }
                }
                else
                {
                    if (!GlobalData.StartBusinessChannelEnable)
                    {
                        return;
                    }
                }

                if (!deviceRuntime.Enable) return;
                if (Disposed) return;
                if (idSet.Contains(deviceRuntime.Id)) return;
                DriverBase driver = null;
                try
                {
                    driver = CreateDriver(deviceRuntime);


                    Drivers.TryRemove(deviceRuntime.Id, out _);

                    // 将驱动程序对象添加到驱动程序集合中
                    Drivers.TryAdd(driver.DeviceId, driver);

                    // 将当前通道线程分配给驱动程序对象
                    driver.DeviceThreadManage = this;


                    // 初始化驱动程序对象，并加载源读取
                    driver.InitChannel(Channel);

                    if (Channel != null && Drivers.Count <= 1)
                    {
                        await Channel.SetupAsync(Channel.Config.Clone()).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    // 如果初始化过程中发生异常，设置初始化状态为失败，并记录警告日志
                    if (driver != null)
                        driver.IsInitSuccess = false;
                    LogMessage?.LogWarning(ex, Localizer["InitFail", CurrentChannel.PluginName, driver?.DeviceName]);
                }

                // 创建令牌并与驱动程序对象的设备ID关联，用于取消操作
                var cts = new CancellationTokenSource();
                var token = cts.Token;

                if (!CancellationTokenSources.TryAdd(driver.DeviceId, cts))
                {
                    try
                    {
                        cts.Cancel();
                        cts.SafeDispose();
                    }
                    catch
                    {
                    }
                }

                // 初始化业务线程
                var driverTask = new DoTask((t => DoWork(driver, t)), driver.LogMessage, null);
                DriverTasks.TryAdd(driver.DeviceId, driverTask);


                driverTask.Start(token);


            }, Environment.ProcessorCount).ConfigureAwait(false);

            ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxCompletionPortThreads);
            var taskCount = GlobalData.Devices.Count;
            if (taskCount * 3 > maxWorkerThreads)
            {
                var result = ThreadPool.SetMaxThreads(taskCount + maxWorkerThreads, taskCount + maxCompletionPortThreads);
            }

        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
        }
    }

    /// <summary>
    /// 移除指定设备
    /// </summary>
    /// <param name="deviceId">要移除的设备ID</param>
    public async Task RemoveDeviceAsync(long deviceId)
    {
        try
        {
            await NewDeviceLock.WaitAsync().ConfigureAwait(false);

            await PrivateRemoveDevicesAsync(Enumerable.Repeat(deviceId, 1)).ConfigureAwait(false);
            DeviceRuntimeDispatchService.Dispatch(null);
        }
        finally
        {
            NewDeviceLock.Release();

        }
    }

    /// <summary>
    /// 移除指定设备
    /// </summary>
    /// <param name="deviceIds">要移除的设备ID</param>
    public async Task RemoveDeviceAsync(IEnumerable<long> deviceIds)
    {
        try
        {
            await NewDeviceLock.WaitAsync().ConfigureAwait(false);

            await PrivateRemoveDevicesAsync(deviceIds).ConfigureAwait(false);
            DeviceRuntimeDispatchService.Dispatch(null);
        }
        finally
        {
            NewDeviceLock.Release();

        }
    }

    /// <summary>
    /// 移除指定设备
    /// </summary>
    /// <param name="deviceIds">要移除的设备ID</param>
    private async Task PrivateRemoveDevicesAsync(IEnumerable<long> deviceIds)
    {
        try
        {
            ConcurrentList<VariableRuntime> saveDevices = new();
            await deviceIds.ParallelForEachAsync(async (deviceId, cancellationToken) =>
            {
                // 查找具有指定设备ID的驱动程序对象
                if (!Drivers.TryRemove(deviceId, out var driver)) return;
                if (!DriverTasks.TryRemove(deviceId, out var task)) return;

                if (IsCollectChannel == true)
                {
                    saveDevices.AddRange(driver.VariableRuntimes.Where(a => a.Value.SaveValue).Select(a => a.Value));
                }

                // 取消驱动程序的操作
                if (CancellationTokenSources.TryRemove(deviceId, out var token))
                {
                    if (token != null)
                    {
                        token.Cancel();
                        token.Dispose();
                    }
                }
                driver.Stop();
                await task.StopAsync().ConfigureAwait(false);
            }, Environment.ProcessorCount).ConfigureAwait(false);


            await Task.Delay(100).ConfigureAwait(false);

            // 如果是采集通道，更新变量初始值
            if (IsCollectChannel == true)
            {
                try
                {
                    //添加保存数据变量读取操作
                    var saveVariable = saveDevices.Select(a => (Variable)a).ToList();

                    await GlobalData.VariableService.UpdateInitValueAsync(saveVariable).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    LogMessage.LogWarning(ex, "SaveValue");
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
        }
    }

    /// <summary>
    /// 创建插件实例，并根据设备属性设置实例
    /// </summary>
    /// <param name="deviceRuntime">当前设备</param>
    /// <returns>插件实例</returns>
    private static DriverBase CreateDriver(DeviceRuntime deviceRuntime)
    {
        var pluginService = GlobalData.PluginService;
        var driver = pluginService.GetDriver(deviceRuntime.PluginName);

        // 初始化插件配置项
        driver.InitDevice(deviceRuntime);

        // 设置设备属性到插件实例
        pluginService.SetDriverProperties(driver, deviceRuntime.DevicePropertys);

        return driver;
    }


    private async ValueTask DoWork(DriverBase driver, CancellationToken token)
    {
        try
        {
            if (token.IsCancellationRequested)
            {
                driver.Stop();
                return;
            }

            // 只有当驱动成功初始化后才执行操作
            if (driver.IsInitSuccess)
            {
                if (!driver.IsStarted)
                    await driver.StartAsync(token).ConfigureAwait(false); // 调用驱动的启动前异步方法，如果已经执行，会直接返回

                var result = await driver.ExecuteAsync(token).ConfigureAwait(false); // 执行驱动的异步执行操作

                // 根据执行结果进行不同的处理
                if (result == ThreadRunReturnTypeEnum.None)
                {
                    // 如果驱动处于离线状态且为采集驱动，则根据配置的间隔时间进行延迟
                    if (driver.CurrentDevice.DeviceStatus == DeviceStatusEnum.OffLine && IsCollectChannel == true)
                    {
                        driver.CurrentDevice.CheckEnable = false;
                        await Task.Delay(Math.Max(Math.Min(((CollectBase)driver).CollectProperties.ReIntervalTime, ChannelThreadOptions.CheckInterval / 2) - CycleInterval, 3000), token).ConfigureAwait(false);
                        driver.CurrentDevice.CheckEnable = true;
                    }
                    else
                    {
                        await Task.Delay(CycleInterval, token).ConfigureAwait(false); // 默认延迟一段时间后再继续执行
                    }
                }
                else if (result == ThreadRunReturnTypeEnum.Continue)
                {
                    await Task.Delay(1000, token).ConfigureAwait(false); // 如果执行结果为继续，则延迟一段较短的时间后再继续执行
                }
                else if (result == ThreadRunReturnTypeEnum.Break && token.IsCancellationRequested)
                {
                    driver.Stop(); // 执行驱动的释放操作
                    return; // 结束当前循环
                }
            }
            else
            {
                await Task.Delay(60000, token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            if (token.IsCancellationRequested)
                driver.Stop();
            return;
        }
        catch (ObjectDisposedException)
        {
            if (token.IsCancellationRequested)
                driver.Stop();
            return;
        }


    }


    #endregion

    #region 设备冗余切换


    private void GlobalData_DeviceStatusChangeEvent(DeviceRuntime deviceRuntime, DeviceBasicData deviceData)
    {
        if (deviceRuntime.ChannelId != ChannelId) return;

        if (deviceRuntime.RedundantEnable && deviceRuntime.Driver != null)
        {
            if (deviceRuntime.RedundantSwitchType == RedundantSwitchTypeEnum.OffLine)
            {
                _ = Task.Run(async () =>
                {
                    if (deviceRuntime.Driver != null)
                    {
                        if (deviceRuntime.DeviceStatus == DeviceStatusEnum.OffLine && (deviceRuntime.Driver?.IsInitSuccess == false || deviceRuntime.Driver?.IsStarted == true) && deviceRuntime.Driver?.DisposedValue != true)
                        {
                            await Task.Delay(deviceRuntime.RedundantScanIntervalTime).ConfigureAwait(false);//10s后再次检测
                            if (deviceRuntime.DeviceStatus == DeviceStatusEnum.OffLine && (deviceRuntime.Driver?.IsInitSuccess == false || deviceRuntime.Driver?.IsStarted == true) && deviceRuntime.Driver?.DisposedValue != true)
                            {
                                //冗余切换
                                if (deviceRuntime.RedundantEnable)
                                {
                                    await DeviceRedundantThreadAsync(deviceRuntime.Id).ConfigureAwait(false);
                                }
                            }
                        }
                    }
                });
            }

        }
    }

    private static void SetRedundantDevice(DeviceRuntime? dev, Device? newDev)
    {
        dev.DevicePropertys = newDev.DevicePropertys;
        dev.Description = newDev.Description;
        dev.ChannelId = newDev.ChannelId;
        dev.IntervalTime = newDev.IntervalTime;
        dev.Name = newDev.Name;
    }

    /// <inheritdoc/>
    public async Task DeviceRedundantThreadAsync(long deviceId)
    {
        try
        {

            if (!CurrentChannel.DeviceRuntimes.TryGetValue(deviceId, out var deviceRuntime)) return;

            //实际上DevicerRuntime是不变的，一直都是主设备对象，只是获取备用设备，改变设备插件属性
            //这里先停止采集，操作会使线程取消，需要重新恢复线程
            await RemoveDeviceAsync(deviceRuntime.Id).ConfigureAwait(false);
            if (deviceRuntime.RedundantType == RedundantTypeEnum.Standby)
            {
                var newDev = await GlobalData.DeviceService.GetDeviceByIdAsync(deviceRuntime.Id).ConfigureAwait(false);//获取设备属性
                if (newDev == null)
                {
                    LogMessage?.LogWarning($"Device with id {deviceRuntime.Id} not found");
                }
                else
                {
                    //冗余切换时，改变全部属性，但不改变变量信息
                    SetRedundantDevice(deviceRuntime, newDev);
                    deviceRuntime.RedundantType = RedundantTypeEnum.Primary;
                    LogMessage?.LogInformation($"Device {deviceRuntime.Name} switched to primary channel");
                }
            }
            else
            {
                try
                {
                    var newDev = await GlobalData.DeviceService.GetDeviceByIdAsync(deviceRuntime.RedundantDeviceId ?? 0).ConfigureAwait(false);
                    if (newDev == null)
                    {
                        LogMessage?.LogWarning($"Failed to update device thread, device with id {deviceRuntime.RedundantDeviceId} does not exist");
                    }
                    else
                    {
                        SetRedundantDevice(deviceRuntime, newDev);
                        deviceRuntime.RedundantType = RedundantTypeEnum.Standby;
                        LogMessage?.LogInformation($"Device {deviceRuntime.Name} switched to standby channel");
                    }
                }
                catch
                {
                }
            }

            //找出新的通道，添加设备线程


            if (!GlobalData.Channels.TryGetValue(deviceRuntime.ChannelId, out var channelRuntime))
                LogMessage?.LogWarning($"device {deviceRuntime.Name} cannot found channel with id{deviceRuntime.ChannelId}");


            deviceRuntime.Init(channelRuntime);
            await channelRuntime.DeviceThreadManage.RestartDeviceAsync(deviceRuntime, false).ConfigureAwait(false);

        }
        catch (Exception ex)
        {
            LogMessage.LogWarning(ex);
        }
    }

    /// <inheritdoc/>
    private async Task CheckRedundantAsync()
    {
        while (!Disposed)
        {
            try
            {
                //检测设备线程假死
                await Task.Delay(1000).ConfigureAwait(false);
                foreach (var kv in Drivers)
                {
                    if (Disposed) return;
                    var deviceRuntime = kv.Value.CurrentDevice;
                    if (deviceRuntime.RedundantEnable && deviceRuntime.Driver != null && deviceRuntime.RedundantSwitchType == RedundantSwitchTypeEnum.Script)
                    {
                        _ = Task.Run(async () =>
                        {
                            if (deviceRuntime.Driver != null)
                            {
                                if ((deviceRuntime.Driver?.IsInitSuccess == false || deviceRuntime.Driver?.IsStarted == true) && deviceRuntime.Driver?.DisposedValue != true)
                                {
                                    await Task.Delay(deviceRuntime.RedundantScanIntervalTime).ConfigureAwait(false);//10s后再次检测
                                    if (Disposed) return;
                                    if ((deviceRuntime.Driver?.IsInitSuccess == false || deviceRuntime.Driver?.IsStarted == true) && deviceRuntime.Driver?.DisposedValue != true)
                                    {
                                        //冗余切换
                                        if (deviceRuntime.RedundantEnable)
                                        {
                                            await DeviceRedundantThreadAsync(deviceRuntime.Id).ConfigureAwait(false);
                                        }
                                    }
                                }
                            }
                        });
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
                LogMessage.LogError(ex, nameof(CheckRedundantAsync));
            }
        }
    }

    #endregion

    #region 假死检测

    /// <inheritdoc/>
    private async Task CheckThreadAsync()
    {
        while (!Disposed)
        {
            try
            {
                //检测设备线程假死
                await Task.Delay(ChannelThreadOptions.CheckInterval).ConfigureAwait(false);
                if (Disposed) return;

                var num = Drivers.Count;
                foreach (var driver in Drivers.Select(a => a.Value).ToList())
                {
                    try
                    {
                        if (Disposed) return;
                        if (driver.CurrentDevice != null)
                        {
                            //线程卡死/初始化失败检测
                            if (((driver.IsStarted && driver.CurrentDevice.ActiveTime != DateTime.UnixEpoch.ToLocalTime() && driver.CurrentDevice.ActiveTime.AddMinutes(ChannelThreadOptions.CheckInterval) <= DateTime.Now)
                                || (driver.IsInitSuccess == false)) && !driver.DisposedValue)
                            {
                                //如果线程处于暂停状态，跳过
                                if (driver.CurrentDevice.DeviceStatus == DeviceStatusEnum.Pause)
                                    continue;
                                //如果初始化失败
                                if (!driver.IsInitSuccess)
                                    LogMessage?.LogWarning($"Device {driver.CurrentDevice.Name} initialization failed, restarting thread");
                                else
                                    LogMessage?.LogWarning($"Device {driver.CurrentDevice.Name} thread died, restarting thread");
                                //重启线程
                                await RestartDeviceAsync(driver.CurrentDevice, false).ConfigureAwait(false);
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessage.LogError(ex, nameof(CheckThreadAsync));
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
                LogMessage.LogError(ex, nameof(CheckThreadAsync));
            }
        }
    }

    #endregion

    #region 外部获取

    internal IDriver? GetDriver(long deviceId)
    {
        return Drivers.TryGetValue(deviceId, out var driver) ? driver : null;
    }

    internal bool Has(long deviceId)
    {
        return Drivers.ContainsKey(deviceId);
    }
    bool Disposed;
    public async ValueTask DisposeAsync()
    {
        Disposed = true;
        try
        {
            Channel?.SafeDispose();

            LogMessage?.LogInformation(Localizer["ChannelDispose", CurrentChannel?.Name ?? string.Empty]);

            await NewDeviceLock.WaitAsync().ConfigureAwait(false);

            await PrivateRemoveDevicesAsync(Drivers.Keys).ConfigureAwait(false);
        }
        finally
        {
            NewDeviceLock.Release();
        }
    }



    #endregion 外部获取

}
