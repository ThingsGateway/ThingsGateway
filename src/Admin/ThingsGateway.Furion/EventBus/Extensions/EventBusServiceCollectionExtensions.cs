﻿// ------------------------------------------------------------------------
// 版权信息
// 版权归百小僧及百签科技（广东）有限公司所有。
// 所有权利保留。
// 官方网站：https://baiqian.com
//
// 许可证信息
// 项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。
// 许可证的完整文本可以在源代码树根目录中的 LICENSE-APACHE 和 LICENSE-MIT 文件中找到。
// ------------------------------------------------------------------------

using ThingsGateway.EventBus;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// EventBus 模块服务拓展
/// </summary>
[SuppressSniffer]
public static class EventBusServiceCollectionExtensions
{
    /// <summary>
    /// 添加 EventBus 模块注册
    /// </summary>
    /// <param name="services">服务集合对象</param>
    /// <param name="configureOptionsBuilder">事件总线配置选项构建器委托</param>
    /// <returns>服务集合实例</returns>
    public static IServiceCollection AddEventBus(this IServiceCollection services, Action<EventBusOptionsBuilder> configureOptionsBuilder)
    {
        // 创建初始事件总线配置选项构建器
        var eventBusOptionsBuilder = new EventBusOptionsBuilder();
        configureOptionsBuilder.Invoke(eventBusOptionsBuilder);

        return services.AddEventBus(eventBusOptionsBuilder);
    }

    /// <summary>
    /// 添加 EventBus 模块注册
    /// </summary>
    /// <param name="services">服务集合对象</param>
    /// <param name="eventBusOptionsBuilder">事件总线配置选项构建器</param>
    /// <returns>服务集合实例</returns>
    public static IServiceCollection AddEventBus(this IServiceCollection services, EventBusOptionsBuilder eventBusOptionsBuilder = default)
    {
        // 初始化事件总线配置项
        eventBusOptionsBuilder ??= new EventBusOptionsBuilder();

        // 注册内部服务
        services.AddInternalService(eventBusOptionsBuilder);

        // 构建事件总线服务
        eventBusOptionsBuilder.Build(services);

        // 通过工厂模式创建
        services.AddHostedService(serviceProvider =>
        {
            // 创建事件总线后台服务对象
            var eventBusHostedService = ActivatorUtilities.CreateInstance<EventBusHostedService>(
                serviceProvider
                , eventBusOptionsBuilder.UseUtcTimestamp
                , eventBusOptionsBuilder.FuzzyMatch
                , eventBusOptionsBuilder.GCCollect
                , eventBusOptionsBuilder.LogEnabled);

            // 订阅未察觉任务异常事件
            var unobservedTaskExceptionHandler = eventBusOptionsBuilder.UnobservedTaskExceptionHandler;
            if (unobservedTaskExceptionHandler != default)
            {
                eventBusHostedService.UnobservedTaskException += unobservedTaskExceptionHandler;
            }

            return eventBusHostedService;
        });

        return services;
    }

    /// <summary>
    /// 注册内部服务
    /// </summary>
    /// <param name="services">服务集合对象</param>
    /// <param name="eventBusOptionsBuilder">事件总线配置选项构建器</param>
    /// <returns>服务集合实例</returns>
    private static IServiceCollection AddInternalService(this IServiceCollection services, EventBusOptionsBuilder eventBusOptionsBuilder)
    {
        // 创建默认内存通道事件源对象
        var defaultStorerOfChannel = new ChannelEventSourceStorer(eventBusOptionsBuilder.ChannelCapacity);

        // 注册后台任务队列接口/实例为单例，采用工厂方式创建
        services.AddSingleton<IEventSourceStorer>(_ =>
        {
            return defaultStorerOfChannel;
        });

        // 注册默认内存通道事件发布者
        services.AddSingleton<IEventPublisher, ChannelEventPublisher>();

        // 注册事件总线工厂
        services.AddSingleton<IEventBusFactory, EventBusFactory>();

        return services;
    }
}