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
///     Server-Sent Events 事件处理程序
/// </summary>
public interface IHttpServerSentEventsEventHandler
{
    /// <summary>
    ///     用于在与事件源的连接打开时的操作
    /// </summary>
    void OnOpen();

    /// <summary>
    ///     用于在从事件源接收到数据时的操作
    /// </summary>
    /// <param name="serverSentEventsData">
    ///     <see cref="ServerSentEventsData" />
    /// </param>
    /// <returns>
    ///     <see cref="Task" />
    /// </returns>
    Task OnMessageAsync(ServerSentEventsData serverSentEventsData);

    /// <summary>
    ///     用于在事件源连接未能打开时的操作
    /// </summary>
    /// <param name="exception">
    ///     <see cref="Exception" />
    /// </param>
    void OnError(Exception exception);
}