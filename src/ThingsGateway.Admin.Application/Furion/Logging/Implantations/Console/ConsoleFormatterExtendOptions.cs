//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

// 版权归百小僧及百签科技（广东）有限公司所有。

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace ThingsGateway.Logging;

/// <summary>
/// 控制台默认格式化选项拓展
/// </summary>
public sealed class ConsoleFormatterExtendOptions : ConsoleFormatterOptions
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public ConsoleFormatterExtendOptions()
        : base()
    {
        // 默认启用控制台日志上下文功能
        IncludeScopes = true;
    }

    /// <summary>
    /// 控制是否启用颜色
    /// </summary>
    public LoggerColorBehavior ColorBehavior { get; set; }

    /// <summary>
    /// 自定义日志消息格式化程序
    /// </summary>
    public Func<LogMessage, string> MessageFormat { get; set; }

    /// <summary>
    /// 日期格式化
    /// </summary>
    public string DateFormat { get; set; } = "yyyy-MM-dd HH:mm:ss.fffffff zzz dddd";

    /// <summary>
    /// 自定义日志筛选器
    /// </summary>
    public Func<LogMessage, bool> WriteFilter { get; set; }

    /// <summary>
    /// 自定义格式化日志处理程序
    /// </summary>
    public Action<LogMessage, IExternalScopeProvider, TextWriter, string, ConsoleFormatterExtendOptions> WriteHandler { get; set; }

    /// <summary>
    /// 显示跟踪/请求 Id
    /// </summary>
    public bool WithTraceId { get; set; } = false;
}