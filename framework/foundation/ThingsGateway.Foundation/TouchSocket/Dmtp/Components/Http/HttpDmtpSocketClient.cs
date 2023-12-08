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
    /// Http服务器辅助类
    /// </summary>
    public class HttpDmtpSocketClient : HttpSocketClient, IHttpDmtpSocketClient
    {
        internal Func<DmtpActor> m_internalOnRpcActorInit;
        private DmtpActor m_dmtpActor;

        /// <inheritdoc/>
        public IDmtpActor DmtpActor { get => this.m_dmtpActor; }

        /// <inheritdoc/>
        public bool IsHandshaked => this.DmtpActor != null && this.DmtpActor.IsHandshaked;

        /// <summary>
        /// 验证超时时间
        /// </summary>
        public TimeSpan VerifyTimeout => this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).VerifyTimeout;

        /// <summary>
        /// 连接令箭
        /// </summary>
        public string VerifyToken => this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).VerifyToken;

        #region 断开

        /// <inheritdoc/>
        public override void Close(string msg = "")
        {
            if (this.m_dmtpActor != null)
            {
                this.m_dmtpActor.SendClose(msg);
                this.m_dmtpActor.Close(msg);
            }
            base.Close(msg);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            this.m_dmtpActor?.SafeDispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="e"></param>
        protected override async Task OnDisconnected(DisconnectEventArgs e)
        {
            this.m_dmtpActor?.Close(e.Message);
            await base.OnDisconnected(e);
        }

        #endregion 断开

        #region ResetId

        /// <inheritdoc cref="IDmtpActor.ResetId(string)"/>
        public override void ResetId(string newId)
        {
            if (this.m_dmtpActor == null)
            {
                base.ResetId(newId);
                return;
            }
            this.m_dmtpActor.ResetId(newId);
        }

        ///<inheritdoc cref="IDmtpActor.ResetIdAsync(string)"/>
        public async Task ResetIdAsync(string newId)
        {
            if (this.m_dmtpActor == null)
            {
                base.ResetId(newId);
                return;
            }
            await this.m_dmtpActor.ResetIdAsync(newId);
        }

        #endregion ResetId

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="request"></param>
        protected override async Task OnReceivedHttpRequest(HttpRequest request)
        {
            if (request.IsMethod(DmtpUtility.Dmtp) && request.IsUpgrade() &&
                string.Equals(request.Headers.Get(HttpHeaders.Upgrade), DmtpUtility.Dmtp, StringComparison.OrdinalIgnoreCase))
            {
                request.SafeDispose();

                this.SetRpcActor(this.m_internalOnRpcActorInit.Invoke());
                this.DefaultSend(new HttpResponse().SetStatus(101, "Switching Protocols").BuildAsBytes());
                return;
            }
            await base.OnReceivedHttpRequest(request);
        }

        /// <inheritdoc/>
        protected override async Task ReceivedData(ReceivedDataEventArgs e)
        {
            if (this.Protocol == DmtpUtility.DmtpProtocol && e.RequestInfo is DmtpMessage message)
            {
                if (!await this.m_dmtpActor.InputReceivedData(message))
                {
                    await this.PluginManager.RaiseAsync(nameof(IDmtpReceivedPlugin.OnDmtpReceived), this, new DmtpMessageEventArgs(message));
                }
            }
            await base.ReceivedData(e);
        }

        private Task OnDmtpIdChanged(DmtpActor actor, IdChangedEventArgs e)
        {
            this.DirectResetId(e.NewId);
            return EasyTask.CompletedTask;
        }

        private void SetRpcActor(DmtpActor actor)
        {
            actor.Id = this.Id;
            actor.IdChanged = this.OnDmtpIdChanged;
            actor.OutputSendAsync = this.ThisDmtpActorOutputSendAsync;
            actor.OutputSend = this.ThisDmtpActorOutputSend;
            actor.Client = this;
            actor.Closed = this.OnDmtpActorClose;
            actor.Routing = this.OnDmtpActorRouting;
            actor.Handshaked = this.OnDmtpActorHandshaked;
            actor.Handshaking = this.OnDmtpActorHandshaking;
            actor.CreatedChannel = this.OnDmtpActorCreatedChannel;
            actor.Logger = this.Logger;
            this.m_dmtpActor = actor;

            this.Protocol = DmtpUtility.DmtpProtocol;
            this.SetDataHandlingAdapter(new TcpDmtpAdapter());
        }

        private void ThisDmtpActorOutputSend(DmtpActor actor, ArraySegment<byte>[] transferBytes)
        {
            base.Send(transferBytes);
        }

        private Task ThisDmtpActorOutputSendAsync(DmtpActor actor, ArraySegment<byte>[] transferBytes)
        {
            return base.SendAsync(transferBytes);
        }

        #region 内部委托绑定

        private Task OnDmtpActorClose(DmtpActor actor, string msg)
        {
            base.BreakOut(false, msg);
            return EasyTask.CompletedTask;
        }

        private Task OnDmtpActorCreatedChannel(DmtpActor actor, CreateChannelEventArgs e)
        {
            return this.OnCreatedChannel(e);
        }

        private Task OnDmtpActorHandshaked(DmtpActor actor, DmtpVerifyEventArgs e)
        {
            return this.OnHandshaked(e);
        }

        private Task OnDmtpActorHandshaking(DmtpActor actor, DmtpVerifyEventArgs e)
        {
            if (e.Token == this.VerifyToken)
            {
                e.IsPermitOperation = true;
            }
            else
            {
                e.Message = "Token不受理";
            }

            return this.OnHandshaking(e);
        }

        private Task OnDmtpActorRouting(DmtpActor actor, PackageRouterEventArgs e)
        {
            return this.OnRouting(e);
        }

        #endregion 内部委托绑定

        #region 事件

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
        /// 在验证Token时
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
        /// 在需要转发路由包时。
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

        #endregion 事件

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
    }
}