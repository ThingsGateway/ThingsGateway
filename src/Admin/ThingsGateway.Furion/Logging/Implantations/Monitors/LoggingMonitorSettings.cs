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

using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

using System.Text.Encodings.Web;
using System.Text.Json;

namespace ThingsGateway.Logging;

/// <summary>
/// 日志监视器配置
/// </summary>
/// <remarks>默认配置节点：Logging:Monitor，支持自定义</remarks>
[SuppressSniffer]
public sealed class LoggingMonitorSettings
{
    /// <summary>
    /// 全局启用
    /// </summary>
    public bool GlobalEnabled { get; set; } = false;

    /// <summary>
    /// 配置包含拦截的方法名列表（完全限定名格式：程序集名称.类名.方法名），注意无需添加参数签名
    /// </summary>
    /// <remarks>结合 <seealso cref="GlobalEnabled"/> 使用，当 <see cref="GlobalEnabled"/> 为 false 时有效，</remarks>
    public string[] IncludeOfMethods { get; set; } = Array.Empty<string>();

    /// <summary>
    /// 配置排除拦截的方法名列表（完全限定名格式：程序集名称.类名.方法名），注意无需添加参数签名
    /// </summary>
    /// <remarks>结合 <seealso cref="GlobalEnabled"/> 使用，当 <see cref="GlobalEnabled"/> 为 true 时有效，</remarks>
    public string[] ExcludeOfMethods { get; set; } = Array.Empty<string>();

    /// <summary>
    /// 配置方法更多信息
    /// </summary>
    public LoggingMonitorMethod[] MethodsSettings { get; set; } = Array.Empty<LoggingMonitorMethod>();

    /// <summary>
    /// 业务日志消息级别
    /// </summary>
    /// <remarks>控制 Oops.Oh 或 Oops.Bah 日志记录位置，默认写入 <see cref="LogLevel.Information"/></remarks>
    public LogLevel BahLogLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// 默认输出日志级别
    /// </summary>
    public LogLevel LogLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// 是否记录返回值
    /// </summary>
    /// <remarks>bool 类型，默认输出</remarks>
    public bool WithReturnValue { get; set; } = true;

    /// <summary>
    /// 设置返回值阈值
    /// </summary>
    /// <remarks>配置返回值字符串阈值，超过这个阈值将截断，默认全量输出</remarks>
    public int ReturnValueThreshold { get; set; } = 0;

    /// <summary>
    /// 配置 Json 输出行为
    /// </summary>
    public JsonBehavior JsonBehavior { get; set; } = JsonBehavior.None;

    /// <summary>
    /// 配置 序列化属性命名规则（返回值）
    /// </summary>
    public ContractResolverTypes ContractResolver { get; set; } = ContractResolverTypes.CamelCase;

    /// <summary>
    /// 配置序列化忽略的属性名称
    /// </summary>
    public string[] IgnorePropertyNames { get; set; }

    /// <summary>
    /// 配置序列化忽略的属性类型
    /// </summary>
    public Type[] IgnorePropertyTypes { get; set; }

    /// <summary>
    /// 自定义日志筛选器
    /// </summary>
    public Func<FilterContext, bool> WriteFilter { get; set; }

    /// <summary>
    /// 是否 Mvc Filter 方式注册
    /// </summary>
    /// <remarks>解决过去 Mvc Filter 全局注册的问题</remarks>
    internal bool IsMvcFilterRegister { get; set; } = true;

    /// <summary>
    /// 是否来自全局触发器
    /// </summary>
    /// <remarks>解决局部和全局触发器同时配置触发两次问题</remarks>
    internal bool FromGlobalFilter { get; set; } = false;

    /// <summary>
    /// 添加日志更多配置
    /// </summary>
    internal static Action<ILogger, LogContext, FilterContext> Configure { get; private set; }

    /// <summary>
    /// 自定义日志筛选器
    /// </summary>
    internal static Func<FilterContext, bool> InternalWriteFilter { get; set; }

    /// <summary>
    /// 配置日志更多功能
    /// </summary>
    /// <param name="configure"></param>
    public void ConfigureLogger(Action<ILogger, LogContext, FilterContext> configure)
    {
        Configure = configure;
    }

    /// <summary>
    /// JSON 输出格式化
    /// </summary>
    public bool JsonIndented { get; set; } = false;

    /// <summary>
    /// 是否处理 Long 转 String
    /// </summary>
    public bool LongTypeConverter { get; set; } = false;

    /// <summary>
    /// 配置 Json 写入选项
    /// </summary>
    public JsonWriterOptions JsonWriterOptions { get; set; } = new JsonWriterOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        SkipValidation = true
    };
}