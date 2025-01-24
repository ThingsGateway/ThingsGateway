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

using Microsoft.Extensions.Options;

using Swashbuckle.AspNetCore.SwaggerGen;

using ThingsGateway;
using ThingsGateway.SpecificationDocument;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 规范化接口服务拓展类
/// </summary>
[SuppressSniffer]
public static class SpecificationDocumentServiceCollectionExtensions
{
    /// <summary>
    /// 添加规范化文档服务
    /// </summary>
    /// <param name="mvcBuilder">Mvc 构建器</param>
    /// <param name="configure">自定义配置</param>
    /// <returns>服务集合</returns>
    public static IMvcBuilder AddSpecificationDocuments(this IMvcBuilder mvcBuilder, Action<SwaggerGenOptions> configure = default)
    {
        mvcBuilder.Services.AddSpecificationDocuments(configure);

        return mvcBuilder;
    }

    /// <summary>
    /// 添加规范化文档服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configure">自定义配置</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddSpecificationDocuments(this IServiceCollection services, Action<SwaggerGenOptions> configure = default)
    {
        // 解决服务重复注册问题
        if (services.Any(u => u.ServiceType == typeof(IConfigureOptions<SchemaGeneratorOptions>)))
        {
            return services;
        }

        // 判断是否启用规范化文档
        if (App.Settings.InjectSpecificationDocument != true) return services;

        // 添加配置
        services.AddConfigurableOptions<SpecificationDocumentSettingsOptions>();
        services.AddEndpointsApiExplorer();

        // 添加Swagger生成器服务
        services.AddSwaggerGen(options => SpecificationDocumentBuilder.BuildGen(options, configure));

        // 添加 MiniProfiler 服务
        AddMiniProfiler(services);

        return services;
    }

    /// <summary>
    /// 添加 MiniProfiler 配置
    /// </summary>
    /// <param name="services"></param>
    private static void AddMiniProfiler(IServiceCollection services)
    {
        // 注册MiniProfiler 组件
        if (App.Settings.InjectMiniProfiler != true) return;

        services.AddMiniProfiler(options =>
        {
            // 减少非 Swagger 页面请求监听问题
            options.ShouldProfile = (req) =>
            {
                if (!req.Headers.ContainsKey("request-from")) return false;
                return true;
            };

            options.RouteBasePath = "/index-mini-profiler";
            options.EnableMvcFilterProfiling = false;
            options.EnableMvcViewProfiling = false;
        });
    }
}