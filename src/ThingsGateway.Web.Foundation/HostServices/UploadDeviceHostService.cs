using Furion.FriendlyException;
using Furion.Logging.Extensions;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Linq;
using System.Threading;

using TouchSocket.Core;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 设备上传后台服务
/// </summary>
public class UploadDeviceHostService : BackgroundService
{
    /// <inheritdoc cref="UploadDeviceHostService"/>
    public UploadDeviceHostService(ILogger<UploadDeviceHostService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;

        using var serviceScope = scopeFactory.CreateScope();
        _pluginService = serviceScope.ServiceProvider.GetService<PluginCore>();
        _uploadDeviceService = serviceScope.ServiceProvider.GetService<IUploadDeviceService>();
    }

    /// <summary>
    /// 全部设备子线程
    /// </summary>
    public ConcurrentList<UploadDeviceCore> UploadDeviceCores { get; private set; } = new();

    private ILogger<UploadDeviceHostService> _logger { get; set; }
    private PluginCore _pluginService { get; set; }
    private IServiceScopeFactory _scopeFactory { get; set; }
    private IUploadDeviceService _uploadDeviceService { get; set; }
    #region 设备创建更新结束

    /// <summary>
    /// 控制设备线程启停
    /// </summary>
    public void ConfigDeviceThread(long deviceId, bool isStart)
    {
        if (deviceId == 0)
            UploadDeviceCores.ForEach(it => it.PasueThread(isStart));
        else
            UploadDeviceCores.FirstOrDefault(it => it.DeviceId == deviceId)?.PasueThread(isStart);
    }

    /// <summary>
    /// 删除设备线程，并且释放资源
    /// </summary>
    public void RemoveDeviceThread()
    {
        var dev = UploadDeviceCores;
        foreach (var device in dev)
        {
            try
            {
                device.Dispose();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, device.Device.Name);
            }
        }
        UploadDeviceCores.Clear();

    }
    /// <summary>
    /// 重启全部设备
    /// </summary>
    /// <returns></returns>
    public async Task RestartDeviceThread()
    {
        try
        {
            Task result = Task.Factory.StartNew(() =>
            {
                RemoveDeviceThread();
                StartDeviceThread();

                GC.Collect();
            }, _stoppingToken);
            await result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, nameof(RestartDeviceThread));
        }
    }

    /// <summary>
    /// 启动设备线程
    /// </summary>
    public void StartDeviceThread()
    {
        var devs = (_uploadDeviceService.GetUploadDeviceRuntime());
        foreach (var item in devs)
        {
            if (!_stoppingToken.IsCancellationRequested)
            {
                UploadDeviceCore deviceCollectCore = new(_scopeFactory);
                deviceCollectCore.Init(item);
                deviceCollectCore.StartThread();
                UploadDeviceCores.Add(deviceCollectCore);
            }
        }
    }
    /// <summary>
    /// 更新设备线程
    /// </summary>
    public void UpDeviceThread(long devId, bool isUpdateDb = true)
    {
        if (!_stoppingToken.IsCancellationRequested)
        {

            var devcore = UploadDeviceCores.FirstOrDefault(it => it?.DeviceId == devId);
            if (devcore == null) { throw Oops.Bah($"更新设备线程失败，不存在{devId}为id的设备"); }
            //这里先停止上传，操作会使线程取消，需要重新恢复线程
            devcore.StopThread();
            UploadDeviceRunTime dev = null;
            if (isUpdateDb)
                dev = (_uploadDeviceService.GetUploadDeviceRuntime(devId)).FirstOrDefault();
            else
                dev = devcore.Device;
            if (dev == null) { _logger.LogError($"更新设备线程失败，不存在{devId}为id的设备"); }
            devcore.Init(dev);
            devcore.StartThread();

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
        var driverId = _uploadDeviceService.GetDeviceById(devId).PluginId;
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
            var driver = _pluginService.AddDriver(id, driverPlugin);
            var Propertys = _pluginService.GetDriverProperties(driver);
            if (devId != 0)
            {
                var devcore = UploadDeviceCores.FirstOrDefault(it => it.Device.Id == devId);
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

    /// <summary>
    /// 获取变量上传属性
    /// </summary>
    /// <param name="driverId"></param>
    /// <param name="dependencyProperties"></param>
    /// <returns></returns>
    public List<DependencyProperty> GetVariablePropertys(long driverId, List<DependencyProperty> dependencyProperties = null)
    {
        using var serviceScope = _scopeFactory.CreateScope();
        var driverPluginService = serviceScope.ServiceProvider.GetService<IDriverPluginService>();
        var driverPlugin = driverPluginService.GetDriverPluginById(driverId);
        var id = YitIdHelper.NextId();
        try
        {
            var driver = _pluginService.AddDriver(id, driverPlugin);
            var Propertys = _pluginService.GetDriverVariableProperties(driver);
            if (dependencyProperties != null)
            {
                dependencyProperties.ForEach(it =>
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

        RemoveDeviceThread();
        await base.StopAsync(cancellationToken);
    }
    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        while (!stoppingToken.IsCancellationRequested)
        {
            //这里不采用CancellationToken控制子线程，直接循环保持，结束时调用子设备线程Dispose
            //检测设备上传线程假死
            var num = UploadDeviceCores.Count;
            for (int i = 0; i < num; i++)
            {
                UploadDeviceCore devcore = UploadDeviceCores[i];
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
                        _logger?.LogWarning(devcore.Device.Name + "上传线程假死，重启线程中");
                    UpDeviceThread(devcore.DeviceId, false);
                    i--;
                    num--;

                    GC.Collect();

                }
            }
            await Task.Delay(100000, stoppingToken);
        }
    }

    #endregion



}

