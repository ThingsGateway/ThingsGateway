//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

using NewLife.Threading;

using ThingsGateway.Core.Extension;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 插件基类
/// </summary>
public abstract class DriverBase : DisposableObject
{
    /// <inheritdoc cref="DriverBase"/>
    public DriverBase()
    {
        PluginService = NetCoreApp.RootServices.GetRequiredService<IPluginService>();
        RpcService = NetCoreApp.RootServices.GetRequiredService<IRpcService>();
        Localizer = NetCoreApp.CreateLocalizerByType(typeof(DriverBase))!;
    }

    /// <summary>
    /// 当前设备
    /// </summary>
    public DeviceRunTime? CurrentDevice { get; protected set; }

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
    /// 插件配置项
    /// </summary>
    public abstract object DriverProperties { get; }

    /// <summary>
    /// 插件UI Type，继承<see cref="IDriverUIBase"/>如果不存在，返回null
    /// </summary>
    public virtual Type DriverUIType { get; }

    /// <summary>
    /// 是否执行了BeforStart方法
    /// </summary>
    public bool IsBeforStarted { get; protected set; } = false;

    /// <summary>
    /// 是否采集插件
    /// </summary>
    public virtual bool IsCollectDevice => CurrentDevice.PluginType == PluginTypeEnum.Collect;

    /// <summary>
    /// 是否初始化成功
    /// </summary>
    public bool IsInitSuccess { get; internal set; } = true;

    /// <summary>
    /// 是否继续运行
    /// </summary>
    public bool KeepRun => CurrentDevice?.KeepRun == true;

    public List<IEditorItem> PluginPropertyEditorItems
    {
        get
        {
            if (CurrentDevice?.PluginName?.IsNullOrWhiteSpace() == true)
            {
                var result = PluginService.GetDriverPropertyTypes(CurrentDevice.PluginName, this);
                return result.EditorItems.ToList();
            }
            else
            {
                var editorItems = PluginServiceUtil.GetEditorItems(DriverProperties?.GetType());
                return editorItems.ToList();
            }
        }
    }

    /// <summary>
    /// 底层驱动，有可能为null
    /// </summary>
    public abstract IProtocol? Protocol { get; }

    /// <summary>
    /// RPC服务
    /// </summary>
    public IRpcService RpcService { get; }

    /// <summary>
    /// 全局插件服务
    /// </summary>
    protected IPluginService PluginService { get; private set; }

    private IStringLocalizer Localizer { get; }

    /// <summary>
    /// 配置底层的通道插件,通常在使用前都执行一次获取新的插件管理器
    /// </summary>
    public virtual void ConfigurePlugins()
    {
        if (Protocol != null)
        {
            FoundataionConfig.ConfigurePlugins(Protocol.ConfigurePlugins());
        }
    }

    /// <summary>
    /// 是否连接成功，注意非通用设备需重写
    /// </summary>
    public virtual bool IsConnected()
    {
        return Protocol?.OnLine == true;
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
            var str = keepRun == false ? "DeviceTaskPause" : "DeviceTaskContinue";
            Logger?.LogInformation(Localizer["str", DeviceName]);
            this.CurrentDevice.KeepRun = keepRun;
        }
    }

    public override string ToString()
    {
        return Protocol?.ToString() ?? base.ToString();
    }

    #region 任务管理器传入

    /// <summary>
    /// 任务管理器
    /// </summary>
    public ChannelThread ChannelThread { get; internal set; }

    /// <summary>
    /// 当前插件目录
    /// </summary>
    public string Directory { get; internal set; }

    /// <summary>
    /// 底层驱动配置
    /// </summary>
    public TouchSocketConfig FoundataionConfig => ChannelThread.FoundataionConfig;

    /// <summary>
    /// 日志
    /// </summary>
    public ILogger Logger => ChannelThread.Logger;

    /// <summary>
    /// 底层日志，需由线程管理器传入
    /// </summary>
    public LoggerGroup LogMessage => ChannelThread.LogMessage;

    /// <summary>
    /// 日志路径
    /// </summary>
    public string LogPath => ChannelThread.LogPath;

    /// <summary>
    /// 写入锁
    /// </summary>
    protected internal EasyLock WriteLock => ChannelThread.WriteLock;

    #endregion 任务管理器传入

    #region 插件生命周期

    /// <summary>
    /// 在停止设备线程后执行的异步操作。
    /// </summary>
    /// <returns>表示异步操作的任务</returns>
    public void AfterStop()
    {
        lock (this)
        {
            if (!DisposedValue)
            {
                try
                {
                    // 执行资源释放操作
                    this.SafeDispose();
                    // 根据是否正在采集设备来从全局设备集合或业务设备集合中移除指定设备ID的驱动程序对象
                    //if (!HostedServiceUtil.ManagementHostedService.StartBusinessDeviceEnable)
                    {
                        if (IsCollectDevice)
                        {
                            //lock (GlobalData.CollectDevices)
                            {
                                GlobalData.CollectDevices.RemoveWhere(it => it.Value.Id == DeviceId);

                                GlobalData.Variables.RemoveWhere(it => it.Value.DeviceId == DeviceId);
                            }
                        }
                        else
                        {
                            //lock (GlobalData.BusinessDevices)
                            {
                                GlobalData.BusinessDevices.RemoveWhere(it => it.Value.Id == DeviceId);
                            }
                        }
                    }

                    IsInitSuccess = true;
                    IsBeforStarted = false;
                }
                catch (Exception ex)
                {
                    // 记录 Dispose 方法执行失败的错误信息
                    Logger?.LogError(ex, "Dispose");
                }

                // 记录设备线程已停止的信息
                Logger?.LogInformation(Localizer["DeviceTaskStop", DeviceName]);
            }
        }
    }

    /// <summary>
    /// 在线程开始之前执行异步操作。
    /// </summary>
    /// <param name="cancellationToken">取消操作的令牌。</param>
    /// <returns>表示异步操作的任务。</returns>
    public virtual async ValueTask BeforStartAsync(CancellationToken cancellationToken)
    {
        // 如果已经执行过初始化，则直接返回
        if (IsBeforStarted)
        {
            return;
        }

        try
        {
            // 如果已经取消了操作，则直接返回
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            // 记录设备任务开始信息
            Logger?.LogInformation(Localizer["DeviceTaskStart", DeviceName]);

            var timeout = 30; // 设置超时时间为30秒

            try
            {
                // 异步执行初始化操作，并设置超时时间
                await ProtectedBeforStartAsync(cancellationToken).WaitAsync(TimeSpan.FromSeconds(timeout), cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            catch (TimeoutException)
            {
                // 如果初始化操作超时，则记录警告信息
                Logger?.LogWarning(Localizer["DeviceTaskStartTimeout", DeviceName, timeout]);
            }

            // 设置设备状态为当前时间
            CurrentDevice.SetDeviceStatus(DateTime.Now);
        }
        catch (Exception ex)
        {
            // 记录执行过程中的异常信息，并设置设备状态为异常
            Logger?.LogWarning(ex, "BeforStart Fail");
            CurrentDevice.SetDeviceStatus(DateTime.Now, 999, ex.Message);
        }
        finally
        {
            // 标记已执行初始化
            IsBeforStarted = true;
        }
    }

    /// <summary>
    /// 执行任务的间隔操作。
    /// </summary>
    /// <param name="cancellationToken">取消操作的令牌。</param>
    /// <returns>表示异步操作结果的枚举。</returns>
    public virtual async ValueTask<ThreadRunReturnTypeEnum> ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            // 如果取消操作被请求，则返回中断状态
            if (cancellationToken.IsCancellationRequested)
            {
                return ThreadRunReturnTypeEnum.Break;
            }

            // 如果标志为停止，则暂停执行
            if (!KeepRun)
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
                // 如果不是采集设备，则直接更新设备状态为当前时间与错误计数
                if (!IsCollectDevice)
                {
                    CurrentDevice.SetDeviceStatus(TimerX.Now, 0);
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
                    // 如果不是采集设备，则直接更新设备状态为当前时间与错误计数
                    if (!IsCollectDevice)
                    {
                        CurrentDevice.SetDeviceStatus(TimerX.Now, CurrentDevice.ErrorCount + 1);
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
            LogMessage?.LogError(ex, $"Execute");
            CurrentDevice.SetDeviceStatus(TimerX.Now, CurrentDevice.ErrorCount + 1, ex.Message);
            return ThreadRunReturnTypeEnum.None;
        }
    }

    /// <summary>
    /// 内部初始化
    /// </summary>
    internal virtual void Init(DeviceRunTime device)
    {
        CurrentDevice = device;
    }

    #endregion 插件生命周期

    #region 插件重写

    /// <summary>
    /// 初始化，在开始前执行，异常时会标识重启
    /// </summary>
    /// <param name="channel">通道，当通道类型为<see cref="ChannelTypeEnum.Other"/>时，传入null</param>
    public abstract void Init(IChannel? channel = null);

    /// <summary>
    /// 获取设备变量打包列表/特殊方法列表
    /// </summary>
    /// <param name="collectVariableRunTimes"></param>
    public virtual void LoadSourceRead(IEnumerable<VariableRunTime> collectVariableRunTimes)
    {
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        Protocol?.Dispose();
        base.Dispose(disposing);
    }

    /// <summary>
    /// 开始通讯执行的方法
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task ProtectedBeforStartAsync(CancellationToken cancellationToken)
    {
        if (Protocol?.Channel != null)
            await Protocol.Channel.ConnectAsync(3000, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 间隔执行
    /// </summary>
    protected abstract ValueTask ProtectedExecuteAsync(CancellationToken cancellationToken);

    #endregion 插件重写
}
