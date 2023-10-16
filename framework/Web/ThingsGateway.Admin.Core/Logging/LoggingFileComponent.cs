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

using Furion;
using Furion.Logging;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.Text;

using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace ThingsGateway.Admin.Core;

/// <summary>
/// 日志写入文件的组件
/// </summary>
public sealed class LoggingFileComponent : IServiceComponent
{
    /// <inheritdoc/>
    public void Load(IServiceCollection services, ComponentContext componentContext)
    {
        var logFileEnable = App.GetConfig<bool?>("Logging:LogEnable:File");
        if (logFileEnable != true) return;

        //获取默认日志等级
        var defaultLevel = App.GetConfig<LogLevel?>("Logging:LogLevel:File");
        //获取程序根目录
        var rootPath = App.HostEnvironment.ContentRootPath;
        if (defaultLevel != null)//如果默认日志等级不是空
        {
            //遍历日志等级
            foreach (LogLevel level in Enum.GetValues(typeof(LogLevel)))
            {
                //如果日志等级是默认等级和最大等级之间
                if (level >= defaultLevel && level != LogLevel.None)
                {
                    //每天创建一个日志文件
                    services.AddLogging(builder =>
                   {
                       var fileName = $"logs/{level}/{{0:yyyy}}-{{0:MM}}-{{0:dd}}{{0:zz}}.log";
                       builder.AddFile(fileName, options =>
                       {
                           SetLogOptions(options, level);//日志格式化
                       });
                   });
                }
            }
        }
        else
        {
            //添加日志文件
            services.AddFileLogging("logs/{0:yyyy}-{0:MM}-{0:dd}{0:zz}.log", options =>
            {
                SetLogOptions(options, null);//日志格式化
            });
        }
    }

    /// <summary>
    /// 日志格式化
    /// </summary>
    /// <param name="options"></param>
    /// <param name="logLevel"></param>
    private static void SetLogOptions(FileLoggerOptions options, LogLevel? logLevel)
    {
        //每天创建一个日志文件
        var rootPath = App.HostEnvironment.ContentRootPath;
        if (logLevel != null)//如果日志等级不为空
        {
            //过滤日志等级
            options.WriteFilter = (logMsg) =>
            {
                //不写入LoggingMonitor
                if (logMsg.LogName == "System.Logging.LoggingMonitor")
                    return false;
                //只写入NetCore日志
                if (!logMsg.LogName.StartsWith("System") && !logMsg.LogName.StartsWith("Microsoft"))
                    return false;
                return logMsg.LogLevel == logLevel;
            };
        }
        //定义日志文件名
        options.FileNameRule = fileName =>
        {
            return rootPath + "\\" + string.Format(fileName, DateTimeExtensions.CurrentDateTime);
        };
        options.FileSizeLimitBytes = 50 * 1024 * 1024;//日志最大50M
        options.MessageFormat = logMsg =>
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
            };
    }
}