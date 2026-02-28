//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;

using ThingsGateway.Foundation.Common.Json.Extension;

using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Rpc.DmtpRpc.Generators;
using TouchSocket.Sockets;

namespace ThingsGateway.Gateway.Application;

internal sealed class RedundancyTask : IRpcDriver, IAsyncDisposable
{
    private readonly ILogger _logger;
    private readonly IRedundancyService _redundancyService;
    private readonly GatewayRedundantSerivce _gatewayRedundantSerivce;

    public RedundancyTask(ILogger logger)
    {
        _logger = logger;
        _gatewayRedundantSerivce = App.RootServices.GetRequiredService<GatewayRedundantSerivce>();
        _redundancyService = App.RootServices.GetRequiredService<IRedundancyService>();
        // 创建新的文件日志记录器，并设置日志级别为 Trace
        LogPath = "Logs/RedundancyLog";
        TextLogger = TextFileLogger.GetMultipleFileLogger(LogPath);
        TextLogger.LogLevel = TouchSocket.Core.LogLevel.Trace;
    }

    public ILog LogMessage { get; set; }
    public TextFileLogger TextLogger { get; }
    public string LogPath { get; }
    private TcpDmtpClient _tcpDmtpClient;
    private TcpDmtpService _tcpDmtpService;

    private void Log_Out(TouchSocket.Core.LogLevel logLevel, object source, string message, Exception exception)
    {
        _logger?.Log_Out(logLevel, source, message, exception);
    }

    private ScheduledAsyncTask scheduledTask;

    private int GetBatchSize()
    {
        // 默认批量数量
        const int defaultSize = 10000;
        const int highMemorySize = 100000;
        const long memoryThreshold = 2048; // 2GB，单位MB

        return (GlobalData.HardwareJob.HardwareInfo.AvailableMemory > memoryThreshold)
            ? highMemorySize
            : defaultSize;
    }
    /// <summary>
    /// 主站
    /// </summary>
    private async Task DoMasterWork(object? state, CancellationToken stoppingToken)
    {
        try
        {
            bool online = false;

            var waitInvoke = CreateDmtpInvokeOption(stoppingToken);

            try
            {
                if (_tcpDmtpService.Clients.Count != 0)
                {
                    online = true;
                }
                // 如果 online 为 true，表示设备在线
                if (online)
                {
                    int batchSize = 50;

                    var deviceRunTimes = GlobalData.ReadOnlyIdDevices.Where(a => a.Value.IsCollect == true).Select(a => a.Value).Concat([GlobalData.MemoryDeviceRuntime]).Batch(batchSize);

                    foreach (var item in _tcpDmtpService.Clients)
                    {
                        foreach (var deviceDataWithValues in deviceRunTimes)
                        {
                            // 将 GlobalData.CollectDevices 和 GlobalData.Variables 同步到从站
                            await item.GetDmtpRpcActor().UpDataAsync(deviceDataWithValues.AdaptListDeviceDataWithValue(), waitInvoke).ConfigureAwait(false);
                        }
                        LogMessage?.LogTrace($"{item.Id} Update StandbyStation data success");
                    }
                }
            }
            catch (Exception ex)
            {
                // 输出警告日志，指示同步数据到从站时发生错误
                LogMessage?.LogWarning(ex, "Synchronize data to standby site error");
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
            LogMessage?.LogWarning(ex, "Execute");
        }
    }

    /// <summary>
    /// 从站
    /// </summary>
    private async Task DoSlaveWork(object? state, CancellationToken stoppingToken)
    {
        try
        {
            bool online = false;
            var waitInvoke = CreateDmtpInvokeOption(stoppingToken);
            await _tcpDmtpClient.TryConnectAsync().ConfigureAwait(false);

            {
                // 初始化读取错误计数器
                var readErrorCount = 0;
                // 当读取错误次数小于最大错误计数时循环执行
                while (readErrorCount < RedundancyOptions.MaxErrorCount)
                {
                    try
                    {
                        using var cts = new CancellationTokenSource(10000);
                        online = (await _tcpDmtpClient.PingAsync(cts.Token).ConfigureAwait(false)).IsSuccess;
                        if (online)
                            break;
                        else
                        {
                            readErrorCount++;
                            await Task.Delay(RedundancyOptions.SyncInterval, stoppingToken).ConfigureAwait(false);
                        }
                    }
                    catch
                    {
                        // 捕获异常，增加读取错误计数器
                        readErrorCount++;
                        await Task.Delay(RedundancyOptions.SyncInterval, stoppingToken).ConfigureAwait(false);
                    }
                }
            }

            // 如果设备不在线
            if (!online)
            {
                // 无法获取状态，启动本机
                await ActiveAsync().ConfigureAwait(false);
            }
            else
            {
                // 如果设备在线
                LogMessage?.LogTrace($"Ping ActiveStation {RedundancyOptions.MasterUri} success");
                await StandbyAsync().ConfigureAwait(false);
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
            LogMessage?.LogWarning(ex, "Execute");
        }
    }

    private WaitLock _switchLock = new(nameof(RedundancyTask));

    private bool first;
    private async Task StandbyAsync()
    {
        try
        {
            await _switchLock.WaitAsync().ConfigureAwait(false);
            if (_gatewayRedundantSerivce.StartCollectChannelEnable)
            {
                // 输出日志，指示主站已恢复，从站将切换到备用状态
                if (first)
                    LogMessage?.Warning("Master site has recovered, local machine (standby) will switch to standby state");

                // 将 IsStart 设置为 false，表示当前设备为从站，切换到备用状态
                _gatewayRedundantSerivce.StartCollectChannelEnable = false;
                _gatewayRedundantSerivce.StartBusinessChannelEnable = RedundancyOptions?.IsStartBusinessDevice ?? false;
                await RestartAsync().ConfigureAwait(false);
            }
        }
        finally
        {
            _switchLock.Release();
            first = true;
        }
    }

    private async Task ActiveAsync()
    {
        try
        {
            await _switchLock.WaitAsync().ConfigureAwait(false);

            _gatewayRedundantSerivce.StartBusinessChannelEnable = true;
            if (!_gatewayRedundantSerivce.StartCollectChannelEnable)
            {
                // 输出日志，指示无法连接冗余站点，本机将切换到正常状态
                if (first)
                    LogMessage?.Warning("Cannot connect to redundant site, local machine will switch to normal state");
                _gatewayRedundantSerivce.StartCollectChannelEnable = true;
                await RestartAsync().ConfigureAwait(false);
            }
        }
        finally
        {
            _switchLock.Release();
            first = true;
        }
    }
    private static async Task RestartAsync()
    {
        await GlobalData.ChannelRuntimeService.RestartChannelAsync(GlobalData.ReadOnlyIdChannels.Values).ConfigureAwait(false);
        await GlobalData.MemoryChannelThreadManage.RestartChannelAsync(GlobalData.MemoryChannelRuntime).ConfigureAwait(false);
    }

    public async Task StartRedundancyTaskAsync()
    {
        await StopRedundancyTaskAsync().ConfigureAwait(false);
        RedundancyOptions = (await _redundancyService.GetRedundancyAsync().ConfigureAwait(false)).AdaptRedundancyOptions();

        if (RedundancyOptions?.Enable == true)
        {
            if (RedundancyOptions.IsMaster)
            {
                _tcpDmtpService = await GetTcpDmtpService(RedundancyOptions).ConfigureAwait(false);

                await _tcpDmtpService.StartAsync().ConfigureAwait(false);//启动
                await ActiveAsync().ConfigureAwait(false);
            }
            else
            {
                _tcpDmtpClient = await GetTcpDmtpClient(RedundancyOptions).ConfigureAwait(false);
                await StandbyAsync().ConfigureAwait(false);
            }
        }
        else
        {
            await ActiveAsync().ConfigureAwait(false);
        }

        if (RedundancyOptions?.Enable == true)
        {
            LogMessage?.LogInformation($"Redundancy task started");
            if (RedundancyOptions.IsMaster)
            {
                scheduledTask = new ScheduledAsyncTask(RedundancyOptions.SyncInterval, DoMasterWork, null, null, CancellationToken.None);
            }
            else
            {
                scheduledTask = new ScheduledAsyncTask(5000, DoSlaveWork, null, null, CancellationToken.None);
            }

            scheduledTask.Start();
        }
    }
    public async Task StopRedundancyTaskAsync()
    {
        if (scheduledTask?.Enable == true)
        {
            LogMessage?.LogInformation($"Redundancy task stoped");
            scheduledTask?.Stop();
        }

        if (_tcpDmtpService != null)
        {
            try
            {
                await _tcpDmtpService.StopAsync().ConfigureAwait(false);
            }
            catch
            {
            }
            _tcpDmtpService.TryDispose();
        }
        if (_tcpDmtpClient != null)
        {
            try
            {
                await _tcpDmtpClient.CloseAsync().ConfigureAwait(false);
            }
            catch
            {
            }
            _tcpDmtpClient.TryDispose();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopRedundancyTaskAsync().ConfigureAwait(false);
        TextLogger?.TryDispose();
        scheduledTask?.SafeDispose();

        _tcpDmtpService?.SafeDispose();
        _tcpDmtpClient?.SafeDispose();
        _tcpDmtpService = null;
        _tcpDmtpClient = null;
    }

    #region

    private async Task<TcpDmtpClient> GetTcpDmtpClient(RedundancyOptions redundancy)
    {
        var log = new LoggerGroup() { LogLevel = TouchSocket.Core.LogLevel.Trace };
        log?.AddLogger(new EasyLogger(Log_Out) { LogLevel = TouchSocket.Core.LogLevel.Trace });
        log?.AddLogger(TextLogger);
        LogMessage = log;
        var tcpDmtpClient = new TcpDmtpClient();
#pragma warning disable CA2000 // 丢失范围之前释放对象
        var config = new TouchSocketConfig()
               .SetRemoteIPHost(redundancy.MasterUri)
               .SetAdapterOption(a => a.MaxPackageSize = 0x20000000)
               .SetDmtpOption(a => a.VerifyToken = redundancy.VerifyToken)
               .ConfigureContainer(a =>
               {
                   a.AddLogger(LogMessage);
                   a.AddRpcStore(store =>
                   {
                       store.RegisterServer<IRedundantRpcServer>(new RedundantRpcServer(this));

                   });
               })
               .ConfigurePlugins(a =>
               {
                   a.UseTcpSessionCheckClear();
                   a.UseDmtpRpc(a => a.ConfigureDefaultSerializationSelector(b =>
                   {
                       b.UseSystemTextJson(json =>
                       {
                           json.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals;
                           json.Converters.Add(new SystemTextJsonByteArrayToNumberArrayConverter());
                           json.Converters.Add(new JTokenSystemTextJsonConverter());
                           json.Converters.Add(new JValueSystemTextJsonConverter());
                           json.Converters.Add(new JObjectSystemTextJsonConverter());
                           json.Converters.Add(new JArraySystemTextJsonConverter());
                           json.TypeInfoResolver = new DefaultJsonTypeInfoResolver();
                       });
                   }));


               });
#pragma warning restore CA2000 // 丢失范围之前释放对象

        await tcpDmtpClient.SetupAsync(config).ConfigureAwait(false);
        return tcpDmtpClient;
    }

    private async Task<TcpDmtpService> GetTcpDmtpService(RedundancyOptions redundancy)
    {
        var log = new LoggerGroup() { LogLevel = TouchSocket.Core.LogLevel.Trace };
        log?.AddLogger(new EasyLogger(Log_Out) { LogLevel = TouchSocket.Core.LogLevel.Trace });
        log?.AddLogger(TextLogger);
        LogMessage = log;
        var tcpDmtpService = new TcpDmtpService();
#pragma warning disable CA2000 // 丢失范围之前释放对象
        var config = new TouchSocketConfig()
               .SetListenIPHosts(redundancy.MasterUri)
               .SetAdapterOption(a => a.MaxPackageSize = 0x20000000)
               .SetDmtpOption(a => a.VerifyToken = redundancy.VerifyToken)
               .ConfigureContainer(a =>
               {
                   a.AddLogger(LogMessage);
                   a.AddRpcStore(store =>
                   {
                       store.RegisterServer<IRedundantRpcServer>(new RedundantRpcServer(this));

                   });
               })
               .ConfigurePlugins(a =>
               {
                   a.UseTcpSessionCheckClear();
                   a.UseDmtpRpc(a => a.ConfigureDefaultSerializationSelector(b =>
                   {
                       b.UseSystemTextJson(json =>
                       {


                           json.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals;
                           json.Converters.Add(new SystemTextJsonByteArrayToNumberArrayConverter());
                           json.Converters.Add(new JTokenSystemTextJsonConverter());
                           json.Converters.Add(new JValueSystemTextJsonConverter());
                           json.Converters.Add(new JObjectSystemTextJsonConverter());
                           json.Converters.Add(new JArraySystemTextJsonConverter());
                           json.TypeInfoResolver = new DefaultJsonTypeInfoResolver();
                       });
                   }));

               });
#pragma warning restore CA2000 // 丢失范围之前释放对象

        await tcpDmtpService.SetupAsync(config).ConfigureAwait(false);
        return tcpDmtpService;
    }

    private DmtpInvokeOption CreateDmtpInvokeOption(CancellationToken cancellationToken)
    {
        return new DmtpInvokeOption()
        {
            FeedbackType = FeedbackType.WaitInvoke,
            Token = cancellationToken,
            SerializationType = SerializationType.Json,
        };
    }
    private RedundancyOptions RedundancyOptions;

    #endregion

    #region RedundancyForcedSync

    WaitLock ForcedSyncWaitLock = new WaitLock(nameof(RedundancyTask));
    public async Task RedundancyForcedSync(CancellationToken cancellationToken = default)
    {
        await ForcedSyncWaitLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!RedundancyOptions.IsMaster)
                return;

            var invokeOption = CreateDmtpInvokeOption(cancellationToken);

            try
            {
                await EnsureChannelOpenAsync(cancellationToken).ConfigureAwait(false);

                // 如果在线，执行同步
                bool online = RedundancyOptions.IsServer
                    ? _tcpDmtpService.Clients.Count > 0
                    : _tcpDmtpClient.Online;

                if (!online)
                {
                    LogMessage?.LogWarning("RedundancyForcedSync data error, no client online");
                    return;
                }

                if (RedundancyOptions.IsServer)
                {
                    foreach (var client in _tcpDmtpService.Clients)
                    {
                        await InvokeSyncDataAsync(client, invokeOption, cancellationToken).ConfigureAwait(false);
                    }
                }
                else
                {
                    await InvokeSyncDataAsync(_tcpDmtpClient, invokeOption, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                LogMessage?.LogWarning(ex, "RedundancyForcedSync data error");
            }
        }
        finally
        {
            ForcedSyncWaitLock.Release();
        }
    }

    private async Task InvokeSyncDataAsync(IDmtpActorObject client, DmtpInvokeOption invokeOption, CancellationToken cancellationToken)
    {
        int maxBatchSize = GetBatchSize() / 10;

        var groups = GlobalData.IdVariables.Select(a => a.Value).GroupBy(a => a.DeviceRuntime).Where(a => a.Key != null);

        var channelBatch = new HashSet<Channel>();
        var deviceBatch = new HashSet<Device>();
        var variableBatch = new List<MemoryVariable>();
        foreach (var group in groups)
        {

            var channel = group.Key.ChannelRuntime.AdaptChannel();
            var device = group.Key.AdaptDevice();
            if (!group.Key.ChannelRuntime.IsMemory)
            {
                channelBatch.Add(channel);
                deviceBatch.Add(device);
            }

            foreach (var variable in group)
            {
                if (!group.Key.ChannelRuntime.IsMemory)
                {
                    channelBatch.Add(channel);
                    deviceBatch.Add(device);
                }
                if (variable is MemoryVariableRuntime memoryVariableRuntime)
                {
                    variableBatch.Add(memoryVariableRuntime.AdaptMemoryVariable());
                }
                else
                {
                    variableBatch.Add(variable.AdaptMemoryVariable());
                }

                if (variableBatch.Count >= maxBatchSize)
                {
                    // 发送一批
                    await client.GetDmtpRpcActor().SyncDataAsync(channelBatch.ToList(), deviceBatch.ToList(), variableBatch, invokeOption).ConfigureAwait(false);

                    variableBatch.Clear();
                    channelBatch.Remove(channel);
                    deviceBatch.Remove(device);
                }
            }
        }

        // 发送最后剩余的一批
        if (variableBatch.Count > 0)
        {
            await client.GetDmtpRpcActor().SyncDataAsync(channelBatch.ToList(), deviceBatch.ToList(), variableBatch, invokeOption).ConfigureAwait(false);
        }

        LogMessage?.LogDebug($"RedundancyForcedSync data success");
    }

    #endregion

    #region  Rpc

    /// <summary>
    /// 异步写入方法
    /// </summary>
    /// <param name="writeInfoLists">要写入的变量及其对应的数据</param>
    /// <param name="cancellationToken">取消操作的通知</param>
    /// <returns>写入操作的结果字典</returns>
    public ValueTask<IDictionary<string, IDictionary<string, OperResult<object>>>> InvokeMethodAsync(Dictionary<VariableRuntime, JsonNode> writeInfoLists, CancellationToken cancellationToken)
    {
        return RpcAsync(writeInfoLists, cancellationToken);
    }

    private async ValueTask<IDictionary<string, IDictionary<string, OperResult<object>>>> RpcAsync(Dictionary<VariableRuntime, JsonNode> writeInfoLists, CancellationToken cancellationToken)
    {
        IDictionary<string, IDictionary<string, OperResult<object>>>? dataResult = new Dictionary<string, IDictionary<string, OperResult<object>>>();

        Dictionary<string, Dictionary<string, string>> deviceDatas = new();
        foreach (var item in writeInfoLists)
        {
            if (deviceDatas.TryGetValue(item.Key.DeviceName ?? string.Empty, out var variableDatas))
            {
                variableDatas.TryAdd(item.Key.Name, item.Value?.ToString() ?? string.Empty);
            }
            else
            {
                deviceDatas.TryAdd(item.Key.DeviceName ?? string.Empty, new());
                deviceDatas[item.Key.DeviceName ?? string.Empty].TryAdd(item.Key.Name, item.Value?.ToString() ?? string.Empty);
            }
        }

        if (RedundancyOptions.IsMaster)
            return NoOnline(dataResult, deviceDatas);

        var invokeOption = CreateDmtpInvokeOption(cancellationToken);

        try
        {
            await EnsureChannelOpenAsync(cancellationToken).ConfigureAwait(false);
            bool online = RedundancyOptions.IsServer ? _tcpDmtpService.Clients.Count > 0 : _tcpDmtpClient.Online;

            if (!online)
            {
                LogMessage?.LogWarning("Rpc error, no client online");
                return NoOnline(dataResult, deviceDatas);
            }

            if (RedundancyOptions.IsServer)
            {
                await InvokeRpcServerAsync(deviceDatas, dataResult, invokeOption).ConfigureAwait(false);
            }
            else
            {
                dataResult = await InvokeRpcClientAsync(deviceDatas, invokeOption).ConfigureAwait(false);
            }

            LogMessage?.LogDebug("Rpc success");
            return dataResult;
        }
        catch (OperationCanceledException)
        {
            return NoOnline(dataResult, deviceDatas);
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex, "Rpc error");
            return NoOnline(dataResult, deviceDatas);
        }
    }

    private Task<IDictionary<string, IDictionary<string, OperResult<object>>>> InvokeRpcClientAsync(
        Dictionary<string, Dictionary<string, string>> deviceDatas,
        DmtpInvokeOption invokeOption)
    {
        return _tcpDmtpClient.GetDmtpRpcActor().RpcAsync(deviceDatas, invokeOption);
    }
    private async Task InvokeRpcServerAsync(
        Dictionary<string, Dictionary<string, string>> deviceDatas,
        IDictionary<string, IDictionary<string, OperResult<object>>> dataResult,
        DmtpInvokeOption invokeOption)
    {
        var w1 = new Dictionary<TcpDmtpSessionClient, Dictionary<string, Dictionary<string, string>>>();

        foreach (var (key, value) in deviceDatas)
        {
            if (!GlobalData.TryGetDeviceRuntime(key, out var device))
            {
                continue;
            }

            if (!_tcpDmtpService.TryGetClient(device.Tag, out var client))
            {
                // 客户端未在线
                dataResult.TryAdd(key, value.ToDictionary(v => v.Key, _ => new OperResult<object>("No online")));
                continue;
            }

            // 去除 endpoint 前缀
            var deviceName = key;

            if (!w1.TryGetValue(client, out var variableDatas))
            {
                variableDatas = new Dictionary<string, Dictionary<string, string>>();
                w1.Add(client, variableDatas);
            }

            variableDatas[deviceName] = value;
        }

        foreach (var (client, variableDatas) in w1)
        {
            try
            {
                var data = await client.GetDmtpRpcActor().RpcAsync(variableDatas, invokeOption).ConfigureAwait(false);

                dataResult.AddRange(data);
            }
            catch (Exception ex)
            {
                foreach (var (deviceName, vars) in variableDatas)
                {
                    var errorDict = vars.ToDictionary(v => v.Key, _ => new OperResult<object>(ex));
                    dataResult[deviceName] = errorDict;
                }
            }
        }
    }

    private static IDictionary<string, IDictionary<string, OperResult<object>>> NoOnline(IDictionary<string, IDictionary<string, OperResult<object>>> dataResult, Dictionary<string, Dictionary<string, string>> deviceDatas)
    {
        foreach (var item in deviceDatas)
        {
            dataResult.TryAdd(item.Key, new Dictionary<string, OperResult<object>>());

            foreach (var vItem in item.Value)
            {
                dataResult[item.Key].TryAdd(vItem.Key, new OperResult<object>("No online"));
            }
        }
        return dataResult;
    }

    /// <summary>
    /// 异步写入方法
    /// </summary>
    /// <param name="writeInfoLists">要写入的变量及其对应的数据</param>
    /// <param name="cancellationToken">取消操作的通知</param>
    /// <returns>写入操作的结果字典</returns>
    public ValueTask<IDictionary<string, IDictionary<string, OperResult<object>>>> InVokeWriteAsync(Dictionary<VariableRuntime, JsonNode> writeInfoLists, CancellationToken cancellationToken)
    {
        return RpcAsync(writeInfoLists, cancellationToken);
    }
    private async Task EnsureChannelOpenAsync(CancellationToken cancellationToken)
    {
        if (RedundancyOptions.IsServer)
        {
            if (_tcpDmtpService.ServerState != ServerState.Running)
            {
                if (_tcpDmtpService.ServerState != ServerState.Stopped)
                    await _tcpDmtpService.StopAsync(cancellationToken).ConfigureAwait(false);

                await _tcpDmtpService.StartAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        else
        {
            if (!_tcpDmtpClient.Online)
                await _tcpDmtpClient.TryConnectAsync().ConfigureAwait(false);
        }
    }

    #endregion
}
