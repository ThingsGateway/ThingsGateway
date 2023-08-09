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

using ThingsGateway.Foundation.Serial;

using TouchSocket.Core;

namespace ThingsGateway.Blazor;

/// <inheritdoc/>
public partial class SerialClientPage
{
    /// <summary>
    /// 日志输出
    /// </summary>
    public Action<LogLevel, object, string, Exception> LogAction;

    private TouchSocketConfig config;

    private readonly SerialProperty serialProperty = new();

    private SerialClient SerialClient { get; set; } = new();

    /// <inheritdoc/>
    public void Dispose()
    {
        SerialClient.SafeDispose();
    }
    /// <summary>
    /// 获取对象
    /// </summary>
    /// <returns></returns>
    public SerialClient GetSerialClient()
    {
        config?.Dispose();
        config = new TouchSocketConfig();
        var LogMessage = new TouchSocket.Core.LoggerGroup() { LogLevel = TouchSocket.Core.LogLevel.Trace };
        LogMessage.AddLogger(new EasyLogger(LogOut) { LogLevel = TouchSocket.Core.LogLevel.Trace });
        config.ConfigureContainer(a => a.RegisterSingleton<ILog>(LogMessage));
        config.SetSerialProperty(serialProperty);
        //载入配置
        SerialClient.Setup(config);
        return SerialClient;
    }
    private async Task ConnectAsync()
    {
        try
        {
            SerialClient.Close();
            await GetSerialClient().ConnectAsync();
        }
        catch (Exception ex)
        {
            LogAction?.Invoke(LogLevel.Error, null, null, ex);
        }

    }
    private void DisConnect()
    {
        try
        {
            SerialClient.Close();
        }
        catch (Exception ex)
        {
            LogAction?.Invoke(LogLevel.Error, null, null, ex);
        }
    }
    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        config ??= new TouchSocketConfig();
        var LogMessage = new TouchSocket.Core.LoggerGroup() { LogLevel = TouchSocket.Core.LogLevel.Trace };
        LogMessage.AddLogger(new EasyLogger(LogOut) { LogLevel = TouchSocket.Core.LogLevel.Trace });
        config.ConfigureContainer(a => a.RegisterSingleton<ILog>(LogMessage));
        SerialClient = new SerialClient();
        SerialClient.Setup(config);
        base.OnInitialized();
    }

    private void LogOut(LogLevel logLevel, object source, string message, Exception exception) => LogAction?.Invoke(logLevel, source, message, exception);

}