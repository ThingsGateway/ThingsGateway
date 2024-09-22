//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using MQTTnet;
using MQTTnet.Client;

using Newtonsoft.Json.Linq;

using System.Text;

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Json.Extension;

namespace ThingsGateway.Plugin.Mqtt;

/// <summary>
/// MqttClient
/// </summary>
public partial class MqttCollect : CollectBase
{
    private IMqttClient _mqttClient;

    private MqttClientOptions _mqttClientOptions;

    private MqttClientSubscribeOptions _mqttSubscribeOptions;

    private WaitLock ConnectLock = new();


    #region mqtt方法


    private Task MqttClient_ApplicationMessageReceivedAsync(MQTTnet.Client.MqttApplicationMessageReceivedEventArgs args)
    {
        try
        {
            if (TopicItemDict.TryGetValue(args.ApplicationMessage.Topic, out var tuples))
            {

                JToken json = JToken.Parse(Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment));
                DateTime dateTime = DateTime.Now;
                foreach (var item in tuples)
                {
                    try
                    {
                        var jtoken = json.SelectToken(item.Item1);
                        object value;
                        if (jtoken is JValue jValue)
                        {
                            value = jValue.Value;
                        }
                        else
                        {
                            value = jtoken;
                        }
                        item.Item2.SetValue(value, dateTime);
                    }
                    catch (Exception ex)
                    {
                        LogMessage.LogTrace($"parse error: topic  {Environment.NewLine}{args.ApplicationMessage.Topic}  {Environment.NewLine} json {Environment.NewLine}{json} {Environment.NewLine} select: {item.Item1} {Environment.NewLine} {ex}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage.LogTrace($"parse error: topic  {Environment.NewLine}{args.ApplicationMessage.Topic} {Environment.NewLine} {ex}");
        }
        return Task.CompletedTask;


    }

    private async Task MqttClient_ConnectedAsync(MQTTnet.Client.MqttClientConnectedEventArgs args)
    {
        //连接成功后订阅相关主题
        if (_mqttSubscribeOptions != null)
        {
            var subResult = await _mqttClient.SubscribeAsync(_mqttSubscribeOptions).ConfigureAwait(false);
            if (subResult.Items.Any(a => a.ResultCode > (MqttClientSubscribeResultCode)10))
            {
                LogMessage?.LogWarning($"Subscribe fail  {subResult.Items
                    .Where(a => a.ResultCode > (MqttClientSubscribeResultCode)10)
                    .Select(a =>
                    new
                    {
                        Topic = a.TopicFilter.Topic,
                        ResultCode = a.ResultCode.ToString()
                    }
                    )
                    .ToJsonNetString()}");
            }
        }
    }

    /// <summary>
    /// 上传mqtt，返回上传结果
    /// </summary>
    private async ValueTask<OperResult> MqttUpAsync(string topic, string payLoad, CancellationToken cancellationToken)
    {
        try
        {
            var isConnect = await TryMqttClientAsync(cancellationToken).ConfigureAwait(false);
            if (isConnect.IsSuccess)
            {
                var variableMessage = new MqttApplicationMessageBuilder()
    .WithTopic(topic).WithRetainFlag(true)
    .WithPayload(payLoad).Build();
                var result = await _mqttClient.PublishAsync(variableMessage, cancellationToken).ConfigureAwait(false);
                if (result.IsSuccess)
                {
                    if (LogMessage.LogLevel <= TouchSocket.Core.LogLevel.Trace)
                        LogMessage.LogTrace($"Topic：{topic}{Environment.NewLine}PayLoad：{payLoad}");
                    return OperResult.Success;
                }
                else
                {
                    return new OperResult($"Upload fail{result.ReasonString}");
                }
            }
            else
            {
                return isConnect;
            }
        }
        catch (Exception ex)
        {
            return new OperResult($"Upload fail", ex);
        }
    }

    private async ValueTask<OperResult> TryMqttClientAsync(CancellationToken cancellationToken)
    {
        if (_mqttClient?.IsConnected == true)
            return OperResult.Success;
        return await Cilent().ConfigureAwait(false);

        async ValueTask<OperResult> Cilent()
        {
            if (_mqttClient?.IsConnected == true)
                return OperResult.Success;
            try
            {
                await ConnectLock.WaitAsync().ConfigureAwait(false);
                if (_mqttClient?.IsConnected == true)
                    return OperResult.Success;
                using var timeoutToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(_driverPropertys.ConnectTimeout));
                using CancellationTokenSource stoppingToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutToken.Token);
                if (_mqttClient?.IsConnected == true)
                    return OperResult.Success;
                if (_mqttClient == null)
                {
                    return new OperResult("mqttClient is null");
                }
                var result = await _mqttClient.ConnectAsync(_mqttClientOptions, stoppingToken.Token).ConfigureAwait(false);
                if (_mqttClient.IsConnected)
                {
                    return OperResult.Success;
                }
                else
                {
                    if (timeoutToken.IsCancellationRequested)
                        return new OperResult($"Connect timeout");
                    else
                        return new OperResult($"Connect fail {result.ReasonString}");
                }
            }
            catch (Exception ex)
            {
                return new OperResult(ex);
            }
            finally
            {
                ConnectLock.Release();
            }
        }
    }


    #endregion mqtt方法
}
