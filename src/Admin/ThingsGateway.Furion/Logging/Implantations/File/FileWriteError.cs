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

namespace ThingsGateway.Logging;

/// <summary>
/// 文件写入错误信息上下文
/// </summary>
[SuppressSniffer]
public sealed class FileWriteError
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="currentFileName">当前日志文件名</param>
    /// <param name="exception">异常对象</param>
    internal FileWriteError(string currentFileName, Exception exception)
    {
        CurrentFileName = currentFileName;
        Exception = exception;
    }

    /// <summary>
    /// 当前日志文件名
    /// </summary>
    public string CurrentFileName { get; private set; }

    /// <summary>
    /// 引起文件写入异常信息
    /// </summary>
    public Exception Exception { get; private set; }

    /// <summary>
    /// 备用日志文件名
    /// </summary>
    internal string RollbackFileName { get; private set; }

    /// <summary>
    /// 配置日志文件写入错误后新的备用日志文件名
    /// </summary>
    /// <param name="rollbackFileName">备用日志文件名</param>
    public void UseRollbackFileName(string rollbackFileName)
    {
        RollbackFileName = rollbackFileName;
    }
}