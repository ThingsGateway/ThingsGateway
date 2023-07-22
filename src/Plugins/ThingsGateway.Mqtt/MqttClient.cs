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

using Mapster;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MQTTnet;
using MQTTnet.Client;



using SqlSugar;

using System.Collections.Concurrent;
using System.Text;

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Extension;
using ThingsGateway.Foundation.Extension.Enumerator;
using ThingsGateway.Web.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Mqtt;


public class MqttClient : UpLoadBase
{
    private List<CollectDeviceRunTime> _collectDevice;
    private ConcurrentQueue<DeviceData> _collectDeviceRunTimes = new();
    private ConcurrentQueue<VariableData> _collectVariableRunTimes = new();
    private GlobalDeviceData _globalDeviceData;
    public override Type DriverDebugUIType => null;

    private IMqttClient _mqttClient;

    private MqttClientOptions _mqttClientOptions;

    private MqttClientSubscribeOptions _mqttSubscribeOptions;

    private RpcSingletonService _rpcCore;
    private List<DeviceVariableRunTime> _uploadVariables = new();

    private CollectDeviceWorker collectDeviceHostService;
    private MqttClientProperty driverPropertys = new();

    private TimerTick exDeviceTimerTick;
    private TimerTick exVariableTimerTick;
    private EasyLock lockobj = new();
    private MqttClientVariableProperty variablePropertys = new();

    public MqttClient(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
    }


    public override UpDriverPropertyBase DriverPropertys => driverPropertys;


    public override List<DeviceVariableRunTime> UploadVariables => _uploadVariables;


    public override VariablePropertyBase VariablePropertys => variablePropertys;
    public override async Task BeforStartAsync(CancellationToken cancellationToken)
    {
        if (_mqttClient != null)
        {
            var result = await TryMqttClientAsync(cancellationToken);
            if (!result.IsSuccess)
            {
                _logger?.LogWarning(ToString() + $"-连接MqttServer失败：{result.Message}");
            }
        }
    }
    public override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (!driverPropertys.IsInterval)
            {
                ////变化推送
                var varList = _collectVariableRunTimes.ToListWithDequeue();
                if (varList?.Count != 0)
                {
                    //分解List，避免超出mqtt字节大小限制
                    var varData = varList.ChunkTrivialBetter(driverPropertys.SplitSize);
                    foreach (var item in varData)
                    {
                        try
                        {
                            if (!cancellationToken.IsCancellationRequested)
                            {
                                await MqttUp($"{driverPropertys.VariableTopic}", item.GetSciptListValue(driverPropertys.BigTextScriptVariableModel), cancellationToken);

                            }
                            else
                            {
                                break;
                            }

                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, ToString());
                        }

                    }
                }
            }
            else
            {
                if (exVariableTimerTick.IsTickHappen())
                {
                    try
                    {
                        var varList = _uploadVariables.Adapt<List<VariableData>>();
                        if (varList?.Count != 0)
                        {
                            //分解List，避免超出mqtt字节大小限制
                            var varData = varList.ChunkTrivialBetter(driverPropertys.SplitSize);
                            foreach (var item in varData)
                            {
                                try
                                {
                                    if (!cancellationToken.IsCancellationRequested)
                                    {

                                        await MqttUp($"{driverPropertys.VariableTopic}", item.GetSciptListValue(driverPropertys.BigTextScriptVariableModel), cancellationToken);

                                    }
                                    else
                                    {
                                        break;
                                    }

                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarning(ex, ToString());
                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, ToString());
                    }

                }


            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, ToString());
        }
        try
        {
            if (!driverPropertys.IsInterval)
            {
                ////变化推送
                var devList = _collectDeviceRunTimes.ToListWithDequeue();
                if (devList?.Count != 0)
                {
                    //分解List，避免超出mqtt字节大小限制
                    var devData = devList.ChunkTrivialBetter(driverPropertys.SplitSize);
                    foreach (var item in devData)
                    {
                        try
                        {
                            if (!cancellationToken.IsCancellationRequested)
                            {
                                await MqttUp($"{driverPropertys.DeviceTopic}", item.GetSciptListValue(driverPropertys.BigTextScriptDeviceModel), cancellationToken);
                            }
                            else
                            {
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, ToString());
                        }
                    }

                }

            }

            else
            {
                if (exDeviceTimerTick.IsTickHappen())
                {
                    var devList = _collectDevice.Adapt<List<DeviceData>>(); ;
                    if (devList?.Count != 0)
                    {
                        //分解List，避免超出mqtt字节大小限制
                        var devData = devList.ChunkTrivialBetter(driverPropertys.SplitSize);
                        foreach (var item in devData)
                        {
                            try
                            {
                                if (!cancellationToken.IsCancellationRequested)
                                {
                                    await MqttUp($"{driverPropertys.DeviceTopic}", item.GetSciptListValue(driverPropertys.BigTextScriptDeviceModel), cancellationToken);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, ToString());
                            }
                        }

                    }
                }


            }

        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, ToString());
        }

        if (driverPropertys.CycleInterval > UploadDeviceThread.CycleInterval + 50)
        {
            try
            {
                await Task.Delay(driverPropertys.CycleInterval - UploadDeviceThread.CycleInterval, cancellationToken);
            }
            catch
            {
            }
        }
        else
        {

        }

    }

    /// <summary>
    /// 上传mqtt内容，并进行离线缓存
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="payLoad"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task MqttUp(string topic, string payLoad, CancellationToken cancellationToken)
    {
        var variableMessage = new MqttApplicationMessageBuilder()
.WithTopic(topic)
.WithPayload(payLoad).Build();
        var isConnect = await TryMqttClientAsync(cancellationToken);
        if (isConnect.IsSuccess)
        {
            //连接成功时补发缓存数据
            var cacheData = await CacheDb.GetCacheData();
            foreach (var item in cacheData)
            {
                var cacheMessage = new MqttApplicationMessageBuilder()
.WithTopic(item.Topic)
.WithPayload(item.CacheStr).Build();
                var cacheResult = await _mqttClient.PublishAsync(cacheMessage);
                if (cacheResult.IsSuccess)
                {
                    await CacheDb.DeleteCacheData(item.Id);
                    logMessage.Trace(LogMessageHeader + $"主题：{item.Topic}{Environment.NewLine}负载：{item.CacheStr}");
                }
            }

            var result = await _mqttClient.PublishAsync(variableMessage);
            if (!result.IsSuccess)
            {
                await CacheDb.AddCacheData(topic, payLoad, driverPropertys.CacheMaxCount);
            }
            else
            {
                logMessage.Trace(LogMessageHeader + $"主题：{topic}{Environment.NewLine}负载：{payLoad}");
            }
        }
        else
        {
            await CacheDb.AddCacheData(topic, payLoad, driverPropertys.CacheMaxCount);
        }
    }



    public override OperResult IsConnected()
    {
        if (_mqttClient?.IsConnected == true)
        {
            return OperResult.CreateSuccessResult();
        }
        else
        {
            return new OperResult();
        }
    }

    public override string ToString()
    {
        return $" {nameof(MqttClient)} IP:{driverPropertys.IP} Port:{driverPropertys.Port}";
    }

    protected override void Dispose(bool disposing)
    {
        try
        {
            _globalDeviceData?.AllVariables?.ForEach(a => a.VariableValueChange -= VariableValueChange);

            _globalDeviceData?.CollectDevices?.ForEach(a =>
            {
                a.DeviceStatusCahnge -= DeviceStatusCahnge;
            });
            _mqttClient?.SafeDispose();
            _mqttClient = null;
            _uploadVariables = null;
            _collectDeviceRunTimes.Clear();
            _collectVariableRunTimes.Clear();
            _collectDeviceRunTimes = null;
            _collectVariableRunTimes = null;
            base.Dispose(disposing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ToString());
        }
    }
    protected override void Init(UploadDeviceRunTime device)
    {
        var mqttFactory = new MqttFactory(new PrivateLogger(_logger));
        _mqttClientOptions = mqttFactory.CreateClientOptionsBuilder()
                        .WithClientId(driverPropertys.ConnectId)
           .WithCredentials(driverPropertys.UserName, driverPropertys.Password)//账密
           .WithTcpServer(driverPropertys.IP, driverPropertys.Port)//服务器
           .WithCleanSession(true)
           .WithKeepAlivePeriod(TimeSpan.FromSeconds(120.0))
           .Build();
        _mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(
                f =>
                {
                    f.WithTopic(driverPropertys.RpcWriteTopic);
                })
                     .WithTopicFilter(
                f =>
                {
                    f.WithTopic(driverPropertys.QuestRpcTopic);
                })
            .Build();
        _mqttClient = mqttFactory.CreateMqttClient();
        _mqttClient.ConnectedAsync += _mqttClient_ConnectedAsync;
        _mqttClient.ApplicationMessageReceivedAsync += _mqttClient_ApplicationMessageReceivedAsync;
        var serviceScope = _scopeFactory.CreateScope();
        _globalDeviceData = serviceScope.ServiceProvider.GetService<GlobalDeviceData>();
        _rpcCore = serviceScope.ServiceProvider.GetService<RpcSingletonService>();
        collectDeviceHostService = serviceScope.GetBackgroundService<CollectDeviceWorker>();

        var tags = _globalDeviceData.AllVariables.Where(a => a.VariablePropertys.ContainsKey(device.Id))
           .Where(b => GetPropertyValue(b, nameof(variablePropertys.Enable)).GetBoolValue())
           .ToList();

        _uploadVariables = tags;

        _collectDevice = _globalDeviceData.CollectDevices.Where(a => _uploadVariables.Select(b => b.DeviceId).Contains(a.Id)).ToList();
        if (!driverPropertys.IsInterval)
        {
            _collectDevice.ForEach(a =>
            {
                a.DeviceStatusCahnge += DeviceStatusCahnge;
            });
            _uploadVariables.ForEach(a =>
            {
                a.VariableValueChange += VariableValueChange;
            });
        }

        if (driverPropertys.UploadInterval <= 1000) driverPropertys.UploadInterval = 1000;
        exVariableTimerTick = new(driverPropertys.UploadInterval);
        exDeviceTimerTick = new(driverPropertys.UploadInterval);

    }
    private async Task _mqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
    {
        if (arg.ApplicationMessage.Topic == driverPropertys.QuestRpcTopic && arg.ApplicationMessage.PayloadSegment.Count > 0)
        {
            await AllPublishAsync(CancellationToken.None);
            return;
        }

        if (!driverPropertys.DeviceRpcEnable || arg.ClientId.IsNullOrEmpty())
            return;
        if (arg.ApplicationMessage.Topic != driverPropertys.RpcWriteTopic)
            return;
        var rpcData = Encoding.UTF8.GetString(arg.ApplicationMessage.PayloadSegment).FromJson<MqttRpcNameVaueWithId>();
        if (rpcData == null)
            return;

        MqttRpcResult mqttRpcResult = new();
        try
        {
            var nv = rpcData.Adapt<KeyValuePair<string, string>>();
            var tag = _uploadVariables.FirstOrDefault(a => a.Name == nv.Key);
            if (tag != null)
            {
                var rpcEnable = GetPropertyValue(tag, nameof(variablePropertys.VariableRpcEnable)).ToBoolean();
                if (rpcEnable == true)
                {
                    var result = await _rpcCore.InvokeDeviceMethodAsync(ToString() + "-" + arg.ClientId, nv, CancellationToken.None);

                    mqttRpcResult = new() { Message = result.Message, RpcId = rpcData.RpcId, Success = result.IsSuccess };

                }
                else
                {
                    mqttRpcResult = new() { Message = "权限不足，变量不支持写入", RpcId = rpcData.RpcId, Success = false };
                }

            }
            else
            {
                mqttRpcResult = new() { Message = "不存在该变量", RpcId = rpcData.RpcId, Success = false };
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, ToString());
            mqttRpcResult = new() { Message = "Failed", RpcId = rpcData.RpcId, Success = false };
        }
        try
        {
            var variableMessage = new MqttApplicationMessageBuilder()
.WithTopic($"{driverPropertys.RpcSubTopic}")
.WithPayload(mqttRpcResult.ToJson()).Build();
            var isConnect = await TryMqttClientAsync(CancellationToken.None);
            if (isConnect.IsSuccess)
                await _mqttClient.PublishAsync(variableMessage);
        }
        catch
        {
        }
    }

    private async Task _mqttClient_ConnectedAsync(MqttClientConnectedEventArgs arg)
    {
        var subResult = await _mqttClient.SubscribeAsync(_mqttSubscribeOptions);
        if (subResult.Items.Any(a => a.ResultCode > (MqttClientSubscribeResultCode)10))
        {
            _logger.LogError("订阅失败-" + subResult.Items
                .Where(a => a.ResultCode > (MqttClientSubscribeResultCode)10)
                .ToJson());
        }
    }
    private async Task AllPublishAsync(CancellationToken cancellationToken)
    {
        //保留消息
        //分解List，避免超出mqtt字节大小限制
        var varData = _globalDeviceData.AllVariables.Adapt<List<VariableData>>().ChunkTrivialBetter(driverPropertys.SplitSize);
        var devData = _globalDeviceData.AllVariables.Adapt<List<DeviceData>>().ChunkTrivialBetter(driverPropertys.SplitSize);
        var isConnect = await TryMqttClientAsync(cancellationToken);
        foreach (var item in devData)
        {
            var devMessage = new MqttApplicationMessageBuilder()
.WithTopic($"{driverPropertys.DeviceTopic}")
.WithPayload(item.GetSciptListValue(driverPropertys.BigTextScriptDeviceModel)).Build();
            if (isConnect.IsSuccess)
                await _mqttClient.PublishAsync(devMessage);
        }

        foreach (var item in varData)
        {
            var varMessage = new MqttApplicationMessageBuilder()
            .WithTopic($"{driverPropertys.VariableTopic}")
            .WithPayload(item.GetSciptListValue(driverPropertys.BigTextScriptVariableModel)).Build();
            if (isConnect.IsSuccess)
                await _mqttClient.PublishAsync(varMessage);
        }
    }

    private void DeviceStatusCahnge(CollectDeviceRunTime collectDeviceRunTime)
    {
        _collectDeviceRunTimes.Enqueue(collectDeviceRunTime.Adapt<DeviceData>());
    }

    private async Task<OperResult> TryMqttClientAsync(CancellationToken cancellationToken)
    {
        if (_mqttClient?.IsConnected == true)
            return OperResult.CreateSuccessResult();
        return await Cilent();

        async Task<OperResult> Cilent()
        {
            if (_mqttClient?.IsConnected == true)
                return OperResult.CreateSuccessResult();
            try
            {
                await lockobj.LockAsync();
                if (_mqttClient?.IsConnected == true)
                    return OperResult.CreateSuccessResult();
                using var timeoutToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(driverPropertys.ConnectTimeOut));
                using CancellationTokenSource StoppingToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutToken.Token);
                if (_mqttClient?.IsConnected == true)
                    return OperResult.CreateSuccessResult();
                if (_mqttClient == null)
                {
                    CurDevice.LastErrorMessage = "未初始化";
                    return new OperResult("未初始化");

                }
                var result = await _mqttClient?.ConnectAsync(_mqttClientOptions, StoppingToken.Token);
                if (result.ResultCode == MqttClientConnectResultCode.Success)
                {
                    return OperResult.CreateSuccessResult();
                }
                else
                {
                    CurDevice.LastErrorMessage = "result.ReasonString";
                    return new OperResult(result.ReasonString);
                }
            }
            catch (Exception ex)
            {
                CurDevice.LastErrorMessage = ex.Message;
                return new OperResult(ex);
            }
            finally
            {
                lockobj.UnLock();
            }
        }
    }

    private void VariableValueChange(DeviceVariableRunTime collectVariableRunTime)
    {
        _collectVariableRunTimes.Enqueue(collectVariableRunTime.Adapt<VariableData>());
    }
}
