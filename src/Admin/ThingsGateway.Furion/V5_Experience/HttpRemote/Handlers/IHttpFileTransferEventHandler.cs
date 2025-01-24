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

namespace ThingsGateway.HttpRemote;

/// <summary>
///     HTTP 文件传输事件处理程序
/// </summary>
public interface IHttpFileTransferEventHandler
{
    /// <summary>
    ///     用于处理在文件开始传输时的操作
    /// </summary>
    void OnTransferStarted();

    /// <summary>
    ///     用于传输进度发生变化时的操作
    /// </summary>
    /// <param name="fileTransferProgress">
    ///     <see cref="FileTransferProgress" />
    /// </param>
    /// <returns>
    ///     <see cref="Task" />
    /// </returns>
    Task OnProgressChangedAsync(FileTransferProgress fileTransferProgress);

    /// <summary>
    ///     用于处理在文件传输完成时的操作
    /// </summary>
    /// <param name="duration">总耗时（毫秒）</param>
    void OnTransferCompleted(long duration);

    /// <summary>
    ///     用于处理在文件传输发生异常时的操作
    /// </summary>
    /// <param name="exception">
    ///     <see cref="Exception" />
    /// </param>
    void OnTransferFailed(Exception exception);
}