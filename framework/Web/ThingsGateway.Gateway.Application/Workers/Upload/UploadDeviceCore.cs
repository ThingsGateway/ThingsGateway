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

using Microsoft.Extensions.Logging;

using ThingsGateway.Foundation.Extension;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 设备子线程服务
/// </summary>
public class UploadDeviceCore
{
    /// <summary>
    /// 全局插件服务
    /// </summary>
    private readonly PluginSingletonService _pluginService;
    /// <summary>
    /// 读写锁
    /// </summary>
    private readonly EasyLock easyLock = new();
    /// <summary>
    /// 当前设备信息
    /// </summary>
    private UploadDeviceRunTime _device;

    /// <summary>
    /// 当前的驱动插件实例
    /// </summary>
    private UpLoadBase _driver;
    /// <summary>
    /// 日志
    /// </summary>
    private ILogger _logger;
    /// <summary>
    /// 是否初始化成功
    /// </summary>
    private bool isInitSuccess = true;

    /// <inheritdoc cref="UploadDeviceCore"/>
    public UploadDeviceCore()
    {
        _pluginService = App.GetService<PluginSingletonService>();
        DriverPluginService = App.GetService<IDriverPluginService>();
    }

    /// <summary>
    /// 当前设备
    /// </summary>
    public UploadDeviceRunTime Device => _device;

    /// <summary>
    /// 当前设备Id
    /// </summary>
    public long DeviceId => (long)(_device?.Id.ToLong());

    /// <summary>
    /// 当前插件
    /// </summary>
    public UpLoadBase Driver => _driver;

    /// <summary>
    /// 初始化成功
    /// </summary>
    public bool IsInitSuccess => isInitSuccess;

    /// <summary>
    /// 日志
    /// </summary>
    public ILogger Logger => _logger;

    /// <summary>
    /// 当前设备全部设备属性，执行初始化后获取正确值
    /// </summary>
    public List<DependencyProperty> Propertys { get; private set; }

    private IDriverPluginService DriverPluginService { get; set; }

    /// <summary>
    /// 暂停上传
    /// </summary>
    public void PasueThread(bool keepRun)
    {
        lock (this)
        {
            var str = keepRun == false ? "设备线程上传暂停" : "设备线程上传继续";
            _logger?.LogInformation($"{str}:{_device.Name}");
            this.Device.KeepRun = keepRun;
        }
    }

    #region 插件处理

    /// <summary>
    /// 获取插件
    /// </summary>
    /// <returns></returns>
    private UpLoadBase CreatDriver()
    {

        var driverPlugin = DriverPluginService.GetDriverPluginById(_device.PluginId);
        if (driverPlugin != null)
        {
            try
            {
                _driver = (UpLoadBase)_pluginService.GetDriver(driverPlugin);
                Propertys = _pluginService.GetDriverProperties(_driver);
            }
            catch (Exception ex)
            {
                throw Oops.Oh($"创建插件失败：{ex}");
            }

        }
        else
        {
            throw Oops.Oh($"找不到驱动{driverPlugin.AssembleName}");
        }
        //设置插件配置项
        SetPluginProperties(_device.DevicePropertys);
        return _driver;

    }

    private void InitDriver()
    {
        //初始化插件
        _driver.Init(_logger, _device);
        //变量打包
        _device.UploadVariableCount = _driver.UploadVariables?.Count ?? 0;
    }

    /// <summary>
    /// 设置驱动插件的属性值
    /// </summary>
    private void SetPluginProperties(List<DependencyProperty> deviceProperties)
    {
        if (deviceProperties == null) return;
        _pluginService.SetDriverProperties(_driver, deviceProperties);
    }

    #endregion

    #region 核心读写


    /// <summary>
    /// 线程开始时执行
    /// </summary>
    /// <returns></returns>
    internal async Task BeforeActionAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested) return;
            if (_device == null)
            {
                _logger?.LogError($"{nameof(UploadDeviceRunTime)}不能为null");
                isInitSuccess = false;
                return;
            }
            if (_driver == null)
            {
                _logger?.LogWarning($"{_device.Name} - 插件不能为null");
                isInitSuccess = false;
                return;
            }

            _logger?.LogInformation($"{_device.Name}上传设备线程开始");


            InitDriver();

            try
            {
                if (Device.KeepRun == true)
                {
                    await _driver.BeforStartAsync(cancellationToken);
                    Device.SetDeviceStatus(DateTimeExtensions.CurrentDateTime);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, _device.Name);
                Device.SetDeviceStatus(null, Device.ErrorCount + 1, ex.Message);
            }
            isInitSuccess = true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, _device.Name);
            isInitSuccess = false;
            Device.SetDeviceStatus(DateTimeExtensions.CurrentDateTime, 999, ex.Message);
        }
    }

    /// <summary>
    /// 结束后
    /// </summary>
    internal async Task FinishActionAsync()
    {
        try
        {
            _logger?.LogInformation($"{_device.Name}上传线程停止中");
            await _driver?.AfterStopAsync();
            _driver?.SafeDispose();
            _logger?.LogInformation($"{_device.Name}上传线程已停止");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"{Device.Name} 释放失败");
        }
        finally
        {
            isInitSuccess = false;
        }


    }

    /// <summary>
    /// 初始化
    /// </summary>
    internal bool Init(UploadDeviceRunTime device)
    {
        if (device == null)
        {
            _logger?.LogError($"{nameof(UploadDeviceRunTime)}不能为null");
            return false;
        }
        try
        {
            bool isUpDevice = Device != device;
            _device = device;
            _logger = App.GetService<ILoggerFactory>().CreateLogger("上传设备：" + _device.Name);
            //更新插件信息
            CreatDriver();
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, device.Name);
            return false;
        }

    }

    /// <summary>
    /// 执行一次读取
    /// </summary>
    internal async Task<ThreadRunReturn> RunActionAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return ThreadRunReturn.Break;
            if (_device == null)
            {
                _logger?.LogError($"{nameof(UploadDeviceRunTime)}不能为null");
                return ThreadRunReturn.Continue;
            }
            if (_driver == null)
            {
                _logger?.LogWarning($"{_device.Name} - 插件不能为null");
                return ThreadRunReturn.Continue;
            }

            if (Device.KeepRun == false)
            {
                //上传暂停
                return ThreadRunReturn.Continue;
            }

            if (cancellationToken.IsCancellationRequested)
                return ThreadRunReturn.Break;
            await _driver.ExecuteAsync(cancellationToken);

            //获取设备连接状态
            if (_driver.IsConnected())
            {
                //更新设备活动时间
                Device.SetDeviceStatus(DateTimeExtensions.CurrentDateTime, 0);
            }
            else
            {
                Device.SetDeviceStatus(DateTimeExtensions.CurrentDateTime, 999);
            }
            if (cancellationToken.IsCancellationRequested)
                return ThreadRunReturn.Break;

            //正常返回None
            return ThreadRunReturn.None;

        }
        catch (TaskCanceledException)
        {
            return ThreadRunReturn.Break;
        }
        catch (ObjectDisposedException)
        {
            return ThreadRunReturn.Break;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, $"上传线程循环异常{_device.Name}");
            Device.SetDeviceStatus(null, Device.ErrorCount + 1, ex.Message);
            return ThreadRunReturn.None;
        }
    }



    #endregion

}

