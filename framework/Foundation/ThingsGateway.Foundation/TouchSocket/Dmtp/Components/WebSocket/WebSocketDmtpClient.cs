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
using System.Net.WebSockets;
/* 项目“ThingsGateway.Foundation (netstandard2.0)”的未合并的更改
在此之前:
using System.Threading.Tasks;
using ThingsGateway.Foundation.Core;
在此之后:
using System.Threading.Tasks;

using ThingsGateway.Foundation.Core;
*/

/* 项目“ThingsGateway.Foundation (net45)”的未合并的更改
在此之前:
using System.Threading.Tasks;
using ThingsGateway.Foundation.Core;
在此之后:
using System.Threading.Tasks;

using ThingsGateway.Foundation.Core;
*/

/* 项目“ThingsGateway.Foundation (net6.0)”的未合并的更改
在此之前:
using System.Threading.Tasks;
using ThingsGateway.Foundation.Core;
在此之后:
using System.Threading.Tasks;

using ThingsGateway.Foundation.Core;
*/


namespace ThingsGateway.Foundation.Dmtp
{
    /// <summary>
    /// WebSocketDmtpClient
    /// </summary>
    public class WebSocketDmtpClient : BaseSocket, IWebSocketDmtpClient
    {
        /// <summary>
        /// WebSocketDmtpClient
        /// </summary>
        public WebSocketDmtpClient()
        {
            this.m_receiveCounter = new ValueCounter
            {
                Period = TimeSpan.FromSeconds(1),
                OnPeriod = this.OnReceivePeriod
            };
            this.m_sendCounter = new ValueCounter
            {
                Period = TimeSpan.FromSeconds(1),
                OnPeriod = this.OnSendPeriod
            };
        }

        #region 字段

        private readonly SemaphoreSlim m_semaphoreForConnect = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim m_semaphoreForSend = new SemaphoreSlim(1, 1);
        private ClientWebSocket m_client;
        private SealedDmtpActor m_dmtpActor;
        private Func<string, Task<IDmtpActor>> m_findDmtpActor;
        private int m_receiveBufferSize = 1024 * 10;
        private ValueCounter m_receiveCounter;
        private int m_sendBufferSize = 1024 * 10;
        private ValueCounter m_sendCounter;
        private TcpDmtpAdapter m_dmtpAdapter;

        #endregion 字段

        /// <inheritdoc/>
        public bool CanSend => this.m_client.State == WebSocketState.Open;

        /// <summary>
        /// 客户端配置
        /// </summary>
        public TouchSocketConfig Config { get; private set; }

        /// <inheritdoc/>
        public IContainer Container { get; private set; }

        /// <summary>
        /// 断开连接
        /// </summary>
        public DisconnectEventHandler<WebSocketDmtpClient> Disconnected { get; set; }

        /// <inheritdoc/>
        public IDmtpActor DmtpActor { get => this.m_dmtpActor; }

        /// <inheritdoc/>
        public string Id => this.m_dmtpActor?.Id;

        /// <inheritdoc/>
        public bool IsHandshaked { get; private set; }

        /// <inheritdoc/>
        public DateTime LastReceivedTime => this.m_receiveCounter.LastIncrement;

        /// <inheritdoc/>
        public DateTime LastSendTime => this.m_sendCounter.LastIncrement;

        /// <inheritdoc/>
        public IPluginsManager PluginsManager { get; private set; }

        /// <inheritdoc/>
        public Protocol Protocol { get; set; } = DmtpUtility.DmtpProtocol;

        /// <inheritdoc/>
        public override int ReceiveBufferSize => this.m_receiveBufferSize;

        /// <inheritdoc/>
        public IPHost RemoteIPHost { get; private set; }

        /// <inheritdoc/>
        public override int SendBufferSize => this.m_sendBufferSize;

        /// <summary>
        /// 发送<see cref="IDmtpActor"/>关闭消息。
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public void Close(string msg = "")
        {
            this.m_dmtpActor.SendClose(msg);
            this.m_dmtpActor.Close(msg);
            this.PrivateClose(msg);
        }

        /// <inheritdoc/>
        public Task ConnectAsync(int timeout = 5000)
        {
            return this.ConnectAsync(CancellationToken.None, timeout);
        }

        /// <inheritdoc/>
        public async Task ConnectAsync(CancellationToken token, int timeout = 5000)
        {
            try
            {
                await this.m_semaphoreForConnect.WaitAsync();
                if (this.IsHandshaked)
                {
                    return;
                }

                if (this.m_client == null || this.m_client.State != WebSocketState.Open)
                {
                    this.m_client.SafeDispose();
                    this.m_client = new ClientWebSocket();
                    await this.m_client.ConnectAsync(this.RemoteIPHost, token);

                    this.m_dmtpActor = new SealedDmtpActor(false)
                    {
                        OutputSend = this.OnDmtpActorSend,
                        OutputSendAsync = this.OnDmtpActorSendAsync,
                        Routing = this.OnDmtpActorRouting,
                        Handshaking = this.OnDmtpActorHandshaking,
                        Handshaked = this.OnDmtpActorHandshaked,
                        Closed = this.OnDmtpActorClose,
                        Logger = this.Logger,
                        Client = this,
                        FindDmtpActor = this.m_findDmtpActor,
                        CreatedChannel = this.OnDmtpActorCreateChannel
                    }; ;

                    this.m_dmtpAdapter = new TcpDmtpAdapter()
                    {
                        ReceivedCallBack = this.PrivateHandleReceivedData
                    };
                    _ = this.BeginReceive();
                }

                this.m_dmtpActor.Handshake(this.Config.GetValue(DmtpConfigExtension.VerifyTokenProperty),
                    this.Config.GetValue(DmtpConfigExtension.DefaultIdProperty),
                    timeout, this.Config.GetValue(DmtpConfigExtension.MetadataProperty), token);
                this.IsHandshaked = true;
            }
            finally
            {
                this.m_semaphoreForConnect.Release();
            }
        }

        /// <inheritdoc/>
        public bool Ping(string targetId, int timeout = 5000)
        {
            return this.DmtpActor.Ping(targetId, timeout);
        }

        /// <inheritdoc/>
        public bool Ping(int timeout = 5000)
        {
            return this.DmtpActor.Ping(timeout);
        }

        /// <inheritdoc/>
        public void ResetId(string newId)
        {
            this.DmtpActor.ResetId(newId);
        }

        /// <summary>
        /// 配置
        /// </summary>
        /// <param name="config"></param>
        /// <exception cref="Exception"></exception>
        public IWebSocketDmtpClient Setup(TouchSocketConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            this.ThrowIfDisposed();

            this.BuildConfig(config);

            this.PluginsManager.Raise(nameof(ILoadingConfigPlugin.OnLoadingConfig), this, new ConfigEventArgs(config));
            this.LoadConfig(this.Config);
            this.PluginsManager.Raise(nameof(ILoadedConfigPlugin.OnLoadedConfig), this, new ConfigEventArgs(config));

            return this;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (this.DisposedValue)
            {
                return;
            }
            if (disposing)
            {
                this.BreakOut($"调用{nameof(Dispose)}", true);
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="config"></param>
        protected virtual void LoadConfig(TouchSocketConfig config)
        {
            this.RemoteIPHost = config.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty);
            this.Logger ??= this.Container.Resolve<ILog>();

            if (this.Container.IsRegistered(typeof(IDmtpRouteService)))
            {
                this.m_findDmtpActor = this.Container.Resolve<IDmtpRouteService>().FindDmtpActor;
            }
        }

        /// <summary>
        /// 已断开连接。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDisconnected(DisconnectEventArgs e)
        {
            if (this.PluginsManager.Raise(nameof(ITcpDisconnectedPlugin.OnTcpDisconnected), this, e))
            {
                return;
            }
            this.Disconnected?.Invoke(this, e);
        }

        private async Task BeginReceive()
        {
            try
            {
                while (true)
                {
                    using (var byteBlock = new ByteBlock(this.ReceiveBufferSize))
                    {
                        var result = await this.m_client.ReceiveAsync(new ArraySegment<byte>(byteBlock.Buffer, 0, byteBlock.Capacity), default);
                        if (result.Count == 0)
                        {
                            break;
                        }
                        byteBlock.SetLength(result.Count);
                        this.m_receiveCounter.Increment(result.Count);

                        this.m_dmtpAdapter.ReceivedInput(byteBlock);
                    }
                }

                this.BreakOut("远程终端主动关闭", false);
            }
            catch (Exception ex)
            {
                this.BreakOut(ex.Message, false);
            }
        }

        private void BreakOut(string msg, bool manual)
        {
            lock (this.SyncRoot)
            {
                if (this.IsHandshaked)
                {
                    this.IsHandshaked = false;
                    this.m_client.CloseAsync(WebSocketCloseStatus.NormalClosure, msg, CancellationToken.None);
                    this.m_client.SafeDispose();
                    this.DmtpActor.SafeDispose();
                    this.m_dmtpAdapter.SafeDispose();
                    this.OnDisconnected(new DisconnectEventArgs(manual, msg));
                }
            }
        }

        private void BuildConfig(TouchSocketConfig config)
        {
            this.Config = config;

            if (!(config.GetValue(TouchSocketCoreConfigExtension.ContainerProperty) is IContainer container))
            {
                container = new Container();
            }

            if (!container.IsRegistered(typeof(ILog)))
            {
                container.RegisterSingleton<ILog, LoggerGroup>();
            }

            if (!(config.GetValue(TouchSocketCoreConfigExtension.PluginsManagerProperty) is IPluginsManager pluginsManager))
            {
                pluginsManager = new PluginsManager(container);
            }

            if (container.IsRegistered(typeof(IPluginsManager)))
            {
                pluginsManager = container.Resolve<IPluginsManager>();
            }
            else
            {
                container.RegisterSingleton<IPluginsManager>(pluginsManager);
            }

            if (config.GetValue(TouchSocketCoreConfigExtension.ConfigureContainerProperty) is Action<IContainer> actionContainer)
            {
                actionContainer.Invoke(container);
            }

            if (config.GetValue(TouchSocketCoreConfigExtension.ConfigurePluginsProperty) is Action<IPluginsManager> actionPluginsManager)
            {
                pluginsManager.Enable = true;
                actionPluginsManager.Invoke(pluginsManager);
            }
            this.Container = container;
            this.PluginsManager = pluginsManager;
        }

        private void OnReceivePeriod(long value)
        {
            this.m_receiveBufferSize = TouchSocketUtility.HitBufferLength(value);
        }

        private void OnSendPeriod(long value)
        {
            this.m_sendBufferSize = TouchSocketUtility.HitBufferLength(value);
        }

        private void PrivateClose(string msg)
        {
            this.BreakOut($"调用{nameof(Close)}", true);
        }

        private void PrivateHandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            var message = (DmtpMessage)requestInfo;
            if (!this.m_dmtpActor.InputReceivedData(message).GetFalseAwaitResult())
            {
                this.PluginsManager.Raise(nameof(IDmtpReceivedPlugin.OnDmtpReceived), this, new DmtpMessageEventArgs(message));
            }
        }

        #region 内部委托绑定

        private Task OnDmtpActorClose(DmtpActor actor, string arg2)
        {
            this.PrivateClose(arg2);
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

        private void OnDmtpActorSend(DmtpActor actor, ArraySegment<byte>[] transferBytes)
        {
            try
            {
                this.m_semaphoreForSend.Wait();
                for (var i = 0; i < transferBytes.Length; i++)
                {
                    Task task;
                    if (i == transferBytes.Length - 1)
                    {
                        task = this.m_client.SendAsync(transferBytes[i], WebSocketMessageType.Binary, true, CancellationToken.None);
                    }
                    else
                    {
                        task = this.m_client.SendAsync(transferBytes[i], WebSocketMessageType.Binary, false, CancellationToken.None);
                    }
                    task.GetFalseAwaitResult();
                    this.m_sendCounter.Increment(transferBytes[i].Count);
                }
            }
            finally
            {
                this.m_semaphoreForSend.Release();
            }
        }

        private async Task OnDmtpActorSendAsync(DmtpActor actor, ArraySegment<byte>[] transferBytes)
        {
            try
            {
                await this.m_semaphoreForSend.WaitAsync();
                for (var i = 0; i < transferBytes.Length; i++)
                {
                    if (i == transferBytes.Length - 1)
                    {
                        await this.m_client.SendAsync(transferBytes[i], WebSocketMessageType.Binary, true, CancellationToken.None);
                    }
                    else
                    {
                        await this.m_client.SendAsync(transferBytes[i], WebSocketMessageType.Binary, false, CancellationToken.None);
                    }
                    this.m_sendCounter.Increment(transferBytes[i].Count);
                }
            }
            finally
            {
                this.m_semaphoreForSend.Release();
            }

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

            await this.PluginsManager.RaiseAsync(nameof(IDmtpCreateChannelPlugin.OnCreateChannel), this, e);
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
            await this.PluginsManager.RaiseAsync(nameof(IDmtpHandshakedPlugin.OnDmtpHandshaked), this, e);
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
            await this.PluginsManager.RaiseAsync(nameof(IDmtpHandshakingPlugin.OnDmtpHandshaking), this, e);
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
            await this.PluginsManager.RaiseAsync(nameof(IDmtpRoutingPlugin.OnDmtpRouting), this, e);
        }

        #endregion 事件触发

        #region Receiver

        /// <summary>
        /// 不支持该功能
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public void ClearReceiver()
        {
            throw new NotSupportedException("不支持该功能");
        }

        /// <summary>
        /// 不支持该功能
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public IReceiver CreateReceiver()
        {
            throw new NotSupportedException("不支持该功能");
        }

        #endregion Receiver
    }
}