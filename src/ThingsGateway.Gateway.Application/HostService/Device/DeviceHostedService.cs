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
    /// <summary>
    /// 全部重启锁
    /// </summary>
    protected readonly EasyLock restartLock = new();

    /// <summary>
    /// 单个重启锁
    /// </summary>
    protected readonly EasyLock singleRestartLock = new();

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
        DeviceService = NetCoreApp.RootServices.GetRequiredService<IDeviceService>();
        ChannelService = NetCoreApp.RootServices.GetRequiredService<IChannelService>();
        PluginService = NetCoreApp.RootServices.GetRequiredService<IPluginService>();
        Localizer = NetCoreApp.CreateLocalizerByType(typeof(DeviceHostedService))!;
        DispatchService = NetCoreApp.RootServices.GetService<IDispatchService<DeviceRunTime>>();
    }

    /// <summary>
    /// 插件列表
    /// </summary>
    public IEnumerable<DriverBase> DriverBases => ChannelThreads.SelectMany(a => a.GetDriverEnumerable()).Where(a => a.CurrentDevice != null).OrderByDescending(a => a.CurrentDevice.DeviceStatus);

    /// <summary>
    /// 设备子线程列表
    /// </summary>
    protected ConcurrentList<ChannelThread> ChannelThreads { get; set; } = new();

    private IStringLocalizer Localizer { get; }


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
    /// 根据设备生成或获取通道线程管理器
    /// </summary>
    /// <param name="driverBase">驱动程序实例</param>
    /// <returns>通道线程管理器</returns>
    protected ChannelThread GetChannelThread(DriverBase driverBase)
    {
        try
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
        }
        catch (Exception ex)
        {
            driverBase.SafeDispose();
            _logger.LogWarning(ex, nameof(GetChannelThread));
            return null;
        }
        // 创建新的通道线程的内部方法
        ChannelThread NewChannelThread(DriverBase driverBase, long channelId)
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

            // 创建新的通道线程，并将驱动程序添加到其中
            ChannelThread channelThread = new ChannelThread(channel, (a =>
            {
                return ChannelService.GetChannel(channel, a);
            }));
            channelThread.AddDriver(driverBase);
            channelThread.Channel?.Setup(channelThread.FoundataionConfig.Clone());
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
            if (HostedServiceUtil.ManagementHostedService.StartCollectDeviceEnable)
                await item.StartThreadAsync().ConfigureAwait(false);
        }
        else
        {
            if (HostedServiceUtil.ManagementHostedService.StartBusinessDeviceEnable)
            {
                // 启动通道线程
                await item.StartThreadAsync().ConfigureAwait(false);
            }
        }
    }
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
    #region 重写

    protected abstract Task<IEnumerable<DeviceRunTime>> GetDeviceRunTimeAsync(long deviceId);


    /// <summary>
    /// 更新设备线程
    /// </summary>
    public abstract Task RestartChannelThreadAsync(long deviceId, bool isChanged, bool deleteCache = false);

    #endregion
}

public delegate Task RestartEventHandler();
