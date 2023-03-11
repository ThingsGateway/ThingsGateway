using Furion.FriendlyException;
using Furion.Logging.Extensions;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Linq;
using System.Threading;

using TouchSocket.Core;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 设备采集后台服务
/// </summary>
public class CollectDeviceHostService : BackgroundService
{
    private readonly ILogger<CollectDeviceHostService> _logger;
    private GlobalCollectDeviceData _globalCollectDeviceData;
    private PluginCore _pluginService;
    public ConcurrentList<CollectDeviceCore> CollectDeviceCores { get; private set; } = new();
    ICollectDeviceService _collectDeviceService { get; set; }
    private static IServiceScopeFactory _scopeFactory;
    public CollectDeviceHostService(ILogger<CollectDeviceHostService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;

        using var serviceScope = scopeFactory.CreateScope();
        _globalCollectDeviceData = serviceScope.ServiceProvider.GetService<GlobalCollectDeviceData>();
        _pluginService = serviceScope.ServiceProvider.GetService<PluginCore>();
        _collectDeviceService = serviceScope.ServiceProvider.GetService<ICollectDeviceService>();
    }

    #region 设备创建更新结束

    /// <summary>
    /// 更新设备线程
    /// </summary>
    public async Task UpDeviceThread(long devId, bool isUpdateDb = true)
    {
        if (!_stoppingToken.IsCancellationRequested)
        {
            StopOtherHostService(CollectDeviceCores.Select(a => a.Device).ToList());

            var devcore = CollectDeviceCores.FirstOrDefault(it => it?.DeviceId == devId);
            if (devcore == null) { throw Oops.Bah($"更新设备线程失败，不存在{devId}为id的设备"); }
            //这里先停止采集，操作会使线程取消，需要重新恢复线程
            devcore.StopThread();

            CollectDeviceRunTime dev = null;
            if (isUpdateDb)
                dev = (await _collectDeviceService.GetCollectDeviceRuntime(devId)).FirstOrDefault();
            else
                dev = devcore.Device;
            if (dev == null) { _logger.LogError($"更新设备线程失败，不存在{devId}为id的设备"); }
            devcore.Init(dev);
            devcore.StartThread();

            StartOtherHostService();


        }
    }

    /// <summary>
    /// 删除设备线程，并且释放资源
    /// </summary>
    /// <param name="devices"></param>
    public void RemoveDeviceThread(long devId)
    {
        var deviceThread = CollectDeviceCores.FirstOrDefault(x => x.DeviceId == devId);

        if (deviceThread != null)
        {
            deviceThread.Dispose();
            CollectDeviceCores.Remove(deviceThread);
        }
    }

    public async Task RestartDeviceThread()
    {
        try
        {
            Task result = await Task.Factory.StartNew(async () =>
        {
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
                var devs = (await _collectDeviceService.GetCollectDeviceRuntime());
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
        }, _stoppingToken);

            await result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, nameof(RestartDeviceThread));
        }
    }

    public void StopOtherHostService(List<CollectDeviceRunTime> oldDeviceRuntime)
    {
        if (oldDeviceRuntime?.Count > 0)
        {
            oldDeviceRuntime.ForEach(a => a.DeviceVariableRunTimes.ForEach(b => b.VariableCollectChange -= VariableCollectChange));
            oldDeviceRuntime.ForEach(a => a.DeviceVariableRunTimes.ForEach(b => b.VariableValueChange -= VariableValueChange));


            var alarmHostService = _scopeFactory.GetBackgroundService<AlarmHostService>();
            var valueHisHostService = _scopeFactory.GetBackgroundService<ValueHisHostService>();
            alarmHostService.Stop(oldDeviceRuntime);
            valueHisHostService.Stop(oldDeviceRuntime);
            var uploadDeviceHostService = _scopeFactory.GetBackgroundService<UploadDeviceHostService>();
            uploadDeviceHostService.RemoveDeviceThread();

        }


    }

    private void VariableValueChange(CollectVariableRunTime collectVariableRunTime)
    {
        if (VariableValueChanges != null)
            CollectVariableRunTimeChangeds.Enqueue(collectVariableRunTime);
    }

    private void VariableCollectChange(CollectVariableRunTime collectVariableRunTime)
    {
        if (VariableCollectChanges != null)
            CollectVariableRunTimeCollects.Enqueue(collectVariableRunTime);
    }

    public void StartOtherHostService()
    {
        CollectDeviceCores.ForEach(a => a.Device.DeviceVariableRunTimes.ForEach(b => b.VariableCollectChange += VariableCollectChange));
        CollectDeviceCores.ForEach(a => a.Device.DeviceVariableRunTimes.ForEach(b => b.VariableValueChange += VariableValueChange));

        var alarmHostService = _scopeFactory.GetBackgroundService<AlarmHostService>();
        var valueHisHostService = _scopeFactory.GetBackgroundService<ValueHisHostService>();
        alarmHostService.Start();
        valueHisHostService.Start();
        var uploadDeviceHostService = _scopeFactory.GetBackgroundService<UploadDeviceHostService>();
        uploadDeviceHostService.StartDeviceThread();
    }

    /// <summary>
    /// 控制设备线程启停
    /// </summary>
    public void ConfigDeviceThread(long deviceId, bool isStart)
    {
        if (deviceId == 0)
            CollectDeviceCores.ForEach(it => it.PasueThread(isStart));
        else
            CollectDeviceCores.FirstOrDefault(it => it.DeviceId == deviceId)?.PasueThread(isStart);
    }

    #endregion

    #region 设备信息获取

    public List<string> GetDeviceMethods(long devId)
    {
        var id = YitIdHelper.NextId();
        using var serviceScope = _scopeFactory.CreateScope();
        var driverPluginService = serviceScope.ServiceProvider.GetService<IDriverPluginService>();
        var driverId = _collectDeviceService.GetDeviceById(devId).PluginId;
        try
        {
            var driverPlugin = driverPluginService.GetDriverPluginById(driverId);
            var driver = (DriverBase)_pluginService.AddDriver(id, driverPlugin);
            var Propertys = _pluginService.GetMethod(driver);
            return Propertys.Select(it => it.Name).ToList();
        }
        finally
        {
            _pluginService.DeleteDriver(id, driverId);
        }

    }

    public DriverBase GetImportUI(long devId)
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


    public List<DependencyProperty> GetDevicePropertys(long driverId, long devId = 0)
    {
        using var serviceScope = _scopeFactory.CreateScope();
        var driverPluginService = serviceScope.ServiceProvider.GetService<IDriverPluginService>();
        var driverPlugin = driverPluginService.GetDriverPluginById(driverId);
        var id = YitIdHelper.NextId();
        try
        {
            var driver = (DriverBase)_pluginService.AddDriver(id, driverPlugin);
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
            return Propertys;
        }
        finally
        {
            _pluginService.DeleteDriver(id, driverId);
        }
    }
    #endregion

    #region worker服务
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken);
    }

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
        await base.StopAsync(cancellationToken);
    }
    private CancellationToken _stoppingToken;
    private IntelligentConcurrentQueue<CollectVariableRunTime> CollectVariableRunTimeCollects { get; set; } = new(100000);
    private IntelligentConcurrentQueue<CollectVariableRunTime> CollectVariableRunTimeChangeds { get; set; } = new(100000);
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [AdaptIgnore]
    public VariableCahngeListEventHandler VariableCollectChanges { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [AdaptIgnore]
    public VariableCahngeListEventHandler VariableValueChanges { get; set; }
    /// <summary>
    /// 变量触发变化
    /// </summary>
    public delegate void VariableCahngeListEventHandler(List<CollectVariableRunTime> collectVariableRunTimes);
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RestartDeviceThread();
        while (!stoppingToken.IsCancellationRequested)
        {
            //这里不采用CancellationToken控制子线程，直接循环保持，结束时调用子设备线程Dispose
            //检测设备采集线程假死
            var num = CollectDeviceCores.Count;
            for (int i = 0; i < num; i++)
            {
                CollectDeviceCore devcore = CollectDeviceCores[i];
                if (devcore.Device.ActiveTime != DateTime.MinValue && devcore.Device.ActiveTime.AddMinutes(3) <= DateTime.Now)
                {
                    if (devcore.StoppingTokens.Last().Token.IsCancellationRequested)
                        continue;
                    if (devcore.Device.DeviceStatus == DeviceStatusEnum.Pause)
                        continue;
                    _logger?.LogWarning(devcore.Device.Name + "采集线程假死，重启线程中");
                    await UpDeviceThread(devcore.DeviceId, false);
                    i--;
                    num--;

                    GC.Collect();

                }
            }

            var list1 = CollectVariableRunTimeCollects.ToListWithDequeue(100000);
            if (list1 != null && list1.Count > 0)
            {
                VariableCollectChanges?.Invoke(list1);
            }


            var list2 = CollectVariableRunTimeChangeds.ToListWithDequeue(100000);
            if (list2 != null && list2.Count > 0)
            {
                VariableValueChanges?.Invoke(list2);
            }

            await Task.Delay(500, stoppingToken);
        }
    }

    #endregion



}

