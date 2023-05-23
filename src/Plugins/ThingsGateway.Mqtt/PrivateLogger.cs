#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Microsoft.Extensions.Logging;

using MQTTnet.Diagnostics;

using TouchSocket.Core;

namespace ThingsGateway.Mqtt
{
    internal class PrivateLogger : IMqttNetLogger
    {
        public bool IsEnabled => true;
        ILogger _logger;
        public PrivateLogger(ILogger logger)
        {
            _logger = logger;
        }
        public void Publish(MqttNetLogLevel logLevel, string source, string message, object[] parameters, Exception exception)
        {
            switch (logLevel)
            {
                case MqttNetLogLevel.Verbose:
                    _logger?.Log(LogLevel.Trace, 0, exception, source + "-" + message, parameters);
                    break;

                case MqttNetLogLevel.Info:
                    _logger?.Log(LogLevel.Information, 0, exception, source + "-" + message, parameters);
                    break;

                case MqttNetLogLevel.Warning:
                    _logger?.Log(LogLevel.Warning, 0, exception, source + "-" + message, parameters);
                    break;

                case MqttNetLogLevel.Error:
                    _logger?.Log(LogLevel.Warning, 0, exception, source + "-" + message, parameters);
                    break;
            }
        }
    }

}
