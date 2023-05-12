using Mapster;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MQTTnet;
using MQTTnet.Client;

using NewLife.Serialization;

using System.Collections.Concurrent;
using System.Text;

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Extension;
using ThingsGateway.Web.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Mqtt;

public class MqttClientProperty : UpDriverPropertyBase
{
    [DeviceProperty("是否间隔上传", "False时为变化检测上传")] public bool IsInterval { get; set; } = false;
    [DeviceProperty("上传间隔时间", "最小1000ms")] public int UploadInterval { get; set; } = 1000;

    [DeviceProperty("IP", "")] public string IP { get; set; } = "127.0.0.1";

    [DeviceProperty("端口", "")] public int Port { get; set; } = 1883;
    [DeviceProperty("账号", "")] public string UserName { get; set; } = "admin";
    [DeviceProperty("密码", "")] public string Password { get; set; } = "123456";
    [DeviceProperty("连接Id", "")] public string ConnectId { get; set; } = "ThingsGatewayId";

    [DeviceProperty("连接超时时间", "")] public int ConnectTimeOut { get; set; } = 3000;

    [DeviceProperty("线程循环间隔", "最小500ms")] public int CycleInterval { get; set; } = 1000;

    [DeviceProperty("允许Rpc写入", "")] public bool DeviceRpcEnable { get; set; }

    [DeviceProperty("数据请求RpcTopic", "这个主题接收到任何数据都会把全部的信息发送到变量/设备主题中")] public string QuestRpcTopic { get; set; } = "ThingsGateway/Quest";


    [DeviceProperty("设备Topic", "")] public string DeviceTopic { get; set; } = "ThingsGateway/Device";
    [DeviceProperty("变量Topic", "")] public string VariableTopic { get; set; } = "ThingsGateway/Variable";
    [DeviceProperty("Rpc返回Topic", "")] public string RpcSubTopic { get; set; } = "ThingsGateway/RpcSub";

    [DeviceProperty("Rpc写入Topic", "")] public string RpcWriteTopic { get; set; } = "ThingsGateway/RpcWrite";
    [DeviceProperty("设备实体脚本", "查看文档说明，为空时不起作用")] public string BigTextScriptDeviceModel { get; set; }

    [DeviceProperty("变量实体脚本", "查看文档说明，为空时不起作用")] public string BigTextScriptVariableModel { get; set; }
}
public class MqttClientVariableProperty : VariablePropertyBase
{
    [VariableProperty("启用", "")]
    public bool Enable { get; set; } = true;
    [VariableProperty("允许写入", "")]
    public bool VariableRpcEnable { get; set; } = true;
}
public class MqttClient : UpLoadBase
{
    private List<CollectDeviceRunTime> _collectDevice;
    private UploadDevice _curDevice;
    private GlobalCollectDeviceData _globalCollectDeviceData;

    private IMqttClient _mqttClient;

    private MqttClientOptions _mqttClientOptions;

    private MqttClientSubscribeOptions _mqttSubscribeOptions;

    private RpcSingletonService _rpcCore;
    private List<CollectVariableRunTime> _uploadVariables = new();

    private CollectDeviceWorker collectDeviceHostService;

    private ConcurrentQueue<DeviceData> CollectDeviceRunTimes = new();
    private ConcurrentQueue<VariableData> CollectVariableRunTimes = new();
    private MqttClientProperty driverPropertys = new();

    private TimerTick exVariableTimerTick;
    private TimerTick exDeviceTimerTick;
    private EasyLock lockobj = new();
    private MqttClientVariableProperty variablePropertys = new();

    public MqttClient(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
    }


    public override UpDriverPropertyBase DriverPropertys => driverPropertys;


    public override List<CollectVariableRunTime> UploadVariables => _uploadVariables;


    public override VariablePropertyBase VariablePropertys => variablePropertys;
    public override async Task BeforStartAsync()
    {

        if (_mqttClient != null)
        {
            var result = await TryMqttClientAsync();
            if (!result.IsSuccess)
            {
                _logger?.LogWarning(ToString() + $"-连接MqttServer失败：{result.Message}");
            }
        }
    }

    public override void Dispose()
    {
        try
        {
            lockobj.Lock();
            _globalCollectDeviceData?.CollectVariables?.ForEach(a => a.VariableValueChange -= VariableValueChange);

            _globalCollectDeviceData?.CollectDevices?.ForEach(a =>
            {
                a.DeviceStatusCahnge -= DeviceStatusCahnge;
            });
            _mqttClient?.Dispose();
            _mqttClient = null;
        }
        finally
        {
            lockobj.UnLock();
        }

    }
    public override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (!driverPropertys.IsInterval)
            {
                ////变化推送
                var varList = CollectVariableRunTimes.ToListWithDequeue();
                if (varList?.Count != 0)
                {
                    //分解List，避免超出mqtt字节大小限制
                    var varData = varList.ChunkTrivialBetter(500);
                    foreach (var item in varData)
                    {
                        try
                        {
                            if (!cancellationToken.IsCancellationRequested)
                            {
                                var variableMessage = new MqttApplicationMessageBuilder()
    .WithTopic($"{driverPropertys.VariableTopic}")
    .WithPayload(item.GetSciptListValue(driverPropertys.BigTextScriptVariableModel)).Build();
                                var isConnect = await TryMqttClientAsync();
                                if (isConnect.IsSuccess)
                                    await _mqttClient.PublishAsync(variableMessage, cancellationToken);
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
                            var varData = varList.ChunkTrivialBetter(500);
                            foreach (var item in varData)
                            {
                                try
                                {
                                    if (!cancellationToken.IsCancellationRequested)
                                    {
                                        var variableMessage = new MqttApplicationMessageBuilder()
            .WithTopic($"{driverPropertys.VariableTopic}")
            .WithPayload(item.GetSciptListValue(driverPropertys.BigTextScriptVariableModel)).Build();
                                        var isConnect = await TryMqttClientAsync();
                                        if (isConnect.IsSuccess)
                                            await _mqttClient.PublishAsync(variableMessage, cancellationToken);
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
                var devList = CollectDeviceRunTimes.ToListWithDequeue();
                if (devList?.Count != 0)
                {
                    //分解List，避免超出mqtt字节大小限制
                    var devData = devList.ChunkTrivialBetter(500);
                    foreach (var item in devData)
                    {
                        try
                        {
                            if (!cancellationToken.IsCancellationRequested)
                            {
                                var variableMessage = new MqttApplicationMessageBuilder()
                        .WithTopic($"{driverPropertys.DeviceTopic}")
                        .WithPayload(item.GetSciptListValue(driverPropertys.BigTextScriptDeviceModel)).Build();
                                var isConnect = await TryMqttClientAsync();
                                if (isConnect.IsSuccess)
                                    await _mqttClient.PublishAsync(variableMessage);
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
                        var devData = devList.ChunkTrivialBetter(500);
                        foreach (var item in devData)
                        {
                            try
                            {
                                if (!cancellationToken.IsCancellationRequested)
                                {
                                    var variableMessage = new MqttApplicationMessageBuilder()
                            .WithTopic($"{driverPropertys.DeviceTopic}")
                            .WithPayload(item.GetSciptListValue(driverPropertys.BigTextScriptDeviceModel)).Build();
                                    var isConnect = await TryMqttClientAsync();
                                    if (isConnect.IsSuccess)
                                        await _mqttClient.PublishAsync(variableMessage);
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
            await Task.Delay(driverPropertys.CycleInterval - UploadDeviceThread.CycleInterval);
        }
        else
        {

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

    protected override void Init(UploadDevice device)
    {
        _curDevice = device;
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
        using var serviceScope = _scopeFactory.CreateScope();
        _globalCollectDeviceData = serviceScope.ServiceProvider.GetService<GlobalCollectDeviceData>();
        _rpcCore = serviceScope.ServiceProvider.GetService<RpcSingletonService>();
        collectDeviceHostService = serviceScope.GetBackgroundService<CollectDeviceWorker>();

        var tags = _globalCollectDeviceData.CollectVariables.Where(a => a.VariablePropertys.ContainsKey(device.Id))
           .Where(b => b.VariablePropertys[device.Id].Any(c =>
           {
               if (c.PropertyName == nameof(variablePropertys.Enable))
               {
                   if (c.Value?.GetBoolValue() == true)
                       return true;
                   else
                       return false;
               }
               else
                   return false;
           }))
           .ToList();

        _uploadVariables = tags;

        _collectDevice = _globalCollectDeviceData.CollectDevices.Where(a => _uploadVariables.Select(b => b.DeviceId).Contains(a.Id)).ToList();
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
            await AllPublishAsync();
            return;
        }

        if (!driverPropertys.DeviceRpcEnable || arg.ClientId.IsNullOrEmpty())
            return;
        if (arg.ApplicationMessage.Topic != driverPropertys.RpcWriteTopic)
            return;
        var rpcData = Encoding.UTF8.GetString(arg.ApplicationMessage.PayloadSegment).ToJsonEntity<MqttRpcNameVaueWithId>();
        if (rpcData == null)
            return;

        MqttRpcResult mqttRpcResult = new();
        try
        {
            var nv = rpcData.Adapt<NameValue>();
            var tag = _uploadVariables.FirstOrDefault(a => a.Name == nv.Name);
            if (tag != null)
            {
                var rpcEnable = tag.VariablePropertys[_curDevice.Id]?.Any(c =>
                  {
                      if (c.PropertyName == nameof(variablePropertys.VariableRpcEnable))
                      {
                          if (c.Value?.GetBoolValue() == true)
                              return true;
                          else
                              return false;
                      }
                      else
                          return false;
                  });
                if (rpcEnable == true)
                {
                    var result = await _rpcCore.InvokeDeviceMethodAsync(ToString() + "-" + arg.ClientId, nv);

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
            var isConnect = await TryMqttClientAsync();
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
    private async Task AllPublishAsync()
    {
        //保留消息
        //分解List，避免超出mqtt字节大小限制
        var varData = _globalCollectDeviceData.CollectVariables.Adapt<List<VariableData>>().ChunkTrivialBetter(500);
        var devData = _globalCollectDeviceData.CollectVariables.Adapt<List<DeviceData>>().ChunkTrivialBetter(500);
        var isConnect = await TryMqttClientAsync();
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
        CollectDeviceRunTimes.Enqueue(collectDeviceRunTime.Adapt<DeviceData>());
    }

    private async Task<OperResult> TryMqttClientAsync(bool reconnect = false)
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
                using (var timeoutToken = new CancellationTokenSource(TimeSpan.FromSeconds(driverPropertys.ConnectTimeOut)))
                {
                    if (_mqttClient?.IsConnected == true)
                        return OperResult.CreateSuccessResult();
                    if (_mqttClient == null)
                        return new OperResult("未初始化");
                    var result = await _mqttClient?.ConnectAsync(_mqttClientOptions, timeoutToken.Token);
                    if (result.ResultCode == MqttClientConnectResultCode.Success)
                    {

                        return OperResult.CreateSuccessResult();

                    }
                    else
                    {
                        return new OperResult(result.ReasonString);
                    }
                }
            }
            catch (Exception ex)
            {
                return new OperResult(ex);
            }
            finally
            {
                lockobj.UnLock();
            }
        }
    }

    private void VariableValueChange(CollectVariableRunTime collectVariableRunTime)
    {
        CollectVariableRunTimes.Enqueue(collectVariableRunTime.Adapt<VariableData>());
    }
}
