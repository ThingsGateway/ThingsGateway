//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.Gateway.Application;

using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;


namespace ThingsGateway.Gateway.Management;


internal class RedundancyHostedService : BackgroundService, IRedundancyHostedService
{
    private readonly ILogger _logger;
    private readonly IRedundancyService _redundancyService;
    /// <inheritdoc cref="RedundancyHostedService"/>
    public RedundancyHostedService(ILogger<RedundancyHostedService> logger, IStringLocalizer<RedundancyHostedService> localizer, IRedundancyService redundancyService)
    {
        _logger = logger;
        Logger = new EasyLogger(LogOut);
        Localizer = localizer;
        _redundancyService = redundancyService;
    }
    private EasyLogger Logger { get; }
    private IStringLocalizer Localizer { get; }
    private DoTask RedundancyTask { get; set; }

    /// <summary>
    /// 重启锁
    /// </summary>
    private WaitLock RedundancyRestartLock { get; } = new();

    private TcpDmtpClient _tcpDmtpClient;
    private TcpDmtpService _tcpDmtpService;
    private TcpDmtpClient GetTcpDmtpClient(RedundancyOptions redundancy)
    {
        var tcpDmtpClient = new TcpDmtpClient();
        var config = new TouchSocketConfig()
               .SetRemoteIPHost(redundancy.PrimaryUri)
               .SetAdapterOption(new AdapterOption() { MaxPackageSize = 1024 * 1024 * 1024 })
               .SetDmtpOption(new DmtpOption() { VerifyToken = redundancy.VerifyToken })
               .ConfigureContainer(a =>
               {
                   a.AddEasyLogger(LogOut);
                   a.AddRpcStore(store =>
                   {
                       store.RegisterServer(new ReverseCallbackServer(this));
                   });
               })
               .ConfigurePlugins(a =>
               {
                   a.UseDmtpRpc();
                   a.UseDmtpHeartbeat()//使用Dmtp心跳
                   .SetTick(TimeSpan.FromMilliseconds(redundancy.HeartbeatInterval))
                   .SetMaxFailCount(redundancy.MaxErrorCount);
               });

        tcpDmtpClient.Setup(config);
        return tcpDmtpClient;
    }

    private TcpDmtpService GetTcpDmtpService(RedundancyOptions redundancy)
    {
        var tcpDmtpService = new TcpDmtpService();
        var config = new TouchSocketConfig()
               .SetListenIPHosts(redundancy.PrimaryUri)
               .SetAdapterOption(new AdapterOption() { MaxPackageSize = 1024 * 1024 * 1024 })
               .SetDmtpOption(new DmtpOption() { VerifyToken = redundancy.VerifyToken })
               .ConfigureContainer(a =>
               {
                   a.AddEasyLogger(LogOut);
                   a.AddRpcStore(store =>
                   {
                       store.RegisterServer(new ReverseCallbackServer(this));
                   });
               })
               .ConfigurePlugins(a =>
               {
                   a.UseDmtpRpc();
                   a.UseDmtpHeartbeat()//使用Dmtp心跳
                   .SetTick(TimeSpan.FromMilliseconds(redundancy.HeartbeatInterval))
                   .SetMaxFailCount(redundancy.MaxErrorCount);
               });

        tcpDmtpService.Setup(config);
        return tcpDmtpService;
    }

    private void LogOut(TouchSocket.Core.LogLevel logLevel, object source, string message, Exception exception)
    {
        _logger?.Log_Out(logLevel, source, message, exception);
    }

    private Task StartAsync()
    {
        return GlobalData.CollectDeviceHostedService.StartAsync();
    }

    private Task StopAsync()
    {
        //停止采集
        return GlobalData.CollectDeviceHostedService.StopAsync(!GlobalData.BusinessDeviceHostedService.StartBusinessDeviceEnable);
    }



    /// <summary>
    /// 主站
    /// </summary>
    /// <param name="redundancy">冗余配置</param>
    /// <param name="tcpDmtpService">服务</param>
    /// <param name="stoppingToken">取消任务的 CancellationToken</param>
    private async ValueTask DoPrimaryWork(RedundancyOptions redundancy, TcpDmtpService tcpDmtpService, CancellationToken stoppingToken)
    {
        // 延迟一段时间，避免过于频繁地执行任务
        await Task.Delay(500, stoppingToken).ConfigureAwait(false);
        try
        {
            bool online = false;
            var waitInvoke = new DmtpInvokeOption()
            {
                FeedbackType = FeedbackType.WaitInvoke,
                Token = stoppingToken,
                Timeout = 30000,
                SerializationType = SerializationType.Json,
            };

            try
            {
                if (tcpDmtpService.Clients.Any())
                {
                    online = true;
                }
                // 如果 online 为 true，表示设备在线
                if (online)
                {
                    var deviceRunTimes = GlobalData.ReadOnlyCollectDevices.Values.Adapt<List<DeviceDataWithValue>>();
                    var variableRunTimes = GlobalData.ReadOnlyVariables.Select(a => a.Value).Adapt<List<VariableDataWithValue>>();
                    var variableRunTimes1 = variableRunTimes.ChunkBetter(80000);
                    var variableRuntimes1Count = variableRunTimes1.Count();
                    int itemsPerList = (int)Math.Ceiling((double)deviceRunTimes.Count / variableRuntimes1Count);
                    var deviceRunTimes1 = deviceRunTimes.ChunkBetter(itemsPerList, true).ToList();

                    int i = 0;
                    List<Task> tasks = new List<Task>();
                    foreach (var item in variableRunTimes1)
                    {
                        List<DeviceDataWithValue> devices = new();
                        if (deviceRunTimes1.Count >= i + 1)
                        {
                            devices = deviceRunTimes1[i].ToList();
                        }
                        var variables = item.ToList();
                        // 将 GlobalData.CollectDevices 和 GlobalData.Variables 同步到从站
                        Task task = tcpDmtpService.Clients.FirstOrDefault().GetDmtpRpcActor().InvokeAsync(
                                         nameof(ReverseCallbackServer.UpdateGatewayDataAsync), null, waitInvoke, devices, variables);
                        tasks.Add(task);
                        i++;
                    }
                    await Task.WhenAll(tasks).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                // 输出警告日志，指示同步数据到从站时发生错误
                Logger.LogWarning(ex, Localizer["ErrorSynchronizingData"]);
            }
            await Task.Delay(redundancy.SyncInterval, stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Execute");
        }
    }

    /// <summary>
    /// 备用站
    /// </summary>
    /// <param name="redundancy">冗余配置</param>
    /// <param name="tcpDmtpClient">服务</param>
    /// <param name="stoppingToken">取消任务的 CancellationToken</param>
    private async ValueTask DoStandbyWork(RedundancyOptions redundancy, TcpDmtpClient tcpDmtpClient, CancellationToken stoppingToken)
    {
        // 延迟一段时间，避免过于频繁地执行任务
        await Task.Delay(5000, stoppingToken).ConfigureAwait(false);
        try
        {
            bool online = false;
            var waitInvoke = new DmtpInvokeOption()
            {
                FeedbackType = FeedbackType.WaitInvoke,
                Token = stoppingToken,
                Timeout = 30000,
                SerializationType = SerializationType.Json,
            };

            try
            {
                await tcpDmtpClient.TryConnectAsync().ConfigureAwait(false);

                {
                    // 初始化读取错误计数器
                    var readErrorCount = 0;
                    // 当读取错误次数小于最大错误计数时循环执行
                    while (readErrorCount < redundancy.MaxErrorCount)
                    {
                        try
                        {
                            // 发送 Ping 请求以检查设备是否在线，超时时间为 10000 毫秒
                            online = await tcpDmtpClient.PingAsync(10000).ConfigureAwait(false);
                            if (online)
                                break;
                            else
                            {
                                readErrorCount++;
                                await Task.Delay(redundancy.SyncInterval).ConfigureAwait(false);
                            }
                        }
                        catch
                        {
                            // 捕获异常，增加读取错误计数器
                            readErrorCount++;
                            await Task.Delay(redundancy.SyncInterval).ConfigureAwait(false);
                        }
                    }
                }


                // 如果设备不在线
                if (!online)
                {
                    // 无法获取状态，启动本机
                    await PrimaryStartAsync();
                }
                else
                {
                    // 如果设备在线
                    await StandbyStopAsync();
                }
            }
            finally
            {
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Execute");
        }
    }

#endregion 线程任务

    #region worker服务

    private WaitLock _switchLock = new();
    public async Task<OperResult> SwitchModeAsync()
    {
        if (_redundancy?.Enable != true)
            return new("Redundancy not enabled");
        try
        {
            OperResult operResult = new();
            var waitInvoke = new DmtpInvokeOption()
            {
                FeedbackType = FeedbackType.WaitInvoke,
                //Token = stoppingToken,
                Timeout = 30000,
                SerializationType = SerializationType.Json,
            };

            if (GlobalData.CollectDeviceHostedService.StartCollectDeviceEnable)
            {

                //切换本地
                await RedundancyStopAsync();
                await RedundancyStartAsync(!GlobalData.CollectDeviceHostedService.StartCollectDeviceEnable);

                if (_tcpDmtpService?.Clients.Count > 0)
                {
                    var result = await _tcpDmtpService.Clients.FirstOrDefault().GetDmtpRpcActor().InvokeTAsync<OperResultClass>(nameof(ReverseCallbackServer.SetGatewayState), null, waitInvoke, GlobalData.CollectDeviceHostedService.StartCollectDeviceEnable);
                    operResult = new(result);
                }
            }
            else
            {
                if (_tcpDmtpClient != null)
                {

                    var result = await _tcpDmtpClient.GetDmtpRpcActor().InvokeTAsync<OperResultClass>(nameof(ReverseCallbackServer.SetGatewayState), waitInvoke, GlobalData.CollectDeviceHostedService.StartCollectDeviceEnable);

                    //切换成功
                    if (result.IsSuccess)
                    {
                        //切换本地
                        await RedundancyStopAsync();
                        await RedundancyStartAsync(!GlobalData.CollectDeviceHostedService.StartCollectDeviceEnable);

                    }
                    operResult = new(result);
                }
            }
            return operResult;
        }
        finally
        {
        }


    }


    /// <inheritdoc/>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return RedundancyStartAsync();
    }

    public async Task<OperResult> RedundancyStartAsync(bool? isPrimary = null)
    {
        try
        {
            await RedundancyRestartLock.WaitAsync().ConfigureAwait(false); // 等待获取锁，以确保只有一个线程可以执行以下代码

            if (RedundancyTask != null)
            {
                await RedundancyTask.StopAsync(TimeSpan.FromSeconds(30)).ConfigureAwait(false); // 停止现有任务，等待最多30秒钟
            }
            await BeforeStartAsync(isPrimary);
            if (_redundancy?.Enable == true)
            {
                if (_redundancy.IsPrimary)
                {
                    RedundancyTask = new DoTask(a => DoPrimaryWork(_redundancy, _tcpDmtpService, a), _logger); // 创建新的任务
                }
                else
                {
                    RedundancyTask = new DoTask(a => DoStandbyWork(_redundancy, _tcpDmtpClient, a), _logger); // 创建新的任务
                }

                RedundancyTask?.Start(); // 启动任务
            }

            return new();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Start"); // 记录错误日志
            return new(ex);
        }
        finally
        {
            RedundancyRestartLock.Release(); // 释放锁
        }
    }
    private Redundancy _redundancy;
    /// <summary>
    /// 1为主站
    /// </summary>
    private bool Status { get; set; }
    private async Task BeforeStartAsync(bool? isPrimary = null)
    {
        _redundancy = await _redundancyService.GetRedundancyAsync();

        if (_redundancy?.Enable == true)
        {
            _tcpDmtpService = GetTcpDmtpService(_redundancy);
            _tcpDmtpClient = GetTcpDmtpClient(_redundancy);
            if (isPrimary != null)
            {
                _redundancy.IsPrimary = isPrimary.Value;
            }
            if (_redundancy.IsPrimary)
            {
                await _tcpDmtpService.StartAsync().ConfigureAwait(false);//启动
                await PrimaryStartAsync();
            }
            else
            {
                await StandbyStopAsync();
            }
        }
        else
        {
            await PrimaryStartAsync();
        }
    }

    private async Task StandbyStopAsync()
    {
        try
        {
            await _switchLock.WaitAsync();
            if (GlobalData.CollectDeviceHostedService.StartCollectDeviceEnable)
            {
                // 输出日志，指示主站已恢复，从站将切换到备用状态
                Logger.LogInformation(Localizer["SwitchStandbyState"]);

                // 将 IsStart 设置为 false，表示当前设备为从站，切换到备用状态
                GlobalData.CollectDeviceHostedService.StartCollectDeviceEnable = false;
                GlobalData.BusinessDeviceHostedService.StartBusinessDeviceEnable = _redundancy?.IsStartBusinessDevice ?? true;
                await StopAsync();
                await StartAsync();
            }
        }
        finally
        {
            _switchLock.Release();
        }
    }

    private async Task PrimaryStartAsync()
    {
        try
        {
            await _switchLock.WaitAsync();

            GlobalData.BusinessDeviceHostedService.StartBusinessDeviceEnable = true;
            if (!GlobalData.CollectDeviceHostedService.StartCollectDeviceEnable)
            {
                // 输出日志，指示无法连接冗余站点，本机将切换到正常状态
                Logger.LogInformation(Localizer["SwitchPrimaryState"]);
                GlobalData.CollectDeviceHostedService.StartCollectDeviceEnable = true;
                await StopAsync();
                await StartAsync();
            }
        }
        finally
        {
            _switchLock.Release();
        }
    }

    public async Task RedundancyStopAsync()
    {
        try
        {
            await RedundancyRestartLock.WaitAsync().ConfigureAwait(false); // 等待获取锁，以确保只有一个线程可以执行以下代码

            if (RedundancyTask != null)
            {
                await RedundancyTask.StopAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false); // 停止任务，等待最多10秒钟
            }
            if (_tcpDmtpService != null)
            {
                try
                {
                    await _tcpDmtpService.StopAsync();
                }
                catch
                {
                }
            }
            if (_tcpDmtpClient != null)
            {
                try
                {
                    await _tcpDmtpClient.CloseAsync();
                }
                catch
                {
                }
            }
            _tcpDmtpService?.Dispose();
            _tcpDmtpClient?.Dispose();
            RedundancyTask = null;
            _tcpDmtpService = null;
            _tcpDmtpClient = null;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Stop"); // 记录错误日志
        }
        finally
        {
            RedundancyRestartLock.Release(); // 释放锁
        }
    }

    #endregion worker服务
}
