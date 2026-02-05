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

using System.Collections.Concurrent;

using ThingsGateway.Common.Extension;
using ThingsGateway.Gateway.Application.Extensions;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 设备线程管理
/// </summary>
internal sealed class DeviceThreadManage : IAsyncDisposable, IDeviceThreadManage
{
    Microsoft.Extensions.Logging.ILogger? _logger;

    /// <summary>
    /// 通道线程构造函数，用于初始化通道线程实例。
    /// </summary>
    /// <param name="channelRuntime">通道表</param>
    public DeviceThreadManage(ChannelRuntime channelRuntime)
    {
#pragma warning disable CA2000 // 丢失范围之前释放对象
        var config = new TouchSocketConfig();
#pragma warning restore CA2000 // 丢失范围之前释放对象
        LogMessage = new LoggerGroup() { LogLevel = TouchSocket.Core.LogLevel.Warning };//不显示调试日志
        // 配置容器中注册日志记录器实例
        config.ConfigureContainer(a => a.RegisterSingleton<ILog>(LogMessage));

        // 设置通道信息
        CurrentChannel = channelRuntime;

        _logger = App.RootServices.GetService<Microsoft.Extensions.Logging.ILoggerFactory>().CreateLogger($"DeviceThreadManage[{channelRuntime.Name}]");
        // 添加默认日志记录器
        LogMessage?.AddLogger(new EasyLogger(_logger.Log_Out) { LogLevel = TouchSocket.Core.LogLevel.Trace });

        // 根据配置获取通道实例
        var driver = GlobalData.PluginService.GetDriver(channelRuntime.PluginName);

        if (driver is IReceivedFoundationDevice foundationDevice && foundationDevice.ReceivedFoundationDevice != null)
        {
            ChannelObject.Reset(channelRuntime.GetChannel(config, foundationDevice.ReceivedFoundationDevice));
        }
        else
        {
            ChannelObject.Reset(channelRuntime.GetChannel(config));
        }

        //初始设置输出文本日志
        SetLog(CurrentChannel.LogLevel);

        channelRuntime.DeviceThreadManage = this;

        GlobalData.DeviceStatusChangeEvent += GlobalData_DeviceStatusChangeEvent;

        LogMessage?.LogInformation(string.Format(AppResource.ChannelCreate, channelRuntime.Name));

        CancellationToken = CancellationTokenSource.Token;

        CancellationToken.Register(() => GlobalData.DeviceStatusChangeEvent -= GlobalData_DeviceStatusChangeEvent);

        CheckThreadAsyncTimer = ScheduledTaskHelper.GetTask(ManageHelper.ChannelThreadOptions.CheckInterval.ToString(), CheckThreadAsync, null, LogMessage, CancellationToken);
        CheckRedundantAsyncTimer = ScheduledTaskHelper.GetTask("5000", CheckRedundantAsync, null, LogMessage, CancellationToken);
        CheckThreadAsyncTimer.Start();
        CheckRedundantAsyncTimer.Start();
    }

    private readonly IScheduledTask CheckThreadAsyncTimer;
    private readonly IScheduledTask CheckRedundantAsyncTimer;

    private readonly CancellationTokenSource CancellationTokenSource = new();

    private readonly CancellationToken CancellationToken;

    #region 日志

    private WaitLock SetLogLock = new(nameof(DeviceThreadManage));
    public async Task SetLogAsync(LogLevel? logLevel = null, bool upDataBase = true)
    {
        try
        {
            await SetLogLock.WaitAsync().ConfigureAwait(false);
            bool up = false;

            if (upDataBase && ((logLevel != null && CurrentChannel.LogLevel != logLevel)))
            {
                up = true;
            }

            if (logLevel != null)
                CurrentChannel.LogLevel = logLevel.Value;
            if (up)
            {
                //更新数据库
                await GlobalData.ChannelService.UpdateLogAsync(CurrentChannel.Id, CurrentChannel.LogLevel).ConfigureAwait(false);
            }

            SetLog(CurrentChannel.LogLevel);
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
    private void SetLog(LogLevel? logLevel = null)
    {
        LogMessage.LogLevel = logLevel ?? TouchSocket.Core.LogLevel.Trace;
        // 移除旧的文件日志记录器并释放资源
        if (TextLogger != null)
        {
            LogMessage?.RemoveLogger(TextLogger);
            TextLogger?.Dispose();
        }

        // 创建新的文件日志记录器，并设置日志级别为 Trace
        TextLogger = TextFileLogger.GetMultipleFileLogger(LogPath);
        TextLogger.LogLevel = logLevel ?? TouchSocket.Core.LogLevel.Trace;
        // 将文件日志记录器添加到日志消息组中
        LogMessage?.AddLogger(TextLogger);
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

    public ChannelObject ChannelObject { get; } = new();
    public IChannel? Channel => ChannelObject.Channel;

    public ChannelRuntime CurrentChannel { get; }

    /// <summary>
    /// 任务
    /// </summary>
    internal NonBlockingDictionary<long, TaskSchedulerLoop> DriverTasks { get; } = new();

    public int TaskCount => DriverTasks.Count;
    /// <summary>
    /// 取消令箭列表
    /// </summary>
    private NonBlockingDictionary<long, CancellationTokenSource> CancellationTokenSources { get; set; } = new();

    /// <summary>
    /// 插件列表
    /// </summary>
    private NonBlockingDictionary<long, DriverBase> Drivers { get; } = new();

    public IChannelThreadManage ChannelThreadManage { get; internal set; }

    #endregion

    #region 设备管理

    private readonly WaitLock NewDeviceLock = new(nameof(DeviceThreadManage));

    /// <summary>
    /// 向当前通道添加设备
    /// </summary>
    public async Task RestartDeviceAsync(DeviceRuntime deviceRuntime, bool deleteCache)
    {
        try
        {
            await NewDeviceLock.WaitAsync(App.HostApplicationLifetime.ApplicationStopping).ConfigureAwait(false);
            await PrivateRestartDeviceAsync([deviceRuntime], deleteCache).ConfigureAwait(false);
        }
        finally
        {
            NewDeviceLock.Release();
        }
    }

    /// <summary>
    /// 向当前通道添加设备
    /// </summary>
    public async Task RestartDeviceAsync(IList<DeviceRuntime> deviceRuntimes, bool deleteCache)
    {
        try
        {
            await NewDeviceLock.WaitAsync(App.HostApplicationLifetime.ApplicationStopping).ConfigureAwait(false);
            await PrivateRestartDeviceAsync(deviceRuntimes, deleteCache).ConfigureAwait(false);
        }
        finally
        {
            NewDeviceLock.Release();
        }
    }

    private async Task PrivateRestartDeviceAsync(IList<DeviceRuntime> deviceRuntimes, bool deleteCache)
    {
        try
        {
            await PrivateRemoveDevicesAsync(deviceRuntimes.Select(a => a.Id).ToArray()).ConfigureAwait(false);

            if (Disposed)
            {
                return;
            }
            if (App.HostApplicationLifetime.ApplicationStopping.IsCancellationRequested)
            {
                return;
            }
            if (deleteCache)
            {
                var basePath = CacheDBUtil.GetCacheFileBasePath();

                var strings = deviceRuntimes.Select(a => a.Name).ToHashSet();
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
                if (App.HostApplicationLifetime.ApplicationStopping.IsCancellationRequested)
                {
                    return;
                }
                if (Disposed)
                {
                    return;
                }

                //备用设备实时取消
                var redundantDeviceId = deviceRuntime.RedundantDeviceId;
                if (GlobalData.TryGetDeviceRuntime(redundantDeviceId ?? 0, out var redundantDeviceRuntime))
                {
                    if (GlobalData.TryGetDeviceThreadManage(redundantDeviceRuntime, out var redundantDeviceThreadManage))
                    {
                        if (redundantDeviceThreadManage != this)
                        {
                            await redundantDeviceThreadManage.RemoveDeviceAsync(redundantDeviceRuntime.Id).ConfigureAwait(false);
                        }
                        else
                        {
                            await PrivateRemoveDevicesAsync([redundantDeviceRuntime.Id]).ConfigureAwait(false);
                        }
                        redundantDeviceThreadManage.LogMessage?.LogInformation($"The device {redundantDeviceRuntime.Name} is standby and no communication tasks are created");

                        if (redundantDeviceRuntime.RedundantType == RedundantTypeEnum.Primary)
                            SetRedundantDevice(redundantDeviceRuntime, deviceRuntime);
                    }
                }

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
                if (idSet.Contains(deviceRuntime.Id) && deviceRuntime.RedundantType != RedundantTypeEnum.Primary)
                {
                    var pDevice = GlobalData.IdDevices.FirstOrDefault(a => a.Value.RedundantDeviceId == deviceRuntime.Id);
                    if (pDevice.Value?.RedundantType != RedundantTypeEnum.Standby)
                    {
                        LogMessage?.LogInformation($"The device {deviceRuntime.Name} is standby and no communication tasks are created");
                        return;
                    }
                }
                DriverBase driver = null;

                // 创建令牌并与驱动程序对象的设备ID关联，用于取消操作
                var cts = new CancellationTokenSource();
                var token = cts.Token;
                try
                {
                    driver = CreateDriver(deviceRuntime);
                    if (driver == null)
                    {
                        LogMessage?.LogWarning(string.Format(AppResource.InitFail, CurrentChannel.PluginName, driver?.DeviceName));
                        return;
                    }
                    //初始状态
                    deviceRuntime.DeviceStatus = DeviceStatusEnum.Default;

                    Drivers.TryRemove(deviceRuntime.Id, out _);

                    // 将驱动程序对象添加到驱动程序集合中
                    Drivers.TryAdd(driver.DeviceId, driver);

                    // 将当前通道线程分配给驱动程序对象
                    driver.DeviceThreadManage = this;

                    // 初始化驱动程序对象，并加载源读取
                    await driver.InitChannelAsync(ChannelObject, token).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    // 如果初始化过程中发生异常，设置初始化状态为失败，并记录警告日志
                    if (driver != null)
                        driver.IsInitSuccess = false;
                    LogMessage?.LogWarning(ex, string.Format(AppResource.InitFail, CurrentChannel.PluginName, driver?.DeviceName));
                }
                if (driver == null)
                {
                    LogMessage?.LogWarning(string.Format(AppResource.InitFail, CurrentChannel.PluginName, driver?.DeviceName));
                    return;
                }

                if (driver?.DeviceId > 0)
                {
                    if (CancellationTokenSources.TryGetValue(driver.DeviceId, out var oldCts))
                    {
                        try
                        {
                            await oldCts.SafeCancelAsync().ConfigureAwait(false);
                            oldCts.SafeDispose();
                        }
                        catch
                        {
                        }
                    }
                }

                foreach (var item in driver.IdVariableRuntimes)
                {
                    item.Value.SetErrorMessage(null);
                }

                CancellationTokenSources.TryAdd(driver.DeviceId, cts);

                _ = Task.Factory.StartNew((state) => DriverStart(state, token), driver, token, TaskCreationOptions.None, TaskScheduler.Default);
            }, App.HostApplicationLifetime.ApplicationStopping).ConfigureAwait(false);
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

            await PrivateRemoveDevicesAsync([deviceId]).ConfigureAwait(false);
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
    public async Task RemoveDeviceAsync(IList<long> deviceIds)
    {
        try
        {
            await NewDeviceLock.WaitAsync().ConfigureAwait(false);

            await PrivateRemoveDevicesAsync(deviceIds).ConfigureAwait(false);
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
    private async Task PrivateRemoveDevicesAsync(IList<long> deviceIds)
    {
        try
        {
            ConcurrentList<VariableRuntime> saveVariableRuntimes = new();
            await deviceIds.ParallelForEachAsync(async (deviceId, cancellationToken) =>
             {
                 var now = DateTime.Now;
                 // 查找具有指定设备ID的驱动程序对象
                 if (Drivers.TryRemove(deviceId, out var driver))
                 {
                     driver.CurrentDevice.SetDeviceStatus(now, true, "Communication connection has been removed");
                     if (IsCollectChannel == true)
                     {
                         foreach (var a in driver.IdVariableRuntimes)
                         {
                             a.Value.SetValue(a.Value.Value, now, false);
                             a.Value.SetErrorMessage("Communication connection has been removed");
                             if (a.Value.SaveValue && !a.Value.DynamicVariable)
                             {
                                 saveVariableRuntimes.Add(a.Value);
                             }
                         }

                     }

                 }

                 // 取消驱动程序的操作
                 if (CancellationTokenSources.TryRemove(deviceId, out var token))
                 {
                     if (token != null)
                     {
                         await token.SafeCancelAsync().ConfigureAwait(false);
                         token.SafeDispose();
                         if (driver != null)
                         {
                             await driver.StopAsync().ConfigureAwait(false);
                         }
                     }
                 }

                 if (DriverTasks.TryRemove(deviceId, out var task))
                 {
                     task.Stop();
                 }
             }).ConfigureAwait(false);

            await Task.Delay(100).ConfigureAwait(false);

            // 如果是采集通道，更新变量初始值
            if (IsCollectChannel == true)
            {
                try
                {
                    //添加保存数据变量读取操作
                    var saveVariables = new List<Variable>();
                    foreach (var item in saveVariableRuntimes)
                    {
                        var data = item.AdaptVariable();
                        data.InitValue = item.Value;
                        saveVariables.Add(data);
                    }
                    await GlobalData.VariableService.UpdateInitValueAsync(saveVariables).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    LogMessage?.LogWarning(ex, "SaveValue");
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
        pluginService.SetDriverProperties(driver.DriverProperties, deviceRuntime.DevicePropertys);

        return driver;
    }

    private async Task DriverStart(object? state, CancellationToken token)
    {
        try
        {
            if (state is not DriverBase driver) return;
            token.ThrowIfCancellationRequested();
            // 只有当驱动成功初始化后才执行操作
            if (driver.IsInitSuccess)
            {
                if (!driver.IsStarted)
                    await driver.StartAsync(token).ConfigureAwait(false);

                driver.GetTasks(token); // 执行驱动的异步执行操作

                DriverTasks.TryAdd(driver.DeviceId, driver.TaskSchedulerLoop);

                token.ThrowIfCancellationRequested();
                lock (driver.TaskSchedulerLoop)
                {
                    if (!driver.DisposedValue && !token.IsCancellationRequested)
                        driver.TaskSchedulerLoop.Start();
                }
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (ObjectDisposedException)
        {
            return;
        }
    }

    #endregion

    #region 设备冗余切换

    private void GlobalData_DeviceStatusChangeEvent(DeviceRuntime deviceRuntime, DeviceBasicData deviceData)
    {
        if (deviceRuntime.DeviceStatus != DeviceStatusEnum.OffLine) return;
        if (deviceRuntime.ChannelId != ChannelId) return;
        try
        {
            if (GlobalData.IsRedundant(deviceRuntime.Id) && deviceRuntime.Driver != null)
            {
                if (deviceRuntime.RedundantSwitchType == RedundantSwitchTypeEnum.OffLine)
                {
                    _ = Task.Run(async () =>
                    {
                        if (deviceRuntime.Driver != null)
                        {
                            if (deviceRuntime.DeviceStatus == DeviceStatusEnum.OffLine && (deviceRuntime.Driver?.IsInitSuccess == false || deviceRuntime.Driver?.IsStarted == true) && deviceRuntime.Driver?.DisposedValue != true)
                            {
                                await Task.Delay(deviceRuntime.RedundantScanIntervalTime, CancellationToken).ConfigureAwait(false);//10s后再次检测
                                if (deviceRuntime.DeviceStatus == DeviceStatusEnum.OffLine && (deviceRuntime.Driver?.IsInitSuccess == false || deviceRuntime.Driver?.IsStarted == true) && deviceRuntime.Driver?.DisposedValue != true && deviceRuntime.RedundantType != RedundantTypeEnum.Standby)
                                {
                                    //冗余切换
                                    if (GlobalData.IsRedundant(deviceRuntime.Id))
                                    {
                                        if (!CancellationToken.IsCancellationRequested)
                                            await DeviceRedundantThreadAsync(deviceRuntime.Id, default).ConfigureAwait(false);
                                    }
                                }
                            }
                        }
                    }, CancellationToken);
                }
            }
        }
        catch
        {
        }
    }

    private static void SetRedundantDevice(DeviceRuntime? deviceRuntime, DeviceRuntime? newDeviceRuntime)
    {
        //传入变量
        //newDeviceRuntime.VariableRuntimes.ParallelForEach(a => a.Value.SafeDispose());
        deviceRuntime.VariableRuntimes.ParallelForEach(a => a.Value.Init(newDeviceRuntime));
        GlobalData.VariableRuntimeDispatchService.Dispatch(null);
    }

    /// <inheritdoc/>
    public async Task DeviceRedundantThreadAsync(long deviceId, CancellationToken cancellationToken)
    {
        try
        {
            DeviceRuntime newDeviceRuntime = null;

            if (!CurrentChannel.DeviceRuntimes.TryGetValue(deviceId, out var deviceRuntime)) return;

            //实际上DevicerRuntime是不变的，一直都是主设备对象，只是获取备用设备，改变设备插件属性
            //这里先停止采集，操作会使线程取消，需要重新恢复线程

            //注意切换后需要刷新业务设备的变量和采集设备集合
            await RemoveDeviceAsync(deviceRuntime.Id).ConfigureAwait(false);

            //获取主设备
            var devices = GlobalData.IdDevices;

            if (deviceRuntime.RedundantEnable && deviceRuntime.RedundantDeviceId != null)
            {
                if (!GlobalData.TryGetDeviceRuntime(deviceRuntime.RedundantDeviceId ?? 0, out newDeviceRuntime))
                {
                    devices.TryGetValue(deviceRuntime.RedundantDeviceId ?? 0, out var newDev);
                    if (newDev == null)
                    {
                        LogMessage?.LogWarning($"Device with deviceId {deviceRuntime.RedundantDeviceId} not found");
                    }
                    else
                    {
                        newDeviceRuntime = newDev.AdaptDeviceRuntime();
                        SetRedundantDevice(deviceRuntime, newDeviceRuntime);
                    }
                }
                else
                {
                    SetRedundantDevice(deviceRuntime, newDeviceRuntime);
                }
            }
            else
            {
                newDeviceRuntime = GlobalData.ReadOnlyIdDevices.FirstOrDefault(a => a.Value.RedundantDeviceId == deviceRuntime.Id).Value;
                if (newDeviceRuntime == null)
                {
                    var newDev = devices.FirstOrDefault(a => a.Value.RedundantDeviceId == deviceRuntime.Id).Value;
                    if (newDev == null)
                    {
                        LogMessage?.LogWarning($"Device with redundantDeviceId {deviceRuntime.Id} not found");
                    }
                    else
                    {
                        newDeviceRuntime = newDev.AdaptDeviceRuntime();
                        SetRedundantDevice(deviceRuntime, newDeviceRuntime);
                    }
                }
                else
                {
                    SetRedundantDevice(deviceRuntime, newDeviceRuntime);
                }
            }

            if (newDeviceRuntime == null) return;

            deviceRuntime.RedundantType = RedundantTypeEnum.Standby;
            newDeviceRuntime.RedundantType = RedundantTypeEnum.Primary;
            if (newDeviceRuntime.Id != deviceRuntime.Id)
                LogMessage?.LogInformation($"Device {deviceRuntime.Name} switched to standby channel");

            //找出新的通道，添加设备线程

            if (!GlobalData.IdChannels.TryGetValue(newDeviceRuntime.ChannelId, out var channelRuntime))
                LogMessage?.LogWarning($"device {newDeviceRuntime.Name} cannot found channel with id{newDeviceRuntime.ChannelId}");

            newDeviceRuntime.Init(channelRuntime);
            GlobalData.ChannelDeviceRuntimeDispatchService.Dispatch(null);

            await channelRuntime.DeviceThreadManage.RestartDeviceAsync(newDeviceRuntime, false).ConfigureAwait(false);
            channelRuntime.DeviceThreadManage.LogMessage?.LogInformation($"Device {newDeviceRuntime.Name} switched to primary channel");

            //需要重启业务线程
            var businessDeviceRuntimes = GlobalData.IdDevices.Where(a => a.Value.Driver is BusinessBase && ((BusinessBase)a.Value.Driver).CollectDevices.ContainsKey(a.Key) == true).Select(a => a.Value).ToArray();
            await businessDeviceRuntimes.ParallelForEachAsync(async (businessDeviceRuntime, token) =>
              {
                  if (businessDeviceRuntime.Driver != null)
                  {
                      try
                      {
                          await businessDeviceRuntime.Driver.AfterVariablesChangedAsync(token).ConfigureAwait(false);
                      }
                      catch (Exception ex)
                      {
                          LogMessage?.LogWarning(ex);
                      }
                  }
              }, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
        }
    }

    /// <inheritdoc/>
    private async Task CheckRedundantAsync(object? state, CancellationToken cancellationToken)
    {
        try
        {
            foreach (var kv in Drivers)
            {
                if (Disposed) return;
                var deviceRuntime = kv.Value.CurrentDevice;
                if (deviceRuntime != null && GlobalData.IsRedundant(deviceRuntime.Id) && deviceRuntime.Driver != null && deviceRuntime.RedundantSwitchType == RedundantSwitchTypeEnum.Script)
                {
                    {
                        if (deviceRuntime.Driver != null)
                        {
                            if (deviceRuntime.RedundantScript.GetExpressionsResult(deviceRuntime, LogMessage).ToBoolean(true) && (deviceRuntime.Driver?.IsInitSuccess == false || deviceRuntime.Driver?.IsStarted == true) && deviceRuntime.Driver?.DisposedValue != true)
                            {
                                await Task.Delay(deviceRuntime.RedundantScanIntervalTime, cancellationToken).ConfigureAwait(false);//10s后再次检测
                                if (Disposed) return;
                                if ((deviceRuntime.RedundantScript.GetExpressionsResult(deviceRuntime, LogMessage).ToBoolean(true) && deviceRuntime.Driver?.IsInitSuccess == false || deviceRuntime.Driver?.IsStarted == true) && deviceRuntime.Driver?.DisposedValue != true && deviceRuntime.RedundantType != RedundantTypeEnum.Standby)
                                {
                                    //冗余切换
                                    if (GlobalData.IsRedundant(deviceRuntime.Id))
                                    {
                                        if (!cancellationToken.IsCancellationRequested)
                                            await DeviceRedundantThreadAsync(deviceRuntime.Id, cancellationToken).ConfigureAwait(false);
                                    }
                                }
                            }
                        }
                    }
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
            LogMessage?.LogError(ex, nameof(CheckRedundantAsync));
        }
    }

    #endregion

    #region 假死检测

    /// <inheritdoc/>
    private async Task CheckThreadAsync(object? state, CancellationToken cancellationToken)
    {

        try
        {
            if (Disposed) return;

            var num = Drivers.Count;
            foreach (var driver in Drivers.Select(a => a.Value).Where(a => a != null).ToList())
            {
                try
                {
                    if (Disposed) return;
                    if (driver.CurrentDevice != null)
                    {
                        //线程卡死/初始化失败检测
                        if (((driver.IsStarted && driver.CurrentDevice.ActiveTime != DateTime.UnixEpoch.ToLocalTime() && driver.CurrentDevice.ActiveTime.AddMilliseconds(ManageHelper.ChannelThreadOptions.CheckInterval) <= DateTime.Now)
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
                            if (!cancellationToken.IsCancellationRequested)
                                await RestartDeviceAsync(driver.CurrentDevice, false).ConfigureAwait(false);
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMessage?.LogError(ex, nameof(CheckThreadAsync));
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
            LogMessage?.LogError(ex, nameof(CheckThreadAsync));
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
            CheckThreadAsyncTimer.SafeDispose();
            CheckRedundantAsyncTimer.SafeDispose();

            await CancellationTokenSource.SafeCancelAsync().ConfigureAwait(false);
            CancellationTokenSource.SafeDispose();
            GlobalData.DeviceStatusChangeEvent -= GlobalData_DeviceStatusChangeEvent;
            await NewDeviceLock.WaitAsync().ConfigureAwait(false);
            await PrivateRemoveDevicesAsync(Drivers.Select(a => a.Key).ToArray()).ConfigureAwait(false);
            if (Channel?.ReadOnlyCollects.Count == 0)
            {
                Channel?.SafeDispose();
            }
            else
            {
                LogMessage?.LogInformation("通道管理器在释放时，仍然检测存在采集设备，无法释放通道对象，可能是因为共用了通道服务");
            }

            LogMessage?.LogInformation(string.Format(AppResource.ChannelDispose, CurrentChannel?.Name ?? string.Empty));

            await Task.Delay(1000).ConfigureAwait(false);

            if (LogMessage?.Logs != null)
            {
                foreach (var item in LogMessage.Logs)
                {
                    item.TryDispose();
                }
            }
            _logger?.TryDispose();
        }
        finally
        {
            NewDeviceLock.Release();
        }
    }

    #endregion 外部获取

}
