﻿// 版权归百小僧及百签科技（广东）有限公司所有。

using Furion;
using Furion.CorsAccessor;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using ThingsGateway;
using ThingsGateway.Core;

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
        var corsAccessorSettings = App.Configuration.GetSection("CorsAccessorSettings").Get<CorsAccessorSettingsOptions>();

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
