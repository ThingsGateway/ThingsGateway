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

using Microsoft.Extensions.Logging;
using System.Collections.Frozen;
using System.Text;

using LogLevel = TouchSocket.Core.LogLevel;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 插件基类
/// </summary>
public abstract class DriverBase : AsyncDisposableObject, IDriver
{
    /// <inheritdoc cref="DriverBase"/>
    public DriverBase()
    {

        Localizer = App.CreateLocalizerByType(typeof(DriverBase))!;
    }

    #region 属性


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




    private IReadOnlyList<IEditorItem> pluginPropertyEditorItems;
    public IReadOnlyList<IEditorItem> PluginPropertyEditorItems
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

    private IStringLocalizer Localizer { get; }

    #endregion 属性

    public abstract ChannelTypeEnum[] SupportedChannelTypes();
    //{
    //    return [ChannelTypeEnum.Other];
    //}
    public virtual bool GetAuthentication(out DateTime? expireTime)
    {
        expireTime = null;
        return true;
    }

    public string GetAuthString()
    {
        if (PluginServiceUtil.IsEducation(GetType()))
        {
            using ValueStringBuilder stringBuilder = new();
            var ret = GetAuthentication(out var expireTime);
            if (ret)
            {
                stringBuilder.Append(Localizer["Authorized"]);
            }
            else
            {
                stringBuilder.Append(Localizer["Unauthorized"]);
            }

            stringBuilder.Append("   ");
            if (expireTime.HasValue && (DateTime.Now - expireTime.Value).TotalHours > -72)
            {
                stringBuilder.Append(',');
                stringBuilder.Append(Localizer["ExpireTime", expireTime.Value.ToString("yyyy-MM-dd HH")]);
            }

            return stringBuilder.ToString();
        }
        return string.Empty;
    }



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
    /// 暂停
    /// </summary>
    public bool Pause => CurrentDevice?.Pause == true;

    protected object pauseLock = new object();
    /// <summary>
    /// 暂停
    /// </summary>
    /// <param name="pause">暂停</param>
    public virtual void PauseThread(bool pause)
    {
        lock (pauseLock)
        {
            if (CurrentDevice == null) return;
            LogMessage?.LogInformation(pause == true ? string.Format(AppResource.DeviceTaskPause, DeviceName) : string.Format(AppResource.DeviceTaskContinue, DeviceName));
            CurrentDevice.Pause = pause;

            if (CurrentDevice.Pause)
                TaskSchedulerLoop.Stop();
            else
                TaskSchedulerLoop.Start();
        }
    }

    #region 任务管理器传入

    public IDeviceThreadManage DeviceThreadManage { get; internal set; }

    public string PluginDirectory => CurrentChannel?.PluginInfo?.Directory;

    public ChannelRuntime CurrentChannel => DeviceThreadManage?.CurrentChannel;

    #endregion 任务管理器传入

    #region 日志

    private readonly WaitLock SetLogLock = new(nameof(DriverBase));
    public async Task SetLogAsync(LogLevel? logLevel = null, bool upDataBase = true)
    {
        try
        {
            await SetLogLock.WaitAsync().ConfigureAwait(false);
            bool up = false;

            if (upDataBase && ((logLevel != null && CurrentDevice.LogLevel != logLevel)))
            {
                up = true;
            }

            if (logLevel != null)
                CurrentDevice.LogLevel = logLevel.Value;
            if (up && CurrentDevice.Id != MemoryConst.MemoryDeviceId)
            {
                //更新数据库
                await GlobalData.DeviceService.UpdateLogAsync(CurrentDevice.Id, CurrentDevice.LogLevel).ConfigureAwait(false);
            }

            SetLog(CurrentDevice.LogLevel);
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
    private void SetLog(LogLevel? logLevel = null)
    {
        LogMessage.LogLevel = logLevel ?? TouchSocket.Core.LogLevel.Trace;
        // 移除旧的文件日志记录器并释放资源
        if (TextLogger != null)
        {
            LogMessage?.RemoveLogger(TextLogger);
            TextLogger?.Dispose();
        }

        // 创建新的文件日志记录器，并设置日志级别为 Trace
        TextLogger = TextFileLogger.GetMultipleFileLogger(LogPath);
        TextLogger.LogLevel = logLevel ?? TouchSocket.Core.LogLevel.Trace;
        // 将文件日志记录器添加到日志消息组中
        LogMessage?.AddLogger(TextLogger);
    }

    private TextFileLogger? TextLogger;

    public TouchSocket.Core.LoggerGroup LogMessage { get; private set; }

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

        LogMessage = new TouchSocket.Core.LoggerGroup() { LogLevel = TouchSocket.Core.LogLevel.Warning };//不显示调试日志

        // 添加默认日志记录器
        LogMessage?.AddLogger(new TouchSocket.Core.EasyLogger(Log_Out) { LogLevel = TouchSocket.Core.LogLevel.Trace });

        SetLog(CurrentDevice.LogLevel);

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
    /// 在任务开始之前
    /// </summary>
    /// <param name="cancellationToken">取消操作的令牌。</param>
    /// <returns>表示异步操作的任务。</returns>
    internal virtual async Task StartAsync(CancellationToken cancellationToken)
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
            LogMessage?.LogInformation(string.Format(AppResource.DeviceTaskStart, DeviceName));

            var timeout = 60; // 设置超时时间为 60 秒

            var task = ProtectedStartAsync(cancellationToken);
            try
            {
                // 异步执行初始化操作，并设置超时时间
                await task.WaitAsync(TimeSpan.FromSeconds(timeout), cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            catch (TimeoutException)
            {
                // 如果初始化操作超时，则记录警告信息
                LogMessage?.LogInformation(string.Format(AppResource.DeviceTaskStartTimeout, DeviceName, timeout));
            }

            // 设置设备状态为当前时间
            CurrentDevice.SetDeviceStatus(TimerX.Now, false);
        }
        catch (Exception ex)
        {
            await OnProtectedStartError(cancellationToken).ConfigureAwait(false);
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

    protected internal TaskSchedulerLoop TaskSchedulerLoop { get; protected set; }

    /// <summary>
    /// 获取任务
    /// </summary>
    /// <param name="cancellationToken">取消操作的令牌。</param>
    /// <returns>表示异步操作结果的枚举。</returns>
    internal virtual void GetTasks(CancellationToken cancellationToken)
    {
        TaskSchedulerLoop = new(ProtectedGetTasks(cancellationToken));

        //var count = GlobalData.ChannelThreadManage.DeviceThreadManages.Select(a => a.Value.TaskCount).Sum();
        //ThreadPool.GetMinThreads(out var wt, out var io);
        //if (wt < count + 128)
        //{
        //    wt = count + 256;
        //    ThreadPool.SetMinThreads(wt, io);
        //    GlobalData.GatewayMonitorHostedService.Logger.LogInformation($"set min threads count {wt}, device tasks count {count}");
        //}

    }

    protected abstract List<IScheduledTask> ProtectedGetTasks(CancellationToken cancellationToken);

    protected WaitLock stopLock = new(nameof(DriverBase));
    /// <summary>
    /// 已停止任务，释放插件
    /// </summary>
    internal virtual async Task StopAsync()
    {
        if (!DisposedValue)
        {
            await stopLock.WaitAsync().ConfigureAwait(false);
            try
            {


                if (!DisposedValue)
                {
                    // 记录设备线程已停止的信息
                    LogMessage?.LogInformation(string.Format(AppResource.DeviceTaskStop, DeviceName));

                    await Task.Delay(50).ConfigureAwait(false);

                    // 执行资源释放操作
                    await this.SafeDisposeAsync().ConfigureAwait(false);


                }
            }
            catch (Exception ex)
            {
                // 记录 Dispose 方法执行失败的错误信息
                LogMessage?.LogError(ex, "Dispose");
            }
            finally
            {
                stopLock.Release();
            }
        }
    }

    protected override async Task DisposeAsync(bool disposing)
    {
        await base.DisposeAsync(disposing).ConfigureAwait(false);
        if (TaskSchedulerLoop != null)
        {
            lock (TaskSchedulerLoop)
            {
                TaskSchedulerLoop.Stop();
            }
        }

        TextLogger?.Dispose();
        _logger?.TryDispose();
        IdVariableRuntimes = null;
        var device = CurrentDevice;
        if (device != null)
            device.Driver = null;

        LogMessage?.Logs?.ForEach(a => a.TryDispose());
        LogMessage = null;
        pluginPropertyEditorItems = null;
        DeviceThreadManage = null;
    }


    /// <summary>
    /// 开始通讯执行的方法
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    internal virtual Task OnProtectedStartError(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    #endregion 插件生命周期

    #region 插件重写


    /// <summary>
    /// 开始通讯执行的方法
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual Task ProtectedStartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 内部初始化
    /// </summary>
    internal virtual void ProtectedInitDevice(DeviceRuntime device)
    {
    }

    /// <summary>
    /// 当前关联的变量
    /// </summary>
    public IReadOnlyDictionary<long, VariableRuntime> IdVariableRuntimes { get; protected set; } = FrozenDictionary<long, VariableRuntime>.Empty;

    public abstract bool IsConnected();

    public ChannelObject? ChannelObject { get; private set; }
    public IChannel? Channel => ChannelObject?.Channel;

    /// <summary>
    /// 初始化，在开始前执行，异常时会标识重启
    /// </summary>
    /// <param name="channelObject">通道</param>
    /// <param name="cancellationToken"></param>
    internal protected virtual async Task InitChannelAsync(ChannelObject channelObject, CancellationToken cancellationToken)
    {
        ChannelObject = channelObject;

        if (Channel != null && Channel.PluginManager == null)
        {
            try
            {
                await Channel.Lock.WaitAsync(cancellationToken).ConfigureAwait(false);

                if (Channel != null && Channel.PluginManager == null)
                {
                    await Channel.SetupAsync(Channel.Config.CloneAndDispose()).ConfigureAwait(false);
                }
            }
            finally
            {
                Channel.Lock.Release();
            }
        }


        await AfterVariablesChangedAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 变量更改后， 重新初始化变量列表，获取设备变量打包列表/特殊方法列表等
    /// </summary>
    public abstract Task AfterVariablesChangedAsync(CancellationToken cancellationToken);

    #endregion 插件重写


}
