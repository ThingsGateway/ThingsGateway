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

using Masa.Blazor;

using Microsoft.AspNetCore.Components;

using MQTTnet;
using MQTTnet.Extensions.Rpc;

using System.Collections.Generic;
using System.Text;

using ThingsGateway.Plugin.Mqtt;

namespace ThingsGateway.Foundation.Demo;

/// <summary>
/// MqttClientDebugPage
/// </summary>
public partial class MqttClientDebugPage : IDisposable
{
    private DriverDebugUIPage driverDebugUIPage;
    private MqttClientPage mqttClientPage;

    [Inject]
    IPopupService PopupService { get; set; }

    /// <inheritdoc/>
    public void Dispose()
    {
        mqttClientPage.SafeDispose();
    }
    /// <inheritdoc/>
    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            if (mqttClientPage != null)
            {
                mqttClientPage.LogAction = driverDebugUIPage.LogOut;
            }
            //初始化
            driverDebugUIPage.Address = "ThingsGateway/Variable";

            driverDebugUIPage.WriteValue = new MqttRpcNameVaueWithId()
            {
                RpcId = Guid.NewGuid().ToString(),
                WriteInfos = new Dictionary<string, string>()
{
    { "tag1", "123" }
}
            }.ToJsonString();
            ;
            mqttClientPage.IP = "127.0.0.1";
            mqttClientPage.Port = 1883;
            mqttClientPage.UserName = "admin";
            mqttClientPage.Password = "111111";
            mqttClientPage.StateHasChangedAsync();

            //载入配置
            StateHasChanged();
            driverDebugUIPage.Sections.Clear();
        }

        base.OnAfterRender(firstRender);
    }

    private async Task SubscribeAsync()
    {
        try
        {
            var mqttSubscribeOptions = mqttClientPage.MqttFactory.CreateSubscribeOptionsBuilder()
.WithTopicFilter(
f =>
{
    f.WithTopic(driverDebugUIPage.Address);
})
.Build();

            await mqttClientPage.MqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);
            driverDebugUIPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Information, DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(driverDebugUIPage.InitTimezone.TimezoneOffset) + " - " + $"订阅{driverDebugUIPage.Address}成功"));

        }
        catch (Exception ex)
        {
            driverDebugUIPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Error, DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(driverDebugUIPage.InitTimezone.TimezoneOffset) + " - " + ex.Message));
        }

    }
    private async Task UnsubscribeAsync()
    {
        try
        {
            var mqttSubscribeOptions = mqttClientPage.MqttFactory.CreateUnsubscribeOptionsBuilder()
.WithTopicFilter(driverDebugUIPage.Address)
.Build();

            await mqttClientPage.MqttClient.UnsubscribeAsync(mqttSubscribeOptions, CancellationToken.None);
            driverDebugUIPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Information, DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(driverDebugUIPage.InitTimezone.TimezoneOffset) + " - " + $"取消订阅{driverDebugUIPage.Address}成功"));
        }
        catch (Exception ex)
        {

            driverDebugUIPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Error, DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(driverDebugUIPage.InitTimezone.TimezoneOffset) + " - " + ex.Message));
        }

    }
    string PublishTopic;
    string PublishValue;
    private async Task PublishAsync()
    {
        try
        {
            var devMessage = new MqttApplicationMessageBuilder()
.WithTopic($"{PublishTopic}")
.WithPayload(PublishValue).Build();
            await mqttClientPage.MqttClient.PublishAsync(devMessage, CancellationToken.None);
            driverDebugUIPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Information, DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(driverDebugUIPage.InitTimezone.TimezoneOffset) + " - " + $"发布{PublishTopic}成功"));
        }
        catch (Exception ex)
        {
            driverDebugUIPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Error, DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(driverDebugUIPage.InitTimezone.TimezoneOffset) + " - " + ex.Message));
        }

    }
    MqttRpcTopicPair MqttRpcTopicPair = new() { RequestTopic = "ThingsGateway/RpcWrite", ResponseTopic = "ThingsGateway/RpcSub" };
    private async Task RpcExecuteAsync()
    {
        try
        {
            using MqttRpcClient mqttRpcClient = new(mqttClientPage.MqttClient);
            var data = await mqttRpcClient.ExecuteAsync(MqttRpcTopicPair, driverDebugUIPage.WriteValue, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce, TimeSpan.FromSeconds(10));
            var str = Encoding.UTF8.GetString(data);
            driverDebugUIPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Information, DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(driverDebugUIPage.InitTimezone.TimezoneOffset) + " - " + str));
        }
        catch (Exception ex)
        {
            driverDebugUIPage.Messages.Add((Microsoft.Extensions.Logging.LogLevel.Error, DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat(driverDebugUIPage.InitTimezone.TimezoneOffset) + " - " + ex.Message));
        }

    }
}