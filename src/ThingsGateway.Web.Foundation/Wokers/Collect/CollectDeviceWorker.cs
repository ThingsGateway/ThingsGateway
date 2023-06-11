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
    private GlobalCollectDeviceData _globalCollectDeviceData;
    private PluginSingletonService _pluginService;
    /// <inheritdoc/>
    public CollectDeviceWorker(ILogger<CollectDeviceWorker> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        ThreadPool.SetMaxThreads(100000, 100000);
        var serviceScope = scopeFactory.CreateScope();
        _globalCollectDeviceData = serviceScope.ServiceProvider.GetService<GlobalCollectDeviceData>();
        _pluginService = serviceScope.ServiceProvider.GetService<PluginSingletonService>();
        serviceScope.ServiceProvider.GetService<HardwareInfoService>();
        _collectDeviceService = serviceScope.ServiceProvider.GetService<ICollectDeviceService>();
    }
    /// <summary>
    /// 采集设备List
    /// </summary>
    public List<CollectDeviceCore> CollectDeviceCores => CollectDeviceThreads.SelectMany(a => a.CollectDeviceCores).ToList();

    /// <summary>
    /// 采集设备List
    /// </summary>
    public List<CollectDeviceRunTime> CollectDeviceRunTimes => CollectDeviceThreads.SelectMany(a => a.CollectDeviceCores.Select(b => b.Device))?.ToList();
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
            var collectDeviceRunTimes = (await _collectDeviceService.GetCollectDeviceRuntimeAsync());
            foreach (var collectDeviceRunTime in collectDeviceRunTimes)
            {
                if (!_stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        CollectDeviceCore deviceCollectCore = new(_scopeFactory);
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

    private CollectDeviceThread DeviceThread(CollectDeviceCore deviceCollectCore)
    {
        if (deviceCollectCore.Driver == null)
            return null;
        var changelID = deviceCollectCore.Driver.ChannelID();
        if (changelID != null)
        {
            foreach (var collectDeviceThread in CollectDeviceThreads)
            {
                if (collectDeviceThread.ChangelID == changelID)
                {
                    collectDeviceThread.CollectDeviceCores.Add(deviceCollectCore);
                    return collectDeviceThread;
                }
            }
        }
        return NewDeviceThread(deviceCollectCore, changelID);

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
        foreach (var deviceThread in CollectDeviceThreads)
        {
            try
            {
                deviceThread.SafeDispose();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, deviceThread.ToString());
            }
        }
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
    private void StopOtherHostService()
    {
        if (CollectDeviceRunTimes?.Count > 0)
        {
            using var scope = _scopeFactory.CreateScope();

            var alarmHostService = scope.GetBackgroundService<AlarmWorker>();
            var valueHisHostService = scope.GetBackgroundService<ValueHisWorker>();
            alarmHostService?.Stop(CollectDeviceRunTimes);
            valueHisHostService?.Stop(CollectDeviceRunTimes);
            var uploadDeviceHostService = scope.GetBackgroundService<UploadDeviceWorker>();
            uploadDeviceHostService.StopDeviceThread();

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
        var stoppingToken = new CancellationTokenSource();
        _stoppingToken = stoppingToken.Token;
        stoppingToken.Cancel();
        stoppingToken?.SafeDispose();
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
            //这里不采用CancellationToken控制子线程，直接循环保持，结束时调用子设备线程Dispose
            //检测设备采集线程假死
            var num = CollectDeviceCores.Count;
            for (int i = 0; i < num; i++)
            {
                CollectDeviceCore devcore = CollectDeviceCores[i];
                if (devcore.Device != null)
                {
                    if (
    (devcore.Device.ActiveTime != DateTime.MinValue && devcore.Device.ActiveTime.AddMinutes(3) <= DateTime.UtcNow)
    || devcore.IsInitSuccess == false
    )
                    {
                        if (devcore.StoppingTokens.Last().Token.IsCancellationRequested)
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
                }

            }

            await Task.Delay(100000, stoppingToken);
        }
    }

    #endregion



}

