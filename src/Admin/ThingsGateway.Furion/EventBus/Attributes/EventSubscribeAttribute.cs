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

using ThingsGateway.Extensitions.EventBus;

namespace ThingsGateway.EventBus;

/// <summary>
/// 事件处理程序特性
/// </summary>
/// <remarks>
/// <para>作用于 <see cref="IEventSubscriber"/> 实现类实例方法</para>
/// <para>支持多个事件 Id 触发同一个事件处理程序</para>
/// </remarks>
[SuppressSniffer, AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class EventSubscribeAttribute : Attribute
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="eventId">事件 Id</param>
    /// <remarks>只支持事件类型和 Enum 类型</remarks>
    public EventSubscribeAttribute(object eventId)
    {
        if (eventId is string)
        {
            EventId = eventId as string;
        }
        else if (eventId is Enum)
        {
            EventId = (eventId as Enum).ParseToString();
        }
        else throw new ArgumentException("Only support string or Enum data type.");
    }

    /// <summary>
    /// 事件 Id
    /// </summary>
    public string EventId { get; set; }

    /// <summary>
    /// 是否启用模糊匹配消息
    /// </summary>
    /// <remarks>支持正则表达式，bool 类型，默认为 null</remarks>
    public object FuzzyMatch { get; set; } = null;

    /// <summary>
    /// 是否启用执行完成触发 GC 回收
    /// </summary>
    /// <remarks>bool 类型，默认为 null</remarks>
    public object GCCollect { get; set; } = null;

    /// <summary>
    /// 重试次数
    /// </summary>
    public int NumRetries { get; set; } = 0;

    /// <summary>
    /// 重试间隔时间
    /// </summary>
    /// <remarks>默认1000毫秒</remarks>
    public int RetryTimeout { get; set; } = 1000;

    /// <summary>
    /// 可以指定特定异常类型才重试
    /// </summary>
    public Type[] ExceptionTypes { get; set; }

    /// <summary>
    /// 重试失败策略配置
    /// </summary>
    /// <remarks>如果没有注册，必须通过 options.AddFallbackPolicy(type) 注册</remarks>
    public Type FallbackPolicy { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    /// <remarks>数值越大的先执行</remarks>
    public int Order { get; set; } = 0;
}