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

namespace ThingsGateway.Foundation.Demo;

/// <inheritdoc/>
public partial class TcpServerPage : IDisposable
{
    /// <summary>
    /// 日志输出
    /// </summary>
    public Action<LogLevel, object, string, Exception> LogAction;

    private TouchSocketConfig _config;

    private string _ip = "127.0.0.1";

    private int _port = 502;

    private TcpService _tcpServer { get; set; } = new();

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Dispose()
    {
        _tcpServer.SafeDispose();
    }

    /// <summary>
    /// 获取对象
    /// </summary>
    /// <returns></returns>
    public TcpService GetTcpServer()
    {
        _config ??= new TouchSocketConfig();
        var LogMessage = new LoggerGroup() { LogLevel = LogLevel.Trace };
        LogMessage.AddLogger(new EasyLogger(LogOut) { LogLevel = LogLevel.Trace });
        _config.ConfigureContainer(a => a.RegisterSingleton<ILog>(LogMessage));
        _config.SetListenIPHosts(new IPHost[] { new IPHost(_ip + ":" + _port) });
        //载入配置
        _tcpServer.Setup(_config);
        return _tcpServer;
    }

    internal void StateHasChangedAsync()
    {
        StateHasChanged();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="firstRender"></param>
    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            var LogMessage = new LoggerGroup() { LogLevel = LogLevel.Trace };
            LogMessage.AddLogger(new EasyLogger(LogOut) { LogLevel = LogLevel.Trace });
            _config.ConfigureContainer(a => a.RegisterSingleton<ILog>(LogMessage));
            _config.SetListenIPHosts(new IPHost[] { new IPHost(_ip + ":" + _port) });
            _tcpServer.Setup(_config);
        }
        base.OnAfterRender(firstRender);
    }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        _config ??= new TouchSocketConfig();

        base.OnInitialized();
    }

    private void Connect()
    {
        try
        {
            _tcpServer.Stop();
            GetTcpServer().Start();
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
            _tcpServer.Stop();
        }
        catch (Exception ex)
        {
            LogAction?.Invoke(LogLevel.Error, null, null, ex);
        }
    }

    private void LogOut(LogLevel logLevel, object source, string message, Exception exception) => LogAction?.Invoke(logLevel, source, message, exception);
}