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
    public class WebSocketFeature : PluginBase, ITcpReceivedPlugin, IHttpPlugin
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

        private readonly IPluginsManager m_pluginsManager;

        private string m_wSUrl = "/ws";

        /// <summary>
        /// WebSocketFeature
        /// </summary>
        /// <param name="pluginsManager"></param>
        public WebSocketFeature(IPluginsManager pluginsManager)
        {
            this.m_pluginsManager = pluginsManager ?? throw new ArgumentNullException(nameof(pluginsManager));
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
        public Func<IHttpSocketClient, HttpContext, bool> VerifyConnection { get; set; }

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
                if (this.VerifyConnection.Invoke(client, e.Context))
                {
                    e.Handled = true;
                    client.SwitchProtocolToWebSocket(e.Context);
                    return;
                }
            }
            await e.InvokeNext();
        }

        /// <inheritdoc/>
        public Task OnTcpReceived(ITcpClientBase client, ReceivedDataEventArgs e)
        {
            if (client.Protocol == Protocol.WebSocket)
            {
                if (e.RequestInfo is WSDataFrame dataFrame)
                {
                    e.Handled = true;
                    this.OnHandleWSDataFrame(client, new WSDataFrameEventArgs(dataFrame));
                    return EasyTask.CompletedTask;
                }
            }
            return e.InvokeNext();
        }

        /// <summary>
        /// 验证连接
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public WebSocketFeature SetVerifyConnection(Func<IHttpSocketClient, HttpContext, bool> func)
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

        /// <summary>
        /// 处理WS数据帧。覆盖父类方法将不会触发插件。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnHandleWSDataFrame(ITcpClientBase client, WSDataFrameEventArgs e)
        {
            if (this.AutoClose && e.DataFrame.Opcode == WSDataType.Close)
            {
                var msg = e.DataFrame.PayloadData?.ToString();
                this.m_pluginsManager.Raise(nameof(IWebSocketClosingPlugin.OnWebSocketClosing), client, new MsgPermitEventArgs() { Message = msg });
                client.Close(msg);
                return;
            }
            if (this.AutoPong && e.DataFrame.Opcode == WSDataType.Ping)
            {
                ((HttpSocketClient)client).PongWS();
                return;
            }

            this.m_pluginsManager.Raise(nameof(IWebSocketReceivedPlugin.OnWebSocketReceived), client, e);
        }

        private bool ThisVerifyConnection(IHttpSocketClient client, HttpContext context)
        {
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