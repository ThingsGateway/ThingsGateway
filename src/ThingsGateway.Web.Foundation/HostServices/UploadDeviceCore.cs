using Furion.FriendlyException;
using Furion.Logging.Extensions;

using Microsoft.Extensions.Logging;

using System.Linq;
using System.Reflection;
using System.Threading;

using TouchSocket.Core;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 设备子线程服务
/// </summary>
public class UploadDeviceCore : DisposableObject
{

    /// <summary>
    /// 循环线程取消标识
    /// </summary>
    public ConcurrentList<CancellationTokenSource> StoppingTokens = new();

    /// <summary>
    /// 当前设备信息
    /// </summary>
    private UploadDeviceRunTime _device;

    /// <summary>
    /// 当前的驱动插件实例
    /// </summary>
    private UpLoadBase _driver { get; set; }

    private ILogger _logger;
    /// <summary>
    /// 全局插件服务
    /// </summary>
    private PluginCore _pluginService;


    private IServiceScopeFactory _scopeFactory;

    /// <inheritdoc cref="UploadDeviceCore"/>
    public UploadDeviceCore(IServiceScopeFactory scopeFactory)
    {

        _scopeFactory = scopeFactory;
        using var scope = scopeFactory.CreateScope();

        _pluginService = scope.ServiceProvider.GetService<PluginCore>();
        _driverPluginService = scope.ServiceProvider.GetService<IDriverPluginService>();

    }

    /// <summary>
    /// 当前设备
    /// </summary>
    public UploadDeviceRunTime Device => _device;
    /// <summary>
    /// 当前设备Id
    /// </summary>
    public long DeviceId => (_device?.Id).ToLong();

    /// <summary>
    /// 当前设备全部设备属性，执行初始化后获取正确值
    /// </summary>
    public List<DependencyProperty> Propertys { get; private set; }
    IDriverPluginService _driverPluginService { get; set; }
    internal bool isInitSuccess;
    /// <summary>
    /// 初始化，在设备子线程创建或更新时才会执行
    /// </summary>
    public void Init(UploadDeviceRunTime device)
    {
        if (device == null) return;
        try
        {
            _device = device;
            using var scope = _scopeFactory.CreateScope();
            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            _logger = loggerFactory.CreateLogger("上传设备:" + _device.Name);
            try
            {
                UpDriver();
                SetPluginProperties(_device.DevicePropertys);
                _driver.IsLogOut = _device.IsLogOut;
                _driver.Init(_logger, _device);
                isInitSuccess = true;
            }
            catch (Exception ex)
            {
                isInitSuccess = false;
                _logger.LogError(ex, $"{_device.Name}Init失败");
            }
            StoppingTokens.Add(new());
            InitTask();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, device.Name);
        }


        void UpDriver()
        {
            var driverPlugin = _driverPluginService.GetDriverPluginById(Device.PluginId);
            _driver = (UpLoadBase)_pluginService.AddDriver(DeviceId, driverPlugin);
            if (driverPlugin != null)
            {
                Propertys = _pluginService.GetDriverProperties(_driver);
            }
            else
            {
                throw Oops.Oh($"找不到驱动{driverPlugin.AssembleName}");
            }
        }
    }

    #region 设备子线程上传启动停止

    private Task<Task> DeviceTask;

    /// <summary>
    /// 初始化
    /// </summary>
    public void InitTask()
    {
        DeviceTask = new Task<Task>(() =>
        {
            CancellationTokenSource StoppingToken = StoppingTokens.Last();
            return Task.Factory.StartNew(async (a) =>
            {
                if (!isInitSuccess)
                    return;
                _logger?.LogInformation($"{_device.Name}上传设备线程开始");
                try
                {
                    if (Device?.Enable == true)
                    {
                        //驱动插件执行循环前方法
                        await _driver?.BeforStart();
                    }
                    if (!StoppingToken.IsCancellationRequested)
                    {
                        await Task.Delay(500);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, _device.Name + "BeforStart错误");
                    Device.DeviceStatus = DeviceStatusEnum.OffLine;
                    Device.DeviceOffMsg = "BeforStart错误";
                }
                while (!StoppingToken.Token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(100, StoppingToken.Token);

                        if (_driver == null) continue;

                        if (Device?.Enable == false)
                        {
                            Device.DeviceStatus = DeviceStatusEnum.Pause;
                            continue;
                        }
                        try
                        {
                            if (Device.DeviceStatus != DeviceStatusEnum.OnLineButNoInitialValue && Device.DeviceStatus != DeviceStatusEnum.OnLine)
                                Device.DeviceStatus = DeviceStatusEnum.OnLineButNoInitialValue;
                            Device.ActiveTime = DateTime.Now;
                            await _driver.ExecuteAsync(StoppingToken.Token);
                            if (_driver.Success().IsSuccess)
                            {
                                Device.DeviceStatus = DeviceStatusEnum.OnLine;
                            }
                            else
                            {
                                Device.DeviceStatus = DeviceStatusEnum.OffLine;
                            }
                            if (StoppingToken.Token.IsCancellationRequested)
                                break;
                        }
                        catch (TaskCanceledException)
                        {

                        }
                        catch (Exception ex)
                        {
                            _logger?.LogWarning(ex, $"上传线程循环异常{_device.Name}");
                        }
                    }
                    catch (TaskCanceledException)
                    {

                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, $"上传线程循环异常{_device.Name}");
                    }
                }

            }, StoppingToken.Token
 , TaskCreationOptions.LongRunning);
        }
         );
    }

    /// <summary>
    /// 暂停上传
    /// </summary>
    public void PasueThread(bool enable)
    {
        lock (this)
        {
            var str = enable == false ? "设备线程上传暂停" : "设备线程上传继续";
            _logger?.LogInformation($"{str}:{_device.Name}");
            this.Device.Enable = enable;
        }
    }

    /// <summary>
    /// 开始上传
    /// </summary>
    public void StartThread()
    {
        DeviceTask?.Start();
    }
    /// <summary>
    /// 停止上传
    /// </summary>
    public void StopThread()
    {
        try
        {
            CancellationTokenSource StoppingToken = StoppingTokens.LastOrDefault();
            StoppingToken?.Cancel();
            _logger?.LogInformation($"{_device.Name}上传线程停止中");
            var devResult = DeviceTask?.Result;
            if (devResult?.Status != TaskStatus.Canceled)
            {
                if (devResult?.Wait(5000) == true)
                {
                    _logger?.LogInformation($"{_device.Name}上传线程已停止");
                }
                else
                {
                    _logger?.LogInformation($"{_device.Name}上传线程停止超时，已强制取消");
                }
            }
            DeviceTask?.Dispose();
            if (StoppingToken != null)
            {
                StoppingTokens.Remove(StoppingToken);
            }

            try
            {
                _driver?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{Device.Name} Dispose Error: {ex.Message}");
            }

        }
        finally
        {
            _pluginService.DeleteDriver(DeviceId, Device.PluginId);
        }
    }
    #endregion


    /// <summary>
    /// 设置驱动插件的属性值
    /// </summary>
    public void SetPluginProperties(List<DependencyProperty> deviceProperties)
    {
        if (deviceProperties == null) return;
        var pluginPropertys = _driver.GetType().GetProperties();
        foreach (var propertyInfo in pluginPropertys)
        {
            var propAtt = propertyInfo.GetCustomAttribute(typeof(DevicePropertyAttribute));
            var deviceProperty = deviceProperties.FirstOrDefault(x => x.PropertyName == propertyInfo.Name);
            if (propAtt == null || deviceProperty == null || string.IsNullOrEmpty(deviceProperty?.Value?.ToString())) continue;
            var value = ReadWriteHelpers.ObjToTypeValue(propertyInfo, deviceProperty.Value);
            propertyInfo.SetValue(_driver, value);
        }
    }


    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        StopThread();
    }




}

