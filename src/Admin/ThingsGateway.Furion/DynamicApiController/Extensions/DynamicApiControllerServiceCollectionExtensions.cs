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

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;

using System.Reflection;

using ThingsGateway;
using ThingsGateway.DynamicApiController;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 动态接口控制器拓展类
/// </summary>
[SuppressSniffer]
public static class DynamicApiControllerServiceCollectionExtensions
{
    /// <summary>
    /// 添加动态接口控制器服务
    /// </summary>
    /// <param name="mvcBuilder">Mvc构建器</param>
    /// <returns>Mvc构建器</returns>
    public static IMvcBuilder AddDynamicApiControllers(this IMvcBuilder mvcBuilder)
    {
        mvcBuilder.Services.AddDynamicApiControllers();

        return mvcBuilder;
    }


    /// <summary>
    /// 配置动态 WebAPI
    /// </summary>
    /// <remarks>请确保在 <c>AddDynamicApiControllers()</c> 或 <c>Inject()</c> 之前注册。</remarks>
    /// <param name="services"></param>
    /// <param name="configure"></param>
    public static void ConfigureDynamicApiController(this IServiceCollection services, Action<DynamicApiControllerBuilder> configure)
    {
        var dynamicApiControllerBuilder = new DynamicApiControllerBuilder();
        configure?.Invoke(dynamicApiControllerBuilder);

        services.TryAddSingleton(dynamicApiControllerBuilder);
    }

    /// <summary>
    /// 添加动态接口控制器服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddDynamicApiControllers(this IServiceCollection services)
    {
        // 解决服务重复注册问题
        if (services.Any(u => u.ServiceType == typeof(MvcActionDescriptorChangeProvider)))
        {
            return services;
        }

        var partManager = services.FirstOrDefault(s => s.ServiceType == typeof(ApplicationPartManager))?.ImplementationInstance as ApplicationPartManager
            ?? throw new InvalidOperationException($"`{nameof(AddDynamicApiControllers)}` must be invoked after `{nameof(MvcServiceCollectionExtensions.AddControllers)}`.");

        // 解决项目类型为 <Project Sdk="Microsoft.NET.Sdk"> 不能加载 API 问题，默认支持 <Project Sdk="Microsoft.NET.Sdk.Web">
        foreach (var assembly in App.Assemblies)
        {
            if (partManager.ApplicationParts.Any(u => u.Name != assembly.GetName().Name))
            {
                partManager.ApplicationParts.Add(new AssemblyPart(assembly));
            }
        }

        // 载入模块化/插件程序集部件
        if (App.ExternalAssemblies.Any())
        {
            foreach (var assembly in App.ExternalAssemblies)
            {
                if (partManager.ApplicationParts.Any(u => u.Name != assembly.GetName().Name))
                {
                    partManager.ApplicationParts.Add(new AssemblyPart(assembly));
                }
            }
        }

        // 添加控制器特性提供器
        partManager.FeatureProviders.Add(new DynamicApiControllerFeatureProvider());

        // 添加动态 WebAPI 运行时感知服务
        services.AddSingleton<MvcActionDescriptorChangeProvider>()
                .AddSingleton<IActionDescriptorChangeProvider>(provider => provider.GetRequiredService<MvcActionDescriptorChangeProvider>());
        services.AddSingleton<IDynamicApiRuntimeChangeProvider, DynamicApiRuntimeChangeProvider>();

        // 添加配置
        services.AddConfigurableOptions<DynamicApiControllerSettingsOptions>();

        // 配置 Mvc 选项
        services.Configure<MvcOptions>(options =>
        {
            // 添加应用模型转换器
            options.Conventions.Add(new DynamicApiControllerApplicationModelConvention(services));

            // 添加 text/plain 请求 Body 参数支持
            options.InputFormatters.Add(new TextPlainMediaTypeFormatter());
        });

        return services;
    }

    /// <summary>
    /// 添加外部程序集部件集合
    /// </summary>
    /// <param name="mvcBuilder">Mvc构建器</param>
    /// <param name="assemblies"></param>
    /// <returns>Mvc构建器</returns>
    public static IMvcBuilder AddExternalAssemblyParts(this IMvcBuilder mvcBuilder, IEnumerable<Assembly> assemblies)
    {
        var partManager = mvcBuilder.PartManager;
        // 载入程序集部件
        if (partManager != null && assemblies != null && assemblies.Any())
        {
            foreach (var assembly in assemblies)
            {
                if (partManager.ApplicationParts.Any(u => u.Name != assembly.GetName().Name))
                {
                    mvcBuilder.AddApplicationPart(assembly);
                }
            }
        }

        return mvcBuilder;
    }
}