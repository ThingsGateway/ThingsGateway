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

using Microsoft.Extensions.Logging;

using System.Linq;
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
    private UpLoadBase _driver;

    private ILogger _logger;

    /// <summary>
    /// 全局插件服务
    /// </summary>
    private PluginSingletonService _pluginService;

    private IServiceScopeFactory _scopeFactory;
    private bool isInitSuccess = true;

    /// <inheritdoc cref="UploadDeviceCore"/>
    public UploadDeviceCore(IServiceScopeFactory scopeFactory)
    {

        _scopeFactory = scopeFactory;
        var scope = scopeFactory.CreateScope();

        _pluginService = scope.ServiceProvider.GetService<PluginSingletonService>();
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
    private IDriverPluginService _driverPluginService { get; set; }


    #region 单控

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
    /// <summary>
    /// 停止上传
    /// </summary>
    private void StopThread()
    {
        lock (this)
        {
            CancellationTokenSource StoppingToken = StoppingTokens.LastOrDefault();
            StoppingToken?.Cancel();
            StoppingToken?.SafeDispose();
        }
    }


    #endregion


    #region 插件处理

    /// <summary>
    /// 获取插件
    /// </summary>
    /// <returns></returns>
    private UpLoadBase CreatDriver()
    {
        try
        {
            var driverPlugin = _driverPluginService.GetDriverPluginById(Device.PluginId);
            _driver = (UpLoadBase)_pluginService.GetDriver(DeviceId, driverPlugin);
            if (driverPlugin != null)
            {
                Propertys = _pluginService.GetDriverProperties(_driver);
            }
            else
            {
                throw Oops.Oh($"找不到驱动{driverPlugin.AssembleName}");
            }
            //设置插件配置项
            SetPluginProperties(Device.DevicePropertys);
            return _driver;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{_device.Name}初始化失败");
        }
        return null;
    }
    private void InitDriver()
    {
        //初始化插件
        _driver.Init(_logger, _device);
        //变量分包
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
    /// 已经停止
    /// </summary>
    public bool IsExited;

    /// <summary>
    /// 开始前
    /// </summary>
    public async Task<bool> BeforeActionAsync(CancellationToken cancellationToken)
    {
        try
        {
            IsExited = false;
            _logger?.LogInformation($"{_device.Name}上传设备线程开始");

            StoppingTokens.Add(new());

            if (_driver != null)
            {
                InitDriver();
            }
            else
            {
                Device.ErrorCount = 999;
                Device.LastErrorMessage = "获取插件失败";
                return false;
            }
            try
            {
                if (Device?.KeepRun == true)
                {
                    //驱动插件执行循环前方法
                    Device.ActiveTime = DateTime.UtcNow;
                    await _driver?.BeforStartAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, _device.Name + "开始前发生错误");
                Device.ErrorCount = 999;
                Device.LastErrorMessage = "开始前发生错误：" + ex.Message;
            }
            isInitSuccess = true;
            return isInitSuccess;

        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, _device.Name + "初始化失败");
            Device.ErrorCount = 999;
            Device.LastErrorMessage = "初始化失败：" + ex.Message;
        }
        isInitSuccess = false;
        return isInitSuccess;
    }
    /// <summary>
    /// 结束后
    /// </summary>
    public void FinishAction()
    {
        IsExited = true;
        try
        {
            _logger?.LogInformation($"{_device.Name}上传线程停止中");
            _driver?.SafeDispose();
            _pluginService.DeleteDriver(DeviceId, Device.PluginId);
            _logger?.LogInformation($"{_device.Name}上传线程已停止");
        }
        catch (Exception ex)
        {
            _logger.LogError($"{Device.Name} 释放失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 初始化，在设备子线程创建或更新时才会执行
    /// </summary>
    public void Init(UploadDeviceRunTime device)
    {
        if (device == null) return;
        try
        {
            _device = device;
            var scope = _scopeFactory.CreateScope();
            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            _logger = loggerFactory.CreateLogger("上传设备:" + _device.Name);
            //更新插件信息
            CreatDriver();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, device.Name);
        }
    }
    /// <summary>
    /// 运行
    /// </summary>
    public async Task<ThreadRunReturn> RunActionAsync(CancellationToken cancellationToken)
    {
        try
        {

            using CancellationTokenSource StoppingToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, StoppingTokens.LastOrDefault().Token);
            if (_driver == null) return ThreadRunReturn.Continue;

            if (Device?.KeepRun == false)
            {
                return ThreadRunReturn.Continue; ;
            }
            if (StoppingToken.IsCancellationRequested)
                return ThreadRunReturn.Break;

            Device.ActiveTime = DateTime.UtcNow;
            await _driver.ExecuteAsync(StoppingToken.Token);

            if (StoppingToken.IsCancellationRequested)
                return ThreadRunReturn.Break;
            var oper = _driver.IsConnected();
            if (oper.IsSuccess)
            {
                Device.ErrorCount = 0;
            }
            else
            {
                Device.ErrorCount = 999;
            }
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
            Device.ErrorCount += 1;
            Device.LastErrorMessage = ex.Message;
            return ThreadRunReturn.None;
        }
    }

    #endregion


    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        StopThread();
    }




}

