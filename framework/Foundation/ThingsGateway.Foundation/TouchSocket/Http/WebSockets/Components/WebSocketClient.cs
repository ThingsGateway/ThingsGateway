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

//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.Http.WebSockets
{
    /// <summary>
    /// WebSocketClient用户终端简单实现。
    /// </summary>
    public class WebSocketClient : WebSocketClientBase
    {
        /// <summary>
        /// 收到WebSocket数据
        /// </summary>
        public WSDataFrameEventHandler<WebSocketClient> Received { get; set; }

        /// <inheritdoc/>
        protected override Task OnReceivedWSDataFrame(WSDataFrameEventArgs e)
        {
            if (this.Received != null)
            {
                return this.Received.Invoke(this, e);
            }
            return base.OnReceivedWSDataFrame(e);
        }
    }

    /// <summary>
    /// WebSocket用户终端。
    /// </summary>
    public class WebSocketClientBase : HttpClientBase, IWebSocketClient
    {
        #region Connect
        /// <summary>
        /// 连接到ws服务器
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="token"></param>
        /// <exception cref="WebSocketConnectException"></exception>
        public override void Connect(int timeout, CancellationToken token)
        {
            try
            {
                this.m_semaphoreSlim.Wait(token);
                if (!this.Online)
                {
                    base.Connect(timeout, token);
                }

                var iPHost = this.Config.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty);
                var url = iPHost.PathAndQuery;
                var request = WSTools.GetWSRequest(this.RemoteIPHost.Host, url, this.GetWebSocketVersion(), out var base64Key);
                this.OnHandshaking(new HttpContextEventArgs(new HttpContext(request)))
                    .GetFalseAwaitResult();

                var response = this.Request(request, timeout: timeout, token: token);
                if (response.StatusCode != 101)
                {
                    throw new WebSocketConnectException($"协议升级失败，信息：{response.StatusMessage}，更多信息请捕获WebSocketConnectException异常，获得HttpContext得知。", new HttpContext(request, response));
                }
                var accept = response.Headers.Get("sec-websocket-accept").Trim();
                if (accept.IsNullOrEmpty() || !accept.Equals(WSTools.CalculateBase64Key(base64Key).Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    this.MainSocket.SafeDispose();
                    throw new WebSocketConnectException($"WS服务器返回的应答码不正确，更多信息请捕获WebSocketConnectException异常，获得HttpContext得知。", new HttpContext(request, response));
                }

                this.SetAdapter(new WebSocketDataHandlingAdapter());
                this.SetValue(WebSocketFeature.HandshakedProperty, true);
                response.Flag = true;
                Task.Factory.StartNew(PrivateOnHandshaked, new HttpContextEventArgs(new HttpContext(request, response)));
            }
            finally
            {
                this.m_semaphoreSlim.Release();
            }
        }

        /// <inheritdoc/>
        public override async Task ConnectAsync(int timeout, CancellationToken token)
        {
            try
            {
                await this.m_semaphoreSlim.WaitAsync(timeout, token);
                if (!this.Online)
                {
                    await base.ConnectAsync(timeout, token);
                }

                var iPHost = this.Config.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty);
                var url = iPHost.PathAndQuery;
                var request = WSTools.GetWSRequest(this.RemoteIPHost.Host, url, this.GetWebSocketVersion(), out var base64Key);

                await this.OnHandshaking(new HttpContextEventArgs(new HttpContext(request)));

                var response = await this.RequestAsync(request, timeout: timeout, token: token);
                if (response.StatusCode != 101)
                {
                    throw new WebSocketConnectException($"协议升级失败，信息：{response.StatusMessage}，更多信息请捕获WebSocketConnectException异常，获得HttpContext得知。", new HttpContext(request, response));
                }
                var accept = response.Headers.Get("sec-websocket-accept").Trim();
                if (accept.IsNullOrEmpty() || !accept.Equals(WSTools.CalculateBase64Key(base64Key).Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    this.MainSocket.SafeDispose();
                    throw new WebSocketConnectException($"WS服务器返回的应答码不正确，更多信息请捕获WebSocketConnectException异常，获得HttpContext得知。", new HttpContext(request, response));
                }

                this.SetAdapter(new WebSocketDataHandlingAdapter());
                this.SetValue(WebSocketFeature.HandshakedProperty, true);
                response.Flag = true;
                _ = Task.Factory.StartNew(PrivateOnHandshaked, new HttpContextEventArgs(new HttpContext(request, response)));
            }
            finally
            {
                this.m_semaphoreSlim.Release();
            }
        }
        #endregion Connect

        #region 字段

        private readonly SemaphoreSlim m_semaphoreSlim = new SemaphoreSlim(1, 1);

        #endregion 字段

        #region 事件

        /// <summary>
        /// 表示完成握手后。
        /// </summary>
        public HttpContextEventHandler<WebSocketClientBase> Handshaked { get; set; }

        /// <summary>
        /// 表示在即将握手连接时。
        /// </summary>
        public HttpContextEventHandler<WebSocketClientBase> Handshaking { get; set; }

        private Task PrivateOnHandshaked(object obj)
        {
            return this.OnHandshaked((HttpContextEventArgs)obj);
        }

        /// <summary>
        /// 表示完成握手后。
        /// </summary>
        /// <param name="e"></param>
        protected virtual async Task OnHandshaked(HttpContextEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }
            if (this.Handshaked != null)
            {
                await this.Handshaked.Invoke(this, e);
                if (e.Handled)
                {
                    return;
                }
            }
            await this.PluginsManager.RaiseAsync(nameof(IWebSocketHandshakedPlugin.OnWebSocketHandshaked), this, e);
        }

        /// <summary>
        /// 表示在即将握手连接时。
        /// </summary>
        /// <param name="e"></param>
        protected virtual async Task OnHandshaking(HttpContextEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }
            if (this.Handshaking != null)
            {
                await this.Handshaking.Invoke(this, e).ConfigureFalseAwait();
                if (e.Handled)
                {
                    return;
                }
            }
            await this.PluginsManager.RaiseAsync(nameof(IWebSocketHandshakingPlugin.OnWebSocketHandshaking), this, e).ConfigureFalseAwait();
        }

        #endregion 事件

        /// <inheritdoc/>
        protected override async Task ReceivedData(ReceivedDataEventArgs e)
        {
            if (this.GetHandshaked())
            {
                var dataFrame = (WSDataFrame)e.RequestInfo;

                if (this.TryGetValue(WebSocketClientExtension.WebSocketProperty, out var internalWebSocket))
                {
                    if (await internalWebSocket.TryInputReceiveAsync(dataFrame).ConfigureFalseAwait())
                    {
                        return;
                    }
                }
                await this.OnReceivedWSDataFrame(new WSDataFrameEventArgs(dataFrame));
            }
            else
            {
                if (e.RequestInfo is HttpResponse response)
                {
                    response.Flag = false;
                    await base.ReceivedData(e);
                    SpinWait.SpinUntil(() =>
                    {
                        return (bool)response.Flag;
                    }, 3000);
                }
            }

            await base.ReceivedData(e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="e"></param>
        protected override async Task OnDisconnected(DisconnectEventArgs e)
        {
            this.SetValue(WebSocketFeature.HandshakedProperty, false);
            if (this.TryGetValue(WebSocketClientExtension.WebSocketProperty, out var internalWebSocket))
            {
                _ = internalWebSocket.TryInputReceiveAsync(null);
            }
            await base.OnDisconnected(e);
        }

        /// <summary>
        /// 当收到WS数据时。
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected virtual async Task OnReceivedWSDataFrame(WSDataFrameEventArgs e)
        {
            await this.PluginsManager.RaiseAsync(nameof(IWebSocketReceivedPlugin.OnWebSocketReceived), this, e).ConfigureFalseAwait();
        }
    }
}