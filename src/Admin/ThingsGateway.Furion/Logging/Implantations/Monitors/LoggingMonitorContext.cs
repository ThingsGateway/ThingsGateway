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

using Microsoft.AspNetCore.Http;

namespace ThingsGateway.Logging;

/// <summary>
/// LoggingMonitor 上下文
/// </summary>
[SuppressSniffer]
public static class LoggingMonitorContext
{
    internal const string KEY = nameof(LoggingMonitorContext);

    /// <summary>
    /// 追加附加信息
    /// </summary>
    /// <param name="items"></param>
    public static void Append(Dictionary<string, object> items)
    {
        var httpContextItems = App.HttpContext?.Items;
        if (httpContextItems == null)
        {
            return;
        }

        if (httpContextItems.ContainsKey(KEY))
        {
            httpContextItems.Remove(KEY);
        }

        httpContextItems.Add(KEY, items);
    }

    /// <summary>
    /// 追加附加信息
    /// </summary>
    /// <param name="action"></param>
    public static void Append(Action<Dictionary<string, object>, HttpContext> action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        var httpContext = App.HttpContext;
        if (httpContext == null)
        {
            return;
        }

        var items = new Dictionary<string, object>();
        action?.Invoke(items, httpContext);

        Append(items);
    }
}