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
public partial class SerialSessionPage : IDisposable
{
    /// <summary>
    /// 日志输出
    /// </summary>
    public Action<LogLevel, object, string, Exception> LogAction;

    private readonly SerialProperty _serialProperty = new();
    private TouchSocketConfig _config;
    private SerialSession _serialSession { get; set; } = new();

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Dispose()
    {
        _serialSession.SafeDispose();
    }

    /// <summary>
    /// 获取对象
    /// </summary>
    /// <returns></returns>
    public SerialSession GetSerialSession()
    {
        _config ??= new TouchSocketConfig();
        var LogMessage = new LoggerGroup() { LogLevel = LogLevel.Trace };
        LogMessage.AddLogger(new EasyLogger(LogOut) { LogLevel = LogLevel.Trace });
        _config.ConfigureContainer(a => a.RegisterSingleton<ILog>(LogMessage));
        _config.SetSerialProperty(_serialProperty);
        //载入配置
        _serialSession.Setup(_config);
        return _serialSession;
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
            _serialSession.Setup(_config);
        }
        base.OnAfterRender(firstRender);
    }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        _config ??= new TouchSocketConfig();

        base.OnInitialized();
    }

    private async Task ConnectAsync()
    {
        try
        {
            _serialSession.Close();
            await GetSerialSession().ConnectAsync();
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
            _serialSession.Close();
        }
        catch (Exception ex)
        {
            LogAction?.Invoke(LogLevel.Error, null, null, ex);
        }
    }
    private void LogOut(LogLevel logLevel, object source, string message, Exception exception) => LogAction?.Invoke(logLevel, source, message, exception);
}