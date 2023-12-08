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

using Microsoft.Extensions.DependencyInjection;

using System.Text;

using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace ThingsGateway.Admin.Core;

/// <summary>
/// 控制台输出组件
/// </summary>
public sealed class LoggingConsoleComponent : IServiceComponent
{
    /// <inheritdoc/>
    public void Load(IServiceCollection services, ComponentContext componentContext)
    {
        services.AddConsoleFormatter(options =>
         {
             options.MessageFormat = (logMsg) =>
             {
                 //如果不是LoggingMonitor日志才格式化
                 if (logMsg.LogName != "System.Logging.LoggingMonitor")
                 {
                     var stringBuilder = new StringBuilder();
                     stringBuilder.AppendLine("【日志级别】：" + logMsg.LogLevel);
                     stringBuilder.AppendLine("【日志类名】：" + logMsg.LogName);
                     stringBuilder.AppendLine("【日志时间】：" + DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat());
                     stringBuilder.AppendLine("【日志内容】：" + logMsg.Message);
                     if (logMsg.Exception != null)
                     {
                         stringBuilder.AppendLine("【异常信息】：" + logMsg.Exception);
                     }
                     return stringBuilder.ToString();
                 }
                 else
                 {
                     return logMsg.Message;
                 }
             };
             options.WriteHandler = (logMsg, scopeProvider, writer, fmtMsg, opt) =>
             {
                 ConsoleColor consoleColor = ConsoleColor.White;
                 switch (logMsg.LogLevel)
                 {
                     case LogLevel.Information:
                         consoleColor = ConsoleColor.DarkGreen;
                         break;

                     case LogLevel.Warning:
                         consoleColor = ConsoleColor.DarkYellow;
                         break;

                     case LogLevel.Error:
                         consoleColor = ConsoleColor.DarkRed;
                         break;
                 }
                 writer.WriteWithColor(fmtMsg, ConsoleColor.Black, consoleColor);
             };
         });
    }
}