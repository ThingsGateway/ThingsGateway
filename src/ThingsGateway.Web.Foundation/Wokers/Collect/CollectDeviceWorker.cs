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
    private static IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CollectDeviceWorker> _logger;
    private GlobalCollectDeviceData _globalCollectDeviceData;
    private PluginSingletonService _pluginService;
    /// <inheritdoc/>
    public CollectDeviceWorker(ILogger<CollectDeviceWorker> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        ThreadPool.SetMaxThreads(100000, 100000);
        using var serviceScope = scopeFactory.CreateScope();
        _globalCollectDeviceData = serviceScope.ServiceProvider.GetService<GlobalCollectDeviceData>();
        _pluginService = serviceScope.ServiceProvider.GetService<PluginSingletonService>();
        serviceScope.ServiceProvider.GetService<HardwareInfoService>();
        _collectDeviceService = serviceScope.ServiceProvider.GetService<ICollectDeviceService>();
    }
    /// <summary>
    /// 设备子线程列表
    /// </summary>
    public ConcurrentList<CollectDeviceCore> CollectDeviceCores { get; private set; } = new();
    ICollectDeviceService _collectDeviceService { get; set; }
    #region 设备创建更新结束

    /// <summary>
    /// 控制设备线程启停
    /// </summary>
    public async Task ConfigDeviceThreadAsync(long deviceId, bool isStart)
    {
        try
        {
            await easyLock.LockAsync();
            if (deviceId == 0)
                CollectDeviceCores.ForEach(it => it.PasueThread(isStart));
            else
                CollectDeviceCores.FirstOrDefault(it => it.DeviceId == deviceId)?.PasueThread(isStart);
        }
        finally
        {
            easyLock.UnLock();
        }

    }

    /// <summary>
    /// 删除设备线程，并且释放资源
    /// </summary>
    private void RemoveDeviceThread(long devId)
    {
        var deviceThread = CollectDeviceCores.FirstOrDefault(x => x.DeviceId == devId);

        if (deviceThread != null)
        {
            deviceThread.Dispose();
            CollectDeviceCores.Remove(deviceThread);
        }
    }

    private EasyLock easyLock = new();
    /// <summary>
    /// 重启采集服务
    /// </summary>
    public async Task RestartDeviceThreadAsync()
    {
        try
        {
            Task result = await Task.Factory.StartNew(async () =>
        {
            try
            {
                await easyLock.LockAsync();

                StopOtherHostService(CollectDeviceCores.Select(a => a.Device)?.ToList());

                var dev = CollectDeviceCores.Select(it => it.Device).ToList();
                foreach (var device in dev)
                {
                    try
                    {
                        if (!_stoppingToken.IsCancellationRequested)
                            RemoveDeviceThread(device.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, device.Name);
                    }
                }
                if (!_stoppingToken.IsCancellationRequested)
                {
                    var devs = (await _collectDeviceService.GetCollectDeviceRuntimeAsync());
                    foreach (var item in devs)
                    {
                        if (!_stoppingToken.IsCancellationRequested)
                        {
                            CollectDeviceCore deviceCollectCore = new(_scopeFactory);
                            deviceCollectCore.Init(item);
                            deviceCollectCore.StartThread();
                            CollectDeviceCores.Add(deviceCollectCore);
                        }
                    }

                }



                StartOtherHostService();
                GC.Collect();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(RestartDeviceThreadAsync));
            }
            finally
            {
                easyLock.UnLock();
            }
        }, _stoppingToken);

            await result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, nameof(RestartDeviceThreadAsync));
        }
    }
    /// <summary>
    /// 启动其他后台服务
    /// </summary>
    private void StartOtherHostService()
    {
        using var scope = _scopeFactory.CreateScope();
        var alarmHostService = scope.GetBackgroundService<AlarmWorker>();
        var valueHisHostService = scope.GetBackgroundService<ValueHisWorker>();
        alarmHostService?.Start();
        valueHisHostService?.Start();
        var uploadDeviceHostService = scope.GetBackgroundService<UploadDeviceWorker>();
        uploadDeviceHostService.StartDeviceThread();
    }
    /// <summary>
    /// 停止其他后台服务
    /// </summary>
    private void StopOtherHostService(List<CollectDeviceRunTime> oldDeviceRuntime)
    {
        if (oldDeviceRuntime?.Count > 0)
        {
            using var scope = _scopeFactory.CreateScope();


            var alarmHostService = scope.GetBackgroundService<AlarmWorker>();
            var valueHisHostService = scope.GetBackgroundService<ValueHisWorker>();
            alarmHostService?.Stop(oldDeviceRuntime);
            valueHisHostService?.Stop(oldDeviceRuntime);
            var uploadDeviceHostService = scope.GetBackgroundService<UploadDeviceWorker>();
            uploadDeviceHostService.RemoveDeviceThread();

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
                    StopOtherHostService(CollectDeviceCores.Select(a => a.Device).ToList());

                var devcore = CollectDeviceCores.FirstOrDefault(it => it?.DeviceId == devId);
                if (devcore == null) { throw Oops.Bah($"更新设备线程失败，不存在{devId}为id的设备"); }
                //这里先停止采集，操作会使线程取消，需要重新恢复线程
                devcore.StopThread();

                CollectDeviceRunTime dev = null;
                if (isUpdateDb)
                    dev = (await _collectDeviceService.GetCollectDeviceRuntimeAsync(devId)).FirstOrDefault();
                else
                    dev = devcore.Device;
                if (dev == null) { _logger.LogError($"更新设备线程失败，不存在{devId}为id的设备"); }
                devcore.Init(dev);
                devcore.StartThread();

                if (isUpdateDb)
                    StartOtherHostService();


            }
        }
        finally
        {
            easyLock.UnLock();
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
            driver?.Dispose();
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
            var driver = (CollectBase)_pluginService.GetDriver(id, driverPlugin);
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
            driver?.Dispose();
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
            return result._driver;
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
        var stoppingToken = new CancellationTokenSource();
        _stoppingToken = stoppingToken.Token;
        stoppingToken.Cancel();

        var dev = CollectDeviceCores.Select(it => it.Device).ToList();
        StopOtherHostService(dev);
        foreach (var device in dev)
        {
            try
            {
                RemoveDeviceThread(device.Id);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, device.Name);
            }
        }
        await Task.Delay(2000);
        await base.StopAsync(cancellationToken);
    }
    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RestartDeviceThreadAsync();
        while (!stoppingToken.IsCancellationRequested)
        {
            //这里不采用CancellationToken控制子线程，直接循环保持，结束时调用子设备线程Dispose
            //检测设备采集线程假死
            var num = CollectDeviceCores.Count;
            for (int i = 0; i < num; i++)
            {
                CollectDeviceCore devcore = CollectDeviceCores[i];
                if (
                    (devcore.Device.ActiveTime != DateTime.MinValue && devcore.Device.ActiveTime.AddMinutes(3) <= DateTime.UtcNow)
                    || devcore.isInitSuccess == false
                    )
                {
                    if (devcore.StoppingTokens.Last().Token.IsCancellationRequested)
                        continue;
                    if (devcore.Device.DeviceStatus == DeviceStatusEnum.Pause)
                        continue;
                    if (!devcore.isInitSuccess)
                        _logger?.LogWarning(devcore.Device.Name + "初始化失败，重启线程中");
                    else
                        _logger?.LogWarning(devcore.Device.Name + "采集线程假死，重启线程中");
                    await UpDeviceThreadAsync(devcore.DeviceId, false);

                    GC.Collect();

                }
                else
                {
                    _logger?.LogDebug(devcore.Device.Name + "检测正常");
                }
            }



            await Task.Delay(100000, stoppingToken);
        }
    }

    #endregion



}

