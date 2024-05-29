//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

// 版权归百小僧及百签科技（广东）有限公司所有。

using System.Text.Json;

using ThingsGateway.Logging;

namespace System;

/// <summary>
/// 日志格式化静态类
/// </summary>
public static class LoggerFormatter
{
    /// <summary>
    /// Json 输出格式化
    /// </summary>
    public static readonly Func<LogMessage, string> Json = (logMsg) =>
    {
        return logMsg.Write(writer => WriteJson(logMsg, writer));
    };

    /// <summary>
    /// Json 输出格式化
    /// </summary>
    public static readonly Func<LogMessage, string> JsonIndented = (logMsg) =>
    {
        return logMsg.Write(writer => WriteJson(logMsg, writer), true);
    };

    /// <summary>
    /// 写入 JSON
    /// </summary>
    /// <param name="logMsg"></param>
    /// <param name="writer"></param>
    private static void WriteJson(LogMessage logMsg, Utf8JsonWriter writer)
    {
        writer.WriteStartObject();

        // 输出日志级别
        writer.WriteString("logLevel", logMsg.LogLevel.ToString());

        // 输出日志时间
        writer.WriteString("logDateTime", logMsg.LogDateTime.ToString("o"));

        // 输出日志类别
        writer.WriteString("logName", logMsg.LogName);

        // 输出日志事件 Id
        writer.WriteNumber("eventId", logMsg.EventId.Id);

        // 输出日志消息
        writer.WriteString("message", logMsg.Message);

        // 输出日志所在线程 Id
        writer.WriteNumber("threadId", logMsg.ThreadId);

        // 输出是否使用 UTC 时间戳
        writer.WriteBoolean("useUtcTimestamp", logMsg.UseUtcTimestamp);

        // 输出请求 TraceId
        writer.WriteString("traceId", logMsg.TraceId);

        // 输出异常信息
        writer.WritePropertyName("exception");
        if (logMsg.Exception == null) writer.WriteNullValue();
        else writer.WriteStringValue(logMsg.Exception.ToString());

        writer.WriteEndObject();
    }
}
