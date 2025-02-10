//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;

using TouchSocket.Core;

namespace ThingsGateway.Upgrade;

/// <summary>
/// 扩展
/// </summary>
[ThingsGateway.DependencyInjection.SuppressSniffer]
public static class LogExtensions
{
    /// <summary>
    /// <see cref="LoggerGroup"/> 日志输出
    /// </summary>
    public static void Log_Out(this ILogger logger, TouchSocket.Core.LogLevel arg1, object arg2, string arg3, Exception arg4)
    {
        switch (arg1)
        {
            case TouchSocket.Core.LogLevel.None:
                logger?.Log(Microsoft.Extensions.Logging.LogLevel.None, 0, arg4, arg3);
                break;

            case TouchSocket.Core.LogLevel.Trace:
                logger?.Log(Microsoft.Extensions.Logging.LogLevel.Trace, 0, arg4, arg3);
                break;

            case TouchSocket.Core.LogLevel.Debug:
                logger?.Log(Microsoft.Extensions.Logging.LogLevel.Debug, 0, arg4, arg3);
                break;

            case TouchSocket.Core.LogLevel.Info:
                logger?.Log(Microsoft.Extensions.Logging.LogLevel.Information, 0, arg4, arg3);
                break;

            case TouchSocket.Core.LogLevel.Warning:
                logger?.Log(Microsoft.Extensions.Logging.LogLevel.Warning, 0, arg4, arg3);
                break;

            case TouchSocket.Core.LogLevel.Error:
                logger?.Log(Microsoft.Extensions.Logging.LogLevel.Error, 0, arg4, arg3);
                break;

            case TouchSocket.Core.LogLevel.Critical:
                logger?.Log(Microsoft.Extensions.Logging.LogLevel.Critical, 0, arg4, arg3);
                break;

            default:
                break;
        }
    }
}
