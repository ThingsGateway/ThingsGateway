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

namespace ThingsGateway.Foundation.Dmtp
{
    /// <summary>
    /// HttpDmtpClient
    /// </summary>
    public partial class HttpDmtpClient : HttpClientBase, IHttpDmtpClient
    {
        #region 字段

        private bool m_allowRoute;
        private Func<string, Task<IDmtpActor>> m_findDmtpActor;
        private DmtpActor m_dmtpActor;
        private readonly SemaphoreSlim m_semaphoreForConnect = new SemaphoreSlim(1, 1);
        #endregion 字段

        /// <inheritdoc cref="IDmtpActor.Id"/>
        public string Id => this.m_dmtpActor.Id;

        /// <inheritdoc cref="IHandshakeObject.IsHandshaked"/>
        public bool IsHandshaked => this.m_dmtpActor != null && this.m_dmtpActor.IsHandshaked;

        /// <inheritdoc/>
        public IDmtpActor DmtpActor { get => this.m_dmtpActor; }

        #region 连接

        /// <summary>
        /// 使用基于Http升级的协议，连接Dmtp服务器
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="token"></param>
        /// <exception cref="Exception"></exception>
        public override void Connect(int timeout, CancellationToken token)
        {
            try
            {
                this.m_semaphoreForConnect.Wait(token);
                if (this.IsHandshaked)
                {
                    return;
                }
                if (!this.Online)
                {
                    base.Connect(timeout, token);
                }

                var request = new HttpRequest()
                    .SetHost(this.RemoteIPHost.Host);
                request.Headers.Add(HttpHeaders.Connection, "upgrade");
                request.Headers.Add(HttpHeaders.Upgrade, DmtpUtility.Dmtp.ToLower());

                request.AsMethod(DmtpUtility.Dmtp);
                var response = this.RequestContent(request);
                if (response.StatusCode == 101)
                {
                    this.SwitchProtocolToDmtp();
                    this.m_dmtpActor.Handshake(

                        this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).VerifyToken, this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).Id, timeout, this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).Metadata, token);
                    return;
                }
                else
                {
                    throw new Exception(response.StatusMessage);
                }
            }
            finally
            {
                this.m_semaphoreForConnect.Release();
            }
        }

        /// <summary>
        /// 异步使用基于Http升级的协议，连接Dmtp服务器
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public override async Task ConnectAsync(int timeout, CancellationToken token)
        {
            try
            {
                await this.m_semaphoreForConnect.WaitAsync(timeout, token);
                if (this.IsHandshaked)
                {
                    return;
                }
                if (!this.Online)
                {
                    await base.ConnectAsync(timeout, token);
                }

                var request = new HttpRequest()
                    .SetHost(this.RemoteIPHost.Host);
                request.Headers.Add(HttpHeaders.Connection, "upgrade");
                request.Headers.Add(HttpHeaders.Upgrade, DmtpUtility.Dmtp.ToLower());

                request.AsMethod(DmtpUtility.Dmtp);
                var response = await this.RequestContentAsync(request);
                if (response.StatusCode == 101)
                {
                    this.SwitchProtocolToDmtp();
                    await this.m_dmtpActor.HandshakeAsync(this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).VerifyToken, this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).Id, timeout, this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).Metadata, token);
                    return;
                }
                else
                {
                    throw new Exception(response.StatusMessage);
                }
            }
            finally
            {
                this.m_semaphoreForConnect.Release();
            }
        }

        #endregion 连接

        #region 断开
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (this.IsHandshaked)
            {
                this.m_dmtpActor?.SafeDispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// 发送<see cref="IDmtpActor"/>关闭消息。
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public override void Close(string msg = "")
        {
            if (this.IsHandshaked)
            {
                this.m_dmtpActor.SendClose(msg);
                this.m_dmtpActor.Close(msg);
            }
            base.Close(msg);
        }

        /// <inheritdoc/>
        protected override async Task OnDisconnected(DisconnectEventArgs e)
        {
            this.m_dmtpActor?.Close(e.Message);
            await base.OnDisconnected(e);
        }

        #endregion

        /// <inheritdoc/>
        protected override async Task ReceivedData(ReceivedDataEventArgs e)
        {
            if (this.Protocol == DmtpUtility.DmtpProtocol && e.RequestInfo is DmtpMessage message)
            {
                if (!await this.m_dmtpActor.InputReceivedData(message))
                {
                    if (this.PluginManager.Enable)
                    {
                        await this.PluginManager.RaiseAsync(nameof(IDmtpReceivedPlugin.OnDmtpReceived), this, new DmtpMessageEventArgs(message));
                    }
                }
                return;
            }
            await base.ReceivedData(e);
        }

        /// <inheritdoc/>
        protected override void LoadConfig(TouchSocketConfig config)
        {
            base.LoadConfig(config);
            if (this.Container.IsRegistered(typeof(IDmtpRouteService)))
            {
                this.m_allowRoute = true;
                this.m_findDmtpActor = this.Container.Resolve<IDmtpRouteService>().FindDmtpActor;
            }
        }


        #region ResetId

        ///<inheritdoc cref="IDmtpActor.ResetId(string)"/>
        public void ResetId(string id)
        {
            this.m_dmtpActor.ResetId(id);
        }

        ///<inheritdoc cref="IDmtpActor.ResetIdAsync(string)"/>
        public Task ResetIdAsync(string newId)
        {
            return this.m_dmtpActor.ResetIdAsync(newId);
        }

        #endregion ResetId

        private void SwitchProtocolToDmtp()
        {
            this.Protocol = DmtpUtility.DmtpProtocol;
            this.SetDataHandlingAdapter(new TcpDmtpAdapter());
            this.m_dmtpActor = new SealedDmtpActor(this.m_allowRoute)
            {
                OutputSend = this.DmtpActorSend,
                OutputSendAsync = this.DmtpActorSendAsync,
                Routing = this.OnDmtpActorRouting,
                Handshaking = this.OnDmtpActorHandshaking,
                Handshaked = this.OnDmtpActorHandshaked,
                Closed = this.OnDmtpActorClose,
                CreatedChannel = this.OnDmtpActorCreateChannel,
                Logger = this.Logger,
                Client = this,
                FindDmtpActor = this.m_findDmtpActor
            };
        }

        #region 内部委托绑定

        private Task OnDmtpActorClose(DmtpActor actor, string msg)
        {
            base.BreakOut(false, msg);
            return EasyTask.CompletedTask;
        }

        private Task OnDmtpActorCreateChannel(DmtpActor actor, CreateChannelEventArgs e)
        {
            return this.OnCreateChannel(e);
        }

        private Task OnDmtpActorHandshaked(DmtpActor actor, DmtpVerifyEventArgs e)
        {
            return this.OnHandshaked(e);
        }

        private Task OnDmtpActorHandshaking(DmtpActor actor, DmtpVerifyEventArgs e)
        {
            return this.OnHandshaking(e);
        }

        private Task OnDmtpActorRouting(DmtpActor actor, PackageRouterEventArgs e)
        {
            return this.OnRouting(e);
        }

        private void DmtpActorSend(DmtpActor actor, ArraySegment<byte>[] transferBytes)
        {
            base.Send(transferBytes);
        }

        private Task DmtpActorSendAsync(DmtpActor actor, ArraySegment<byte>[] transferBytes)
        {
            return base.SendAsync(transferBytes);
        }

        #endregion 内部委托绑定

        #region 事件触发

        /// <summary>
        /// 当创建通道
        /// </summary>
        /// <param name="e"></param>
        protected virtual async Task OnCreateChannel(CreateChannelEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }

            await this.PluginManager.RaiseAsync(nameof(IDmtpCreateChannelPlugin.OnCreateChannel), this, e);
        }

        /// <summary>
        /// 在完成握手连接时
        /// </summary>
        /// <param name="e"></param>
        protected virtual async Task OnHandshaked(DmtpVerifyEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }
            await this.PluginManager.RaiseAsync(nameof(IDmtpHandshakedPlugin.OnDmtpHandshaked), this, e);
        }

        /// <summary>
        /// 即将握手连接时
        /// </summary>
        /// <param name="e">参数</param>
        protected virtual async Task OnHandshaking(DmtpVerifyEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }
            await this.PluginManager.RaiseAsync(nameof(IDmtpHandshakingPlugin.OnDmtpHandshaking), this, e);
        }

        /// <summary>
        /// 当需要转发路由包时
        /// </summary>
        /// <param name="e"></param>
        protected virtual async Task OnRouting(PackageRouterEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }
            await this.PluginManager.RaiseAsync(nameof(IDmtpRoutingPlugin.OnDmtpRouting), this, e);
        }

        #endregion 事件触发
    }
}