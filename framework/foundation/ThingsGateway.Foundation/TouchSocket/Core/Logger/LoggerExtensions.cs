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

namespace ThingsGateway.Foundation.Core;

/// <summary>
/// <inheritdoc/>
/// </summary>
public static class LoggerExtensions
{
    #region LoggerGroup日志

    /// <summary>
    /// 指定在<see cref="LoggerGroup"/>中的特定日志类型中输出中断日志
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="msg"></param>
    public static void Critical<TLog>(this ILog logger, string msg) where TLog : ILog
    {
        logger.Log<TLog>(LogLevel.Critical, null, msg, null);
    }

    /// <summary>
    /// 指定在<see cref="LoggerGroup"/>中的特定日志类型中输出调试日志
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="msg"></param>
    public static void Debug<TLog>(this ILog logger, string msg) where TLog : ILog
    {
        logger.Log<TLog>(LogLevel.Debug, null, msg, null);
    }

    /// <summary>
    /// 指定在<see cref="LoggerGroup"/>中的特定日志类型中输出错误日志
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="msg"></param>
    public static void Error<TLog>(this ILog logger, string msg) where TLog : ILog
    {
        logger.Log<TLog>(LogLevel.Error, null, msg, null);
    }

    /// <summary>
    /// 指定在<see cref="LoggerGroup"/>中的特定日志类型中输出错误日志
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="source"></param>
    /// <param name="msg"></param>
    public static void Error<TLog>(this ILog logger, object source, string msg) where TLog : ILog
    {
        logger.Log<TLog>(LogLevel.Error, source, msg, null);
    }

    /// <summary>
    /// 指定在<see cref="LoggerGroup"/>中的特定日志类型中输出异常日志
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="ex"></param>
    public static void Exception<TLog>(this ILog logger, Exception ex) where TLog : ILog
    {
        logger.Log<TLog>(LogLevel.Error, null, ex.Message, ex);
    }

    /// <summary>
    /// 指定在<see cref="LoggerGroup"/>中的特定日志类型中输出异常日志
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="source"></param>
    /// <param name="ex"></param>
    public static void Exception<TLog>(this ILog logger, object source, Exception ex) where TLog : ILog
    {
        logger.Log<TLog>(LogLevel.Error, source, ex.Message, ex);
    }

    /// <summary>
    /// 指定在<see cref="LoggerGroup"/>中的特定日志类型中输出消息日志
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="msg"></param>
    public static void Info<TLog>(this ILog logger, string msg) where TLog : ILog
    {
        logger.Log<TLog>(LogLevel.Info, null, msg, null);
    }

    /// <summary>
    /// 指定在<see cref="LoggerGroup"/>中的特定日志类型中输出消息日志
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="source"></param>
    /// <param name="msg"></param>
    public static void Info<TLog>(this ILog logger, object source, string msg) where TLog : ILog
    {
        logger.Log<TLog>(LogLevel.Info, source, msg, null);
    }

    /// <summary>
    /// 指定在<see cref="LoggerGroup"/>中的特定日志类型中输出日志
    /// </summary>
    /// <param name="logLevel"></param>
    /// <param name="source"></param>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <param name="logger"></param>
    public static void Log<TLog>(this ILog logger, LogLevel logLevel, object source, string message, Exception exception) where TLog : ILog
    {
        if (logger is LoggerGroup loggerGroup)
        {
            loggerGroup.Log<TLog>(logLevel, source, message, exception);
        }
    }

    /// <summary>
    /// 指定在<see cref="LoggerGroup"/>中的特定日志类型中输出详细日志
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="msg"></param>
    public static void Trace<TLog>(this ILog logger, string msg) where TLog : ILog
    {
        logger.Log<TLog>(LogLevel.Trace, null, msg, null);
    }

    /// <summary>
    /// 指定在<see cref="LoggerGroup"/>中的特定日志类型中输出警示日志
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="msg"></param>
    public static void Warning<TLog>(this ILog logger, string msg) where TLog : ILog
    {
        logger.Log<TLog>(LogLevel.Warning, null, msg, null);
    }

    /// <summary>
    /// 指定在<see cref="LoggerGroup"/>中的特定日志类型中输出警示日志
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="source"></param>
    /// <param name="msg"></param>
    public static void Warning<TLog>(this ILog logger, object source, string msg) where TLog : ILog
    {
        logger.Log<TLog>(LogLevel.Warning, source, msg, null);
    }

    #endregion LoggerGroup日志

    #region 日志

    /// <summary>
    /// 输出中断日志
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="msg"></param>
    public static void Critical(this ILog logger, string msg)
    {
        logger.Log(LogLevel.Critical, null, msg, null);
    }

    /// <summary>
    /// 输出调试日志
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="msg"></param>
    public static void Debug(this ILog logger, string msg)
    {
        logger.Log(LogLevel.Debug, null, msg, null);
    }

    /// <summary>
    /// 输出错误日志
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="msg"></param>
    public static void Error(this ILog logger, string msg)
    {
        logger.Log(LogLevel.Error, null, msg, null);
    }

    /// <summary>
    /// 输出错误日志
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="source"></param>
    /// <param name="msg"></param>
    public static void Error(this ILog logger, object source, string msg)
    {
        logger.Log(LogLevel.Error, source, msg, null);
    }

    /// <summary>
    /// 输出异常日志
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="ex"></param>
    public static void Exception(this ILog logger, Exception ex)
    {
        logger.Log(LogLevel.Error, null, ex.Message, ex);
    }

    /// <summary>
    /// 输出异常日志
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="source"></param>
    /// <param name="ex"></param>
    public static void Exception(this ILog logger, object source, Exception ex)
    {
        logger.Log(LogLevel.Error, source, ex.Message, ex);
    }

    /// <summary>
    /// 输出异常日志
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="source"></param>
    /// <param name="msg"></param>
    /// <param name="ex"></param>
    public static void Exception(this ILog logger, object source, string msg, Exception ex)
    {
        logger.Log(LogLevel.Error, source, msg, ex);
    }

    /// <summary>
    /// 输出消息日志
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="msg"></param>
    public static void Info(this ILog logger, string msg)
    {
        logger.Log(LogLevel.Info, null, msg, null);
    }

    /// <summary>
    /// 输出消息日志
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="source"></param>
    /// <param name="msg"></param>
    public static void Info(this ILog logger, object source, string msg)
    {
        logger.Log(LogLevel.Info, source, msg, null);
    }

    /// <summary>
    /// 输出详细日志
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="msg"></param>
    public static void Trace(this ILog logger, string msg)
    {
        logger.Log(LogLevel.Trace, null, msg, null);
    }

    /// <summary>
    /// 输出警示日志
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="msg"></param>
    public static void Warning(this ILog logger, string msg)
    {
        logger.Log(LogLevel.Warning, null, msg, null);
    }

    /// <summary>
    /// 输出警示日志
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="source"></param>
    /// <param name="msg"></param>
    public static void Warning(this ILog logger, object source, string msg)
    {
        logger.Log(LogLevel.Warning, source, msg, null);
    }

    #endregion 日志

    #region 日志

    /// <summary>
    /// 输出错误日志
    /// </summary>
    public static void LogError(this ILog logger, Exception ex, string msg)
    {
        logger.Log(ThingsGateway.Foundation.Core.LogLevel.Error, null, msg, ex);
    }

    /// <summary>
    /// 输出错误日志
    /// </summary>
    public static void LogError(this ILog logger, Exception ex)
    {
        logger.Log(ThingsGateway.Foundation.Core.LogLevel.Error, null, ex.Message, ex);
    }

    /// <summary>
    /// 输出警示日志
    /// </summary>
    public static void LogWarning(this ILog logger, Exception ex, string msg)
    {
        logger.Log(ThingsGateway.Foundation.Core.LogLevel.Warning, null, msg, ex);
    }

    /// <summary>
    /// 输出警示日志
    /// </summary>
    public static void LogWarning(this ILog logger, Exception ex)
    {
        logger.Log(ThingsGateway.Foundation.Core.LogLevel.Warning, null, ex.Message, ex);
    }

    /// <summary>
    /// 输出警示日志
    /// </summary>
    public static void LogWarning(this ILog logger, string msg)
    {
        logger.Log(ThingsGateway.Foundation.Core.LogLevel.Warning, null, msg, null);
    }

    /// <summary>
    /// 输出提示日志
    /// </summary>
    public static void LogInformation(this ILog logger, string msg)
    {
        logger.Log(ThingsGateway.Foundation.Core.LogLevel.Info, null, msg, null);
    }

    #endregion 日志
}