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

using Furion.FriendlyException;
using Furion.Logging.Extensions;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Linq;
using System.Threading;

using ThingsGateway.Foundation;

using TouchSocket.Core;


namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 设备采集后台服务
/// </summary>
public class CollectDeviceWorker : BackgroundService
{
    private IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CollectDeviceWorker> _logger;
    private ICollectDeviceService _collectDeviceService;
    private GlobalDeviceData _globalDeviceData;
    private PluginSingletonService _pluginService;
    /// <inheritdoc/>
    public CollectDeviceWorker(ILogger<CollectDeviceWorker> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        var serviceScope = scopeFactory.CreateScope();
        _globalDeviceData = serviceScope.ServiceProvider.GetService<GlobalDeviceData>();
        _pluginService = serviceScope.ServiceProvider.GetService<PluginSingletonService>();
        serviceScope.ServiceProvider.GetService<HardwareInfoService>();
        _collectDeviceService = serviceScope.ServiceProvider.GetService<ICollectDeviceService>();
    }
    /// <summary>
    /// 采集设备List
    /// </summary>
    public List<CollectDeviceCore> CollectDeviceCores => CollectDeviceThreads.SelectMany(a => a.CollectDeviceCores).ToList();


    /// <summary>
    /// 设备子线程列表
    /// </summary>
    private ConcurrentList<CollectDeviceThread> CollectDeviceThreads { get; set; } = new();
    #region 设备创建更新结束

    private EasyLock easyLock = new();
    /// <summary>
    /// 控制设备线程启停
    /// </summary>
    public async Task ConfigDeviceThreadAsync(long deviceId, bool isStart)
    {
        try
        {
            await easyLock.LockAsync();
            if (deviceId == 0)
                CollectDeviceCores.ForEach(a => a.PasueThread(isStart));
            else
                CollectDeviceCores.FirstOrDefault(it => it.DeviceId == deviceId)?.PasueThread(isStart);
        }
        finally
        {
            easyLock.UnLock();
        }
    }
    /// <summary>
    /// 重启采集服务
    /// </summary>
    public async Task RestartDeviceThreadAsync()
    {
        try
        {
            await easyLock.LockAsync();
            StopOtherHostService();
            RemoveAllDeviceThread();
            await CreatAllDeviceThreadsAsync();
            StartAllDeviceThreads();
            StartOtherHostService();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, nameof(RestartDeviceThreadAsync));
        }
        finally
        {
            easyLock.UnLock();
        }
    }
    /// <summary>
    /// 更新设备线程
    /// </summary>
    public async Task UpDeviceThreadAsync(long devId, bool isUpdateDb = true)
    {
        try
        {
            await easyLock.LockAsync();
            if (!_stoppingToken.IsCancellationRequested)
            {
                if (isUpdateDb)
                    StopOtherHostService();
                var devThread = CollectDeviceThreads.FirstOrDefault(it => it.CollectDeviceCores.Any(a => a.DeviceId == devId));
                var devCore = devThread.CollectDeviceCores.FirstOrDefault(a => a.DeviceId == devId);
                if (devThread == null) { throw Oops.Bah($"更新设备线程失败，不存在{devId}为id的设备"); }
                //这里先停止采集，操作会使线程取消，需要重新恢复线程
                devThread.StopThread();

                CollectDeviceRunTime dev = null;
                if (isUpdateDb)
                    dev = (await _collectDeviceService.GetCollectDeviceRuntimeAsync(devId)).FirstOrDefault();
                else
                    dev = devCore.Device;
                if (dev == null) { _logger.LogError($"更新设备线程失败，不存在{devId}为id的设备"); }
                devCore.Init(dev);
                devThread.CollectDeviceCores.Remove(devCore);
                if (devThread.CollectDeviceCores.Count == 0)
                {
                    CollectDeviceThreads.Remove(devThread);
                }
                //需判断是否同一通道
                var newDevThread = DeviceThread(devCore);
                newDevThread?.StartThread();
                if (isUpdateDb)
                    StartOtherHostService();
            }
        }
        finally
        {
            easyLock.UnLock();
        }
    }

    /// <summary>
    /// 更新设备线程,切换为冗余通道
    /// </summary>
    public async Task UpDeviceRedundantThreadAsync(long devId)
    {
        try
        {
            await easyLock.LockAsync();
            if (!_stoppingToken.IsCancellationRequested)
            {
                var devThread = CollectDeviceThreads.FirstOrDefault(it => it.CollectDeviceCores.Any(a => a.DeviceId == devId));
                var devCore = devThread.CollectDeviceCores.FirstOrDefault(a => a.DeviceId == devId);
                if (devThread == null) { throw Oops.Bah($"更新设备线程失败，不存在{devId}为id的设备"); }
                //这里先停止采集，操作会使线程取消，需要重新恢复线程
                devThread.StopThread();

                var dev = devCore.Device;
                if (dev.IsRedundant)
                {
                    if (dev.RedundantEnum == RedundantEnum.Standby)
                    {
                        var newDev = (await _collectDeviceService.GetCollectDeviceRuntimeAsync(devId)).FirstOrDefault();
                        if (dev == null) { _logger.LogError($"更新设备线程失败，不存在{devId}为id的设备"); }
                        dev.DevicePropertys = newDev.DevicePropertys;
                        dev.RedundantEnum = RedundantEnum.Primary;
                        _logger?.LogInformation(dev.Name + "切换到主通道");
                    }
                    else
                    {
                        try
                        {
                            var Redundantdev = (await _collectDeviceService.GetCollectDeviceRuntimeAsync(dev.RedundantDeviceId)).FirstOrDefault();
                            dev.DevicePropertys = Redundantdev.DevicePropertys;
                            dev.RedundantEnum = RedundantEnum.Standby;
                            _logger?.LogInformation(dev.Name + "切换到备用通道");
                        }
                        catch
                        {
                        }
                    }
                }
                devCore.Init(dev);
                devThread.CollectDeviceCores.Remove(devCore);
                if (devThread.CollectDeviceCores.Count == 0)
                {
                    CollectDeviceThreads.Remove(devThread);
                }
                //需判断是否同一通道
                var newDevThread = DeviceThread(devCore);
                newDevThread?.StartThread();
            }
        }
        finally
        {
            easyLock.UnLock();
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
                        CollectDeviceCore deviceCollectCore = new(_scopeFactory);
                        deviceCollectCore.Init(collectDeviceRunTime);

                        DeviceThread(deviceCollectCore);
                        await Task.Delay(10);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, collectDeviceRunTime.Name);

                    }


                }
            }

        }


    }

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
            CollectDeviceThread deviceThread = new(_scopeFactory, changelID);
            deviceThread.CollectDeviceCores.Add(deviceCollectCore);
            CollectDeviceThreads.Add(deviceThread);
            return deviceThread;
        }
    }

    /// <summary>
    /// 删除设备线程，并且释放资源
    /// </summary>
    private void RemoveAllDeviceThread()
    {
        CollectDeviceThreads.ParallelForEach(deviceThread =>
        {
            try
            {
                deviceThread.SafeDispose();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, deviceThread.ToString());
            }
        }, 50);
        CollectDeviceThreads.Clear();
    }

    /// <summary>
    /// 开始设备采集线程
    /// </summary>
    /// <returns></returns>
    private void StartAllDeviceThreads()
    {
        if (!_stoppingToken.IsCancellationRequested)
        {
            foreach (var item in CollectDeviceThreads)
            {
                if (!_stoppingToken.IsCancellationRequested)
                {
                    item.StartThread();
                }
            }
        }
    }

    /// <summary>
    /// 启动其他后台服务
    /// </summary>
    private void StartOtherHostService()
    {
        var scope = _scopeFactory.CreateScope();
        var alarmHostService = scope.GetBackgroundService<AlarmWorker>();
        var historyValueService = scope.GetBackgroundService<HistoryValueWorker>();
        alarmHostService?.Start();
        historyValueService?.Start();
        var uploadDeviceHostService = scope.GetBackgroundService<UploadDeviceWorker>();
        uploadDeviceHostService.StartDeviceThread();
        var memoryVariableWorker = scope.GetBackgroundService<MemoryVariableWorker>();
        memoryVariableWorker.Start();
    }
    /// <summary>
    /// 停止其他后台服务
    /// </summary>
    private void StopOtherHostService()
    {
        if (_globalDeviceData.CollectDevices?.Count > 0)
        {
            var scope = _scopeFactory.CreateScope();
            var alarmHostService = scope.GetBackgroundService<AlarmWorker>();
            var historyValueService = scope.GetBackgroundService<HistoryValueWorker>();
            alarmHostService?.Stop(_globalDeviceData.CollectDevices);
            historyValueService?.Stop(_globalDeviceData.CollectDevices);
            var uploadDeviceHostService = scope.GetBackgroundService<UploadDeviceWorker>();
            uploadDeviceHostService.StopDeviceThread();
            var memoryVariableWorker = scope.GetBackgroundService<MemoryVariableWorker>();
            memoryVariableWorker.Stop();
        }


    }
    #endregion

    #region 设备信息获取
    /// <summary>
    /// 获取设备方法
    /// </summary>
    /// <param name="devId"></param>
    /// <returns></returns>
    public List<string> GetDeviceMethods(long devId)
    {
        var id = YitIdHelper.NextId();
        using var serviceScope = _scopeFactory.CreateScope();
        var driverPluginService = serviceScope.ServiceProvider.GetService<IDriverPluginService>();
        var driverId = _collectDeviceService.GetDeviceById(devId).PluginId;
        try
        {
            var driverPlugin = driverPluginService.GetDriverPluginById(driverId);
            var driver = (CollectBase)_pluginService.GetDriver(id, driverPlugin);
            var Propertys = _pluginService.GetMethod(driver);
            driver?.SafeDispose();
            return Propertys.Select(it => it.Name).ToList();
        }
        finally
        {
            _pluginService.DeleteDriver(id, driverId);
        }

    }

    /// <summary>
    /// 获取设备属性
    /// </summary>
    /// <param name="driverId"></param>
    /// <param name="devId"></param>
    /// <returns></returns>
    public List<DependencyProperty> GetDevicePropertys(long driverId, long devId = 0)
    {
        using var serviceScope = _scopeFactory.CreateScope();
        var driverPluginService = serviceScope.ServiceProvider.GetService<IDriverPluginService>();
        var driverPlugin = driverPluginService.GetDriverPluginById(driverId);
        var id = YitIdHelper.NextId();
        try
        {
            var driver = (DriverBase)_pluginService.GetDriver(id, driverPlugin);
            var Propertys = _pluginService.GetDriverProperties(driver);
            if (devId != 0)
            {
                var devcore = CollectDeviceCores.FirstOrDefault(it => it.Device.Id == devId);
                devcore?.Device?.DevicePropertys?.ForEach(it =>
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
        finally
        {

            _pluginService.DeleteDriver(id, driverId);
        }
    }

    /// <summary>
    /// 获取导入变量UI
    /// </summary>
    /// <param name="devId"></param>
    /// <returns></returns>
    public CollectBase GetImportUI(long devId)
    {
        var result = CollectDeviceCores.FirstOrDefault(a => a.DeviceId == devId);
        if (result == null)
        {
            return null;
        }
        else
        {
            return result.Driver;
        }

    }

    /// <summary>
    /// 获取导入变量UI
    /// </summary>
    /// <param name="driverId"></param>
    /// <returns></returns>
    public Type GetDebugUI(long driverId)
    {
        using var serviceScope = _scopeFactory.CreateScope();
        var driverPluginService = serviceScope.ServiceProvider.GetService<IDriverPluginService>();
        var driverPlugin = driverPluginService.GetDriverPluginById(driverId);
        var id = YitIdHelper.NextId();
        try
        {
            var driver = (DriverBase)_pluginService.GetDriver(id, driverPlugin);
            driver?.SafeDispose();
            return driver.DriverDebugUIType;
        }
        finally
        {
            _pluginService.DeleteDriver(id, driverId);
        }

    }

    #endregion

    #region worker服务
    private CancellationToken _stoppingToken;

    /// <inheritdoc/>
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
        await Task.Delay(2000);
        StopOtherHostService();
        RemoveAllDeviceThread();
        await base.StopAsync(cancellationToken);
    }
    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RestartDeviceThreadAsync();
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {

                //这里不采用CancellationToken控制子线程，直接循环保持，结束时调用子设备线程Dispose
                //检测设备采集线程假死
                var num = CollectDeviceCores.Count;
                for (int i = 0; i < num; i++)
                {
                    CollectDeviceCore devcore = CollectDeviceCores[i];
                    if (devcore.Device != null)
                    {

                        if (
        (devcore.Device.ActiveTime != DateTime.MinValue
        && devcore.Device.ActiveTime.AddMinutes(3) <= DateTime.UtcNow)
        || devcore.IsInitSuccess == false
        )
                        {
                            if (devcore.StoppingTokens.LastOrDefault()?.Token.IsCancellationRequested == true)
                                continue;
                            if (devcore.Device.DeviceStatus == DeviceStatusEnum.Pause)
                                continue;
                            if (!devcore.IsInitSuccess)
                                _logger?.LogWarning(devcore.Device.Name + "初始化失败，重启线程中");
                            else
                                _logger?.LogWarning(devcore.Device.Name + "采集线程假死，重启线程中");
                            await UpDeviceThreadAsync(devcore.DeviceId, false);
                            break;
                        }
                        else
                        {
                            _logger?.LogTrace(devcore.Device.Name + "线程检测正常");
                        }


                        if (devcore.Device.DeviceStatus == DeviceStatusEnum.OffLine)
                        {
                            if (devcore.Device.IsRedundant && _collectDeviceService.GetCacheList().Any(a => a.Id == devcore.Device.RedundantDeviceId))
                            {
                                await UpDeviceRedundantThreadAsync(devcore.Device.Id);
                            }
                        }

                    }

                }

                await Task.Delay(300000, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ToString());
            }

        }
    }

    #endregion



}

