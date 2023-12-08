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
    internal class InternalWebSocket : DisposableObject, IWebSocket
    {
        private readonly IHttpClientBase m_client;
        private bool m_receive;
        private readonly AsyncAutoResetEvent m_resetEventForComplateRead = new AsyncAutoResetEvent(false);
        private readonly AsyncAutoResetEvent m_resetEventForRead = new AsyncAutoResetEvent(false);
        private WSDataFrame m_dataFrame;

        public InternalWebSocket(IHttpClientBase client, bool receive)
        {
            this.m_client = client;
            this.m_receive = receive;
        }

        public bool IsHandshaked => this.m_client.GetHandshaked();

        public string Version => this.m_client.GetWebSocketVersion();

        public void Close(string msg)
        {
            this.m_client.CloseWithWS(msg);
            this.m_client.TryShutdown();
            this.m_client.SafeClose(msg);
            this.m_receive = false;
        }

        public async Task CloseAsync(string msg)
        {
            await this.m_client.CloseWithWSAsync(msg);
            this.m_client.TryShutdown();
            this.m_client.SafeClose(msg);
            this.m_receive = false;
        }

        public void Ping()
        {
            this.m_client.PingWS();
        }

        public Task PingAsync()
        {
            return this.m_client.PingWSAsync();
        }

        public void Pong()
        {
            this.m_client.PongWS();
        }

        public Task PongAsync()
        {
            return this.m_client.PongWSAsync();
        }

        public async Task<WebSocketReceiveResult> ReadAsync(CancellationToken token)
        {
            if (!this.m_receive)
            {
                return new WebSocketReceiveResult(this.ComplateRead, null);
            }
            await this.m_resetEventForRead.WaitOneAsync(token).ConfigureFalseAwait();
            return new WebSocketReceiveResult(this.ComplateRead, this.m_dataFrame);
        }

#if NET6_0_OR_GREATER
        public async ValueTask<WebSocketReceiveResult> ValueReadAsync(CancellationToken token)
        {
            if (!this.m_receive)
            {
                return new WebSocketReceiveResult(this.ComplateRead, null);
            }
            await this.m_resetEventForRead.WaitOneAsync(token).ConfigureFalseAwait();
            return new WebSocketReceiveResult(this.ComplateRead, this.m_dataFrame);
        }
#endif

        #region 发送

        public void Send(WSDataFrame dataFrame, bool endOfMessage = true)
        {
            this.m_client.SendWithWS(dataFrame, endOfMessage);
        }

        public void Send(string text, bool endOfMessage = true)
        {
            this.m_client.SendWithWS(text, endOfMessage);
        }

        public Task SendAsync(string text, bool endOfMessage = true)
        {
            return this.m_client.SendWithWSAsync(text, endOfMessage);
        }

        public void Send(byte[] buffer, int offset, int length, bool endOfMessage = true)
        {
            this.m_client.SendWithWS(buffer, offset, length, endOfMessage);
        }

        public void Send(ByteBlock byteBlock, bool endOfMessage = true)
        {
            this.m_client.SendWithWS(byteBlock, endOfMessage);
        }

        public Task SendAsync(byte[] buffer, bool endOfMessage = true)
        {
            return this.m_client.SendWithWSAsync(buffer, endOfMessage);
        }

        public void Send(byte[] buffer, bool endOfMessage = true)
        {
            this.m_client.SendWithWS(buffer, endOfMessage);
        }

        public Task SendAsync(byte[] buffer, int offset, int length, bool endOfMessage = true)
        {
            return this.m_client.SendWithWSAsync(buffer, offset, length, endOfMessage);
        }

        public Task SendAsync(WSDataFrame dataFrame, bool endOfMessage = true)
        {
            return this.m_client.SendWithWSAsync(dataFrame, endOfMessage);
        }

        #endregion 发送

        public async Task<bool> TryInputReceiveAsync(WSDataFrame dataFrame)
        {
            if (!this.m_receive)
            {
                return false;
            }
            this.m_dataFrame = dataFrame;
            this.m_resetEventForRead.Set();
            if (dataFrame == null)
            {
                return true;
            }
            if (await this.m_resetEventForComplateRead.WaitOneAsync(TimeSpan.FromSeconds(10)).ConfigureFalseAwait())
            {
                return true;
            }
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            this.m_client.RemoveValue(WebSocketClientExtension.WebSocketProperty);
            this.m_resetEventForComplateRead.SafeDispose();
            this.m_resetEventForRead.SafeDispose();
            this.m_dataFrame = null;
            base.Dispose(disposing);
        }

        private void ComplateRead()
        {
            this.m_dataFrame = default;
            this.m_resetEventForComplateRead.Set();
        }
    }
}