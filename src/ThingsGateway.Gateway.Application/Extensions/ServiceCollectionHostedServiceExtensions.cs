// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for adding hosted services to an <see cref="IServiceCollection" />.
/// </summary>
[ThingsGateway.DependencyInjection.SuppressSniffer]
public static class ServiceCollectionHostedServiceExtensions
{
    /// <summary>
    /// Add an <see cref="IHostedService"/> registration for the given type.
    /// </summary>
    /// <typeparam name="THostedService">An <see cref="IHostedService"/> to register.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to register with.</param>
    /// <returns>The original <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddGatewayHostedService<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THostedService>(this IServiceCollection services)
        where THostedService : class, IHostedService
    {
        services.AddSingleton<THostedService>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, THostedService>(seriveProvider => seriveProvider.GetService<THostedService>()));

        return services;
    }

    /// <summary>
    /// Add an <see cref="IHostedService"/> registration for the given type.
    /// </summary>
    public static IServiceCollection AddGatewayHostedService<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THostedService>(this IServiceCollection services)
    where TService : class, IHostedService
    where THostedService : class, IHostedService, TService
    {
        services.AddSingleton(typeof(TService), typeof(THostedService));
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, TService>(seriveProvider => seriveProvider.GetService<TService>()));
        return services;
    }
}
