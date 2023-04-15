using Microsoft.Extensions.Logging;

using TouchSocket.Core;

namespace ThingsGateway.Web.Foundation
{
    /// <summary>
    /// 扩展
    /// </summary>
    public static class PluginExtension
    {
        /// <summary>
        /// <see cref="TouchSocket"/> 日志输出
        /// </summary>
        public static void Log_Out(ILogger logger, LogType arg1, object arg2, string arg3, Exception arg4)
        {
            switch (arg1)
            {
                case LogType.None:
                    logger?.Log(LogLevel.None, 0, arg4, arg3);
                    break;
                case LogType.Trace:
                    logger?.Log(LogLevel.Trace, 0, arg4, arg3);
                    break;
                case LogType.Debug:
                    logger?.Log(LogLevel.Debug, 0, arg4, arg3);
                    break;
                case LogType.Info:
                    logger?.Log(LogLevel.Information, 0, arg4, arg3);
                    break;
                case LogType.Warning:
                    logger?.Log(LogLevel.Warning, 0, arg4, arg3);
                    break;
                case LogType.Error:
                    logger?.Log(LogLevel.Error, 0, arg4, arg3);
                    break;
                case LogType.Critical:
                    logger?.Log(LogLevel.Critical, 0, arg4, arg3);
                    break;
                default:
                    break;
            }
        }
    }
}
