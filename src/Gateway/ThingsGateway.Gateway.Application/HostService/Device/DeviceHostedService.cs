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

using Mapster;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

using TouchSocket.Core;
using TouchSocket.SerialPorts;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 设备服务
/// </summary>
internal abstract class DeviceHostedService : BackgroundService, IDeviceHostedService
{
    /// <summary>
    /// 全部重启锁
    /// </summary>
    protected readonly WaitLock restartLock = new();

    /// <summary>
    /// 单个重启锁
    /// </summary>
    protected readonly WaitLock singleRestartLock = new();

    protected ILogger _logger;
    /// <summary>
    /// 在软件关闭时取消
    /// </summary>
    protected CancellationToken _stoppingToken;

    protected IChannelService ChannelService;
    protected IDeviceService DeviceService;
    protected IDispatchService<DeviceRunTime> DispatchService;

    protected IPluginService PluginService;
    public DeviceHostedService()
    {
        DeviceService = App.RootServices.GetRequiredService<IDeviceService>();
        ChannelService = App.RootServices.GetRequiredService<IChannelService>();
        PluginService = App.RootServices.GetRequiredService<IPluginService>();
        Localizer = App.CreateLocalizerByType(typeof(DeviceHostedService))!;
        DispatchService = App.RootServices.GetService<IDispatchService<DeviceRunTime>>();
    }

    /// <inheritdoc/>
    public IEnumerable<DriverBase> DriverBases => ChannelThreads.SelectMany(a => a.GetDriverEnumerable()).Where(a => a.CurrentDevice != null).OrderByDescending(a => a.CurrentDevice.DeviceStatus);

    /// <summary>
    /// 设备子线程列表
    /// </summary>
    protected ConcurrentList<ChannelThread> ChannelThreads { get; set; } = new();

    protected IStringLocalizer Localizer { get; }

    /// <inheritdoc/>
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
                var newChannelThread = await GetChannelThreadAsync(newDriverBase).ConfigureAwait(false);
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

    /// <inheritdoc/>
    public Type GetDebugUI(string pluginName)
    {
        var driverPlugin = PluginService.GetDriver(pluginName);
        driverPlugin?.SafeDispose();
        return driverPlugin?.DriverDebugUIType;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public Type GetDriverUI(string pluginName)
    {
        var driverPlugin = PluginService.GetDriver(pluginName);
        driverPlugin?.SafeDispose();
        return driverPlugin?.DriverUIType;
    }

    /// <inheritdoc/>
    public void PauseThread(long deviceId, bool isStart)
    {
        if (deviceId == 0)
            DriverBases.ForEach(a => a.PauseThread(isStart));
        else
            DriverBases.FirstOrDefault(it => it.DeviceId == deviceId)?.PauseThread(isStart);
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

    protected void DeviceRedundantThread(DeviceRunTime deviceRunTime, DeviceData deviceData)
    {
        _ = Task.Run(async () =>
        {
            var driverBase = DriverBases.FirstOrDefault(a => a.CurrentDevice.Id == deviceData.Id);
            if (driverBase != null)
            {
                if (driverBase.CurrentDevice.DeviceStatus == DeviceStatusEnum.OffLine && (driverBase.IsInitSuccess == false || driverBase.IsBeforStarted) && !driverBase.DisposedValue)
                {
                    await Task.Delay(10000).ConfigureAwait(false);//10s后再次检测
                    if (driverBase.CurrentDevice.DeviceStatus == DeviceStatusEnum.OffLine && (driverBase.IsInitSuccess == false || driverBase.IsBeforStarted) && !driverBase.DisposedValue)
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

    private WaitLock NewChannelLock = new();
    /// <summary>
    /// 根据设备生成或获取通道线程管理器
    /// </summary>
    /// <param name="driverBase">驱动程序实例</param>
    /// <returns>通道线程管理器</returns>
    protected async ValueTask<ChannelThread> GetChannelThreadAsync(DriverBase driverBase)
    {
        try
        {
            var channelId = driverBase.CurrentDevice.ChannelId;
            await NewChannelLock.WaitAsync().ConfigureAwait(false);
            {
                // 尝试从现有的通道线程管理器列表中查找匹配的通道线程
                var channelThread = ChannelThreads.FirstOrDefault(t => t.ChannelId == channelId);
                if (channelThread != null)
                {
                    // 如果找到了匹配的通道线程，则将驱动程序添加到该线程中
                    channelThread.AddDriver(driverBase);
                    await channelThread.Channel.SetupAsync(channelThread.FoundataionConfig.Clone()).ConfigureAwait(false);
                    return channelThread;
                }

                // 如果未找到匹配的通道线程，则创建一个新的通道线程
                return await NewChannelThreadAsync(driverBase, channelId).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            driverBase.SafeDispose();
            _logger.LogWarning(ex, nameof(GetChannelThreadAsync));
            return null;
        }
        finally
        {
            NewChannelLock.Release();
        }
        // 创建新的通道线程的内部方法
        async ValueTask<ChannelThread> NewChannelThreadAsync(DriverBase driverBase, long channelId)
        {
            // 根据通道ID获取通道信息
            var channel = ChannelService.GetChannelById(channelId);
            if (channel == null)
            {
                _logger.LogWarning(Localizer["ChannelNotNull", driverBase.CurrentDevice.Name, channelId]);
                driverBase.SafeDispose();
                return null;
            }
            // 检查通道是否启用
            if (!channel.Enable)
            {
                _logger.LogWarning(Localizer["ChannelNotEnable", driverBase.CurrentDevice.Name, channel.Name]);
                driverBase.SafeDispose();
                return null;
            }
            // 确保通道不为 null
            ArgumentNullException.ThrowIfNull(channel);
            if (ChannelThreads.Count > ChannelThread.MaxCount)
            {
                driverBase.SafeDispose();
                throw new Exception($"Exceeded maximum number of channels：{ChannelThread.MaxCount}");
            }
            if (DriverBases.Select(a => a.CurrentDevice.VariableRunTimes.Count).Sum() > ChannelThread.MaxVariableCount)
            {
                driverBase.SafeDispose();
                throw new Exception($"Exceeded maximum number of variables：{ChannelThread.MaxVariableCount}");
            }

            var wts = Math.Max(ChannelThreads.Count, 10) * 10;
            ThreadPool.SetMaxThreads(wts, wts);


            var config = new TouchSocketConfig();
            var ichannel = await config.GetChannelAsync(channel.ChannelType, channel.RemoteUrl, channel.BindUrl, channel.Adapt<SerialPortOption>()).ConfigureAwait(false);
            // 创建新的通道线程，并将驱动程序添加到其中
            ChannelThread channelThread = new ChannelThread(channel, config, ichannel);
            channelThread.AddDriver(driverBase);
            await channelThread.Channel.SetupAsync(channelThread.FoundataionConfig.Clone()).ConfigureAwait(false);
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
    protected virtual async Task StartChannelThreadAsync(ChannelThread item)
    {
        if (item.IsCollectChannel)
        {
            // 启动通道线程
            if (GlobalData.CollectDeviceHostedService.StartCollectDeviceEnable)
                await item.StartThreadAsync().ConfigureAwait(false);
        }
        else
        {
            if (GlobalData.BusinessDeviceHostedService.StartBusinessDeviceEnable)
            {
                // 启动通道线程
                await item.StartThreadAsync().ConfigureAwait(false);
            }
        }
    }
    private static void SetRedundantDevice(DeviceRunTime? dev, Device? newDev)
    {
        dev.DevicePropertys = newDev.DevicePropertys;
        dev.Description = newDev.Description;
        dev.ChannelId = newDev.ChannelId;
        dev.Enable = newDev.Enable;
        dev.IntervalTime = newDev.IntervalTime;
        dev.Name = newDev.Name;
        dev.PluginName = newDev.PluginName;
    }
    #region 重写

    protected abstract Task<IEnumerable<DeviceRunTime>> GetDeviceRunTimeAsync(long deviceId);

    /// <summary>
    /// 更新设备线程
    /// </summary>
    public abstract Task RestartChannelThreadAsync(long deviceId, bool isChanged, bool deleteCache = false);

    #endregion
}

public delegate Task RestartEventHandler();
