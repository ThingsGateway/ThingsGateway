
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




// 版权归百小僧及百签科技（广东）有限公司所有。

using ThingsGateway.Logging;

namespace Microsoft.Extensions.Logging;

/// <summary>
/// <see cref="ILogger"/> 拓展
/// </summary>
public static class ILoggerExtensions
{
    /// <summary>
    /// 配置日志上下文
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
    /// 配置日志上下文
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
    /// 配置日志上下文
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