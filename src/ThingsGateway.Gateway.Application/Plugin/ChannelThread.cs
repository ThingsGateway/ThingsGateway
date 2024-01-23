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

using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;

using ThingsGateway.Core.Extension;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 设备线程管理
/// </summary>
public class ChannelThread
{
    /// <summary>
    /// 线程最小等待间隔时间
    /// </summary>
    public static volatile int CycleInterval = 10;

    static ChannelThread()
    {
        var cycleInterval = App.GetConfig<int?>("ChannelThread:CycleInterval") ?? 10;
        CycleInterval = cycleInterval < 10 ? 10 : cycleInterval;
    }

    /// <summary>
    /// <inheritdoc cref="TouchSocket.Core.TouchSocketConfig"/>
    /// </summary>
    protected internal TouchSocketConfig FoundataionConfig { get; set; }

    public LoggerGroup LogMessage { get; internal set; }

    /// <summary>
    /// 日志
    /// </summary>
    protected internal ILogger Logger { get; set; }

    /// <summary>
    /// 底层错误日志输出
    /// </summary>
    protected internal virtual void Log_Out(TouchSocket.Core.LogLevel arg1, object arg2, string arg3, Exception arg4)
    {
        if (arg1 >= TouchSocket.Core.LogLevel.Warning)
        {
            foreach (var item in DriverBases)
            {
                item.CurrentDevice.SetDeviceStatus(lastErrorMessage: arg3);
            }
        }
        Logger.Log_Out(arg1, arg2, arg3, arg4);
    }

    public string LogPath { get; }
    private Channel ChannelTable;

    public ChannelThread(Channel channel, Func<TouchSocketConfig, IChannel> getChannel)
    {
        Logger = App.GetService<ILoggerFactory>().CreateLogger($"通道：{channel.Name}");
        ChannelTable = channel;
        ChannelId = channel.Id;
        //底层配置
        LogMessage = new LoggerGroup() { LogLevel = TouchSocket.Core.LogLevel.Trace };
        //默认日志
        LogMessage.AddLogger(new EasyLogger(Log_Out) { LogLevel = TouchSocket.Core.LogLevel.Warning });

        FoundataionConfig = new();
        FoundataionConfig.ConfigureContainer(a => a.RegisterSingleton<ILog>(LogMessage));
        Channel = getChannel(FoundataionConfig);
        LogPath = channel.Name.GetLogPath();
        LogEnable = channel.LogEnable;
    }

    private TextFileLogger? TextLogger;
    private bool logEnable { get; set; }
    private object logEnableLock = new();

    public bool LogEnable
    {
        get
        {
            return logEnable;
        }
        set
        {
            lock (logEnableLock)
            {
                ChannelTable.LogEnable = value;
                logEnable = value;
                if (value)
                {
                    if (TextLogger != null)
                    {
                        LogMessage.RemoveLogger(TextLogger);
                        TextLogger?.Dispose();
                    }
                    //文件日志
                    TextLogger = TextFileLogger.Create(LogPath);
                    TextLogger.LogLevel = TouchSocket.Core.LogLevel.Trace;
                    LogMessage.AddLogger(TextLogger);
                }
                else
                {
                    if (TextLogger != null)
                    {
                        LogMessage.RemoveLogger(TextLogger);
                        TextLogger?.Dispose();
                    }
                }
            }
        }
    }

    public long ChannelId { get; }

    protected IChannel? Channel { get; }

    /// <summary>
    /// 取消令箭列表
    /// </summary>
    private ConcurrentDictionary<long, CancellationTokenSource> CancellationTokenSources { get; set; } = new();

    public void AddDriver(DriverBase driverBase)
    {
        DriverBases.Add(driverBase);
        driverBase.ChannelThread = this;
        driverBase.Logger = Logger;
        driverBase.WriteLock = WriteLock;
        driverBase.LogPath = LogPath;
        driverBase.LogMessage = LogMessage;
        driverBase.FoundataionConfig = FoundataionConfig;
        try
        {
            driverBase.Init(Channel);
            driverBase.LoadSourceRead(driverBase.CurrentDevice?.VariableRunTimes);
        }
        catch (Exception ex)
        {
            driverBase.IsInitSuccess = false;
            driverBase?.Logger?.LogWarning(ex, $"{driverBase.DeviceName} 初始化链路失败");
        }
        var token = CancellationTokenSources.GetOrAdd(0, new CancellationTokenSource());
        CancellationTokenSources.TryAdd(driverBase.DeviceId, CancellationTokenSource.CreateLinkedTokenSource(token.Token));
    }

    /// <summary>
    /// 移除设备，取消相关令箭，等待设备释放
    /// </summary>
    /// <param name="deviceId"></param>
    /// <returns></returns>
    public async Task RemoveDriverAsync(long deviceId)
    {
        var driverBase = DriverBases.FirstOrDefault(a => a.DeviceId == deviceId);
        if (driverBase != null)
        {
            //取消
            var cancellationTokenSource = CancellationTokenSources.FirstOrDefault(a => a.Key == deviceId);

            var token = cancellationTokenSource.Value;
            if (token != null)
            {
                token?.Cancel();
            }
            //using CancellationTokenSource timeoutToken = new(5000);
            //5秒超时返回
            while ((!driverBase.DisposedValue)
                //|| !timeoutToken.IsCancellationRequested
                )
            {
                await Task.Delay(100);
            }
            DriverBases.Remove(driverBase);
            CancellationTokenSources.Remove(deviceId);
            token?.Dispose();
        }
    }

    public DriverBase GetDriver(long deviceId)
    {
        var driverBase = DriverBases.FirstOrDefault(a => a.DeviceId == deviceId);
        return driverBase;
    }

    public IEnumerable<DriverBase> GetDriverEnumerable()
    {
        return DriverBases;
    }

    public bool Has(long deviceId)
    {
        return DriverBases.Any(a => a.DeviceId == deviceId);
    }

    /// <summary>
    /// 插件集合
    /// </summary>
    private ConcurrentList<DriverBase> DriverBases { get; set; } = new();

    /// <summary>
    /// 启停锁
    /// </summary>
    protected EasyLock EasyLock { get; set; } = new();

    /// <summary>
    /// 读写锁
    /// </summary>
    protected EasyLock WriteLock { get; set; } = new();

    /// <summary>
    /// 设备线程
    /// </summary>
    protected internal Task DriverTask { get; set; }

    /// <summary>
    /// 停止插件前，执行取消传播
    /// </summary>
    public virtual void BeforeStopThread()
    {
        CancellationTokenSources.ParallelForEach(cancellationToken =>
        {
            _ = Task.Run(() =>
            {
                try
                {
                    if (!cancellationToken.Value.IsCancellationRequested)
                    {
                        cancellationToken.Value?.Cancel();
                    }
                }
                catch
                {
                }
            });
        });
    }

    /// <summary>
    /// 开始
    /// </summary>
    public virtual async Task StartThreadAsync()
    {
        try
        {
            await EasyLock.WaitAsync();
            if (DriverTask != null)
            {
                Channel?.PluginManager?.SafeDispose();
                FoundataionConfig.RemoveValue(TouchSocketCoreConfigExtension.ConfigurePluginsProperty);
                foreach (var driver in DriverBases)
                {
                    driver?.ConfigurePlugins();
                }
                Channel?.Setup(FoundataionConfig);
            }
            else
            {
                var token = CancellationTokenSources.GetOrAdd(0, new CancellationTokenSource());
                //初始化业务线程
                await InitTaskAsync(token.Token);
                if (DriverTask.Status == TaskStatus.Created)
                    DriverTask?.Start();
            }
        }
        finally
        {
            EasyLock.Release();
        }
    }

    /// <summary>
    /// 停止
    /// </summary>
    public virtual async Task StopThreadAsync()
    {
        if (DriverTask == null)
        {
            return;
        }
        try
        {
            await EasyLock.WaitAsync();
            CancellationTokenSources.ParallelForEach(cancellationToken =>
            {
                try
                {
                    cancellationToken.Value?.SafeDispose();
                }
                catch
                {
                }
            });
            await DriverTask.WaitAsync(CancellationToken.None);
            CancellationTokenSources.Clear();
            DriverTask?.SafeDispose();
            DriverTask = null;
        }
        finally
        {
            EasyLock.Release();
        }
    }

    /// <summary>
    /// 初始化
    /// </summary>
    protected async Task InitTaskAsync(CancellationToken cancellation)
    {
        DriverTask = await Task.Factory.StartNew(async (a) =>
        {
            var stoppingToken = (CancellationToken)a!;
            try
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    await Stop();
                    return;
                }

                foreach (var driver in DriverBases)
                {
                    var stoken = CancellationTokenSources[driver.DeviceId];
                    if (stoken.IsCancellationRequested)
                        continue;
                    await driver.BeforStartAsync(stoken.Token);
                }
                await Task.Delay(CycleInterval, stoppingToken);
                while (!stoppingToken.IsCancellationRequested)
                {
                    foreach (var driver in DriverBases)
                    {
                        var tokens = CancellationTokenSources[driver.DeviceId];

                        try
                        {
                            if (tokens.IsCancellationRequested)
                                if (!driver.DisposedValue)
                                {
                                    await driver.AfterStopAsync();
                                    break;
                                }
                                else
                                    break;

                            var token = CancellationTokenSources[driver.DeviceId].Token;

                            if (!driver.IsBeforStarted)
                            {
                                await driver.BeforStartAsync(token);
                                await Task.Delay(CycleInterval, token);
                            }
                            //初始化成功才能执行
                            if (driver.IsInitSuccess)
                            {
                                var result = await driver.ExecuteAsync(token);
                                if (result == ThreadRunReturnTypeEnum.None)
                                {
                                    //4.0.0.7版本添加离线恢复的间隔时间，共享通道不适用
                                    if (driver.CurrentDevice.DeviceStatus == DeviceStatusEnum.OffLine && driver.IsCollect)
                                    {
                                        if (DriverBases.Count == 1)
                                            await Task.Delay(Math.Min(driver.DriverPropertys.ReIntervalTime, DeviceWorker.CheckIntervalTime / 2) * 1000 - CycleInterval, token);
                                    }
                                    else
                                    {
                                        if (DriverBases.Count == 1)
                                            await Task.Delay(CycleInterval, token);
                                    }
                                }
                                else if (result == ThreadRunReturnTypeEnum.Continue)
                                {
                                    if (DriverBases.Count == 1)
                                        await Task.Delay(1000, token);
                                }
                                else if (result == ThreadRunReturnTypeEnum.Break)
                                {
                                    //如果插件还没释放，执行一次结束函数
                                    if (!driver.DisposedValue)
                                    {
                                        await driver.AfterStopAsync();
                                        break;
                                    }
                                    //当线程返回Break，直接跳出循环
                                    break;
                                }
                            }
                            else
                            {
                                if (DriverBases.Count == 1)
                                    await Task.Delay(1000, token);
                            }
                        }
                        catch (TaskCanceledException)
                        {
                            if (!driver.DisposedValue)
                            {
                                await driver.AfterStopAsync();
                                break;
                            }
                        }
                        catch (ObjectDisposedException)
                        {
                            if (!driver.DisposedValue)
                            {
                                await driver.AfterStopAsync();
                                break;
                            }
                        }
                    }
                    if (DriverBases.Count > 1)
                        await Task.Delay(CycleInterval, stoppingToken);
                    if (DriverBases.Count == 0)
                        await Task.Delay(1000, stoppingToken);
                }
                //注意插件结束函数不能使用取消传播作为条件
                await Stop();
            }
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
            finally
            {
                await Stop();
            }
            async Task Stop()
            {
                {
                    foreach (var driver in DriverBases)
                    {
                        //如果插件还没释放，执行一次结束函数
                        if (!driver.DisposedValue)
                            await driver.AfterStopAsync();
                    }
                }
            }
        }, cancellation, TaskCreationOptions.LongRunning
 );
    }
}