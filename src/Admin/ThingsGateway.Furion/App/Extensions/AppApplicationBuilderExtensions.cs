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

using ThingsGateway;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// 应用中间件拓展类（由框架内部调用）
/// </summary>
[SuppressSniffer]
public static class AppApplicationBuilderExtensions
{

    /// <summary>
    /// 设置默认服务存储器
    /// </summary>
    /// <param name="app"><see cref="WebApplication"/></param>
    /// <remarks>
    /// <para>解决在主机启动前解析服务问题</para>
    /// <para>使用：<code>var app = builder.Build().UseDefaultServiceProvider();</code></para>
    /// </remarks>
    /// <returns><see cref="WebApplication"/></returns>
    public static WebApplication UseDefaultServiceProvider(this WebApplication app)
    {
        InternalApp.RootServices ??= app.Services;

        return app;
    }

    /// <summary>
    /// 注入基础中间件（带Swagger）
    /// </summary>
    /// <param name="app"></param>
    /// <param name="routePrefix">空字符串将为首页</param>
    /// <param name="configure"></param>
    /// <param name="withProxy">解决 Swagger 被代理问题</param>
    /// <returns><see cref="IApplicationBuilder"/></returns>
    public static IApplicationBuilder UseInject(this IApplicationBuilder app, string routePrefix = default, Action<UseInjectOptions> configure = null, bool withProxy = false)
    {
        // 载入中间件配置选项
        var configureOptions = new UseInjectOptions();
        configure?.Invoke(configureOptions);

        app.UseSpecificationDocuments(routePrefix, UseInjectOptions.SwaggerConfigure, UseInjectOptions.SwaggerUIConfigure, withProxy);

        return app;
    }

    /// <summary>
    /// 注入基础中间件（带Swagger）
    /// </summary>
    /// <param name="app"></param>
    /// <param name="configure"></param>
    /// <param name="withProxy">解决 Swagger 被代理问题</param>
    /// <returns><see cref="IApplicationBuilder"/></returns>
    public static IApplicationBuilder UseInject(this IApplicationBuilder app, Action<UseInjectOptions> configure, bool withProxy = false)
    {
        return app.UseInject(default, configure: configure, withProxy: withProxy);
    }

    /// <summary>
    /// 注入基础中间件
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseInjectBase(this IApplicationBuilder app)
    {
        return app;
    }

    /// <summary>
    /// 解决 .NET6 WebApplication 模式下二级虚拟目录错误问题
    /// </summary>
    /// <param name="app"></param>
    /// <returns><see cref="IApplicationBuilder"/></returns>
    public static IApplicationBuilder MapRouteControllers(this IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        return app;
    }

    /// <summary>
    /// 启用 Body 重复读功能
    /// </summary>
    /// <remarks>须在 app.UseRouting() 之前注册</remarks>
    /// <param name="app"></param>
    /// <returns><see cref="IApplicationBuilder"/></returns>
    public static IApplicationBuilder EnableBuffering(this IApplicationBuilder app)
    {
        return app.Use(next => context =>
        {
            context.Request.EnableBuffering();
            return next(context);
        });
    }

    /// <summary>
    /// 添加应用中间件
    /// </summary>
    /// <param name="app">应用构建器</param>
    /// <param name="configure">应用配置</param>
    /// <returns><see cref="IApplicationBuilder"/></returns>
    internal static IApplicationBuilder UseApp(this IApplicationBuilder app, Action<IApplicationBuilder> configure = null)
    {
        // 调用自定义服务
        configure?.Invoke(app);
        return app;
    }
}