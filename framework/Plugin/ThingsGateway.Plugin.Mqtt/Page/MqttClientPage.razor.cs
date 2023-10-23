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

using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Diagnostics;

using System.Text;

using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Foundation.Demo;
/// <summary>
/// MqttClientPage
/// </summary>
public partial class MqttClientPage
{
    /// <summary>
    /// 日志输出
    /// </summary>
    public Action<LogLevel, object, string, Exception> LogAction;

    /// <summary>
    /// OPC
    /// </summary>
    public IMqttClient MqttClient;
    public MqttClientOptions MqttClientOptions;

    public MqttFactory MqttFactory;

    public string ConnectId;

    public string IP;

    public string Password;

    public int Port;

    public string UserName;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Dispose()
    {
        MqttClient.SafeDispose();
    }
    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        var log = new MqttNetEventLogger();
        log.LogMessagePublished += Log_LogMessagePublished;
        MqttFactory = new MqttFactory(log);


        MqttClient = MqttFactory.CreateMqttClient();
        MqttClient.ApplicationMessageReceivedAsync += MqttClient_ApplicationMessageReceivedAsync;
        base.OnInitialized();
    }
    private void Log_LogMessagePublished(object sender, MqttNetLogMessagePublishedEventArgs e)
    {
        new EasyLogger(LogAction) { LogLevel = LogLevel.Trace }.LogOut(e.LogMessage.Level, e.LogMessage.Source, e.LogMessage.Message, e.LogMessage.Exception);
    }
    private async Task Connect()
    {
        try
        {
            await MqttClient.DisconnectAsync();
            await GetMqttClient();
        }
        catch (Exception ex)
        {
            LogAction?.Invoke(LogLevel.Error, null, null, ex);
        }
    }

    private async Task DisConnect()
    {
        try
        {
            await MqttClient.DisconnectAsync();
        }
        catch (Exception ex)
        {
            LogAction?.Invoke(LogLevel.Error, null, null, ex);
        }
    }

    private async Task<IMqttClient> GetMqttClient()
    {
        //载入配置
        MqttClientOptions = MqttFactory.CreateClientOptionsBuilder()
   .WithClientId(ConnectId)
   .WithCredentials(UserName, Password)//账密
   .WithTcpServer(IP, Port)//服务器
   .WithCleanSession(true)
   .WithKeepAlivePeriod(TimeSpan.FromSeconds(120.0))
   .WithoutThrowOnNonSuccessfulConnectResponse()
   .Build();
        await MqttClient.ConnectAsync(MqttClientOptions);
        return MqttClient;
    }

    private void LogOut(byte logLevel, object source, string message, Exception exception) => LogAction?.Invoke((LogLevel)logLevel, source, message, exception);

    private Task MqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        LogAction?.Invoke(LogLevel.Info, this, $"[{args.ApplicationMessage.Topic}]：{Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment)}", null);
        return Task.CompletedTask;
    }

    public Task StateHasChangedAsync()
    {
        return InvokeAsync(StateHasChanged);
    }
}