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
    /// TcpDmtpClient
    /// </summary>
    public partial class TcpDmtpClient : TcpClientBase, ITcpDmtpClient
    {
        /// <summary>
        /// TcpDmtpClient
        /// </summary>
        public TcpDmtpClient()
        {
            this.Protocol = DmtpUtility.DmtpProtocol;
        }

        /// <inheritdoc/>
        public IDmtpActor DmtpActor { get => this.m_dmtpActor; }

        /// <inheritdoc cref="IDmtpActor.Id"/>
        public string Id => this.m_dmtpActor.Id;

        #region 字段

        private readonly SemaphoreSlim m_semaphoreForConnect = new SemaphoreSlim(1, 1);
        private bool m_allowRoute;
        private SealedDmtpActor m_dmtpActor;
        private Func<string, Task<IDmtpActor>> m_findDmtpActor;

        #endregion 字段

        /// <inheritdoc cref="IHandshakeObject.IsHandshaked"/>
        public bool IsHandshaked => this.m_dmtpActor != null && this.m_dmtpActor.IsHandshaked;

        #region 断开

        /// <summary>
        /// 发送<see cref="IDmtpActor"/>关闭消息。
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public override void Close(string msg = "")
        {
            if (this.IsHandshaked)
            {
                this.m_dmtpActor?.SendClose(msg);
                this.m_dmtpActor?.Close(msg);
            }
            base.Close(msg);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (this.IsHandshaked)
            {
                this.m_dmtpActor?.SafeDispose();
            }
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override async Task OnDisconnected(DisconnectEventArgs e)
        {
            this.m_dmtpActor?.Close(e.Message);
            await base.OnDisconnected(e).ConfigureFalseAwait();
        }

        #endregion 断开

        #region 连接

        /// <summary>
        /// 进行Dmtp协议的握手连接
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="token"></param>
        /// <returns></returns>
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

                this.m_dmtpActor.Handshake(this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).VerifyToken,
                    this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).Id, timeout, this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).Metadata, token);
            }
            finally
            {
                this.m_semaphoreForConnect.Release();
            }
        }

        /// <summary>
        /// 异步进行Dmtp协议的握手连接
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="token"></param>
        /// <returns></returns>
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

                await this.m_dmtpActor.HandshakeAsync(this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).VerifyToken,
                     this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).Id, timeout, this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).Metadata, token);
            }
            finally
            {
                this.m_semaphoreForConnect.Release();
            }
        }

        #endregion 连接

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

        #region 发送

        /// <summary>
        /// 不允许直接发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public override void Send(byte[] buffer, int offset, int length)
        {
            throw new Exception("不允许直接发送，请指定任意大于0的协议，然后发送。");
        }

        /// <summary>
        /// 不允许直接发送
        /// </summary>
        /// <param name="transferBytes"></param>
        public override void Send(IList<ArraySegment<byte>> transferBytes)
        {
            throw new Exception("不允许直接发送，请指定任意大于0的协议，然后发送。");
        }

        /// <summary>
        /// 不允许直接发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public override Task SendAsync(byte[] buffer, int offset, int length)
        {
            throw new Exception("不允许直接发送，请指定任意大于0的协议，然后发送。");
        }

        /// <summary>
        /// 不允许直接发送
        /// </summary>
        /// <param name="transferBytes"></param>
        public override Task SendAsync(IList<ArraySegment<byte>> transferBytes)
        {
            throw new Exception("不允许直接发送，请指定任意大于0的协议，然后发送。");
        }

        #endregion 发送

        /// <inheritdoc/>
        protected override void LoadConfig(TouchSocketConfig config)
        {
            config.SetTcpDataHandlingAdapter(() => new TcpDmtpAdapter());
            base.LoadConfig(config);
            if (this.Resolver.IsRegistered(typeof(IDmtpRouteService)))
            {
                this.m_allowRoute = true;
                this.m_findDmtpActor = this.Resolver.Resolve<IDmtpRouteService>().FindDmtpActor;
            }
            this.m_dmtpActor = new SealedDmtpActor(this.m_allowRoute)
            {
                OutputSend = this.DmtpActorSend,
                OutputSendAsync = this.DmtpActorSendAsync,
                Routing = this.OnDmtpActorRouting,
                Handshaking = this.OnDmtpActorHandshaking,
                Handshaked = this.OnDmtpActorHandshaked,
                Closed = this.OnDmtpActorClose,
                Logger = this.Logger,
                Client = this,
                FindDmtpActor = this.m_findDmtpActor,
                CreatedChannel = this.OnDmtpActorCreateChannel
            };
        }

        /// <inheritdoc/>
        protected override async Task ReceivedData(ReceivedDataEventArgs e)
        {
            var message = (DmtpMessage)e.RequestInfo;
            if (!await this.m_dmtpActor.InputReceivedData(message).ConfigureFalseAwait())
            {
                await this.PluginManager.RaiseAsync(nameof(IDmtpReceivedPlugin.OnDmtpReceived), this, new DmtpMessageEventArgs(message)).ConfigureFalseAwait();
            }

            await base.ReceivedData(e).ConfigureFalseAwait();
        }

        #region 内部委托绑定

        private void DmtpActorSend(DmtpActor actor, ArraySegment<byte>[] transferBytes)
        {
            base.Send(transferBytes);
        }

        private Task DmtpActorSendAsync(DmtpActor actor, ArraySegment<byte>[] transferBytes)
        {
            return base.SendAsync(transferBytes);
        }

        private Task OnDmtpActorClose(DmtpActor actor, string msg)
        {
            this.BreakOut(false, msg);
            return EasyTask.CompletedTask;
        }

        private Task OnDmtpActorCreateChannel(DmtpActor actor, CreateChannelEventArgs e)
        {
            return this.OnCreatedChannel(e);
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

        #endregion 内部委托绑定

        #region 事件触发

        /// <summary>
        /// 当创建通道
        /// </summary>
        /// <param name="e"></param>
        protected virtual async Task OnCreatedChannel(CreateChannelEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }

            await this.PluginManager.RaiseAsync(nameof(IDmtpCreateChannelPlugin.OnCreateChannel), this, e).ConfigureFalseAwait();
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
            await this.PluginManager.RaiseAsync(nameof(IDmtpHandshakedPlugin.OnDmtpHandshaked), this, e).ConfigureFalseAwait();
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
            await this.PluginManager.RaiseAsync(nameof(IDmtpHandshakingPlugin.OnDmtpHandshaking), this, e).ConfigureFalseAwait();
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
            await this.PluginManager.RaiseAsync(nameof(IDmtpRoutingPlugin.OnDmtpRouting), this, e).ConfigureFalseAwait();
        }

        #endregion 事件触发
    }
}