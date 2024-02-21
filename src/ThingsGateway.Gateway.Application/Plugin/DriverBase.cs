//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

using Microsoft.Extensions.Logging;

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
        ServiceScope = App.GetService<IServiceScopeFactory>().CreateScope();
        PluginService = ServiceScope.ServiceProvider.GetRequiredService<IPluginService>();
        GlobalData = ServiceScope.ServiceProvider.GetRequiredService<GlobalData>();
        RpcService = ServiceScope.ServiceProvider.GetService<IRpcService>();
    }

    public TouchSocketConfig FoundataionConfig { get; internal set; }

    public override string ToString()
    {
        return Protocol?.ToString() ?? base.ToString();
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
    /// 当前设备全部设备属性，执行初始化后获取正确值
    /// </summary>
    public List<DependencyPropertyWithInfo>? DevicePropertys { get; private set; }

    /// <summary>
    /// 当前插件目录
    /// </summary>
    public string Directory { get; internal set; }

    /// <summary>
    /// 调试UI Type，如果不存在，返回null
    /// </summary>
    public virtual Type DriverDebugUIType { get; }

    /// <summary>
    /// 插件UI Type，继承<see cref="IDriverUIBase"/>如果不存在，返回null
    /// </summary>
    public virtual Type DriverUIType { get; }

    /// <summary>
    /// 插件配置项 ，继承实现<see cref="CollectPropertyBase"/>后，返回继承类
    /// </summary>
    public abstract CollectPropertyBase DriverPropertys { get; }

    /// <summary>
    /// 是否初始化成功
    /// </summary>
    public bool IsInitSuccess { get; internal set; } = true;

    /// <summary>
    /// 是否执行了BeforStart方法
    /// </summary>
    public bool IsBeforStarted { get; protected set; } = false;

    public bool? KeepRun => CurrentDevice?.KeepRun;

    /// <summary>
    /// 日志
    /// </summary>
    protected internal ILogger Logger { get; set; }

    /// <summary>
    /// 底层日志，需由线程管理器传入
    /// </summary>
    public LoggerGroup LogMessage { get; internal set; }

    public virtual bool IsCollect => CurrentDevice.PluginType == PluginTypeEnum.Collect;
    protected internal EasyLock WriteLock { get; set; }

    /// <summary>
    /// 读写锁，通常对于主从协议来说都需要，返回false时，需要在底层实现读写锁
    /// 并且读取或者写入会并发进行，需要另外在底层实现锁
    /// </summary>
    /// <returns></returns>
    protected virtual bool IsSingleThread => true;

    /// <summary>
    /// 全局插件服务
    /// </summary>
    protected IPluginService PluginService { get; private set; }

    protected GlobalData GlobalData { get; private set; }

    /// <summary>
    /// 一般底层驱动，也有可能为null
    /// </summary>
    protected abstract IProtocol? Protocol { get; }

    public IRpcService RpcService { get; }

    /// <summary>
    /// IServiceScope
    /// </summary>
    public IServiceScope ServiceScope { get; }

    /// <summary>
    /// 日志路径
    /// </summary>
    public string LogPath { get; internal set; }

    public ChannelThread ChannelThread { get; internal set; }

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
    /// 结束后
    /// </summary>
    public virtual async Task AfterStopAsync()
    {
        try
        {
            Logger?.LogInformation($"{DeviceName}：设备线程停止中");
            var timeout = 1;
            try
            {
                await ProtectedAfterStopAsync().WaitAsync(TimeSpan.FromMinutes(timeout));
            }
            catch (TimeoutException)
            {
                Logger?.LogWarning($"{DeviceName}：设备线程停止超时-{timeout}min");
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
    public virtual async Task BeforStartAsync(CancellationToken cancellationToken)
    {
        try
        {
            Logger?.LogInformation($"{DeviceName}：设备线程开始");

            if (cancellationToken.IsCancellationRequested) return;

            if (KeepRun == true)
            {
                var timeout = 1;
                try
                {
                    await ProtectedBeforStartAsync(cancellationToken).WaitAsync(TimeSpan.FromMinutes(timeout), cancellationToken);
                }
                catch (TimeoutException)
                {
                    Logger?.LogWarning($"{DeviceName}：设备线程初始启动超时-{timeout}min");
                }
                CurrentDevice.SetDeviceStatus(DateTimeUtil.Now);
            }
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, $"插件执行{nameof(BeforStartAsync)}方法失败");
            CurrentDevice.SetDeviceStatus(DateTimeUtil.Now, 999, ex.Message);
        }
        finally
        {
            if (!cancellationToken.IsCancellationRequested)
                IsBeforStarted = true;
        }
    }

    /// <summary>
    /// 间隔执行
    /// </summary>
    public virtual async Task<ThreadRunReturnTypeEnum> ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (cancellationToken.IsCancellationRequested)
                return ThreadRunReturnTypeEnum.Break;
            if (CurrentDevice == null)
            {
                LogMessage?.Warning($"{nameof(CurrentDevice)}不能为null");
                return ThreadRunReturnTypeEnum.Break;
            }
            if (KeepRun == false)
            {
                //暂停
                return ThreadRunReturnTypeEnum.Continue;
            }

            if (cancellationToken.IsCancellationRequested)
                return ThreadRunReturnTypeEnum.Break;

            //获取设备连接状态
            if (IsConnected())
            {
                //更新设备活动时间,业务设备直接更新状态
                if (!IsCollect)
                    CurrentDevice.SetDeviceStatus(DateTimeUtil.TimerXNow, 0);
                else
                    CurrentDevice.SetDeviceStatus(DateTimeUtil.TimerXNow);
            }
            else
            {
                if (!IsConnected())
                    CurrentDevice.SetDeviceStatus(DateTimeUtil.TimerXNow, 999);
            }
            if (cancellationToken.IsCancellationRequested)
                return ThreadRunReturnTypeEnum.Break;

            await ProtectedExecuteAsync(cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                return ThreadRunReturnTypeEnum.Break;

            //正常返回None
            return ThreadRunReturnTypeEnum.None;
        }
        catch (TaskCanceledException)
        {
            return ThreadRunReturnTypeEnum.Break;
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
            LogMessage?.LogError(ex, $"插件执行{nameof(ExecuteAsync)}方法失败");
            CurrentDevice.SetDeviceStatus(DateTimeUtil.TimerXNow, CurrentDevice.ErrorCount + 1, ex.Message);
            return ThreadRunReturnTypeEnum.None;
        }
    }

    /// <summary>
    /// 内部初始化，在开始前执行，异常时会标识重启
    /// </summary>
    /// <param name="client"></param>
    public virtual void Init(IChannel? channel = null)
    {
        if (CurrentDevice == null)
        {
            Logger?.LogWarning($"{nameof(CurrentDevice)}不能为null");
            IsInitSuccess = false;
        }
    }

    /// <summary>
    /// 初始化
    /// </summary>
    internal virtual void Init(DeviceRunTime device)
    {
        CurrentDevice = device;
        DevicePropertys = PluginService.GetDriverPropertyTypes(device.PluginName, this).Values.ToList();
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
            var str = keepRun == false ? "设备线程暂停" : "设备线程继续";
            Logger?.LogInformation($"{DeviceName}：{str}");
            this.CurrentDevice.KeepRun = keepRun;
        }
    }

    /// <summary>
    /// 默认延时
    /// </summary>
    protected async Task Delay(CancellationToken cancellationToken)
    {
        if (CurrentDevice.IntervalTime > ChannelThread.CycleInterval)
            await Task.Delay(CurrentDevice.IntervalTime - ChannelThread.CycleInterval, cancellationToken);
        else
            await Task.Delay(ChannelThread.CycleInterval, cancellationToken);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        Protocol?.SafeDispose();
        base.Dispose(disposing);
    }

    /// <summary>
    /// 结束通讯执行的方法
    /// </summary>
    /// <returns></returns>
    protected virtual Task ProtectedAfterStopAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 开始通讯执行的方法
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task ProtectedBeforStartAsync(CancellationToken cancellationToken)
    {
        if (Protocol != null)
            if (Protocol.Channel != null)
                await Protocol.Channel.ConnectAsync(3000, cancellationToken);
    }

    /// <summary>
    /// 间隔执行
    /// </summary>
    protected abstract Task ProtectedExecuteAsync(CancellationToken cancellationToken);

    /// <summary>
    /// 获取设备变量打包列表/特殊方法列表
    /// </summary>
    /// <param name="collectVariableRunTimes"></param>
    public virtual void LoadSourceRead(List<VariableRunTime> collectVariableRunTimes)
    {
    }
}