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

using MQTTnet.Diagnostics;

namespace ThingsGateway.Plugin.Mqtt
{
    internal class PrivateLogger : IMqttNetLogger
    {
        readonly ILog LogMessage;
        public PrivateLogger(ILog logger)
        {
            LogMessage = logger;
        }

        public bool IsEnabled => true;
        public void Publish(MqttNetLogLevel logLevel, string source, string message, object[] parameters, Exception exception)
        {
            switch (logLevel)
            {
                case MqttNetLogLevel.Verbose:
                    LogMessage?.Log(LogLevel.Trace, source, message != null ? (parameters != null ? message != null ? (parameters != null ? string.Format(message, parameters) : message) : string.Empty : message) : string.Empty, exception);
                    break;

                case MqttNetLogLevel.Info:
                    LogMessage?.Log(LogLevel.Info, source, message != null ? (parameters != null ? string.Format(message, parameters) : message) : string.Empty, exception);
                    break;

                case MqttNetLogLevel.Warning:
                    LogMessage?.Log(LogLevel.Warning, source, message != null ? (parameters != null ? string.Format(message, parameters) : message) : string.Empty, exception);
                    break;

                case MqttNetLogLevel.Error:
                    LogMessage?.Log(LogLevel.Warning, source, message != null ? (parameters != null ? string.Format(message, parameters) : message) : string.Empty, exception);
                    break;
            }
        }
    }

}
