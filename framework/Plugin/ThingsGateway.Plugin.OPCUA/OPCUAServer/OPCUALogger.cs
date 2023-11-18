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

using Microsoft.Extensions.Logging;

namespace ThingsGateway.Plugin.OPCUA;


internal class OPCUALogger : ILogger
{
    private ILog _log;
    public OPCUALogger(ILog log)
    {
        _log = log;

    }
    /// <summary>
    /// Set the log level
    /// </summary>
    public Microsoft.Extensions.Logging.LogLevel LogLevel { get; set; } = Microsoft.Extensions.Logging.LogLevel.Trace;
    /// <inheritdoc/>
    public IDisposable BeginScope<TState>(TState state) => default;
    /// <inheritdoc/>
    public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => logLevel >= LogLevel;
    /// <inheritdoc/>
    public void Log<TState>(
        Microsoft.Extensions.Logging.LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }
        else
        {
            var message = formatter(state, exception);
            if (logLevel > Microsoft.Extensions.Logging.LogLevel.Warning)
            {
                _log.Log((Foundation.Core.LogLevel)(byte)logLevel, state, message, exception);
            }
        }
    }
}
