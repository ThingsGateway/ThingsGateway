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

using ThingsGateway.Logging;

namespace Microsoft.Extensions.Logging;

/// <summary>
/// <see cref="ILogger"/> 拓展
/// </summary>
[SuppressSniffer]
public static class ILoggerExtensions
{
    /// <summary>
    /// 设置日志上下文
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="properties">建议使用 ConcurrentDictionary 类型</param>
    /// <returns></returns>
    public static IDisposable ScopeContext(this ILogger logger, IDictionary<object, object> properties)
    {
        if (logger == null) throw new ArgumentNullException(nameof(logger));

        return logger.BeginScope(new LogContext { Properties = properties });
    }

    /// <summary>
    /// 设置日志上下文
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static IDisposable ScopeContext(this ILogger logger, Action<LogContext> configure)
    {
        if (logger == null) throw new ArgumentNullException(nameof(logger));

        var logContext = new LogContext();
        configure?.Invoke(logContext);

        return logger.BeginScope(logContext);
    }

    /// <summary>
    /// 设置日志上下文
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static IDisposable ScopeContext(this ILogger logger, LogContext context)
    {
        if (logger == null) throw new ArgumentNullException(nameof(logger));

        return logger.BeginScope(context);
    }
}