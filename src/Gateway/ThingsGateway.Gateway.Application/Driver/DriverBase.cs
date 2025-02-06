//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Microsoft.Extensions.Localization;

using ThingsGateway.NewLife;
using ThingsGateway.NewLife.Threading;
using ThingsGateway.Razor;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 插件基类
/// </summary>
public abstract class DriverBase : DisposableObject, IDriver
{
    /// <inheritdoc cref="DriverBase"/>
    public DriverBase()
    {
        Localizer = App.CreateLocalizerByType(typeof(DriverBase))!;
    }

    #region 属性

    /// <summary>
    /// 当前设备
    /// </summary>
    public DeviceRuntime? CurrentDevice { get; private set; }

    /// <summary>
    /// 当前设备Id
    /// </summary>
    public long DeviceId => CurrentDevice?.Id ?? 0;

    /// <summary>
    /// 当前设备名称
    /// </summary>
    public string? DeviceName => CurrentDevice?.Name;

    /// <summary>
    /// 调试UI Type，如果不存在，返回null
    /// </summary>
    public virtual Type DriverDebugUIType { get; }

    /// <summary>
    /// 插件UI Type，继承<see cref="IDriverUIBase"/>如果不存在，返回null
    /// </summary>
    public virtual Type DriverUIType { get; }

    /// <summary>
    /// 插件属性UI Type，继承<see cref="IPropertyUIBase"/>如果不存在，返回null
    /// </summary>
    public virtual Type DriverPropertyUIType { get; }

    /// <summary>
    /// 插件变量寄存器UI Type，继承<see cref="IAddressUIBase"/>如果不存在，返回null
    /// </summary>
    public virtual Type DriverVariableAddressUIType { get; }

    /// <summary>
    /// 插件配置项
    /// </summary>
    public abstract object DriverProperties { get; }

    /// <summary>
    /// 是否执行了Start方法
    /// </summary>
    public bool IsStarted { get; protected set; } = false;

    /// <summary>
    /// 是否初始化成功，失败时不再执行，等待检测重启
    /// </summary>
    public bool IsInitSuccess { get; internal set; } = true;

    /// <summary>
    /// 是否采集插件
    /// </summary>
    public virtual bool? IsCollectDevice => CurrentDevice?.IsCollect;

    /// <summary>
    /// 暂停
    /// </summary>
    public bool Pause => CurrentDevice?.Pause == true;

    private List<IEditorItem> pluginPropertyEditorItems;
    public List<IEditorItem> PluginPropertyEditorItems
    {
        get
        {
            if (pluginPropertyEditorItems == null)
            {
                pluginPropertyEditorItems = PluginServiceUtil.GetEditorItems(DriverProperties?.GetType()).ToList();
            }
            return pluginPropertyEditorItems;
        }
    }

    /// <summary>
    /// 底层驱动，有可能为null
    /// </summary>
    public virtual IDevice? FoundationDevice { get; }

    private IStringLocalizer Localizer { get; }

    #endregion 属性

    #region 变量管理

    private WaitLock NewVariableLock = new();

    /// <summary>
    /// 动态刷新变量
    /// </summary>
    public async Task RefreshVariableAsync()
    {
        try
        {
            await NewVariableLock.WaitAsync().ConfigureAwait(false);
            AfterVariablesChanged();
        }
        finally
        {
            NewVariableLock.Release();
        }
    }

    #endregion

    /// <summary>
    /// 暂停
    /// </summary>
    /// <param name="pause">暂停</param>
    public void PauseThread(bool pause)
    {
        lock (this)
        {
            if (CurrentDevice == null) return;
            var str = pause == true ? "DeviceTaskPause" : "DeviceTaskContinue";
            LogMessage?.LogInformation(Localizer[str, DeviceName]);
            CurrentDevice.Pause = pause;
        }
    }

    public override string ToString()
    {
        return FoundationDevice?.ToString() ?? base.ToString();
    }

    #region 任务管理器传入

    public IDeviceThreadManage DeviceThreadManage { get; internal set; }

    public string PluginDirectory => CurrentChannel?.PluginInfo?.Directory;

    public ChannelRuntime CurrentChannel => DeviceThreadManage?.CurrentChannel;

    #endregion 任务管理器传入

    #region 日志

    private WaitLock SetLogLock = new();
    public async Task SetLogAsync(bool enable, LogLevel? logLevel = null, bool upDataBase = true)
    {
        try
        {
            await SetLogLock.WaitAsync().ConfigureAwait(false);
            bool up = false;

            if (upDataBase && (CurrentDevice.LogEnable != enable || (logLevel != null && CurrentDevice.LogLevel != logLevel)))
            {
                up = true;
            }

            CurrentDevice.LogEnable = enable;
            if (logLevel != null)
                CurrentDevice.LogLevel = logLevel.Value;
            if (up)
            {
                //更新数据库
                await GlobalData.DeviceService.UpdateLogAsync(CurrentDevice.Id, CurrentDevice.LogEnable, CurrentDevice.LogLevel).ConfigureAwait(false);
            }

            SetLog(CurrentDevice.LogEnable, CurrentDevice.LogLevel);

        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
        }
        finally
        {
            SetLogLock.Release();
        }
    }
    private void SetLog(bool enable, LogLevel? logLevel = null)
    {
        // 如果日志使能状态为 true
        if (enable)
        {

            LogMessage.LogLevel = logLevel ?? TouchSocket.Core.LogLevel.Trace;
            // 移除旧的文件日志记录器并释放资源
            if (TextLogger != null)
            {
                LogMessage.RemoveLogger(TextLogger);
                TextLogger?.Dispose();
            }

            // 创建新的文件日志记录器，并设置日志级别为 Trace
            TextLogger = TextFileLogger.GetMultipleFileLogger(LogPath);
            TextLogger.LogLevel = logLevel ?? TouchSocket.Core.LogLevel.Trace;
            // 将文件日志记录器添加到日志消息组中
            LogMessage.AddLogger(TextLogger);
        }
        else
        {
            if (logLevel != null)
                LogMessage.LogLevel = logLevel.Value;
            //LogMessage.LogLevel = TouchSocket.Core.LogLevel.Warning;
            // 如果日志使能状态为 false，移除文件日志记录器并释放资源
            if (TextLogger != null)
            {
                LogMessage.RemoveLogger(TextLogger);
                TextLogger?.Dispose();
            }
        }
    }

    private TextFileLogger? TextLogger;

    public LoggerGroup LogMessage { get; private set; }

    public string LogPath => CurrentDevice?.LogPath;

    #endregion

    #region 插件生命周期
    Microsoft.Extensions.Logging.ILogger? _logger;
    /// <summary>
    /// 内部初始化
    /// </summary>
    internal void InitDevice(DeviceRuntime device)
    {
        CurrentDevice = device;

        _logger = App.RootServices.GetService<Microsoft.Extensions.Logging.ILoggerFactory>().CreateLogger($"Driver[{CurrentDevice.Name}]");

        LogMessage = new LoggerGroup() { LogLevel = TouchSocket.Core.LogLevel.Warning };//不显示调试日志

        // 添加默认日志记录器
        LogMessage.AddLogger(new EasyLogger(Log_Out) { LogLevel = TouchSocket.Core.LogLevel.Trace });

        SetLog(CurrentDevice.LogEnable, CurrentDevice.LogLevel);

        device.Driver = this;

        ProtectedInitDevice(device);
    }

    private void Log_Out(TouchSocket.Core.LogLevel level, object arg2, string arg3, Exception exception)
    {
        if (level >= TouchSocket.Core.LogLevel.Warning)
        {
            CurrentDevice.SetDeviceStatus(lastErrorMessage: arg3);
        }
        _logger?.Log_Out(level, arg2, arg3, exception);
    }

    /// <summary>
    /// 在循环任务开始之前
    /// </summary>
    /// <param name="cancellationToken">取消操作的令牌。</param>
    /// <returns>表示异步操作的任务。</returns>
    internal async ValueTask StartAsync(CancellationToken cancellationToken)
    {
        // 如果已经执行过初始化，则直接返回
        if (IsStarted)
        {
            return;
        }
        // 如果已经取消了操作，则直接返回
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        try
        {

            // 记录设备任务开始信息
            LogMessage?.LogInformation(Localizer["DeviceTaskStart", DeviceName]);

            var timeout = 60; // 设置超时时间为 60 秒

            try
            {
                // 异步执行初始化操作，并设置超时时间
                await ProtectedStartAsync(cancellationToken).WaitAsync(TimeSpan.FromSeconds(timeout), cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            catch (TimeoutException)
            {
                // 如果初始化操作超时，则记录警告信息
                LogMessage?.LogWarning(Localizer["DeviceTaskStartTimeout", DeviceName, timeout]);
            }

            // 设置设备状态为当前时间
            CurrentDevice.SetDeviceStatus(TimerX.Now);
        }
        catch (Exception ex)
        {
            // 记录执行过程中的异常信息，并设置设备状态为异常
            LogMessage?.LogWarning(ex, "Before Start error");
            CurrentDevice.SetDeviceStatus(TimerX.Now, true, ex.Message);
        }
        finally
        {
            // 标记已执行初始化
            IsStarted = true;
        }
    }

    /// <summary>
    /// 循环任务
    /// </summary>
    /// <param name="cancellationToken">取消操作的令牌。</param>
    /// <returns>表示异步操作结果的枚举。</returns>
    internal async ValueTask<ThreadRunReturnTypeEnum> ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            // 如果取消操作被请求，则返回中断状态
            if (cancellationToken.IsCancellationRequested)
            {
                return ThreadRunReturnTypeEnum.Break;
            }

            // 如果标志为停止，则暂停执行
            if (Pause)
            {
                // 暂停
                return ThreadRunReturnTypeEnum.Continue;
            }

            // 再次检查取消操作是否被请求
            if (cancellationToken.IsCancellationRequested)
            {
                return ThreadRunReturnTypeEnum.Break;
            }

            // 获取设备连接状态并更新设备活动时间
            if (IsConnected())
            {
                // 如果不是采集设备，则直接更新设备状态为当前时间
                if (IsCollectDevice == false)
                {
                    CurrentDevice.SetDeviceStatus(TimerX.Now, false);
                }
                else
                {
                    // 否则，更新设备活动时间
                    CurrentDevice.SetDeviceStatus(TimerX.Now);
                }
            }
            else
            {
                // 如果设备未连接，则更新设备状态为断开
                if (!IsConnected())
                {
                    // 如果不是采集设备，则直接更新设备状态为当前时间
                    if (IsCollectDevice == false)
                    {
                        CurrentDevice.SetDeviceStatus(TimerX.Now, true);
                    }
                }
            }

            // 再次检查取消操作是否被请求
            if (cancellationToken.IsCancellationRequested)
            {
                return ThreadRunReturnTypeEnum.Break;
            }

            // 执行任务操作
            await ProtectedExecuteAsync(cancellationToken).ConfigureAwait(false);

            // 再次检查取消操作是否被请求
            if (cancellationToken.IsCancellationRequested)
            {
                return ThreadRunReturnTypeEnum.Break;
            }

            // 正常返回None状态
            return ThreadRunReturnTypeEnum.None;
        }
        catch (OperationCanceledException)
        {
            return ThreadRunReturnTypeEnum.Break;
        }
        catch (ObjectDisposedException)
        {
            return ThreadRunReturnTypeEnum.Break;
        }
        catch (Exception ex)
        {
            // 记录异常信息，并更新设备状态为异常
            LogMessage?.LogError(ex, "Execute");
            CurrentDevice.SetDeviceStatus(TimerX.Now, true, ex.Message);
            return ThreadRunReturnTypeEnum.None;
        }
    }

    /// <summary>
    /// 已停止循环任务，释放插件
    /// </summary>
    internal void Stop()
    {

        if (!DisposedValue)
        {
            lock (this)
            {
                if (!DisposedValue)
                {
                    try
                    {
                        // 执行资源释放操作
                        Dispose();
                    }
                    catch (Exception ex)
                    {
                        // 记录 Dispose 方法执行失败的错误信息
                        LogMessage?.LogError(ex, "Dispose");
                    }

                    // 记录设备线程已停止的信息
                    LogMessage?.LogInformation(Localizer["DeviceTaskStop", DeviceName]);
                }
            }
        }
    }

    #endregion 插件生命周期

    #region 插件重写
    /// <summary>
    /// 内部初始化
    /// </summary>
    protected virtual void ProtectedInitDevice(DeviceRuntime device)
    {

    }

    /// <summary>
    /// 当前关联的变量
    /// </summary>
    public Dictionary<string, VariableRuntime> VariableRuntimes { get; protected set; } = new();

    /// <summary>
    /// 是否连接成功
    /// </summary>
    public virtual bool IsConnected()
    {
        return FoundationDevice?.OnLine == true;
    }

    /// <summary>
    /// 初始化，在开始前执行，异常时会标识重启
    /// </summary>
    /// <param name="channel">通道，当通道类型为<see cref="ChannelTypeEnum.Other"/>时，传入null</param>
    internal protected virtual void InitChannel(IChannel? channel = null)
    {
        if (channel != null)
            channel.SetupAsync(channel.Config.Clone());
        AfterVariablesChanged();
    }

    /// <summary>
    /// 变量更改后， 重新初始化变量列表，获取设备变量打包列表/特殊方法列表等
    /// </summary>
    public abstract void AfterVariablesChanged();

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        FoundationDevice?.Dispose();
        base.Dispose(disposing);
    }

    /// <summary>
    /// 开始通讯执行的方法
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task ProtectedStartAsync(CancellationToken cancellationToken)
    {
        if (FoundationDevice?.Channel != null)
            await FoundationDevice.Channel.ConnectAsync(FoundationDevice.Channel.ChannelOptions.ConnectTimeout, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 间隔执行
    /// </summary>
    protected abstract ValueTask ProtectedExecuteAsync(CancellationToken cancellationToken);



    #endregion 插件重写
}
