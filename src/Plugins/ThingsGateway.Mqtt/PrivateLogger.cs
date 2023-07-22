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

using TouchSocket.Core;

namespace ThingsGateway.Mqtt
{
    internal class PrivateLogger : IMqttNetLogger
    {
        public bool IsEnabled => true;
        ILog logMessage;
        public PrivateLogger(ILog logger)
        {
            logMessage = logger;
        }
        public void Publish(MqttNetLogLevel logLevel, string source, string message, object[] parameters, Exception exception)
        {
            switch (logLevel)
            {
                case MqttNetLogLevel.Verbose:
                    logMessage?.Log(LogType.Trace, source, message, exception);
                    break;

                case MqttNetLogLevel.Info:
                    logMessage?.Log(LogType.Info, source, message, exception);
                    break;

                case MqttNetLogLevel.Warning:
                    logMessage?.Log(LogType.Warning, source, message, exception);
                    break;

                case MqttNetLogLevel.Error:
                    logMessage?.Log(LogType.Warning, source, message, exception);
                    break;
            }
        }
    }

}
