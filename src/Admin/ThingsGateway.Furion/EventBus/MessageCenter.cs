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

using System.Reflection;

namespace ThingsGateway.EventBus;

/// <summary>
/// 全局事件总线静态类
/// </summary>
[SuppressSniffer]
public static class MessageCenter
{
    /// <summary>
    /// 发布一条消息
    /// </summary>
    /// <param name="eventSource">事件源</param>
    /// <returns><see cref="Task"/> 实例</returns>
    public static Task PublishAsync(IEventSource eventSource)
    {
        return GetEventPublisher().PublishAsync(eventSource);
    }

    /// <summary>
    /// 延迟发布一条消息
    /// </summary>
    /// <param name="eventSource">事件源</param>
    /// <param name="delay">延迟数（毫秒）</param>
    /// <returns><see cref="Task"/> 实例</returns>
    public static Task PublishDelayAsync(IEventSource eventSource, long delay)
    {
        return GetEventPublisher().PublishDelayAsync(eventSource, delay);
    }

    /// <summary>
    /// 发布一条消息
    /// </summary>
    /// <param name="eventId">事件 Id</param>
    /// <param name="payload">事件承载（携带）数据</param>
    /// <param name="cancellationToken"> 取消任务 Token</param>
    /// <returns></returns>
    public static Task PublishAsync(string eventId, object payload = default, CancellationToken cancellationToken = default)
    {
        return GetEventPublisher().PublishAsync(eventId, payload, cancellationToken);
    }

    /// <summary>
    /// 发布一条消息
    /// </summary>
    /// <param name="eventId">事件 Id</param>
    /// <param name="payload">事件承载（携带）数据</param>
    /// <param name="cancellationToken"> 取消任务 Token</param>
    /// <returns></returns>
    public static Task PublishAsync(Enum eventId, object payload = default, CancellationToken cancellationToken = default)
    {
        return GetEventPublisher().PublishAsync(eventId, payload, cancellationToken);
    }

    /// <summary>
    /// 延迟发布一条消息
    /// </summary>
    /// <param name="eventId">事件 Id</param>
    /// <param name="delay">延迟数（毫秒）</param>
    /// <param name="payload">事件承载（携带）数据</param>
    /// <param name="cancellationToken"> 取消任务 Token</param>
    /// <returns><see cref="Task"/> 实例</returns>
    public static Task PublishDelayAsync(string eventId, long delay, object payload = default, CancellationToken cancellationToken = default)
    {
        return GetEventPublisher().PublishDelayAsync(eventId, delay, payload, cancellationToken);
    }

    /// <summary>
    /// 延迟发布一条消息
    /// </summary>
    /// <param name="eventId">事件 Id</param>
    /// <param name="delay">延迟数（毫秒）</param>
    /// <param name="payload">事件承载（携带）数据</param>
    /// <param name="cancellationToken"> 取消任务 Token</param>
    /// <returns><see cref="Task"/> 实例</returns>
    public static Task PublishDelayAsync(Enum eventId, long delay, object payload = default, CancellationToken cancellationToken = default)
    {
        return GetEventPublisher().PublishDelayAsync(eventId, delay, payload, cancellationToken);
    }

    /// <summary>
    /// 添加事件订阅者
    /// </summary>
    /// <param name="eventId">事件 Id</param>
    /// <param name="handler">事件订阅委托</param>
    /// <param name="attribute"><see cref="EventSubscribeAttribute"/> 特性对象</param>
    /// <param name="handlerMethod"><see cref="MethodInfo"/> 对象</param>
    /// <param name="cancellationToken">取消任务 Token</param>
    /// <returns></returns>
    public static Task Subscribe(string eventId, Func<EventHandlerExecutingContext, Task> handler, EventSubscribeAttribute attribute = default, MethodInfo handlerMethod = default, CancellationToken cancellationToken = default)
    {
        return GetEventFactory().Subscribe(eventId
            , handler
            , attribute
            , handlerMethod
            , cancellationToken);
    }

    /// <summary>
    /// 删除事件订阅者
    /// </summary>
    /// <param name="eventId">事件 Id</param>
    /// <param name="cancellationToken">取消任务 Token</param>
    /// <returns></returns>
    public static Task Unsubscribe(string eventId, CancellationToken cancellationToken = default)
    {
        return GetEventFactory().Unsubscribe(eventId, cancellationToken);
    }

    /// <summary>
    /// 获取事件发布者
    /// </summary>
    /// <returns></returns>
    private static IEventPublisher GetEventPublisher()
    {
        return App.GetService<IEventPublisher>(App.RootServices);
    }

    /// <summary>
    /// 获取事件工厂
    /// </summary>
    /// <returns></returns>
    private static IEventBusFactory GetEventFactory()
    {
        return App.GetService<IEventBusFactory>(App.RootServices);
    }
}