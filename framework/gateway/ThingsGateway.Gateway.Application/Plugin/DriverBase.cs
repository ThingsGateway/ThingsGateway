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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 插件基类,注意继承的插件的命名空间需要符合<see cref="ExportHelpers.PluginLeftName"/>前置名称
/// </summary>
public abstract class DriverBase : DisposableObject
{
    /// <inheritdoc cref="DriverBase"/>
    public DriverBase()
    {
        _serviceScope = App.GetService<IServiceScopeFactory>().CreateScope();
        _driverPluginService = _serviceScope.ServiceProvider.GetRequiredService<DriverPluginService>();
        _globalDeviceData = _serviceScope.ServiceProvider.GetRequiredService<GlobalDeviceData>();
        DevicePropertys = _driverPluginService.GetDriverProperties(this);
        RpcSingletonService = _serviceScope.ServiceProvider.GetService<RpcSingletonService>();

        //底层配置
        FoundataionConfig = new TouchSocketConfig();
        LogMessage = new LoggerGroup() { LogLevel = ThingsGateway.Foundation.Core.LogLevel.Trace };
        LogMessage.AddLogger(new EasyLogger(Log_Out) { LogLevel = ThingsGateway.Foundation.Core.LogLevel.Trace });
        FoundataionConfig.ConfigureContainer(a => a.RegisterSingleton<ILog>(LogMessage));
    }
    public override string ToString()
    {
        return _readWrite?.ToString() ?? base.ToString();
    }
    /// <summary>
    /// 当前设备
    /// </summary>
    public DeviceRunTime CurrentDevice { get; protected set; }

    /// <summary>
    /// 当前设备Id
    /// </summary>
    public long DeviceId => CurrentDevice?.Id ?? 0;

    /// <summary>
    /// 当前设备名称
    /// </summary>
    public string DeviceName => CurrentDevice?.Name;

    /// <summary>
    /// 当前设备全部设备属性，执行初始化后获取正确值
    /// </summary>
    public List<DependencyProperty> DevicePropertys { get; private set; }

    public DeviceThread DeviceThread { get; set; }
    /// <summary>
    /// 当前插件目录
    /// </summary>
    public string Directory { get; internal set; }

    /// <summary>
    /// 调试UI Type，如果不存在，返回null
    /// </summary>
    public abstract Type DriverDebugUIType { get; }

    /// <summary>
    /// 插件配置项 ，继承实现<see cref="DriverPropertyBase"/>后，返回继承类，如果不存在，返回null
    /// </summary>
    public abstract DriverPropertyBase DriverPropertys { get; }

    /// <summary>
    /// 插件自定义 UI Type，如果不存在，返回null
    /// </summary>
    public abstract Type DriverUIType { get; }

    /// <summary>
    /// 是否初始化成功
    /// </summary>
    public bool IsInitSuccess { get; protected set; } = true;

    /// <summary>
    /// 是否输出日志
    /// </summary>
    public bool IsLogOut { get; set; }

    public bool? KeepRun => CurrentDevice?.KeepRun;
    /// <summary>
    /// 日志
    /// </summary>
    protected ILogger Logger { get; set; }

    /// <summary>
    /// 日志信息，包含报文/报错等
    /// </summary>
    public ConcurrentLinkedList<(Microsoft.Extensions.Logging.LogLevel, string)> MessageItems { get; set; } = new();

    /// <summary>
    /// 底层日志,如果需要在Blazor界面中显示报文日志，需要输出字符串头部为<see cref="FoundationConst.LogMessageHeader"/>的日志
    /// </summary>
    public LoggerGroup LogMessage { get; private set; }

    /// <summary>
    /// <inheritdoc cref="ThingsGateway.Foundation.Core.TouchSocketConfig"/>
    /// </summary>
    protected internal TouchSocketConfig FoundataionConfig { get; set; }

    /// <summary>
    /// 全局插件服务
    /// </summary>
    protected DriverPluginService _driverPluginService { get; private set; }

    protected GlobalDeviceData _globalDeviceData { get; private set; }
    /// <summary>
    /// 一般底层驱动，也有可能为null
    /// </summary>
    protected abstract IReadWrite _readWrite { get; }

    public RpcSingletonService RpcSingletonService { get; }
    /// <summary>
    /// IServiceScope
    /// </summary>
    protected IServiceScope _serviceScope { get; }

    /// <summary>
    /// 结束后
    /// </summary>
    public virtual async Task AfterStopAsync()
    {
        try
        {
            Logger?.LogInformation($"{DeviceName}：线程停止中");
            try
            {
                await ProtectedAfterStopAsync().WaitAsync(TimeSpan.FromMinutes(1));
            }
            catch (TimeoutException)
            {
                Logger?.LogWarning($"{DeviceName}：线程停止超时");
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, $"插件执行{nameof(ProtectedAfterStopAsync)}方法失败");
            }
            try
            {
                await DisposeAsync();
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, $"插件执行{nameof(DisposeAsync)}方法失败");
            }
            Logger?.LogInformation($"{DeviceName}：线程已停止");
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, $"插件执行{nameof(AfterStopAsync)}方法失败");
        }
        finally
        {
            IsInitSuccess = false;
        }
    }

    /// <summary>
    /// 线程开始时执行
    /// </summary>
    /// <returns></returns>
    public virtual async Task BeforStartAsync(ISenderClient senderClient, CancellationToken cancellationToken)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested) return;
            CurrentDevice.SetDeviceStatus(DateTimeExtensions.CurrentDateTime);
            if (CurrentDevice == null)
            {
                Logger?.LogWarning($"{nameof(CurrentDevice)}不能为null");
                IsInitSuccess = false;
                return;
            }

            Logger?.LogInformation($"{DeviceName}：设备线程开始");
            Init(senderClient);
            try
            {
                if (KeepRun == true)
                {
                    try
                    {
                        await ProtectedBeforStartAsync(cancellationToken).WaitAsync(TimeSpan.FromMinutes(1));
                    }
                    catch (TimeoutException)
                    {
                        Logger?.LogWarning($"{DeviceName}：线程初始启动超时");
                    }
                    CurrentDevice.SetDeviceStatus(DateTimeExtensions.CurrentDateTime);
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, $"插件执行{nameof(ProtectedBeforStartAsync)}方法失败");
                CurrentDevice.SetDeviceStatus(null, CurrentDevice.ErrorCount + 1, ex.Message);
            }

            IsInitSuccess = true;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, $"插件执行{nameof(BeforStartAsync)}方法失败");
            IsInitSuccess = false;
            CurrentDevice.SetDeviceStatus(DateTimeExtensions.CurrentDateTime, 999, ex.Message);
        }
    }
    protected virtual bool IsUploadBase { get; } = false;
    /// <summary>
    /// 间隔执行
    /// </summary>
    public virtual async Task<ThreadRunReturn> ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return ThreadRunReturn.Break;
            if (CurrentDevice == null)
            {
                LogMessage?.Warning($"{nameof(CurrentDevice)}不能为null");
                return ThreadRunReturn.Break;
            }
            if (KeepRun == false)
            {
                //暂停
                return ThreadRunReturn.Continue;
            }

            if (cancellationToken.IsCancellationRequested)
                return ThreadRunReturn.Break;


            //获取设备连接状态
            if (IsConnected())
            {
                //更新设备活动时间
                CurrentDevice.SetDeviceStatus(DateTimeExtensions.CurrentDateTime);
            }
            else
            {
                //if (!IsUploadBase)
                CurrentDevice.SetDeviceStatus(DateTimeExtensions.CurrentDateTime, 999);
            }

            await ProtectedExecuteAsync(cancellationToken);

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
            LogMessage?.LogError(ex, $"插件执行{nameof(ExecuteAsync)}方法失败");
            CurrentDevice.SetDeviceStatus(null, CurrentDevice.ErrorCount + 1, ex.Message);
            return ThreadRunReturn.Continue;
        }
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public virtual void Init(DeviceRunTime device)
    {
        IsLogOut = device.IsLogOut;
        CurrentDevice = device;
        device.Log = LogMessage;
    }

    /// <summary>
    /// 共享链路需重新设置适配器时调用该方法，注意非IReadWrite设备需重写
    /// </summary>
    public virtual void InitDataAdapter()
    {
        _readWrite?.SetDataAdapter();
    }

    /// <summary>
    /// 是否连接成功，注意非IReadWrite设备需重写
    /// </summary>
    public virtual bool IsConnected()
    {
        return _readWrite?.IsConnected() == true;
    }

    /// <summary>
    /// 暂停
    /// </summary>
    /// <param name="keepRun">是否继续</param>
    public virtual void PasueThread(bool keepRun)
    {
        lock (this)
        {
            if (CurrentDevice == null) return;
            var str = keepRun == false ? "设备线程暂停" : "设备线程继续";
            Logger?.LogInformation($"{DeviceName}：{str}");
            this.CurrentDevice.KeepRun = keepRun;
        }
    }
    private static string TruncateString(string originalString, int startLength, int endLength)
    {
        if (originalString.Length <= startLength + endLength)
        {
            // 字符串长度不超过截取长度，不需要截取
            return originalString;
        }
        else
        {
            // 截取字符串并添加省略号
            string truncatedString = $"{originalString.Substring(0, startLength)}{Environment.NewLine}...{Environment.NewLine}{originalString.Substring(originalString.Length - endLength)}";
            return truncatedString;
        }
    }
    /// <summary>
    /// 设备报文输出
    /// </summary>
    internal virtual void AddMessageItem(ThingsGateway.Foundation.Core.LogLevel arg1, object arg2, string arg3, Exception arg4)
    {
        if (IsLogOut)
        {
            //常驻内存注意
            if (arg3.StartsWith(FoundationConst.LogMessageHeader))
            {
                MessageItems.Add(((Microsoft.Extensions.Logging.LogLevel)(byte)arg1, $"{DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat()} - {TruncateString(arg3, 300, 200)}"));
                if (MessageItems.Count > 2000)
                {//超长字符串或者大于阈值
                    MessageItems.Clear();
                }
            }
        }
    }

    /// <summary>
    /// 默认延时
    /// </summary>
    /// <param name="interval"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected static async Task Delay(int interval, CancellationToken cancellationToken)
    {
        await Task.Delay(interval - DeviceThread.CycleInterval, cancellationToken);
    }
    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        _readWrite?.Disconnect();
        _readWrite?.SafeDispose();
        FoundataionConfig.Dispose();
        base.Dispose(disposing);
    }

    /// <summary>
    /// 内部初始化，在开始前执行
    /// </summary>
    /// <param name="client"></param>
    protected abstract void Init(ISenderClient client = null);
    /// <summary>
    /// 底层错误日志输出
    /// </summary>
    protected virtual void Log_Out(ThingsGateway.Foundation.Core.LogLevel arg1, object arg2, string arg3, Exception arg4)
    {
        if (arg1 >= ThingsGateway.Foundation.Core.LogLevel.Warning)
        {
            CurrentDevice.SetDeviceStatus(lastErrorMessage: arg3);
            if (IsLogOut)
            {
                MessageItems.Add(((Microsoft.Extensions.Logging.LogLevel)(byte)arg1, DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat() + " - " + arg3));
                if (MessageItems.Count > 2500)
                {
                    MessageItems.Clear();
                }
            }
        }
        Logger.Log_Out(arg1, arg2, arg3, arg4);
    }

    /// <summary>
    /// 结束通讯执行的方法
    /// </summary>
    /// <returns></returns>
    protected virtual Task ProtectedAfterStopAsync()
    {
        _readWrite?.Disconnect();
        return Task.CompletedTask;
    }

    /// <summary>
    /// 开始通讯执行的方法
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task ProtectedBeforStartAsync(CancellationToken cancellationToken)
    {
        if (_readWrite != null)
            await _readWrite.ConnectAsync(cancellationToken);
        //获取设备连接状态
        if (IsConnected())
        {
            //更新设备活动时间
            CurrentDevice.SetDeviceStatus(DateTimeExtensions.CurrentDateTime, 0);
        }
    }
    /// <summary>
    /// 间隔执行
    /// </summary>
    protected abstract Task ProtectedExecuteAsync(CancellationToken cancellationToken);
}