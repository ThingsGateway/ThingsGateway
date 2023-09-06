#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Furion;
using Furion.FriendlyException;
using Furion.Logging.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ThingsGateway.Application.Extensions;
using ThingsGateway.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Application;

/// <summary>
/// 设备采集后台服务
/// </summary>
public class CollectDeviceWorker : BackgroundService
{
    private readonly ICollectDeviceService _collectDeviceService;
    private readonly ILogger<CollectDeviceWorker> _logger;
    private readonly PluginSingletonService _pluginService;
    /// <inheritdoc/>
    public CollectDeviceWorker(ILogger<CollectDeviceWorker> logger, IServiceProvider serviceProvider)
    {
        ServiceHelper.Services = serviceProvider;
        _logger = logger;
        _pluginService = ServiceHelper.Services.GetService<PluginSingletonService>();
        _collectDeviceService = App.GetService<ICollectDeviceService>();
    }
    /// <summary>
    /// 读取未停止的采集设备List
    /// </summary>
    public List<CollectDeviceCore> CollectDeviceCores => CollectDeviceThreads
        .Where(a => !a.StoppingTokens.Any(b => b.IsCancellationRequested))
        .SelectMany(a => a.CollectDeviceCores).ToList();

    /// <summary>
    /// 设备子线程列表
    /// </summary>
    private ConcurrentList<CollectDeviceThread> CollectDeviceThreads { get; set; } = new();

    #region 设备创建更新结束
    /// <summary>
    /// 全部重启锁
    /// </summary>
    private readonly EasyLock restartLock = new();
    /// <summary>
    /// 单个重启锁
    /// </summary>
    private readonly EasyLock singleRestartLock = new();
    /// <summary>
    /// 控制设备线程启停
    /// </summary>
    /// <param name="deviceId">传入0时全部设备都会执行</param>
    /// <param name="isStart"></param>
    /// <returns></returns>
    public void ConfigDeviceThread(long deviceId, bool isStart)
    {
        if (deviceId == 0)
            CollectDeviceCores.ForEach(a => a.PasueThread(isStart));
        else
            CollectDeviceCores.FirstOrDefault(it => it.DeviceId == deviceId)?.PasueThread(isStart);
    }

    /// <summary>
    /// 重启采集服务
    /// </summary>
    public async Task RestartDeviceThreadAsync()
    {
        try
        {
            //重启操作在未完全之前直接取消
            if (restartLock.IsWaitting)
            {
                return;
            }
            await restartLock.WaitAsync();
            await singleRestartLock.WaitAsync();
            //停止其他后台服务
            await StopOtherHostService();
            //停止全部采集线程
            await RemoveAllDeviceThreadAsync();
            //创建全部采集线程
            await CreatAllDeviceThreadsAsync();

            //开始其他后台服务
            await StartOtherHostService();

            //开始全部采集线程
            await StartAllDeviceThreadsAsync();

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "重启错误");
        }
        finally
        {
            singleRestartLock.Release();
            restartLock.Release();
        }
    }
    /// <summary>
    /// 更新设备线程,切换为冗余通道
    /// </summary>
    public async Task UpDeviceRedundantThreadAsync(long devId)
    {
        try
        {
            //重启操作在未完全之前直接取消
            if (singleRestartLock.IsWaitting)
            {
                return;
            }
            await singleRestartLock.WaitAsync();

            if (!_stoppingToken.IsCancellationRequested)
            {
                var devThread = CollectDeviceThreads.FirstOrDefault(it => it.CollectDeviceCores.Any(a => a.DeviceId == devId));
                var devCore = devThread.CollectDeviceCores.FirstOrDefault(a => a.DeviceId == devId);
                if (devThread == null) { throw Oops.Bah($"更新设备线程失败，不存在{devId}为id的设备"); }
                //这里先停止采集，操作会使线程取消，需要重新恢复线程
                await devThread.StopThreadAsync();

                var dev = devCore.Device;

                if (dev.IsRedundant)
                {
                    if (dev.Redundant == RedundantEnum.Standby)
                    {
                        var newDev = (await _collectDeviceService.GetCollectDeviceRuntimeAsync(devId)).FirstOrDefault();
                        if (dev == null)
                        {
                            _logger.LogError($"更新设备线程失败，不存在{devId}为id的设备");
                        }
                        else
                        {
                            dev.DevicePropertys = newDev.DevicePropertys;
                            dev.Redundant = RedundantEnum.Primary;
                            _logger?.LogInformation(dev.Name + "切换到主通道");
                        }

                    }
                    else
                    {
                        try
                        {
                            var Redundantdev = (await _collectDeviceService.GetCollectDeviceRuntimeAsync(dev.RedundantDeviceId)).FirstOrDefault();
                            if (Redundantdev == null)
                            {
                                _logger.LogError($"更新设备线程失败，不存在{devId}为id的设备");
                            }
                            else
                            {
                                dev.DevicePropertys = Redundantdev.DevicePropertys;
                                dev.Redundant = RedundantEnum.Standby;
                                _logger?.LogInformation(dev.Name + "切换到备用通道");
                            }

                        }
                        catch
                        {
                        }
                    }
                }

                //初始化
                devCore.Init(dev);
                //线程管理器移除后，如果不存在其他设备，也删除线程管理器
                devThread.CollectDeviceCores.Remove(devCore);
                if (devThread.CollectDeviceCores.Count == 0)
                {
                    CollectDeviceThreads.Remove(devThread);
                }
                //需判断是否同一通道
                var newDevThread = DeviceThread(devCore);
                await newDevThread.StartThreadAsync();

            }
        }
        finally
        {
            singleRestartLock.Release();
        }
    }

    /// <summary>
    /// 更新设备线程
    /// </summary>
    public async Task UpDeviceThreadAsync(long devId, bool isUpdateDb = true)
    {
        try
        {
            //重启操作在未完全之前直接取消
            if (singleRestartLock.IsWaitting)
            {
                return;
            }

            await singleRestartLock.WaitAsync();
            if (!_stoppingToken.IsCancellationRequested)
            {
                //如果是组态更改过了，需要重新获取变量/设备运行态的值，其他服务需要先停止
                if (isUpdateDb)
                    await StopOtherHostService();
                var devThread = CollectDeviceThreads.FirstOrDefault(it => it.CollectDeviceCores.Any(a => a.DeviceId == devId));
                var devCore = devThread.CollectDeviceCores.FirstOrDefault(a => a.DeviceId == devId);
                if (devThread == null) { throw Oops.Bah($"更新设备线程失败，不存在{devId}为id的设备"); }
                //这里先停止采集，操作会使线程取消，需要重新恢复线程
                await devThread.StopThreadAsync();

                CollectDeviceRunTime dev = isUpdateDb ? (await _collectDeviceService.GetCollectDeviceRuntimeAsync(devId)).FirstOrDefault() : devCore.Device;

                if (dev == null)
                {
                    //线程管理器移除后，如果不存在其他设备，也删除线程管理器
                    devThread.CollectDeviceCores.Remove(devCore);
                    if (devThread.CollectDeviceCores.Count == 0)
                    {
                        CollectDeviceThreads.Remove(devThread);
                    }
                }
                else
                {
                    //初始化
                    devCore.Init(dev);
                    //线程管理器移除后，如果不存在其他设备，也删除线程管理器
                    devThread.CollectDeviceCores.Remove(devCore);

                    if (devThread.CollectDeviceCores.Count == 0)
                    {
                        CollectDeviceThreads.Remove(devThread);
                    }

                    //需判断是否同一通道
                    var newDevThread = DeviceThread(devCore);
                    await newDevThread.StartThreadAsync();

                    //如果是组态更改过了，需要重新获取变量/设备运行态的值
                    if (isUpdateDb)
                        await StartOtherHostService();
                }

            }
        }
        finally
        {
            singleRestartLock.Release();
        }
    }
    #endregion

    #region Private
    /// <summary>
    /// 创建设备采集线程
    /// </summary>
    /// <returns></returns>
    private async Task CreatAllDeviceThreadsAsync()
    {
        if (!_stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("正在获取采集组态信息");
            var collectDeviceRunTimes = (await _collectDeviceService.GetCollectDeviceRuntimeAsync());
            _logger.LogInformation("获取采集组态信息完成");
            foreach (var collectDeviceRunTime in collectDeviceRunTimes.Where(a => !collectDeviceRunTimes.Any(b => a.Id == b.RedundantDeviceId && b.IsRedundant)))
            {
                if (!_stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        CollectDeviceCore deviceCollectCore = new();
                        deviceCollectCore.Init(collectDeviceRunTime);
                        DeviceThread(deviceCollectCore);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, collectDeviceRunTime.Name);
                    }
                }
            }
        }

    }
    /// <summary>
    /// 根据通道生成/获取线程管理器
    /// </summary>
    /// <param name="deviceCollectCore"></param>
    /// <returns></returns>
    private CollectDeviceThread DeviceThread(CollectDeviceCore deviceCollectCore)
    {
        if (deviceCollectCore.Driver == null)
            return null;
        var changelID = deviceCollectCore.Driver.GetChannelID();
        if (changelID.IsSuccess)
        {
            foreach (var collectDeviceThread in CollectDeviceThreads)
            {
                if (collectDeviceThread.ChangelID == changelID.Content)
                {
                    collectDeviceThread.CollectDeviceCores.Add(deviceCollectCore);
                    return collectDeviceThread;
                }
            }
        }
        return NewDeviceThread(deviceCollectCore, changelID.Content);

        CollectDeviceThread NewDeviceThread(CollectDeviceCore deviceCollectCore, string changelID)
        {
            CollectDeviceThread deviceThread = new(changelID);
            deviceThread.CollectDeviceCores.Add(deviceCollectCore);
            CollectDeviceThreads.Add(deviceThread);
            return deviceThread;
        }
    }

    /// <summary>
    /// 删除设备线程，并且释放资源
    /// </summary>
    private async Task RemoveAllDeviceThreadAsync()
    {
        await CollectDeviceThreads.ParallelForEachAsync(async (deviceThread, token) =>
        {
            try
            {
                await deviceThread.BeforeStopThreadAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, deviceThread.ToString());
            }
        }, 10);

        await CollectDeviceThreads.ParallelForEachAsync(async (deviceThread, token) =>
         {
             try
             {
                 await deviceThread.DisposeAsync();
             }
             catch (Exception ex)
             {
                 _logger?.LogError(ex, deviceThread.ToString());
             }
         }, 10);
        CollectDeviceThreads.Clear();
    }

    /// <summary>
    /// 开始设备采集线程
    /// </summary>
    /// <returns></returns>
    private async Task StartAllDeviceThreadsAsync()
    {
        if (!_stoppingToken.IsCancellationRequested)
        {
            foreach (var item in CollectDeviceThreads)
            {
                if (!_stoppingToken.IsCancellationRequested)
                {
                    await item.StartThreadAsync();
                }
            }
        }
    }

    /// <summary>
    /// 启动其他后台服务
    /// </summary>
    private async Task StartOtherHostService()
    {
        var alarmHostService = ServiceHelper.GetBackgroundService<AlarmWorker>();
        var historyValueService = ServiceHelper.GetBackgroundService<HistoryValueWorker>();
        var uploadDeviceHostService = ServiceHelper.GetBackgroundService<UploadDeviceWorker>();
        var memoryVariableWorker = ServiceHelper.GetBackgroundService<MemoryVariableWorker>();
        await alarmHostService.StartAsync();
        await historyValueService.StartAsync();
        await uploadDeviceHostService.StartAsync();
        await memoryVariableWorker.StartAsync();
    }
    /// <summary>
    /// 停止其他后台服务
    /// </summary>
    private async Task StopOtherHostService()
    {
        var alarmHostService = ServiceHelper.GetBackgroundService<AlarmWorker>();
        var historyValueService = ServiceHelper.GetBackgroundService<HistoryValueWorker>();
        var uploadDeviceHostService = ServiceHelper.GetBackgroundService<UploadDeviceWorker>();
        var memoryVariableWorker = ServiceHelper.GetBackgroundService<MemoryVariableWorker>();
        await alarmHostService.StopAsync();
        await historyValueService.StopAsync();
        await uploadDeviceHostService.StopAsync();
        await memoryVariableWorker.StopAsync();

    }
    #endregion

    #region 设备信息获取
    /// <summary>
    /// GetDebugUI
    /// </summary>
    /// <param name="driverId"></param>
    /// <returns></returns>
    public Type GetDebugUI(long driverId)
    {
        var driverPluginService = App.GetService<IDriverPluginService>();
        var driverPlugin = driverPluginService.GetDriverPluginById(driverId);
        var driver = _pluginService.GetDriver(driverPlugin);
        driver?.SafeDispose();
        return driver.DriverDebugUIType;
    }

    /// <summary>
    /// 获取设备方法
    /// </summary>
    /// <param name="devId"></param>
    /// <returns></returns>
    public List<string> GetDeviceMethods(long devId)
    {
        var driverPluginService = App.GetService<IDriverPluginService>();
        var driverId = _collectDeviceService.GetDeviceById(devId).PluginId;
        var driverPlugin = driverPluginService.GetDriverPluginById(driverId);
        var driver = (CollectBase)_pluginService.GetDriver(driverPlugin);
        var Propertys = _pluginService.GetMethod(driver);
        driver?.SafeDispose();
        return Propertys.Select(it => it.Name).ToList();
    }

    /// <summary>
    /// 获取设备属性
    /// </summary>
    /// <param name="driverId"></param>
    /// <param name="devId"></param>
    /// <returns></returns>
    public List<DependencyProperty> GetDevicePropertys(long driverId, long devId = 0)
    {
        var driverPluginService = App.GetService<IDriverPluginService>();
        var driverPlugin = driverPluginService.GetDriverPluginById(driverId);
        var driver = _pluginService.GetDriver(driverPlugin);
        var Propertys = _pluginService.GetDriverProperties(driver);
        if (devId != 0)
        {
            var devcore = App.GetService<CollectDeviceService>().GetDeviceById(devId);
            devcore?.DevicePropertys?.ForEach(it =>
            {
                var dependencyProperty = Propertys.FirstOrDefault(a => a.PropertyName == it.PropertyName);
                if (dependencyProperty != null)
                {
                    dependencyProperty.Value = it.Value;
                }
            });
        }
        driver?.SafeDispose();
        return Propertys;

    }
    #endregion

    #region worker服务
    /// <summary>
    /// 在软件关闭时取消
    /// </summary>
    private CancellationToken _stoppingToken;

    /// <inheritdoc/>
    public override async Task StartAsync(CancellationToken token)
    {
        var hardwareInfoService = ServiceHelper.Services.GetService<HardwareInfoService>();
        hardwareInfoService.Init();
        await base.StartAsync(token);
    }

    /// <inheritdoc/>
    public override async Task StopAsync(CancellationToken token)
    {
        using var stoppingToken = new CancellationTokenSource();
        _stoppingToken = stoppingToken.Token;
        stoppingToken.Cancel();
        //停止其他后台服务
        await StopOtherHostService();
        //停止全部采集线程
        await RemoveAllDeviceThreadAsync();
        await base.StopAsync(token);
    }
    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //重启采集线程，会启动其他后台服务
        await RestartDeviceThreadAsync();
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                //检测设备采集线程假死
                var num = CollectDeviceCores.Count;
                for (int i = 0; i < num; i++)
                {
                    CollectDeviceCore devcore = CollectDeviceCores[i];
                    if (devcore.Device != null)
                    {
                        //超过30分钟，或者(初始化失败并超过10分钟)会重启
                        if (
        (devcore.Device.ActiveTime != DateTime.MinValue &&
        devcore.Device.ActiveTime.AddMinutes(30) <= SysDateTimeExtensions.CurrentDateTime)
        || (devcore.IsInitSuccess == false && devcore.Device.ActiveTime.AddMinutes(10) <= SysDateTimeExtensions.CurrentDateTime)
        )
                        {
                            //如果线程处于暂停状态，跳过
                            if (devcore.Device.DeviceStatus == DeviceStatusEnum.Pause)
                                continue;
                            //如果初始化失败
                            if (!devcore.IsInitSuccess)
                                _logger?.LogWarning(devcore.Device.Name + "初始化失败，重启线程中");
                            else
                                _logger?.LogWarning(devcore.Device.Name + "采集线程假死，重启线程中");
                            //重启线程
                            await UpDeviceThreadAsync(devcore.Device.Id, false);
                            break;
                        }
                        else
                        {
                            _logger?.LogTrace(devcore.Device.Name + "线程检测正常");
                        }


                        if (devcore.Device.DeviceStatus == DeviceStatusEnum.OffLine)
                        {
                            if (devcore.Device.IsRedundant && _collectDeviceService.GetCacheList(false).Any(a => a.Id == devcore.Device.RedundantDeviceId))
                            {
                                await UpDeviceRedundantThreadAsync(devcore.Device.Id);
                            }
                        }

                    }
                }
                //每5分钟检测一次
                await Task.Delay(300000, stoppingToken);
            }
            catch (TaskCanceledException)
            {

            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ToString());
            }

        }
    }

    #endregion



}

