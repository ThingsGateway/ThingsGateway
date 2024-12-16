//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

using System.Collections.Concurrent;
using System.Threading;

using ThingsGateway.NewLife.Extension;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 设备线程管理
/// </summary>
public class ChannelThread
{
    #region 动态配置

    /// <summary>
    /// 线程等待间隔时间
    /// </summary>
    public static volatile int CycleInterval = 10;

    /// <summary>
    /// 线程最大等待间隔时间
    /// </summary>
    public static int MaxCycleInterval = 100;

    /// <summary>
    /// 线程最小等待间隔时间
    /// </summary>
    public static int MinCycleInterval = 10;

    internal static volatile int MaxCount;

    internal static volatile int MaxVariableCount;

    static ChannelThread()
    {
        var minCycleInterval = App.Configuration.GetSection("ChannelThread:MinCycleInterval").Get<int?>() ?? 10;
        MinCycleInterval = minCycleInterval < 10 ? 10 : minCycleInterval;

        var maxCycleInterval = App.Configuration.GetSection("ChannelThread:MaxCycleInterval").Get<int?>() ?? 100;
        MaxCycleInterval = maxCycleInterval < 100 ? 100 : maxCycleInterval;

        var maxCount = App.Configuration.GetSection("ChannelThread:MaxCount").Get<int?>() ?? 1000;
        MaxCount = maxCount < 10 ? 10 : maxCount;

        var maxVariableCount = App.Configuration.GetSection("ChannelThread:MaxVariableCount").Get<int?>() ?? 1000000;
        MaxVariableCount = maxVariableCount < 1000 ? 1000 : maxVariableCount;

        CycleInterval = MaxCycleInterval;

        Task.Factory.StartNew(SetCycleInterval, TaskCreationOptions.LongRunning);
    }

    private static async Task SetCycleInterval()
    {
        var appLifetime = App.RootServices!.GetService<IHostApplicationLifetime>()!;
        var hardwareJob = GlobalData.HardwareJob;

        List<float> cpus = new();
        while (!((appLifetime?.ApplicationStopping ?? default).IsCancellationRequested || (appLifetime?.ApplicationStopped ?? default).IsCancellationRequested))
        {
            try
            {
                if (hardwareJob?.HardwareInfo?.MachineInfo?.CpuRate == null) continue;
                cpus.Add((float)(hardwareJob.HardwareInfo.MachineInfo.CpuRate * 100));
                if (cpus.Count == 1 || cpus.Count > 5)
                {
                    var avg = cpus.Average();
                    cpus.RemoveAt(0);
                    //Console.WriteLine($"CPU平均值：{avg}");
                    if (avg > 80)
                    {
                        CycleInterval = Math.Max(CycleInterval, (int)(MaxCycleInterval * avg / 100));
                    }
                    else if (avg < 50)
                    {
                        CycleInterval = Math.Min(CycleInterval, MinCycleInterval);
                    }
                }
                await Task.Delay(30000, appLifetime?.ApplicationStopping ?? default).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }

    #endregion 动态配置

    /// <summary>
    /// 通道线程构造函数，用于初始化通道线程实例。
    /// </summary>
    /// <param name="channel">通道实例</param>
    /// <param name="getChannel">获取通道的方法</param>
    public ChannelThread(Channel channel, Func<TouchSocketConfig, IChannel> getChannel)
    {
        Localizer = App.CreateLocalizerByType(typeof(ChannelThread))!;
        // 初始化日志记录器，使用通道名称作为日志记录器的名称
        Logger = App.RootServices.GetService<ILoggerFactory>().CreateLogger($"Channel[{channel.Name}]");

        // 设置通道信息
        ChannelTable = channel;
        ChannelId = channel.Id;

        // 初始化底层配置
        LogMessage = new LoggerGroup() { LogLevel = TouchSocket.Core.LogLevel.Warning };

        // 添加默认日志记录器
        LogMessage.AddLogger(new EasyLogger(Log_Out) { LogLevel = TouchSocket.Core.LogLevel.Trace });

        // 初始化基础配置容器
        var foundataionConfig = new TouchSocketConfig();

        // 配置容器中注册日志记录器实例
        foundataionConfig.ConfigureContainer(a => a.RegisterSingleton<ILog>(LogMessage));

        // 根据配置获取通道实例
        Channel = getChannel(foundataionConfig);

        // 设置日志路径为通道ID对应的日志路径
        LogPath = channel.Id.GetLogPath();

        // 设置日志使能状态
        LogEnable = channel.LogEnable;
    }

    /// <summary>
    /// 是否采集通道
    /// </summary>
    public bool IsCollectChannel { get; private set; }

    /// <summary>
    /// 设备线程
    /// </summary>
    protected internal DoTask DriverTask { get; set; }

    /// <summary>
    /// <inheritdoc cref="TouchSocket.Core.TouchSocketConfig"/>
    /// </summary>
    protected internal TouchSocketConfig FoundataionConfig => Channel?.Config;

    /// <summary>
    /// 读写锁
    /// </summary>
    protected internal WaitLock WriteLock { get; } = new();

    /// <summary>
    /// 启停锁
    /// </summary>
    protected WaitLock RestartLock { get; } = new();

    /// <summary>
    /// 取消令箭列表
    /// </summary>
    private ConcurrentDictionary<long, CancellationTokenSource> CancellationTokenSources { get; set; } = new();

    /// <summary>
    /// 插件集合
    /// </summary>
    private ConcurrentList<DriverBase> DriverBases { get; set; } = new();

    private IStringLocalizer Localizer { get; }

    #region 日志

    public LoggerGroup LogMessage { get; internal set; }

    public string LogPath { get; }

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
        Logger?.Log_Out(arg1, arg2, arg3, arg4);
    }

    #endregion 日志

    #region 通道

    public long ChannelId { get; }
    protected internal IChannel? Channel { get; }
    protected internal Channel ChannelTable { get; }

    #endregion 通道

    #region 调试日志

    private object logEnableLock = new();
    private TextFileLogger? TextLogger;

    /// <summary>
    /// 获取或设置日志使能状态。当设置为 true 时，将启用日志记录功能；当设置为 false 时，将禁用日志记录功能。
    /// </summary>
    public bool LogEnable
    {
        get
        {
            // 返回日志使能状态
            return logEnable;
        }
        set
        {
            // 使用锁确保线程安全
            lock (logEnableLock)
            {
                // 更新通道的日志使能状态
                ChannelTable.LogEnable = value;

                // 更新日志使能状态
                logEnable = value;
                // 如果日志使能状态为 true
                if (value)
                {
                    LogMessage.LogLevel = TouchSocket.Core.LogLevel.Trace;
                    // 移除旧的文件日志记录器并释放资源
                    if (TextLogger != null)
                    {
                        LogMessage.RemoveLogger(TextLogger);
                        TextLogger?.Dispose();
                    }

                    // 创建新的文件日志记录器，并设置日志级别为 Trace
                    TextLogger = TextFileLogger.CreateTextLogger(LogPath);
                    TextLogger.LogLevel = TouchSocket.Core.LogLevel.Trace;

                    // 将文件日志记录器添加到日志消息组中
                    LogMessage.AddLogger(TextLogger);
                }
                else
                {
                    LogMessage.LogLevel = TouchSocket.Core.LogLevel.Warning;
                    // 如果日志使能状态为 false，移除文件日志记录器并释放资源
                    if (TextLogger != null)
                    {
                        LogMessage.RemoveLogger(TextLogger);
                        TextLogger?.Dispose();
                    }
                }
            }
        }
    }

    private bool logEnable { get; set; }

    #endregion 调试日志

    #region 线程管理

    /// <summary>
    /// 向当前通道添加驱动程序。
    /// </summary>
    /// <param name="driverBase">要添加的驱动程序对象。</param>
    internal void AddDriver(DriverBase driverBase)
    {
        if (DriverBases.Count > 0)
        {
            if (DriverBases[0].IsCollectDevice != driverBase.IsCollectDevice)
            {
                Logger?.LogWarning(Localizer["PluginTypeDiff", driverBase.DeviceName, ChannelTable.Name]);
                return;
            }
        }

        // 将驱动程序对象添加到驱动程序集合中
        DriverBases.Add(driverBase);

        // 将当前通道线程分配给驱动程序对象
        driverBase.ChannelThread = this;

        try
        {
            // 初始化驱动程序对象，并加载源读取
            driverBase.Init(Channel);
            driverBase.LoadSourceRead(driverBase.CurrentDevice?.VariableRunTimes.Select(a => a.Value));
        }
        catch (Exception ex)
        {
            // 如果初始化过程中发生异常，设置初始化状态为失败，并记录警告日志
            driverBase.IsInitSuccess = false;
            Logger?.LogWarning(ex, Localizer["InitFail", driverBase.CurrentDevice.PluginName, driverBase.DeviceName]);
        }

        // 创建令牌并与驱动程序对象的设备ID关联，用于取消操作
        lock (CancellationTokenSources)
        {
            if (!CancellationTokenSources.ContainsKey(0))
                CancellationTokenSources.TryAdd(0, new CancellationTokenSource());

            CancellationTokenSources.TryGetValue(0, out var cts);
            if (!CancellationTokenSources.ContainsKey(driverBase.DeviceId))
                CancellationTokenSources.TryAdd(driverBase.DeviceId, CancellationTokenSource.CreateLinkedTokenSource(cts.Token));
        }

        // 更新当前通道是否正在收集数据的状态
        IsCollectChannel = driverBase.IsCollectDevice;
    }

    /// <summary>
    /// 异步移除指定设备ID对应的驱动程序。
    /// </summary>
    /// <param name="deviceId">要移除的设备ID。</param>
    /// <returns>表示异步移除操作的任务。</returns>
    internal async Task RemoveDriverAsync(long deviceId)
    {
        // 查找具有指定设备ID的驱动程序对象
        var driverBase = DriverBases.FirstOrDefault(a => a.DeviceId == deviceId);
        if (driverBase != null)
        {
            // 取消驱动程序的操作
            lock (CancellationTokenSources)
            {
                if (CancellationTokenSources.TryGetValue(deviceId, out var token))
                {
                    if (token != null)
                    {
                        token.Cancel();
                        token.Dispose();
                    }
                }
            }

            await Task.Delay(100).ConfigureAwait(false);

            driverBase.AfterStop();


            // 如果需要移除的是采集设备
            if (IsCollectChannel)
            {
                try
                {
                    //添加保存数据变量读取操作
                    var saveVariable = driverBase.CurrentDevice.VariableRunTimes.Where(a => a.Value.SaveValue).Select(a=> (Variable)a.Value).ToList();

                    if (saveVariable.Count>0)
                    {
                        using var db = DbContext.Db.GetConnectionScopeWithAttr<Variable>().CopyNew();
                        var result = await db.Updateable<Variable>(saveVariable).UpdateColumns(a=>a.Value).ExecuteCommandAsync().ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    LogMessage.LogWarning(ex, "SaveValue");
                }

                driverBase.RemoveCollectDeviceRuntime();
            }
            else
            {
                driverBase.RemoveBusinessDeviceRuntime();
            }

            // 从驱动程序集合和令牌源集合中移除驱动程序对象和相关令牌
            DriverBases.Remove(driverBase);
            CancellationTokenSources.Remove(deviceId);
        }
    }

    #endregion 线程管理

    #region 外部获取

    internal DriverBase GetDriver(long deviceId)
    {
        var driverBase = DriverBases.FirstOrDefault(a => a.DeviceId == deviceId);
        return driverBase;
    }

    internal IEnumerable<DriverBase> GetDriverEnumerable()
    {
        return DriverBases;
    }

    internal bool Has(long deviceId)
    {
        return DriverBases.Any(a => a.DeviceId == deviceId);
    }

    #endregion 外部获取

    #region 线程生命周期

    private int releaseCount = 0;

    /// <summary>
    /// 停止插件前，执行取消传播
    /// </summary>
    internal virtual void BeforeStopThread()
    {
        lock (CancellationTokenSources)
        {
            CancellationTokenSources.TryGetValue(0, out var cts);

            if (cts != null)
            {
                try
                {
                    if (!cts.IsCancellationRequested)// 检查是否已请求取消，若未请求取消则尝试取消操作
                    {
                        cts?.Cancel();
                    }
                }
                catch
                {
                    // 捕获异常以确保不会影响其他令牌的取消操作
                }
            }
        }
    }

    /// <summary>
    /// 异步开始执行线程任务。
    /// </summary>
    internal virtual async Task StartThreadAsync()
    {
        try
        {
            // 等待WaitLock锁的获取
            await RestartLock.WaitAsync().ConfigureAwait(false);

            // 如果DriverTask不为null，则执行以下操作
            if (DriverTask != null)
            {
                // 从FoundataionConfig中移除TouchSocketCoreConfigExtension.ConfigurePluginsProperty
                FoundataionConfig?.RemoveValue(TouchSocketCoreConfigExtension.ConfigurePluginsProperty);

                // 配置每个驱动程序的底层插件
                foreach (var driver in DriverBases)
                {
                    driver?.ConfigurePlugins();
                }
                // 设置通道的底层配置
                Channel?.Setup(FoundataionConfig?.Clone());
            }
            else
            {
                // 初始化业务线程
                DriverTask = new(DoWork, Logger, null);
                lock (CancellationTokenSources)
                {
                    if (!CancellationTokenSources.ContainsKey(0))
                        CancellationTokenSources.TryAdd(0, new CancellationTokenSource());

                    CancellationTokenSources.TryGetValue(0, out var cts);
                    DriverTask.Start(cts.Token);
                }
            }
        }
        finally
        {
            // 释放WaitLock锁
            RestartLock.Release();
        }
    }

    /// <summary>
    /// 异步停止线程任务。
    /// </summary>
    internal virtual async Task StopThreadAsync(bool removeDevice)
    {
        // 如果DriverTask为null，则直接返回，无需执行停止操作
        if (DriverTask == null)
        {
            return;
        }

        try
        {
            // 等待WaitLock锁的获取
            await RestartLock.WaitAsync().ConfigureAwait(false);

            BeforeStopThread();

            // 等待DriverTask最多30s
            await DriverTask.StopAsync(TimeSpan.FromSeconds(30)).ConfigureAwait(false);

            DriverBases.ForEach(a =>
            {
                a.AfterStop();
            });

            // 如果需要移除设备
            if (removeDevice)
            {
                // 如果需要移除的是采集设备
                if (IsCollectChannel)
                {
                    DriverBases.RemoveCollectDeviceRuntime();
                }
                else
                {
                    DriverBases.RemoveBusinessDeviceRuntime();
                }
                DriverBases.Clear();
            }

            // 将DriverTask置为null
            DriverTask = null;

            // 清空CancellationTokenSources集合
            CancellationTokenSources.ForEach(a => a.Value.SafeDispose());
            CancellationTokenSources.Clear();
        }
        finally
        {
            // 释放WaitLock锁
            RestartLock.Release();
        }
    }

    /// <summary>
    /// DoWork
    /// </summary>
    /// <param name="stoppingToken">取消标记。</param>
    protected async ValueTask DoWork(CancellationToken stoppingToken)
    {
        if (Channel?.ChannelType == ChannelTypeEnum.TcpService && IsCollectChannel)
        {
            //DTU采集，建立同一个Tcp服务通道，多个采集设备（对应各个DTU设备），并发采集
            releaseCount = 0;
            List<Task> tasks = new List<Task>();
            ConcurrentList<DriverBase> driverBases = new();
            WaitLock easyLock = new(false);
            using CancellationTokenSource cancellationTokenSource = new();
            foreach (var driver1 in DriverBases)
            {
                var task = DoWork(driver1, DriverBases.Count, stoppingToken, cancellationTokenSource.Token).ContinueWith(_ =>
                {
                    if (driverBases.Count < DriverBases.Count)
                    {
                        if (!driverBases.Any(a => a == driver1))
                            driverBases.Add(driver1);//添加到已完成的任务列表

                        // 如果所有任务都已完成，则取消剩余的等待任务
                        if (driverBases.Count >= DriverBases.Count)
                            cancellationTokenSource.Cancel();

                        _ = Task.Run(async () =>
                        {
                            while (driverBases.Count < DriverBases.Count)
                            {
                                await DoWork(driver1, DriverBases.Count, stoppingToken, cancellationTokenSource.Token).ConfigureAwait(false);
                                await Task.Delay(MinCycleInterval, stoppingToken).ConfigureAwait(false);
                            }
                            Interlocked.Increment(ref releaseCount);
                            if (releaseCount >= DriverBases.Count)
                            {
                                easyLock.Release();
                            }
                        });
                    }
                }, stoppingToken
                );
                tasks.Add(task);
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
            await easyLock.WaitAsync(stoppingToken).ConfigureAwait(false);
        }
        else
        {
            //foreach (var driver in DriverBases)
            //{
            //    await DoWork(driver, DriverBases.Count, stoppingToken, CancellationToken.None).ConfigureAwait(false);
            //}

            ParallelOptions parallelOptions = new();
            parallelOptions.CancellationToken = stoppingToken;
            parallelOptions.MaxDegreeOfParallelism = DriverBases.Count == 0 ? 1 : DriverBases.Count;
            await Parallel.ForEachAsync(DriverBases, parallelOptions, (async (driver, stoppingToken) =>
            {
                await DoWork(driver, DriverBases.Count, stoppingToken, CancellationToken.None).ConfigureAwait(false);
            })).ConfigureAwait(false);
        }

        // 如果驱动实例数量大于1，则延迟一段时间后继续执行下一轮循环
        if (DriverBases.Count > 1)
            await Task.Delay(MinCycleInterval, stoppingToken).ConfigureAwait(false);

        // 如果驱动实例数量为0，则延迟一段时间后继续执行下一轮循环
        if (DriverBases.Count == 0)
            await Task.Delay(1000, stoppingToken).ConfigureAwait(false);
    }

    private async Task DoWork(DriverBase driver, int count, CancellationToken stoppingToken, CancellationToken cancellationToken)
    {
        try
        {
            if (stoppingToken.IsCancellationRequested || cancellationToken.IsCancellationRequested)
                return;
            if (!CancellationTokenSources.TryGetValue(driver.DeviceId, out var stoken))
            {
                await Task.Delay(CycleInterval, stoppingToken).ConfigureAwait(false);
                return;
            }

            using CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, stoken.Token);

            var token = cancellationTokenSource.Token;

            if (token.IsCancellationRequested)
                return;

            // 只有当驱动成功初始化后才执行操作
            if (driver.IsInitSuccess)
            {
                if (!driver.IsBeforStarted)
                    await driver.BeforStartAsync(token).ConfigureAwait(false); // 调用驱动的启动前异步方法，如果已经执行，会直接返回

                var result = await driver.ExecuteAsync(token).ConfigureAwait(false); // 执行驱动的异步执行操作

                // 根据执行结果进行不同的处理
                if (result == ThreadRunReturnTypeEnum.None)
                {
                    // 如果驱动处于离线状态且为采集驱动，则根据配置的间隔时间进行延迟
                    if (driver.CurrentDevice.DeviceStatus == DeviceStatusEnum.OffLine && driver.IsCollectDevice)
                    {
                        if (count == 1)
                            await Task.Delay(Math.Max(Math.Min(((CollectBase)driver).CollectProperties.ReIntervalTime, CollectDeviceHostedService.CheckIntervalTime / 2) * 1000 - CycleInterval, 3000), token).ConfigureAwait(false);
                    }
                    else
                    {
                        if (count == 1)
                            await Task.Delay(CycleInterval, token).ConfigureAwait(false); // 默认延迟一段时间后再继续执行
                    }
                }
                else if (result == ThreadRunReturnTypeEnum.Continue)
                {
                    if (count == 1)
                        await Task.Delay(1000, token).ConfigureAwait(false); // 如果执行结果为继续，则延迟一段较短的时间后再继续执行
                }
                else if (result == ThreadRunReturnTypeEnum.Break && stoppingToken.IsCancellationRequested)
                {
                    driver.AfterStop(); // 执行驱动的释放操作
                    return; // 结束当前循环
                }
            }
            else
            {
                if (count == 1)
                    await Task.Delay(1000, token).ConfigureAwait(false); // 默认延迟一段时间后再继续执行
            }
        }
        catch (OperationCanceledException)
        {
            if (stoppingToken.IsCancellationRequested)
                driver.AfterStop(); // 执行驱动的释放操作
            return;
        }
        catch (ObjectDisposedException)
        {
            if (stoppingToken.IsCancellationRequested)
                driver.AfterStop(); // 执行驱动的释放操作
            return;
        }
    }

    #endregion 线程生命周期
}
