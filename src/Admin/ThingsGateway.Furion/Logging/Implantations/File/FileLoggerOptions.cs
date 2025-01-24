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

using Microsoft.Extensions.Logging;

namespace ThingsGateway.Logging;

/// <summary>
/// 文件日志记录器配置选项
/// </summary>
[SuppressSniffer]
public sealed class FileLoggerOptions
{
    /// <summary>
    /// 追加到已存在日志文件或覆盖它们
    /// </summary>
    public bool Append { get; set; } = true;

    /// <summary>
    /// 控制每一个日志文件最大存储大小，默认无限制，单位是 B，也就是 1024 才等于 1KB
    /// </summary>
    /// <remarks>如果指定了该值，那么日志文件大小超出了该配置就会创建的日志文件，新创建的日志文件命名规则：文件名+[递增序号].log</remarks>
    public long FileSizeLimitBytes { get; set; } = 0;

    /// <summary>
    /// 控制最大创建的日志文件数量，默认无限制，配合 <see cref="FileSizeLimitBytes"/> 使用
    /// </summary>
    /// <remarks>如果指定了该值，那么超出该值将从最初日志文件中从头写入覆盖</remarks>
    public int MaxRollingFiles { get; set; } = 0;

    /// <summary>
    /// 最低日志记录级别
    /// </summary>
    public LogLevel MinimumLevel { get; set; } = LogLevel.Trace;

    /// <summary>
    /// 是否使用 UTC 时间戳，默认 false
    /// </summary>
    public bool UseUtcTimestamp { get; set; }

    /// <summary>
    /// 自定义日志消息格式化程序
    /// </summary>
    public Func<LogMessage, string> MessageFormat { get; set; }

    /// <summary>
    /// 自定义日志筛选器
    /// </summary>
    public Func<LogMessage, bool> WriteFilter { get; set; }

    /// <summary>
    /// 自定义日志文件名格式化程序（规则）
    /// </summary>
    /// <example>
    /// options.FileNameRule = (fileName) => {
    ///     return String.Format(Path.GetFileNameWithoutExtension(fileName) + "_{0:yyyy}-{0:MM}-{0:dd}" + Path.GetExtension(fileName), DateTime.UtcNow);
    ///
    ///     // 或者每天创建一个文件
    ///     // return String.Format(fileName, DateTime.UtcNow);
    /// }
    /// </example>
    public Func<string, string> FileNameRule { get; set; }

    /// <summary>
    /// 自定义日志文件写入错误程序
    /// </summary>
    /// <remarks>主要解决日志在写入过程中文件被打开或其他应用程序占用的情况，一旦出现上述情况可创建备用日志文件继续写入</remarks>
    /// <example>
    /// options.HandleWriteError = (err) => {
    ///     err.UseRollbackFileName(Path.GetFileNameWithoutExtension(err.CurrentFileName)+ "_alt" + Path.GetExtension(err.CurrentFileName));
    /// };
    /// </example>
    public Action<FileWriteError> HandleWriteError { get; set; }

    /// <summary>
    /// 日期格式化
    /// </summary>
    public string DateFormat { get; set; } = "yyyy-MM-dd HH:mm:ss.fffffff zzz dddd";

    /// <summary>
    /// 是否启用日志上下文
    /// </summary>
    public bool IncludeScopes { get; set; } = true;

    /// <summary>
    /// 显示跟踪/请求 Id
    /// </summary>
    public bool WithTraceId { get; set; } = false;

    /// <summary>
    /// 显示堆栈框架（程序集和方法签名）
    /// </summary>
    public bool WithStackFrame { get; set; } = false;

    /// <summary>
    /// 日志消息内容转换（如脱敏处理）
    /// </summary>
    public Func<string, string> MessageProcess { get; set; }
}