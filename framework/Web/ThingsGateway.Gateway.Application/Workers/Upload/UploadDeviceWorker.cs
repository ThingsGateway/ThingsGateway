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

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 设备上传后台服务
/// </summary>
public class UploadDeviceWorker : BackgroundService
{
    private readonly ILogger<UploadDeviceWorker> _logger;

    private PluginSingletonService _pluginService;
    private readonly IHostApplicationLifetime _appLifetime;

    /// <inheritdoc cref="UploadDeviceWorker"/>
    public UploadDeviceWorker(ILogger<UploadDeviceWorker> logger, IHostApplicationLifetime appLifetime)
    {
        _logger = logger;
        _appLifetime = appLifetime;
    }
    /// <summary>
    /// 上传设备List
    /// </summary>
    public List<UploadDeviceCore> UploadDeviceCores => UploadDeviceThreads
        .Where(a => a.UploadDeviceCores.Any(b => b.Device != null))
        .SelectMany(a => a.UploadDeviceCores).ToList();
    /// <summary>
    /// 全部设备子线程
    /// </summary>
    private ConcurrentList<UploadDeviceThread> UploadDeviceThreads { get; set; } = new();

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
    public void ConfigDeviceThread(long deviceId, bool isStart)
    {
        if (deviceId == 0)
            UploadDeviceCores.ForEach(it => it.PasueThread(isStart));
        else
            UploadDeviceCores.FirstOrDefault(it => it.DeviceId == deviceId)?.PasueThread(isStart);
    }
    /// <summary>
    /// 开始
    /// </summary>
    public async Task StartAsync()
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
            CreatAllDeviceThreads();
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
    /// 停止
    /// </summary>
    public async Task StopAsync()
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

            await RemoveAllDeviceThreadAsync();
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
    /// 更新设备线程
    /// </summary>
    public async Task UpDeviceThreadAsync(long devId)
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
                var devThread = UploadDeviceThreads.FirstOrDefault(it => it.UploadDeviceCores.Any(a => a.DeviceId == devId));
                var devCore = devThread.UploadDeviceCores.FirstOrDefault(a => a.DeviceId == devId);
                if (devThread == null) { throw Oops.Bah($"更新设备线程失败，不存在{devId}为id的设备"); }
                //这里先停止上传，操作会使线程取消，需要重新恢复线程
                await devThread.StopThreadAsync();
                var dev = App.GetService<IUploadDeviceService>().GetUploadDeviceRuntime(devId).FirstOrDefault();
                if (dev == null)
                {
                    //线程管理器移除后，如果不存在其他设备，也删除线程管理器
                    devThread.UploadDeviceCores.Remove(devCore);
                    if (devThread.UploadDeviceCores.Count == 0)
                    {
                        UploadDeviceThreads.Remove(devThread);
                    }
                    else
                    {
                        foreach (var item in devThread.UploadDeviceCores)
                        {
                            item.Init(item.Device);
                        }
                        await devThread.StartThreadAsync();

                    }
                }
                else
                {
                    //初始化
                    devCore.Init(dev);

                    //线程管理器移除后，如果不存在其他设备，也删除线程管理器
                    devThread.UploadDeviceCores.Remove(devCore);
                    if (devThread.UploadDeviceCores.Count == 0)
                    {
                        UploadDeviceThreads.Remove(devThread);
                    }
                    else
                    {
                        foreach (var item in devThread.UploadDeviceCores)
                        {
                            item.Init(item.Device);
                        }
                    }
                    //需判断是否同一通道
                    var newDevThread = DeviceThread(devCore);
                    await newDevThread.StartThreadAsync();

                }


            }

        }
        finally
        {
            singleRestartLock.Release();
        }

    }

    #endregion

    #region 核心

    /// <summary>
    /// 创建设备上传线程
    /// </summary>
    /// <returns></returns>
    private void CreatAllDeviceThreads()
    {
        if (!_stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("正在获取采集组态信息");
            var collectDeviceRunTimes = (App.GetService<IUploadDeviceService>().GetUploadDeviceRuntime());
            _logger.LogInformation("获取采集组态信息完成");
            foreach (var collectDeviceRunTime in collectDeviceRunTimes)
            {
                if (!_stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        UploadDeviceCore deviceCollectCore = new();
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

    private UploadDeviceThread DeviceThread(UploadDeviceCore deviceUploadCore)
    {
        UploadDeviceThread deviceThread = new();
        deviceThread.UploadDeviceCores.Add(deviceUploadCore);
        UploadDeviceThreads.Add(deviceThread);
        return deviceThread;
    }

    /// <summary>
    /// 删除设备线程，并且释放资源
    /// </summary>
    private async Task RemoveAllDeviceThreadAsync()
    {
        await UploadDeviceThreads.ParallelForEachAsync(async (deviceThread, cancellationToken) =>
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
        await UploadDeviceThreads.ParallelForEachAsync(async (deviceThread, cancellationToken) =>
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
        UploadDeviceThreads.Clear();
    }


    /// <summary>
    /// 开始设备上传线程
    /// </summary>
    /// <returns></returns>
    private async Task StartAllDeviceThreadsAsync()
    {
        if (!_stoppingToken.IsCancellationRequested)
        {
            foreach (var item in UploadDeviceThreads)
            {
                if (!_stoppingToken.IsCancellationRequested)
                {
                    await item.StartThreadAsync();
                }
            }
        }
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
        var driverPluginService = App.GetService<IDriverPluginService>();
        var driverPlugin = driverPluginService.GetDriverPluginById(driverId);

        var driver = _pluginService.GetDriver(driverPlugin);
        var Propertys = _pluginService.GetDriverProperties(driver);
        if (devId != 0)
        {
            var devcore = App.GetService<UploadDeviceService>().GetDeviceById(devId);
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

    /// <summary>
    /// 获取变量上传属性
    /// </summary>
    /// <param name="driverId"></param>
    /// <param name="dependencyProperties"></param>
    /// <returns></returns>
    public List<DependencyProperty> GetVariablePropertys(long driverId, List<DependencyProperty> dependencyProperties = null)
    {
        var driverPluginService = App.GetService<IDriverPluginService>();
        var driverPlugin = driverPluginService.GetDriverPluginById(driverId);
        var driver = (UpLoadBase)_pluginService.GetDriver(driverPlugin);
        var Propertys = _pluginService.GetDriverVariableProperties(driver);
        dependencyProperties?.ForEach(it =>
            {
                var dependencyProperty = Propertys.FirstOrDefault(a => a.PropertyName == it.PropertyName);
                if (dependencyProperty != null)
                {
                    dependencyProperty.Value = it.Value;
                }
            });
        driver?.SafeDispose();

        return Propertys;
    }

    #endregion

    #region worker服务
    private EasyLock easyLock = new();

    /// <summary>
    /// 在软件关闭时取消
    /// </summary>
    private CancellationToken _stoppingToken;
    /// <inheritdoc/>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await easyLock.WaitAsync();
        _appLifetime.ApplicationStarted.Register(() => { easyLock.Release(); easyLock = null; });
        await base.StartAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        using var stoppingToken = new CancellationTokenSource();
        _stoppingToken = stoppingToken.Token;
        stoppingToken.Cancel();
        await base.StopAsync(cancellationToken);
    }
    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await easyLock.WaitAsync();

        _pluginService = App.GetService<PluginSingletonService>();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {


                //这里不采用CancellationToken控制子线程，直接循环保持，结束时调用子设备线程Dispose
                //检测设备上传线程假死
                var num = UploadDeviceCores.Count;
                for (int i = 0; i < num; i++)
                {
                    UploadDeviceCore devcore = UploadDeviceCores[i];
                    if (devcore.Device != null)
                    {
                        //超过30分钟，或者(初始化失败并超过10分钟)会重启
                        if (
        (devcore.Device.ActiveTime != DateTime.MinValue
        && devcore.Device.ActiveTime.AddMinutes(30) <= DateTimeExtensions.CurrentDateTime)
        || (devcore.IsInitSuccess == false && devcore.Device.ActiveTime.AddMinutes(10) <= DateTimeExtensions.CurrentDateTime)
        )
                        {
                            //如果线程处于暂停状态，跳过
                            if (devcore.Device.DeviceStatus == DeviceStatusEnum.Pause)
                                continue;
                            //如果初始化失败
                            if (!devcore.IsInitSuccess)
                                _logger?.LogWarning($"{devcore.Device.Name}初始化失败，重启线程中");
                            else
                                _logger?.LogWarning($"{devcore.Device.Name}上传线程假死，重启线程中");
                            //重启线程

                            await UpDeviceThreadAsync(devcore.Device.Id);
                            break;

                        }
                        else
                        {
                            _logger?.LogTrace($"{devcore.Device.Name}线程检测正常");
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

