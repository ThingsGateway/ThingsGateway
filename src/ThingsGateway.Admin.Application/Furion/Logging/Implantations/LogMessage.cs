﻿//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

// 版权归百小僧及百签科技（广东）有限公司所有。

using Microsoft.Extensions.Logging;

namespace ThingsGateway.Logging;

/// <summary>
/// 日志结构化消息
/// </summary>
public struct LogMessage
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="logName">记录器类别名称</param>
    /// <param name="logLevel">日志级别</param>
    /// <param name="eventId">事件 Id</param>
    /// <param name="message">日志消息</param>
    /// <param name="exception">异常对象</param>
    /// <param name="context">日志上下文</param>
    /// <param name="state">当前状态值</param>
    /// <param name="logDateTime">日志记录时间</param>
    /// <param name="threadId">线程 Id</param>
    /// <param name="useUtcTimestamp">是否使用 UTC 时间戳</param>
    /// <param name="traceId">请求/跟踪 Id</param>
    internal LogMessage(string logName
        , LogLevel logLevel
        , EventId eventId
        , string message
        , Exception exception
        , LogContext context
        , object state
        , DateTime logDateTime
        , int threadId
        , bool useUtcTimestamp
        , string traceId)
    {
        LogName = logName;
        Message = message;
        LogLevel = logLevel;
        EventId = eventId;
        Exception = exception;
        Context = context;
        State = state;
        LogDateTime = logDateTime;
        ThreadId = threadId;
        UseUtcTimestamp = useUtcTimestamp;
        TraceId = traceId;
    }

    /// <summary>
    /// 记录器类别名称
    /// </summary>
    public string LogName { get; }

    /// <summary>
    /// 日志级别
    /// </summary>
    public LogLevel LogLevel { get; }

    /// <summary>
    /// 事件 Id
    /// </summary>
    public EventId EventId { get; }

    /// <summary>
    /// 日志消息
    /// </summary>
    public string Message { get; internal set; }

    /// <summary>
    /// 异常对象
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// 当前状态值
    /// </summary>
    /// <remarks>可以是任意类型</remarks>
    public object State { get; }

    /// <summary>
    /// 日志记录时间
    /// </summary>
    public DateTime LogDateTime { get; }

    /// <summary>
    /// 线程 Id
    /// </summary>
    public int ThreadId { get; }

    /// <summary>
    /// 是否使用 UTC 时间戳
    /// </summary>
    public bool UseUtcTimestamp { get; }

    /// <summary>
    /// 请求/跟踪 Id
    /// </summary>
    public string TraceId { get; }

    /// <summary>
    /// 日志上下文
    /// </summary>
    public LogContext Context { get; set; }

    /// <summary>
    /// 重写默认输出
    /// </summary>
    /// <returns><see cref="string"/></returns>
    public override readonly string ToString()
    {
        return Penetrates.OutputStandardMessage(this);
    }
}