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
///     WebSocket 客户端配置选项
/// </summary>
public sealed class WebSocketClientOptions
{
    /// <summary>
    ///     <inheritdoc cref="WebSocketClientOptions" />
    /// </summary>
    /// <param name="serverUri">服务器地址</param>
    public WebSocketClientOptions(string serverUri)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(serverUri);

        ServerUri = new Uri(serverUri);
    }

    /// <summary>
    ///     <inheritdoc cref="WebSocketClientOptions" />
    /// </summary>
    /// <param name="serverUri">服务器地址</param>
    public WebSocketClientOptions(Uri serverUri)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(serverUri);

        ServerUri = serverUri;
    }

    /// <summary>
    ///     服务器地址
    /// </summary>
    public Uri ServerUri { get; }

    /// <summary>
    ///     重连的间隔时间（毫秒）
    /// </summary>
    /// <remarks>默认值为 2 秒。</remarks>
    public TimeSpan ReconnectInterval { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>
    ///     最大重连次数
    /// </summary>
    /// <remarks>默认最大重连次数为 10。</remarks>
    public int MaxReconnectRetries { get; set; } = 10;

    /// <summary>
    ///     超时时间
    /// </summary>
    public TimeSpan? Timeout { get; set; }

    /// <summary>
    ///     接收服务器新消息缓冲区大小
    /// </summary>
    /// <remarks>以字节为单位，默认值为 <c>4 KB</c>。</remarks>
    public int ReceiveBufferSize { get; set; } = 1024 * 4;

    /// <summary>
    ///     用于配置 <see cref="ConfigureClientWebSocketOptions" /> 的操作
    /// </summary>
    public Action<ClientWebSocketOptions>? ConfigureClientWebSocketOptions { get; set; }
}