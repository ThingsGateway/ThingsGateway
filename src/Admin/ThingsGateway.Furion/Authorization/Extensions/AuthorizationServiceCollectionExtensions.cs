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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;

using ThingsGateway.Authorization;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 策略授权服务拓展类
/// </summary>
[SuppressSniffer]
public static class AuthorizationServiceCollectionExtensions
{
    /// <summary>
    /// 添加策略授权服务
    /// </summary>
    /// <typeparam name="TAuthorizationHandler">策略授权处理程序</typeparam>
    /// <param name="services">服务集合</param>
    /// <param name="configure">自定义配置</param>
    /// <param name="enableGlobalAuthorize">是否启用全局授权</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddAppAuthorization<TAuthorizationHandler>(this IServiceCollection services, Action<IServiceCollection> configure = null, bool enableGlobalAuthorize = false)
        where TAuthorizationHandler : class, IAuthorizationHandler
    {
        // 注册授权策略提供器
        services.TryAddSingleton<IAuthorizationPolicyProvider, AppAuthorizationPolicyProvider>();

        // 注册策略授权处理程序
        services.TryAddSingleton<IAuthorizationHandler, TAuthorizationHandler>();

        //启用全局授权
        if (enableGlobalAuthorize)
        {
            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new AuthorizeFilter());
            });
        }

        configure?.Invoke(services);
        return services;
    }
}