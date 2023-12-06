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
    /// 基于Http的WebSocket的扩展。
    /// <para>此组件只能挂载在<see cref="HttpService"/>中</para>
    /// </summary>
    [PluginOption(Singleton = true, NotRegister = false)]
    public sealed class WebSocketFeature : PluginBase, ITcpReceivedPlugin, IHttpPlugin, ITcpDisconnectedPlugin
    {
        /// <summary>
        /// 表示是否完成WS握手
        /// </summary>
        public static readonly DependencyProperty<bool> HandshakedProperty =
            DependencyProperty<bool>.Register("Handshaked", false);

        /// <summary>
        /// 表示WebSocketVersion
        /// </summary>
        public static readonly DependencyProperty<string> WebSocketVersionProperty =
            DependencyProperty<string>.Register("WebSocketVersion", "13");

        private readonly IPluginManager m_pluginsManager;

        private string m_wSUrl = "/ws";

        /// <summary>
        /// WebSocketFeature
        /// </summary>
        /// <param name="pluginManager"></param>
        public WebSocketFeature(IPluginManager pluginManager)
        {
            this.m_pluginsManager = pluginManager ?? throw new ArgumentNullException(nameof(pluginManager));
            this.VerifyConnection = this.ThisVerifyConnection;
        }

        /// <summary>
        /// 是否默认处理Close报文。
        /// </summary>
        public bool AutoClose { get; set; } = true;

        /// <summary>
        /// 当收到ping报文时，是否自动回应pong。
        /// </summary>
        public bool AutoPong { get; set; }

        /// <summary>
        /// 验证连接
        /// </summary>
        public Func<IHttpSocketClient, HttpContext, Task<bool>> VerifyConnection { get; set; }

        /// <summary>
        /// 用于WebSocket连接的路径，默认为“/ws”
        /// <para>如果设置为null或空，则意味着所有的连接都将解释为WS</para>
        /// </summary>
        public string WSUrl
        {
            get => this.m_wSUrl;
            set => this.m_wSUrl = string.IsNullOrEmpty(value) ? "/" : value;
        }

        /// <summary>
        /// 不处理Close报文。
        /// </summary>
        /// <returns></returns>
        public WebSocketFeature NoAutoClose()
        {
            this.AutoClose = false;
            return this;
        }

        /// <inheritdoc/>
        public async Task OnHttpRequest(IHttpSocketClient client, HttpContextEventArgs e)
        {
            if (client.Protocol == Protocol.Http)
            {
                if (await this.VerifyConnection.Invoke(client, e.Context))
                {
                    e.Handled = true;
                    await client.SwitchProtocolToWebSocket(e.Context);
                    return;
                }
            }
            await e.InvokeNext();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public async Task OnTcpDisconnected(ITcpClientBase client, DisconnectEventArgs e)
        {
            client.SetValue(HandshakedProperty, false);
            if (client.TryGetValue(WebSocketClientExtension.WebSocketProperty, out var internalWebSocket))
            {
                _ = internalWebSocket.TryInputReceiveAsync(null);
            }
            await e.InvokeNext();
        }

        /// <inheritdoc/>
        public async Task OnTcpReceived(ITcpClientBase client, ReceivedDataEventArgs e)
        {
            if (client.Protocol == Protocol.WebSocket)
            {
                if (e.RequestInfo is WSDataFrame dataFrame)
                {
                    e.Handled = true;
                    await this.OnHandleWSDataFrame(client, dataFrame);
                    return;
                }
            }
            await e.InvokeNext();
        }

        /// <summary>
        /// 验证连接
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public WebSocketFeature SetVerifyConnection(Func<IHttpSocketClient, HttpContext, bool> func)
        {
            this.VerifyConnection = async (client, context) =>
            {
                await EasyTask.CompletedTask;
                return func.Invoke(client, context);
            };
            return this;
        }

        /// <summary>
        /// 验证连接
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public WebSocketFeature SetVerifyConnection(Func<IHttpSocketClient, HttpContext, Task<bool>> func)
        {
            this.VerifyConnection = func;
            return this;
        }

        /// <summary>
        /// 用于WebSocket连接的路径，默认为“/ws”
        /// <para>如果设置为null或空，则意味着所有的连接都将解释为WS</para>
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public WebSocketFeature SetWSUrl(string url)
        {
            this.WSUrl = url;
            return this;
        }

        /// <summary>
        /// 当收到ping报文时，自动回应pong。
        /// </summary>
        /// <returns></returns>
        public WebSocketFeature UseAutoPong()
        {
            this.AutoPong = true;
            return this;
        }

        private async Task OnHandleWSDataFrame(ITcpClientBase client, WSDataFrame dataFrame)
        {
            if (this.AutoClose && dataFrame.IsClose)
            {
                var msg = dataFrame.PayloadData?.ToString();
                await this.m_pluginsManager.RaiseAsync(nameof(IWebSocketClosingPlugin.OnWebSocketClosing), client, new MsgPermitEventArgs() { Message = msg });
                client.Close(msg);
                return;
            }
            if (this.AutoPong && dataFrame.IsPing)
            {
                ((HttpSocketClient)client).PongWS();
                return;
            }
            if (client.TryGetValue(WebSocketClientExtension.WebSocketProperty, out var internalWebSocket))
            {
                if (await internalWebSocket.TryInputReceiveAsync(dataFrame))
                {
                    return;
                }
            }
            await this.m_pluginsManager.RaiseAsync(nameof(IWebSocketReceivedPlugin.OnWebSocketReceived), client, new WSDataFrameEventArgs(dataFrame));
        }

        private async Task<bool> ThisVerifyConnection(IHttpSocketClient client, HttpContext context)
        {
            await EasyTask.CompletedTask;
            if (context.Request.Method == HttpMethod.Get)
            {
                if (this.WSUrl == "/" || context.Request.UrlEquals(this.WSUrl))
                {
                    return true;
                }
            }

            return false;
        }
    }
}