// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://kimdiego2098.github.io/
// QQ群：605534569
// ------------------------------------------------------------------------------

using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;

using System.Collections.Concurrent;

namespace ThingsGateway.ASPNetCore;

/// <summary>
/// 常量、公共方法配置类
/// </summary>
internal static class Penetrates
{
    /// <summary>
    /// 分组分隔符
    /// </summary>
    internal const string GroupSeparator = "##";

    /// <summary>
    /// 请求动词映射字典
    /// </summary>
    internal static ConcurrentDictionary<string, string> VerbToHttpMethods { get; private set; }

    /// <summary>
    /// 控制器排序集合
    /// </summary>
    internal static ConcurrentDictionary<string, (string, int, Type)> ControllerOrderCollection { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    static Penetrates()
    {
        ControllerOrderCollection = new ConcurrentDictionary<string, (string, int, Type)>();

        VerbToHttpMethods = new ConcurrentDictionary<string, string>
        {
            ["post"] = "POST",
            ["add"] = "POST",
            ["create"] = "POST",
            ["insert"] = "POST",
            ["submit"] = "POST",

            ["get"] = "GET",
            ["find"] = "GET",
            ["fetch"] = "GET",
            ["query"] = "GET",
            //["getlist"] = "GET",
            //["getall"] = "GET",

            ["put"] = "PUT",
            ["update"] = "PUT",

            ["delete"] = "DELETE",
            ["remove"] = "DELETE",
            ["clear"] = "DELETE",

            ["patch"] = "PATCH"
        };

        IsApiControllerCached = new ConcurrentDictionary<Type, bool>();
    }

    /// <summary>
    /// <see cref="IsApiController(Type)"/> 缓存集合
    /// </summary>
    private static readonly ConcurrentDictionary<Type, bool> IsApiControllerCached;

    /// <summary>
    /// 是否是Api控制器
    /// </summary>
    /// <param name="type">type</param>
    /// <returns></returns>
    internal static bool IsApiController(Type type)
    {
        return IsApiControllerCached.GetOrAdd(type, Function);

        // 本地静态方法
        static bool Function(Type type)
        {
            // 排除 OData 控制器
            if (type.Assembly.GetName().Name.StartsWith("Microsoft.AspNetCore.OData")) return false;

            // 不能是非公开、基元类型、值类型、抽象类、接口、泛型类
            if (!type.IsPublic || type.IsPrimitive || type.IsValueType || type.IsAbstract || type.IsInterface || type.IsGenericType) return false;

            // 继承 ControllerBase 
            if ((!typeof(Controller).IsAssignableFrom(type) && typeof(ControllerBase).IsAssignableFrom(type))
                // 支持没有继承 ControllerBase 且贴了 [Route] 特性的情况
                || (type.IsDefined(typeof(RouteAttribute), true)))
            {
                // 处理运行时动态生成程序集问题
                if (type.Assembly?.ManifestModule?.Name == "<Unknown>") return true;

                // 解决 ASP.NET Core 启动时自动载入 NuGet 包导致模块化配置 SupportPackageNamePrefixs 出现非预期的结果
                if (!App.EffectiveTypes.Any(t => t == type)) return false;

                return true;
            }

            return false;
        }
    }
    /// <summary>
    /// 默认跨域导出响应头 Key
    /// </summary>
    /// <remarks>解决 ajax，XMLHttpRequest，axios 不能获取请求头问题</remarks>
    private static readonly string[] _defaultExposedHeaders = new[]
    {
        "access-token",
        "x-access-token",
        "Content-Disposition"
    };

    /// <summary>
    /// 设置跨域策略
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="corsAccessorSettings"></param>
    /// <param name="isMiddleware"></param>
    internal static void SetCorsPolicy(CorsPolicyBuilder builder, CorsAccessorSettingsOptions corsAccessorSettings, bool isMiddleware = false)
    {
        // 判断是否设置了来源，因为 AllowAnyOrigin 不能和 AllowCredentials一起公用
        var isNotSetOrigins = corsAccessorSettings.WithOrigins == null || corsAccessorSettings.WithOrigins.Length == 0;

        // https://docs.microsoft.com/zh-cn/aspnet/core/signalr/security?view=aspnetcore-6.0
        var isSupportSignarlR = isMiddleware && corsAccessorSettings.SignalRSupport == true;

        // 设置总是允许跨域源配置
        builder.SetIsOriginAllowed(_ => true);

        // 如果没有配置来源，则允许所有来源
        if (isNotSetOrigins)
        {
            // 解决 SignarlR  不能配置允许所有源问题
            if (!isSupportSignarlR) builder.AllowAnyOrigin();
        }
        else builder.WithOrigins(corsAccessorSettings.WithOrigins)
                    .SetIsOriginAllowedToAllowWildcardSubdomains();

        // 如果没有配置请求标头，则允许所有表头，包含处理 SignarlR 情况
        if ((corsAccessorSettings.WithHeaders == null || corsAccessorSettings.WithHeaders.Length == 0) || isSupportSignarlR) builder.AllowAnyHeader();
        else builder.WithHeaders(corsAccessorSettings.WithHeaders);

        // 如果没有配置任何请求谓词，则允许所有请求谓词
        if (corsAccessorSettings.WithMethods == null || corsAccessorSettings.WithMethods.Length == 0) builder.AllowAnyMethod();
        else
        {
            // 解决 SignarlR 必须允许 GET POST 问题
            if (isSupportSignarlR)
            {
                builder.WithMethods(corsAccessorSettings.WithMethods.Concat(new[] { "GET", "POST" }).Distinct(StringComparer.OrdinalIgnoreCase).ToArray());
            }
            else builder.WithMethods(corsAccessorSettings.WithMethods);
        }

        // 配置跨域凭据，包含处理 SignarlR 情况
        if ((corsAccessorSettings.AllowCredentials == true && !isNotSetOrigins) || isSupportSignarlR) builder.AllowCredentials();

        // 配置响应头，如果前端不能获取自定义的 header 信息，必须配置该项，默认配置了 access-token 和 x-access-token，可取消默认行为
        IEnumerable<string> exposedHeaders = corsAccessorSettings.FixedClientToken == true
            ? _defaultExposedHeaders
            : Array.Empty<string>();
        if (corsAccessorSettings.WithExposedHeaders != null && corsAccessorSettings.WithExposedHeaders.Length > 0)
        {
            exposedHeaders = exposedHeaders.Concat(corsAccessorSettings.WithExposedHeaders).Distinct(StringComparer.OrdinalIgnoreCase);
        }

        if (exposedHeaders.Any()) builder.WithExposedHeaders(exposedHeaders.ToArray());

        // 设置预检过期时间，如果不设置默认为 24小时
        builder.SetPreflightMaxAge(TimeSpan.FromSeconds(corsAccessorSettings.SetPreflightMaxAge ?? 24 * 60 * 60));
    }
}
