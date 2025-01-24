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

using System.Net.WebSockets;

namespace ThingsGateway.HttpRemote;

/// <summary>
///     WebSocket 接收的文本消息的结果类
/// </summary>
public sealed class WebSocketTextReceiveResult : WebSocketReceiveResult
{
    /// <inheritdoc />
    public WebSocketTextReceiveResult(int count, bool endOfMessage)
        : base(count, WebSocketMessageType.Text, endOfMessage)
    {
    }

    /// <inheritdoc />
    public WebSocketTextReceiveResult(int count, bool endOfMessage, WebSocketCloseStatus? closeStatus,
        string? closeStatusDescription)
        : base(count, WebSocketMessageType.Text, endOfMessage, closeStatus, closeStatusDescription)
    {
    }

    /// <summary>
    ///     文本消息
    /// </summary>
    public string Message { get; internal init; } = default!;
}