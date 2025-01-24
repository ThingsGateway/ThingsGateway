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

using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

using ThingsGateway.Reflection;

namespace ThingsGateway.DependencyInjection;

/// <summary>
/// 命名服务提供器默认实现
/// </summary>
/// <typeparam name="TService">目标服务接口</typeparam>
internal sealed class NamedServiceProvider<TService> : INamedServiceProvider<TService>
    where TService : class
{
    /// <summary>
    /// 服务提供器
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="serviceProvider">服务提供器</param>
    public NamedServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// 根据服务名称获取服务
    /// </summary>
    /// <param name="serviceName">服务名称</param>
    /// <returns></returns>
    public TService GetService(string serviceName)
    {
        var services = _serviceProvider.GetServices<TService>();

        if (services
            .OfType<AspectDispatchProxy>()
            .FirstOrDefault(u => ResovleServiceName(((dynamic)u).Target.GetType()) == serviceName) is not TService service)
        {
            service = services.FirstOrDefault(u => ResovleServiceName(u.GetType()) == serviceName);
        }

        return service;
    }

    /// <summary>
    /// 根据服务名称获取服务
    /// </summary>
    /// <typeparam name="ILifetime">服务生存周期接口，<see cref="ITransient"/>，<see cref="IScoped"/>，<see cref="ISingleton"/></typeparam>
    /// <param name="serviceName">服务名称</param>
    /// <returns></returns>
    public TService GetService<ILifetime>(string serviceName)
         where ILifetime : IPrivateDependency
    {
        var resolveNamed = _serviceProvider.GetService<Func<string, ILifetime, object>>();
        return resolveNamed == null ? default : resolveNamed(serviceName, default) as TService;
    }

    /// <summary>
    /// 根据服务名称获取服务
    /// </summary>
    /// <param name="serviceName">服务名称</param>
    /// <returns></returns>
    public TService GetRequiredService(string serviceName)
    {
        // 解析所有实现
        var services = _serviceProvider.GetServices<TService>();

        if (services
            .OfType<AspectDispatchProxy>()
            .FirstOrDefault(u => ResovleServiceName(((dynamic)u).Target.GetType()) == serviceName) is not TService service)
        {
            service = services.FirstOrDefault(u => ResovleServiceName(u.GetType()) == serviceName);
        }

        // 如果服务不存在，抛出异常
        return service ?? throw new InvalidOperationException($"Named service `{serviceName}` is not registered in container.");
    }

    /// <summary>
    /// 根据服务名称获取服务
    /// </summary>
    /// <typeparam name="ILifetime">服务生存周期接口，<see cref="ITransient"/>，<see cref="IScoped"/>，<see cref="ISingleton"/></typeparam>
    /// <param name="serviceName">服务名称</param>
    /// <returns></returns>
    public TService GetRequiredService<ILifetime>(string serviceName)
         where ILifetime : IPrivateDependency
    {
        var resolveNamed = _serviceProvider.GetRequiredService<Func<string, ILifetime, object>>();
        var service = resolveNamed == null ? default : resolveNamed(serviceName, default) as TService;

        // 如果服务不存在，抛出异常
        return service ?? throw new InvalidOperationException($"Named service `{serviceName}` is not registered in container.");
    }

    /// <summary>
    /// 解析服务名称
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private static string ResovleServiceName(Type type)
    {
        if (type.IsDefined(typeof(InjectionAttribute)))
        {
            return type.GetCustomAttribute<InjectionAttribute>().Named;
        }

        return type.Name;
    }
}