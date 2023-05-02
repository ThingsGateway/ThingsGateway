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
