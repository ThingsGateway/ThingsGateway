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

using MQTTnet.Diagnostics;

namespace ThingsGateway.Gateway.Application;

public static class MqttLoggerExtensions
{
    public static void LogOut(this ILog LogMessage, MqttNetLogLevel logLevel, string source, string message, Exception exception)
    {
        switch (logLevel)
        {
            case MqttNetLogLevel.Verbose:
                LogMessage?.Log(Foundation.Core.LogLevel.Trace, source, message, exception);
                break;

            case MqttNetLogLevel.Info:
                LogMessage?.Log(Foundation.Core.LogLevel.Info, source, message, exception);
                break;

            case MqttNetLogLevel.Warning:
                LogMessage?.Log(Foundation.Core.LogLevel.Warning, source, message, exception);
                break;

            case MqttNetLogLevel.Error:
                LogMessage?.Log(Foundation.Core.LogLevel.Warning, source, message, exception);
                break;
        }
    }

    public static void LogOut(this ILogger LogMessage, MqttNetLogLevel logLevel, string source, string message, Exception exception)
    {
        switch (logLevel)
        {
            case MqttNetLogLevel.Verbose:
                LogMessage?.Log(Microsoft.Extensions.Logging.LogLevel.Trace, source, message, exception);
                break;

            case MqttNetLogLevel.Info:
                LogMessage?.Log(Microsoft.Extensions.Logging.LogLevel.Information, source, message, exception);
                break;

            case MqttNetLogLevel.Warning:
                LogMessage?.Log(Microsoft.Extensions.Logging.LogLevel.Warning, source, message, exception);
                break;

            case MqttNetLogLevel.Error:
                LogMessage?.Log(Microsoft.Extensions.Logging.LogLevel.Warning, source, message, exception);
                break;
        }
    }

}
