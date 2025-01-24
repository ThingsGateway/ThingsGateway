// ------------------------------------------------------------------------
// 版权信息
// 版权归百小僧及百签科技（广东）有限公司所有。
// 所有权利保留。
// 官方网站：https://baiqian.com
//
// 许可证信息
// 项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。
// 许可证的完整文本可以在源代码树根目录中的 LICENSE-APACHE 和 LICENSE-MIT 文件中找到。
// ------------------------------------------------------------------------

using Microsoft.Extensions.Logging;

namespace ThingsGateway.Schedule;

/// <summary>
/// 作业调度器日志服务
/// </summary>
public interface IScheduleLogger
{
    /// <summary>
    /// 记录 Information 日志
    /// </summary>
    /// <param name="message">消息</param>
    /// <param name="args">参数</param>
    void LogInformation(string message, params object[] args);

    /// <summary>
    /// 记录 Trace 日志
    /// </summary>
    /// <param name="message">消息</param>
    /// <param name="args">参数</param>
    void LogTrace(string message, params object[] args);

    /// <summary>
    /// 记录 Debug 日志
    /// </summary>
    /// <param name="message">消息</param>
    /// <param name="args">参数</param>
    void LogDebug(string message, params object[] args);

    /// <summary>
    /// 记录 Warning 日志
    /// </summary>
    /// <param name="message">消息</param>
    /// <param name="args">参数</param>
    void LogWarning(string message, params object[] args);

    /// <summary>
    /// 记录 Critical 日志
    /// </summary>
    /// <param name="message">消息</param>
    /// <param name="args">参数</param>
    void LogCritical(string message, params object[] args);

    /// <summary>
    /// 记录 Error 日志
    /// </summary>
    /// <param name="ex">异常消息</param>
    /// <param name="message">消息</param>
    /// <param name="args">参数</param>
    void LogError(Exception ex, string message, params object[] args);

    /// <summary>
    /// 记录日志
    /// </summary>
    /// <param name="logLevel">日志级别</param>
    /// <param name="message">消息</param>
    /// <param name="args">参数</param>
    /// <param name="ex">异常</param>
    void Log(LogLevel logLevel, string message, object[] args = default, Exception ex = default);
}