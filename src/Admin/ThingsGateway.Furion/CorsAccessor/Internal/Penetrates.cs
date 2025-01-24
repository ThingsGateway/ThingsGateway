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

using Microsoft.AspNetCore.Cors.Infrastructure;

namespace ThingsGateway.CorsAccessor;

/// <summary>
/// 常量、公共方法配置类
/// </summary>
internal static class Penetrates
{
    private static readonly string[] _defaultExposedHeaders = new[]
    {
        "access-token",
        "x-access-token",
        "Content-Disposition"
    };

    private static readonly string[] _defaultSignalRMethods = new[] { "GET", "POST" };

    /// <summary>
    /// 设置跨域策略
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="corsAccessorSettings"></param>
    /// <param name="isMiddleware"></param>
    internal static void SetCorsPolicy(CorsPolicyBuilder builder, CorsAccessorSettingsOptions corsAccessorSettings, bool isMiddleware = false)
    {
        var isNotSetOrigins = corsAccessorSettings.WithOrigins == null || corsAccessorSettings.WithOrigins.Length == 0;
        var isSupportSignarlR = isMiddleware && corsAccessorSettings.SignalRSupport == true;

        builder.SetIsOriginAllowed(_ => true);

        if (isNotSetOrigins)
        {
            if (!isSupportSignarlR) builder.AllowAnyOrigin();
        }
        else builder.WithOrigins(corsAccessorSettings.WithOrigins)
                    .SetIsOriginAllowedToAllowWildcardSubdomains();

        if ((corsAccessorSettings.WithHeaders == null || corsAccessorSettings.WithHeaders.Length == 0) || isSupportSignarlR) builder.AllowAnyHeader();
        else builder.WithHeaders(corsAccessorSettings.WithHeaders);

        if (corsAccessorSettings.WithMethods == null || corsAccessorSettings.WithMethods.Length == 0) builder.AllowAnyMethod();
        else
        {
            if (isSupportSignarlR)
            {
                builder.WithMethods(corsAccessorSettings.WithMethods.Concat(_defaultSignalRMethods).Distinct(StringComparer.OrdinalIgnoreCase).ToArray());
            }
            else builder.WithMethods(corsAccessorSettings.WithMethods);
        }

        if ((corsAccessorSettings.AllowCredentials == true && !isNotSetOrigins) || isSupportSignarlR) builder.AllowCredentials();

        IEnumerable<string> exposedHeaders = corsAccessorSettings.FixedClientToken == true
            ? _defaultExposedHeaders
            : Array.Empty<string>();
        if (corsAccessorSettings.WithExposedHeaders != null && corsAccessorSettings.WithExposedHeaders.Length > 0)
        {
            exposedHeaders = exposedHeaders.Concat(corsAccessorSettings.WithExposedHeaders).Distinct(StringComparer.OrdinalIgnoreCase);
        }

        if (exposedHeaders.Any()) builder.WithExposedHeaders(exposedHeaders.ToArray());

        builder.SetPreflightMaxAge(TimeSpan.FromSeconds(corsAccessorSettings.SetPreflightMaxAge ?? 24 * 60 * 60));
    }
}
