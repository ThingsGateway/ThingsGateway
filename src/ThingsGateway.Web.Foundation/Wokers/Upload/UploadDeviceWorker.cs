#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
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
/// 设备上传后台服务
/// </summary>
public class UploadDeviceWorker : BackgroundService
{
    /// <inheritdoc cref="UploadDeviceWorker"/>
    public UploadDeviceWorker(ILogger<UploadDeviceWorker> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;

        var serviceScope = scopeFactory.CreateScope();
        _pluginService = serviceScope.ServiceProvider.GetService<PluginSingletonService>();
        _uploadDeviceService = serviceScope.ServiceProvider.GetService<IUploadDeviceService>();
    }
    /// <summary>
    /// 上传设备List
    /// </summary>
    public List<UploadDeviceCore> UploadDeviceCores => UploadDeviceThreads.SelectMany(a => a.UploadDeviceCores).ToList();

    /// <summary>
    /// 上传设备List
    /// </summary>
    public List<UploadDeviceRunTime> UploadDeviceRunTimes => UploadDeviceThreads.SelectMany(a => a.UploadDeviceCores.Select(b => b.Device))?.ToList();
    private ILogger<UploadDeviceWorker> _logger { get; set; }

    private PluginSingletonService _pluginService { get; set; }

    private IServiceScopeFactory _scopeFactory { get; set; }

    private IUploadDeviceService _uploadDeviceService { get; set; }

    /// <summary>
    /// 全部设备子线程
    /// </summary>
    private ConcurrentList<UploadDeviceThread> UploadDeviceThreads { get; set; } = new();
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
                UploadDeviceCores.ForEach(it => it.PasueThread(isStart));
            else
                UploadDeviceCores.FirstOrDefault(it => it.DeviceId == deviceId)?.PasueThread(isStart);
        }
        finally
        {
            easyLock.UnLock();
        }

    }
    /// <summary>
    /// 停止
    /// </summary>
    public void StopDeviceThread()
    {
        RemoveAllDeviceThread();
    }
    /// <summary>
    /// 开始
    /// </summary>
    public void StartDeviceThread()
    {
        CreatAllDeviceThreads();
        StartAllDeviceThreads();
    }




    /// <summary>
    /// 更新设备线程
    /// </summary>
    public async Task UpDeviceThreadAsync(long devId)
    {
        try
        {
            await easyLock.LockAsync();

            if (!_stoppingToken.IsCancellationRequested)
            {
                var devThread = UploadDeviceThreads.FirstOrDefault(it => it.UploadDeviceCores.Any(a => a.DeviceId == devId));
                var devCore = devThread.UploadDeviceCores.FirstOrDefault(a => a.DeviceId == devId);
                if (devThread == null) { throw Oops.Bah($"更新设备线程失败，不存在{devId}为id的设备"); }
                //这里先停止上传，操作会使线程取消，需要重新恢复线程
                devThread.StopThread();
                UploadDeviceRunTime dev = null;
                dev = (_uploadDeviceService.GetUploadDeviceRuntime(devId)).FirstOrDefault();
                if (dev == null) { _logger.LogError($"更新设备线程失败，不存在{devId}为id的设备"); }
                devCore.Init(dev);
                devThread.UploadDeviceCores.Remove(devCore);
                if (devThread.UploadDeviceCores.Count == 0)
                {
                    UploadDeviceThreads.Remove(devThread);
                }
                //需判断是否同一通道
                var newDevThread = DeviceThread(devCore);
                newDevThread.StartThread();

            }

        }
        finally
        {
            easyLock.UnLock();
        }

    }

    #endregion

    #region 核心
    /// <summary>
    /// 删除设备线程，并且释放资源
    /// </summary>
    private void RemoveAllDeviceThread()
    {
        foreach (var deviceThread in UploadDeviceThreads)
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
        UploadDeviceThreads.Clear();
    }

    /// <summary>
    /// 开始设备上传线程
    /// </summary>
    /// <returns></returns>
    private void StartAllDeviceThreads()
    {
        if (!_stoppingToken.IsCancellationRequested)
        {
            foreach (var item in UploadDeviceThreads)
            {
                if (!_stoppingToken.IsCancellationRequested)
                {
                    item.StartThread();
                }
            }
        }
    }
    /// <summary>
    /// 创建设备上传线程
    /// </summary>
    /// <returns></returns>
    private void CreatAllDeviceThreads()
    {
        if (!_stoppingToken.IsCancellationRequested)
        {
            var uploadDeviceRunTimes = (_uploadDeviceService.GetUploadDeviceRuntime());
            foreach (var uploadDeviceRunTime in uploadDeviceRunTimes)
            {
                if (!_stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        UploadDeviceCore deviceUploadCore = new(_scopeFactory);
                        deviceUploadCore.Init(uploadDeviceRunTime);
                        DeviceThread(deviceUploadCore);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, uploadDeviceRunTime.Name);

                    }
                }
            }

        }


    }

    private UploadDeviceThread DeviceThread(UploadDeviceCore deviceUploadCore)
    {
        UploadDeviceThread deviceThread = new(_scopeFactory);
        deviceThread.UploadDeviceCores.Add(deviceUploadCore);
        UploadDeviceThreads.Add(deviceThread);
        return deviceThread;
    }


    #endregion

    #region 设备信息获取

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
            driver?.SafeDispose();

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
            var driver = (UpLoadBase)_pluginService.GetDriver(id, driverPlugin);
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
            driver?.SafeDispose();

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
        stoppingToken?.SafeDispose();

        RemoveAllDeviceThread();
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
                        _logger?.LogWarning(devcore.Device.Name + "上传线程假死，重启线程中");
                    await UpDeviceThreadAsync(devcore.DeviceId);

                }
                else
                {
                    _logger?.LogTrace(devcore.Device.Name + "线程检测正常");
                }
            }
            await Task.Delay(100000, stoppingToken);
        }
    }

    #endregion



}

