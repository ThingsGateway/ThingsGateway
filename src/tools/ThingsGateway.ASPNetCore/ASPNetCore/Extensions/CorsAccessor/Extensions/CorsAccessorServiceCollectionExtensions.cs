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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using ThingsGateway;
using ThingsGateway.Admin.Application;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 跨域访问服务拓展类
/// </summary>
public static class CorsAccessorServiceCollectionExtensions
{
    /// <summary>
    /// 配置跨域
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="corsOptionsHandler"></param>
    /// <param name="corsPolicyBuilderHandler"></param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddCorsAccessor(this IServiceCollection services, Action<CorsOptions> corsOptionsHandler = default, Action<CorsPolicyBuilder> corsPolicyBuilderHandler = default)
    {
        // 解决服务重复注册问题
        if (services.Any(u => u.ServiceType == typeof(IConfigureOptions<CorsAccessorSettingsOptions>)))
        {
            return services;
        }

        // 添加跨域配置选项
        services.TryAddSingleton<IConfigureOptions<CorsAccessorSettingsOptions>, Microsoft.Extensions.DependencyInjection.ConfigureOptions<CorsAccessorSettingsOptions>>();

        // 获取选项
        var corsAccessorSettings = NetCoreApp.Configuration.GetSection("CorsAccessorSettings").Get<CorsAccessorSettingsOptions>();

        // 添加跨域服务
        services.AddCors(options =>
        {
            // 添加策略跨域
            options.AddPolicy(corsAccessorSettings.PolicyName, builder =>
            {
                // 设置跨域策略
                Penetrates.SetCorsPolicy(builder, corsAccessorSettings);

                // 添加自定义配置
                corsPolicyBuilderHandler?.Invoke(builder);
            });

            // 添加自定义配置
            corsOptionsHandler?.Invoke(options);
        });

        return services;
    }
}
