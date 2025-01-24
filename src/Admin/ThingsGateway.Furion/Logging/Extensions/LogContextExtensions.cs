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

using ThingsGateway.Extensions;

namespace ThingsGateway.Logging;

/// <summary>
/// LogContext 拓展
/// </summary>
[SuppressSniffer]
public static class LogContextExtensions
{
    /// <summary>
    /// 设置上下文数据
    /// </summary>
    /// <param name="logContext"></param>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <returns></returns>
    public static LogContext Set(this LogContext logContext, object key, object value)
    {
        if (logContext == null || key == null) return logContext;

        logContext.Properties ??= new Dictionary<object, object>();

        logContext.Properties.Remove(key);
        logContext.Properties.Add(key, value);
        return logContext;
    }

    /// <summary>
    /// 批量设置上下文数据
    /// </summary>
    /// <param name="logContext"></param>
    /// <param name="properties"></param>
    /// <returns></returns>
    public static LogContext SetRange(this LogContext logContext, IDictionary<object, object> properties)
    {
        if (logContext == null
            || properties == null
            || properties.Count == 0) return logContext;

        foreach (var (key, value) in properties)
        {
            logContext.Set(key, value);
        }

        return logContext;
    }

    /// <summary>
    /// 获取上下文数据
    /// </summary>
    /// <param name="logContext"></param>
    /// <param name="key">键</param>
    /// <returns></returns>
    public static object Get(this LogContext logContext, object key)
    {
        if (logContext == null
            || key == null
            || logContext.Properties == null
            || logContext.Properties.Count == 0) return default;

        var isExists = logContext.Properties.TryGetValue(key, out var value);
        return isExists ? value : null;
    }

    /// <summary>
    /// 获取上下文数据
    /// </summary>
    /// <param name="logContext"></param>
    /// <param name="key">键</param>
    /// <returns></returns>
    public static T Get<T>(this LogContext logContext, object key)
    {
        var value = logContext.Get(key);
        return value.ChangeType<T>();
    }
}