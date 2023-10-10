#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

namespace ThingsGateway.Foundation.Http.WebSockets
{
    /// <summary>
    /// IWebSocket
    /// </summary>
    public interface IWebSocket : IDisposable
    {
        /// <summary>
        /// 表示当前WebSocket是否已经完成连接。
        /// </summary>
        bool IsHandshaked { get; }

        /// <summary>
        /// WebSocket版本
        /// </summary>
        string Version { get; }

        /// <summary>
        /// 发送Close报文。
        /// </summary>
        /// <param name="msg"></param>
        void Close(string msg);

        /// <summary>
        /// 发送Close报文
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        Task CloseAsync(string msg);

        /// <summary>
        /// 发送Ping报文。
        /// </summary>
        void Ping();

        /// <summary>
        /// 发送Ping报文
        /// </summary>
        /// <returns></returns>
        Task PingAsync();

        /// <summary>
        /// 发送Pong报文。
        /// </summary>
        void Pong();

        /// <summary>
        /// 发送Pong报文
        /// </summary>
        /// <returns></returns>
        Task PongAsync();

        /// <summary>
        /// 异步等待读取数据
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<WebSocketReceiveResult> ReadAsync(CancellationToken token);

        /// <summary>
        /// 采用WebSocket协议，发送WS数据。发送结束后，请及时释放<see cref="WSDataFrame"/>
        /// </summary>
        /// <param name="dataFrame"></param>
        /// <param name="endOfMessage"></param>
        void Send(WSDataFrame dataFrame, bool endOfMessage = true);

        /// <summary>
        /// 发送文本消息
        /// </summary>
        /// <param name="text"></param>
        /// <param name="endOfMessage"></param>
        void Send(string text, bool endOfMessage = true);

        /// <summary>
        /// 发送二进制消息
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="endOfMessage"></param>
        void Send(byte[] buffer, int offset, int length, bool endOfMessage = true);

        /// <summary>
        /// 发送二进制消息
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="endOfMessage"></param>
        void Send(ByteBlock byteBlock, bool endOfMessage = true);

        /// <summary>
        /// 发送二进制消息
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="endOfMessage"></param>
        void Send(byte[] buffer, bool endOfMessage = true);

        /// <summary>
        /// 采用WebSocket协议，发送WS数据。发送结束后，请及时释放<see cref="WSDataFrame"/>
        /// </summary>
        /// <param name="dataFrame"></param>
        /// <param name="endOfMessage"></param>
        /// <returns></returns>
        Task SendAsync(WSDataFrame dataFrame, bool endOfMessage = true);

        /// <summary>
        /// 发送文本消息
        /// </summary>
        /// <param name="text"></param>
        /// <param name="endOfMessage"></param>
        /// <returns></returns>
        Task SendAsync(string text, bool endOfMessage = true);

        /// <summary>
        /// 发送二进制消息
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="endOfMessage"></param>
        /// <returns></returns>
        Task SendAsync(byte[] buffer, bool endOfMessage = true);

        /// <summary>
        /// 发送二进制消息
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="endOfMessage"></param>
        /// <returns></returns>
        Task SendAsync(byte[] buffer, int offset, int length, bool endOfMessage = true);
    }
}