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
/// 北向设备服务
/// </summary>
public class UploadDeviceWorker : DeviceWorker
{
    public UploadDeviceWorker(IServiceScopeFactory serviceScopeFactory, IHostApplicationLifetime appLifetime) : base(serviceScopeFactory, appLifetime)
    {
        _logger = _serviceScope.ServiceProvider.GetService<ILoggerFactory>().CreateLogger("北向设备服务");
    }

    #region public 设备创建更新结束

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
            //执行一次删除缓存文件
            LiteDBCache.DeleteOldData(DriverBases.Select(a => a.DeviceId).ToList());
            await StartAllDeviceThreadsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "启动发生错误");
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
            _logger.LogError(ex, "停止发送错误");
        }
        finally
        {
            singleRestartLock.Release();
            restartLock.Release();
        }
    }

    #endregion

    #region public 设备创建更新结束

    /// <summary>
    /// 创建北向设备线程
    /// </summary>
    /// <returns></returns>
    protected void CreatAllDeviceThreads()
    {
        if (!_stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("正在获取北向设备组态信息");
            var deviceRunTimes = (_serviceScope.ServiceProvider.GetService<IUploadDeviceService>().GetDeviceRuntime());
            _logger.LogInformation("获取北向设备组态信息完成");
            foreach (var uploadDeviceRunTime in deviceRunTimes.Where(a => !deviceRunTimes.Any(b => a.Id == b.RedundantDeviceId && b.IsRedundant)))
            {
                if (!_stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        DriverBase driverBase = uploadDeviceRunTime.CreatDriver();
                        driverBase.Init(uploadDeviceRunTime);
                        GetDeviceThread(driverBase);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"{uploadDeviceRunTime.Name}初始化错误！");
                    }
                }
            }
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
        var pluginName = _serviceScope.ServiceProvider.GetService<IUploadDeviceService>().GetDeviceById(devId).PluginName;
        var driverBase = _driverPluginService.GetDriver(pluginName);
        var Propertys = _driverPluginService.GetDriverMethodInfo(driverBase);
        driverBase?.SafeDispose();
        return Propertys.Select(it => it.Description).ToList();
    }

    /// <summary>
    /// 获取设备属性，传入设备Id，相同名称的属性值会被重写
    /// </summary>
    /// <param name="driverId"></param>
    /// <param name="devId"></param>
    /// <returns></returns>
    public List<DependencyProperty> GetDevicePropertys(string pluginName, long devId = 0)
    {
        var driverBase = _driverPluginService.GetDriver(pluginName);
        var Propertys = _driverPluginService.GetDriverProperties(driverBase);
        if (devId != 0)
        {
            var uploadDevice = _serviceScope.ServiceProvider.GetService<IUploadDeviceService>().GetDeviceById(devId);
            uploadDevice?.DevicePropertys?.ForEach(it =>
            {
                var dependencyProperty = Propertys.FirstOrDefault(a => a.PropertyName == it.PropertyName);
                if (dependencyProperty != null && !it.Value.IsNullOrEmpty())
                {
                    dependencyProperty.Value = it.Value;
                }
            });
        }
        driverBase?.SafeDispose();
        return Propertys;
    }

    /// <summary>
    /// 获取变量上传属性
    /// </summary>
    /// <param name="driverId"></param>
    /// <param name="dependencyProperties"></param>
    /// <returns></returns>
    public List<DependencyProperty> GetVariablePropertys(string pluginName, List<DependencyProperty> dependencyProperties = null)
    {
        var driverBase = (UploadBase)_driverPluginService.GetDriver(pluginName);
        var Propertys = _driverPluginService.GetDriverVariableProperties(driverBase);
        dependencyProperties?.ForEach(it =>
        {
            var dependencyProperty = Propertys.FirstOrDefault(a => a.PropertyName == it.PropertyName);
            if (dependencyProperty != null && !it.Value.IsNullOrEmpty())
            {
                dependencyProperty.Value = it.Value;
            }
        });
        driverBase?.SafeDispose();

        return Propertys;
    }

    #endregion

    #region worker服务

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
        await _easyLock?.WaitAsync();
        _driverPluginService = _serviceScope.ServiceProvider.GetService<DriverPluginService>();

        await WhileExecuteAsync(stoppingToken);
    }

    protected virtual async Task WhileExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                //每5分钟检测一次
                await Task.Delay(300000, stoppingToken);

                //检测设备线程假死
                var num = DriverBases.Count;
                for (int i = 0; i < num; i++)
                {
                    DriverBase driverBase = DriverBases[i];
                    try
                    {
                        if (driverBase.CurrentDevice != null && !driverBase.DisposedValue)
                        {
                            if (driverBase.CurrentDevice.DeviceStatus == DeviceStatusEnum.OffLine)
                            {
                                if (driverBase.CurrentDevice.IsRedundant && _serviceScope.ServiceProvider.GetService<IUploadDeviceService>().GetCacheList(false).Any(a => a.Id == driverBase.CurrentDevice.RedundantDeviceId))
                                {
                                    await DeviceRedundantThreadAsync(driverBase.CurrentDevice.Id);
                                }
                            }

                            if (
            (driverBase.CurrentDevice.ActiveTime != DateTime.MinValue &&
            driverBase.CurrentDevice.ActiveTime.AddMinutes(CheckIntervalTime) <= DateTimeExtensions.CurrentDateTime)
            || (driverBase.IsInitSuccess == false && driverBase.CurrentDevice.ActiveTime.AddMinutes(CheckIntervalTime) <= DateTimeExtensions.CurrentDateTime)
            )
                            {
                                //如果线程处于暂停状态，跳过
                                if (driverBase.CurrentDevice.DeviceStatus == DeviceStatusEnum.Pause)
                                    continue;
                                //如果初始化失败
                                if (!driverBase.IsInitSuccess)
                                    _logger?.LogWarning($"{driverBase.CurrentDevice.Name}初始化失败，重启线程中");
                                else
                                    _logger?.LogWarning($"{driverBase.CurrentDevice.Name}线程假死，重启线程中");
                                //重启线程
                                await UpDeviceThreadAsync(driverBase.CurrentDevice.Id, false);
                                break;
                            }
                            else
                            {
                                _logger?.LogTrace($"{driverBase.CurrentDevice.Name}线程检测正常");
                            }
                        }
                    }
                    finally
                    {
                    }
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "线程检测错误");
            }
        }
    }

    protected override Task StartOtherHostService()
    {
        return Task.CompletedTask;
    }

    protected override Task StopOtherHostService()
    {
        return Task.CompletedTask;
    }

    protected override Task<IEnumerable<DeviceRunTime>> GetDeviceRunTimeAsync(long devId)
    {
        return Task.FromResult(_serviceScope.ServiceProvider.GetService<IUploadDeviceService>().GetDeviceRuntime(devId));
    }

    #endregion
}