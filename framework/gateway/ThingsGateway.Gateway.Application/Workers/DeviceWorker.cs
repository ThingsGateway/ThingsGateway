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

using Furion.Logging.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 南向设备服务
/// </summary>
public abstract class DeviceWorker : BackgroundService
{
    protected ILogger _logger;

    /// <summary>
    /// 全部重启锁
    /// </summary>
    protected readonly EasyLock restartLock = new();

    /// <summary>
    /// 单个重启锁
    /// </summary>
    protected readonly EasyLock singleRestartLock = new();

    protected DriverPluginService _driverPluginService;
    protected IServiceScope _serviceScope;
    private readonly IHostApplicationLifetime _appLifetime;

    /// <inheritdoc/>
    public DeviceWorker(IServiceScopeFactory serviceScopeFactory, IHostApplicationLifetime appLifetime)
    {
        _serviceScope = serviceScopeFactory.CreateScope();
        _appLifetime = appLifetime;
    }

    /// <summary>
    /// 插件列表
    /// </summary>
    public List<DriverBase> DriverBases => _deviceThreads
        .Where(a => a.DriverBases.Any(b => b.CurrentDevice != null))
        .SelectMany(a => a.DriverBases).OrderByDescending(a => a.CurrentDevice.DeviceStatus).ToList();

    /// <summary>
    /// 设备子线程列表
    /// </summary>
    protected ConcurrentList<DeviceThread> _deviceThreads { get; set; } = new();

    #region public 设备创建更新结束

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

    #endregion

    #region Private

    /// <summary>
    /// 根据通道生成/获取线程管理器
    /// </summary>
    /// <param name="driverBase"></param>
    /// <returns></returns>
    protected DeviceThread GetDeviceThread(DriverBase driverBase)
    {
        var channelID = driverBase.DriverPropertys.GetChannelID();
        if (!string.IsNullOrEmpty(channelID))
        {
            var deviceThread = _deviceThreads.FirstOrDefault(t => t.ChangelID == channelID);
            if (deviceThread != null)
            {
                driverBase.DeviceThread = deviceThread;
                deviceThread.DriverBases.Add(driverBase);
                return deviceThread;
            }
        }

        return NewDeviceThread(driverBase, channelID);

        DeviceThread NewDeviceThread(DriverBase driverBase, string channelID)
        {
            DeviceThread deviceThread = new DeviceThread(channelID);
            deviceThread.DriverBases.Add(driverBase);
            driverBase.DeviceThread = deviceThread;
            _deviceThreads.Add(deviceThread);
            return deviceThread;
        }
    }

    /// <summary>
    /// 删除设备线程，并且释放资源
    /// </summary>
    protected async Task RemoveAllDeviceThreadAsync()
    {
        _deviceThreads.ForEach((a) =>
       {
           try
           {
               a.BeforeStopThread();
           }
           catch (Exception ex)
           {
               _logger?.LogError(ex, a.ToString());
           }
       });

        await _deviceThreads.ParallelForEachAsync(async (deviceThread, cancellationToken) =>
         {
             try
             {
                 await deviceThread.StopThreadAsync();
             }
             catch (Exception ex)
             {
                 _logger?.LogError(ex, deviceThread.ToString());
             }
         }, 10);

        _deviceThreads.Clear();
    }

    /// <summary>
    /// 开始设备采集线程
    /// </summary>
    /// <returns></returns>
    protected async Task StartAllDeviceThreadsAsync()
    {
        if (!_stoppingToken.IsCancellationRequested)
        {
            foreach (var item in _deviceThreads)
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
    /// GetDebugUI
    /// </summary>
    /// <param name="pluginName"></param>
    /// <returns></returns>
    public Type GetDebugUI(string pluginName)
    {
        var driverPlugin = _driverPluginService.GetDriver(pluginName);
        driverPlugin?.SafeDispose();
        return driverPlugin?.DriverDebugUIType;
    }

    /// <summary>
    /// GetDriverUI
    /// </summary>
    /// <param name="pluginName"></param>
    /// <returns></returns>
    public Type GetDriverUI(string pluginName)
    {
        var driverPlugin = _driverPluginService.GetDriver(pluginName);
        driverPlugin?.SafeDispose();
        return driverPlugin?.DriverUIType;
    }

    #endregion

    #region worker服务

    protected EasyLock _easyLock = new();

    /// <summary>
    /// 在软件关闭时取消
    /// </summary>
    protected CancellationToken _stoppingToken;

    /// <inheritdoc/>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await _easyLock.WaitAsync();
        _appLifetime.ApplicationStarted.Register(() => { _easyLock.Release(); _easyLock = null; });
        await base.StartAsync(cancellationToken);
    }

    /// <summary>
    /// 更新设备线程
    /// </summary>
    public async Task UpDeviceThreadAsync(long devId, bool isChanged = true)
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
                if (isChanged)
                    await StopOtherHostService();
                var deviceThread = _deviceThreads.FirstOrDefault(it => it.DriverBases.Any(a => a.DeviceId == devId));
                var driverBase = deviceThread.DriverBases.FirstOrDefault(a => a.DeviceId == devId);
                if (deviceThread == null) { throw new($"更新设备线程失败，不存在{devId}为id的设备"); }
                //这里先停止采集，操作会使线程取消，需要重新恢复线程
                await deviceThread.StopThreadAsync();

                var dev = isChanged ? (await GetDeviceRunTimeAsync(devId)).FirstOrDefault() : driverBase.CurrentDevice;

                deviceThread.DriverBases.Remove(driverBase);

                if (dev == null)
                {
                    //线程管理器移除后，如果不存在其他设备，也删除线程管理器
                    //单个设备重启时，注意同一线程的其他设备也会停止，需要重新初始化
                    HashSet<DeviceThread> deviceThreads = new HashSet<DeviceThread>();
                    foreach (var item in deviceThread.DriverBases)
                    {
                        var newDriverBase = item.CurrentDevice.CreatDriver();
                        newDriverBase.Init(item.CurrentDevice);
                        deviceThreads.Add(GetDeviceThread(newDriverBase));
                    }
                    foreach (var item in deviceThreads)
                    {
                        await item.StopThreadAsync();
                        await item.StartThreadAsync();
                    }
                    //如果是组态更改过了，需要重新获取变量/设备运行态的值
                    if (isChanged)
                        await StartOtherHostService();
                }
                else
                {
                    //初始化
                    DriverBase newDriverBase = dev.CreatDriver();
                    newDriverBase.Init(dev);
                    //线程管理器移除后，如果不存在其他设备，也删除线程管理器
                    //单个设备重启时，注意同一线程的其他设备也会停止，需要重新初始化
                    HashSet<DeviceThread> deviceThreads = [GetDeviceThread(newDriverBase)];
                    foreach (var item in deviceThread.DriverBases)
                    {
                        var newDriverBase1 = item.CurrentDevice.CreatDriver();
                        newDriverBase1.Init(item.CurrentDevice);
                        deviceThreads.Add(GetDeviceThread(newDriverBase1));
                    }
                    foreach (var item in deviceThreads)
                    {
                        await item.StopThreadAsync();
                        await item.StartThreadAsync();
                    }
                    //如果是组态更改过了，需要重新获取变量/设备运行态的值
                    if (isChanged)
                        await StartOtherHostService();
                }
            }
        }
        finally
        {
            singleRestartLock.Release();
        }
    }

    /// <summary>
    /// 更新设备线程,切换为冗余通道
    /// </summary>
    public async Task DeviceRedundantThreadAsync(long devId)
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
                var deviceThread = _deviceThreads.FirstOrDefault(it => it.DriverBases.Any(a => a.DeviceId == devId));
                var driverBase = deviceThread.DriverBases.FirstOrDefault(a => a.DeviceId == devId);
                if (deviceThread == null) { throw new($"更新设备线程失败，不存在{devId}为id的设备"); }
                //这里先停止采集，操作会使线程取消，需要重新恢复线程
                await deviceThread.StopThreadAsync();

                var dev = driverBase.CurrentDevice;

                if (dev.IsRedundant)
                {
                    if (dev.Redundant == RedundantEnum.Standby)
                    {
                        var newDev = (await GetDeviceRunTimeAsync(devId)).FirstOrDefault();
                        if (dev == null)
                        {
                            _logger.LogError($"更新设备线程失败，不存在{devId}为id的设备");
                        }
                        else
                        {
                            dev.DevicePropertys = newDev.DevicePropertys;
                            dev.Redundant = RedundantEnum.Primary;
                            _logger?.LogInformation($"{dev.Name}：切换到主通道");
                        }
                    }
                    else
                    {
                        try
                        {
                            var Redundantdev = (await GetDeviceRunTimeAsync(dev.RedundantDeviceId)).FirstOrDefault();
                            if (Redundantdev == null)
                            {
                                _logger.LogError($"更新设备线程失败，不存在{devId}为id的设备");
                            }
                            else
                            {
                                dev.DevicePropertys = Redundantdev.DevicePropertys;
                                dev.Redundant = RedundantEnum.Standby;
                                _logger?.LogInformation($"{dev.Name}：切换到备用通道");
                            }
                        }
                        catch
                        {
                        }
                    }
                }

                deviceThread.DriverBases.Remove(driverBase);

                //初始化
                DriverBase newDriverBase = dev.CreatDriver();
                newDriverBase.Init(dev);
                //线程管理器移除后，如果不存在其他设备，也删除线程管理器
                //单个设备重启时，注意同一线程的其他设备也会停止，需要重新初始化
                HashSet<DeviceThread> deviceThreads = [GetDeviceThread(newDriverBase)];
                foreach (var item in deviceThread.DriverBases)
                {
                    var newDriverBase1 = item.CurrentDevice.CreatDriver();
                    newDriverBase1.Init(item.CurrentDevice);
                    deviceThreads.Add(GetDeviceThread(newDriverBase1));
                }
                foreach (var item in deviceThreads)
                {
                    await item.StopThreadAsync();
                    await item.StartThreadAsync();
                }
            }
        }
        finally
        {
            singleRestartLock.Release();
        }
    }

    /// <summary>
    /// 线程检查时间，120分钟
    /// </summary>
    public const int CheckIntervalTime = 7200;

    protected abstract Task StartOtherHostService();

    protected abstract Task StopOtherHostService();

    protected abstract Task<IEnumerable<DeviceRunTime>> GetDeviceRunTimeAsync(long devId);

    #endregion
}