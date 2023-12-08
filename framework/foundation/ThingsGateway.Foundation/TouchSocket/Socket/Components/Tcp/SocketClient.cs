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
using System.Diagnostics;
using System.Net.Sockets;

namespace ThingsGateway.Foundation.Sockets
{
    /// <summary>
    /// SocketClient
    /// </summary>
    [DebuggerDisplay("Id={Id},IPAdress={IP}:{Port}")]
    public class SocketClient : ConfigObject, ISocketClient
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public SocketClient()
        {
            this.Protocol = Protocol.Tcp;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{nameof(SocketClient)}:{IP}:{Port}";
        }

        #region 变量

        private DelaySender m_delaySender;

        private TcpCore m_tcpCore;

        #endregion 变量

        #region 属性

        /// <inheritdoc/>
        public bool CanSend => this.Online;

        /// <inheritdoc/>
        public virtual bool CanSetDataHandlingAdapter => true;

        /// <inheritdoc/>
        public IResolver Resolver { get; private set; }

        /// <inheritdoc/>
        public SingleStreamDataHandlingAdapter DataHandlingAdapter { get; private set; }

        /// <inheritdoc/>
        public string Id { get; private set; }

        /// <inheritdoc/>
        public string IP { get; private set; }

        /// <inheritdoc/>
        public bool IsClient => false;

        /// <inheritdoc/>
        public DateTime LastReceivedTime => this.GetTcpCore().ReceiveCounter.LastIncrement;

        /// <inheritdoc/>
        public DateTime LastSendTime => this.GetTcpCore().SendCounter.LastIncrement;

        /// <inheritdoc/>
        public TcpListenOption ListenOption { get; private set; }

        /// <inheritdoc/>
        public Socket MainSocket { get; private set; }

        /// <inheritdoc/>
        public bool Online { get; private set; }

        /// <inheritdoc/>
        public IPluginManager PluginManager { get; private set; }

        /// <inheritdoc/>
        public int Port { get; private set; }

        /// <inheritdoc/>
        public Protocol Protocol { get; set; }

        /// <inheritdoc/>
        public TcpServiceBase Service { get; private set; }

        /// <inheritdoc/>
        public string ServiceIP { get; private set; }

        /// <inheritdoc/>
        public int ServicePort { get; private set; }

        /// <inheritdoc/>
        public bool UseSsl { get; private set; }

        /// <inheritdoc/>
        public override TouchSocketConfig Config => this.Service?.Config;

        #endregion 属性

        #region Internal

        internal Task AuthenticateAsync(ServiceSslOption sslOption)
        {
            return this.m_tcpCore.AuthenticateAsync(sslOption);
        }

        internal void BeginReceive()
        {
            try
            {
                this.m_tcpCore.BeginIocpReceive();
            }
            catch (Exception ex)
            {
                this.BreakOut(false, ex.ToString());
            }
        }

        internal Task BeginReceiveSsl()
        {
            return this.m_tcpCore.BeginSslReceive();
        }

        internal Task InternalConnected(ConnectedEventArgs e)
        {
            this.Online = true;
            return this.OnConnected(e);
        }

        internal Task InternalConnecting(ConnectingEventArgs e)
        {
            return this.OnConnecting(e);
        }

        internal Task InternalInitialized()
        {
            return this.OnInitialized();
        }

        internal void InternalSetContainer(IResolver containerProvider)
        {
            this.Resolver = containerProvider;
        }

        internal void InternalSetId(string id)
        {
            this.Id = id;
        }

        internal void InternalSetListenOption(TcpListenOption option)
        {
            this.ListenOption = option;
            //this.ReceiveType = option.ReceiveType;
        }

        internal void InternalSetPluginManager(IPluginManager pluginManager)
        {
            this.PluginManager = pluginManager;
        }

        internal void InternalSetService(TcpServiceBase serviceBase)
        {
            this.Service = serviceBase;
        }

        internal void InternalSetSocket(Socket socket)
        {
            this.MainSocket = socket ?? throw new ArgumentNullException(nameof(socket));
            this.IP = socket.RemoteEndPoint.GetIP();
            this.Port = socket.RemoteEndPoint.GetPort();
            this.ServiceIP = socket.LocalEndPoint.GetIP();
            this.ServicePort = socket.LocalEndPoint.GetPort();

            if (this.Config.GetValue(TouchSocketConfigExtension.DelaySenderProperty) is DelaySenderOption senderOption)
            {
                this.m_delaySender = new DelaySender(senderOption, this.GetTcpCore().Send);
            }

            var tcpCore = this.Service.RentTcpCore();
            tcpCore.Reset(socket);
            tcpCore.OnReceived = this.HandleReceived;
            tcpCore.OnBreakOut = this.TcpCoreBreakOut;
            if (this.Config.GetValue(TouchSocketConfigExtension.MinBufferSizeProperty) is int minValue)
            {
                tcpCore.MinBufferSize = minValue;
            }

            if (this.Config.GetValue(TouchSocketConfigExtension.MaxBufferSizeProperty) is int maxValue)
            {
                tcpCore.MaxBufferSize = maxValue;
            }
            this.m_tcpCore = tcpCore;
        }

        /// <summary>
        /// 中断连接
        /// </summary>
        /// <param name="manual"></param>
        /// <param name="msg"></param>
        protected void BreakOut(bool manual, string msg)
        {
            if (this.GetSocketCliectCollection().TryRemove(this.Id, out _))
            {
                if (this.Online)
                {
                    this.Online = false;
                    this.MainSocket.SafeDispose();
                    this.m_delaySender.SafeDispose();
                    this.DataHandlingAdapter.SafeDispose();
                    this.PrivateOnDisconnected(new DisconnectEventArgs(manual, msg)).GetFalseAwaitResult();
                }
            }
        }

        private void HandleReceived(TcpCore core, ByteBlock byteBlock)
        {
            try
            {
                if (this.DisposedValue)
                {
                    return;
                }

                if (this.ReceivingData(byteBlock).ConfigureAwait(false).GetAwaiter().GetResult())
                {
                    return;
                }

                if (this.DataHandlingAdapter == null)
                {
                    this.Logger.Error(this, TouchSocketResource.NullDataAdapter.GetDescription());
                    return;
                }
                this.DataHandlingAdapter.ReceivedInput(byteBlock);
            }
            catch (Exception ex)
            {
                this.Logger.Log(LogLevel.Error, this, "在处理数据时发生错误", ex);
            }
        }

        private void TcpCoreBreakOut(TcpCore core, bool manual, string msg)
        {
            this.BreakOut(manual, msg);
        }

        #endregion Internal

        #region 事件&委托

        /// <inheritdoc/>
        public DisconnectEventHandler<ITcpClientBase> Disconnected { get; set; }

        /// <inheritdoc/>
        public DisconnectEventHandler<ITcpClientBase> Disconnecting { get; set; }

        /// <summary>
        /// 当客户端完整建立Tcp连接。
        /// </summary>
        /// <param name="e"></param>
        protected virtual async Task OnConnected(ConnectedEventArgs e)
        {
            if (await this.PluginManager.RaiseAsync(nameof(ITcpConnectedPlugin.OnTcpConnected), this, e))
            {
                return;
            }
            await this.Service.OnInternalConnected(this, e);
        }

        /// <summary>
        /// 客户端正在连接。
        /// </summary>
        protected virtual async Task OnConnecting(ConnectingEventArgs e)
        {
            if (await this.PluginManager.RaiseAsync(nameof(ITcpConnectingPlugin.OnTcpConnecting), this, e))
            {
                return;
            }
            await this.Service.OnInternalConnecting(this, e);
        }

        /// <summary>
        /// 客户端已断开连接。
        /// </summary>
        /// <param name="e"></param>
        protected virtual async Task OnDisconnected(DisconnectEventArgs e)
        {
            if (this.Disconnected != null)
            {
                await this.Disconnected.Invoke(this, e);
                if (e.Handled)
                {
                    return;
                }
            }

            if (await this.PluginManager.RaiseAsync(nameof(ITcpDisconnectedPlugin.OnTcpDisconnected), this, e))
            {
                return;
            }
            await this.Service.OnInternalDisconnected(this, e);
        }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// </summary>
        /// <param name="e"></param>
        protected virtual async Task OnDisconnecting(DisconnectEventArgs e)
        {
            try
            {
                if (this.Disconnecting != null)
                {
                    await this.Disconnecting.Invoke(this, e);
                    if (e.Handled)
                    {
                        return;
                    }
                }

                if (await this.PluginManager.RaiseAsync(nameof(ITcpDisconnectingPlugin.OnTcpDisconnecting), this, e))
                {
                    return;
                }
                await this.Service.OnInternalDisconnecting(this, e);
            }
            catch (Exception ex)
            {
                this.Logger.Log(LogLevel.Error, this, $"在事件{nameof(this.Disconnecting)}中发生错误。", ex);
            }
        }

        /// <summary>
        /// 当初始化完成时，执行在<see cref="OnConnecting(ConnectingEventArgs)"/>之前。
        /// </summary>
        protected virtual Task OnInitialized()
        {
            return EasyTask.CompletedTask;
        }

        private async Task PrivateOnDisconnected(object obj)
        {
            this.m_receiver?.TryInputReceive(default, default);
            var e = (DisconnectEventArgs)obj;
            try
            {
                await this.OnDisconnected(e);
            }
            catch (Exception)
            {
            }
            finally
            {
                var tcp = this.m_tcpCore;
                this.m_tcpCore = null;
                this.Service.ReturnTcpCore(tcp);
                base.Dispose(true);
            }
        }

        private Task PrivateOnDisconnecting(object obj)
        {
            return this.OnDisconnecting((DisconnectEventArgs)obj);
        }

        #endregion 事件&委托

        /// <inheritdoc/>
        public virtual void Close(string msg = TouchSocketCoreUtility.Empty)
        {
            lock (this.GetTcpCore())
            {
                if (this.Online)
                {
                    this.PrivateOnDisconnecting(new DisconnectEventArgs(true, msg)).GetFalseAwaitResult();
                    this.MainSocket.TryClose();
                    this.BreakOut(true, msg);
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="newId"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ClientNotFindException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual void ResetId(string newId)
        {
            this.DirectResetId(newId);
        }

        /// <inheritdoc/>
        public virtual void SetDataHandlingAdapter(SingleStreamDataHandlingAdapter adapter)
        {
            this.ThrowIfDisposed();
            if (!this.CanSetDataHandlingAdapter)
            {
                throw new Exception($"不允许自由调用{nameof(SetDataHandlingAdapter)}进行赋值。");
            }

            this.SetAdapter(adapter);
        }

        /// <summary>
        /// 直接重置内部Id。
        /// </summary>
        /// <param name="newId"></param>
        protected void DirectResetId(string newId)
        {
            this.ThrowIfDisposed();
            if (string.IsNullOrEmpty(newId))
            {
                throw new ArgumentException($"“{nameof(newId)}”不能为 null 或空。", nameof(newId));
            }

            if (this.Id == newId)
            {
                return;
            }
            var oldId = this.Id;
            if (this.GetSocketCliectCollection().TryRemove(this.Id, out SocketClient socketClient))
            {
                socketClient.Id = newId;
                if (this.GetSocketCliectCollection().TryAdd(socketClient))
                {
                    this.IdChanged(oldId, newId).ConfigureAwait(false).GetAwaiter().GetResult();
                    return;
                }
                else
                {
                    socketClient.Id = oldId;
                    if (this.GetSocketCliectCollection().TryAdd(socketClient))
                    {
                        throw new Exception("Id重复");
                    }
                    else
                    {
                        socketClient.Close("修改新Id时操作失败，且回退旧Id时也失败。");
                    }
                }
            }
            else
            {
                throw new ClientNotFindException(TouchSocketResource.ClientNotFind.GetDescription(oldId));
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            lock (this.GetTcpCore())
            {
                if (this.Online)
                {
                    this.PrivateOnDisconnecting(new DisconnectEventArgs(true, $"{nameof(Dispose)}主动断开")).GetFalseAwaitResult();
                    this.BreakOut(true, $"{nameof(Dispose)}主动断开");
                }
            }
        }

        /// <summary>
        /// 当Id更新的时候触发
        /// </summary>
        /// <param name="oldId"></param>
        /// <param name="newId"></param>
        /// <returns></returns>
        protected async Task IdChanged(string oldId, string newId)
        {
            await this.PluginManager.RaiseAsync(nameof(IIdChangedPlugin.OnIdChanged), this, new IdChangedEventArgs(oldId, newId));
        }

        /// <summary>
        /// 当收到适配器处理的数据时。
        /// </summary>
        /// <returns>如果返回<see langword="true"/>则表示数据已被处理，且不会再向下传递。</returns>
        protected virtual async Task ReceivedData(ReceivedDataEventArgs e)
        {
            await this.PluginManager.RaiseAsync(nameof(ITcpReceivedPlugin.OnTcpReceived), this, e);

            if (e.Handled)
            {
                return;
            }

            await this.Service.OnInternalReceivedData(this, e);
        }

        /// <summary>
        /// 当收到原始数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <returns>如果返回<see langword="true"/>则表示数据已被处理，且不会再向下传递。</returns>
        protected virtual Task<bool> ReceivingData(ByteBlock byteBlock)
        {
            if (this.PluginManager.GetPluginCount(nameof(ITcpReceivingPlugin.OnTcpReceiving)) > 0)
            {
                return this.PluginManager.RaiseAsync(nameof(ITcpReceivingPlugin.OnTcpReceiving), this, new ByteBlockEventArgs(byteBlock));
            }
            return Task.FromResult(false);
        }

        /// <summary>
        /// 当即将发送时，如果覆盖父类方法，则不会触发插件。
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <returns>返回值表示是否允许发送</returns>
        protected virtual async Task<bool> SendingData(byte[] buffer, int offset, int length)
        {
            if (this.PluginManager.GetPluginCount(nameof(ITcpSendingPlugin.OnTcpSending)) > 0)
            {
                var args = new SendingEventArgs(buffer, offset, length);
                await this.PluginManager.RaiseAsync(nameof(ITcpSendingPlugin.OnTcpSending), this, args).ConfigureAwait(false);
                return args.IsPermitOperation;
            }
            return true;
        }

        /// <summary>
        /// 设置适配器，该方法不会检验<see cref="CanSetDataHandlingAdapter"/>的值。
        /// </summary>
        /// <param name="adapter"></param>
        protected void SetAdapter(SingleStreamDataHandlingAdapter adapter)
        {
            if (adapter is null)
            {
                throw new ArgumentNullException(nameof(adapter));
            }

            if (this.Config != null)
            {
                adapter.Config(this.Config);
            }

            adapter.Logger = this.Logger;
            adapter.OnLoaded(this);
            adapter.ReceivedCallBack = this.PrivateHandleReceivedData;
            adapter.SendCallBack = this.DefaultSend;
            adapter.SendAsyncCallBack = this.DefaultSendAsync;
            this.DataHandlingAdapter = adapter;
        }

        private SocketClientCollection GetSocketCliectCollection()
        {
            return this.Service.SocketClients as SocketClientCollection;
        }

        private TcpCore GetTcpCore()
        {
            this.ThrowIfDisposed();
            return this.m_tcpCore ?? throw new ObjectDisposedException(this.GetType().Name);
        }

        private void PrivateHandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            if (this.m_receiver != null)
            {
                if (this.m_receiver.TryInputReceive(byteBlock, requestInfo))
                {
                    return;
                }
            }
            this.ReceivedData(new ReceivedDataEventArgs(byteBlock, requestInfo)).GetFalseAwaitResult();
        }

        #region 发送

        /// <inheritdoc/>
        public void DefaultSend(byte[] buffer, int offset, int length)
        {
            if (this.SendingData(buffer, offset, length).GetFalseAwaitResult())
            {
                if (this.m_delaySender != null)
                {
                    this.m_delaySender.Send(new QueueDataBytes(buffer, offset, length));
                    return;
                }
                this.GetTcpCore().Send(buffer, offset, length);
            }
        }

        /// <inheritdoc/>
        public async Task DefaultSendAsync(byte[] buffer, int offset, int length)
        {
            if (await this.SendingData(buffer, offset, length))
            {
                await this.GetTcpCore().SendAsync(buffer, offset, length);
            }
        }

        #region 同步发送

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="requestInfo"></param>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual void Send(IRequestInfo requestInfo)
        {
            this.ThrowIfDisposed();
            if (this.DataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketResource.NullDataAdapter.GetDescription());
            }
            if (!this.DataHandlingAdapter.CanSendRequestInfo)
            {
                throw new NotSupportedException($"当前适配器不支持对象发送。");
            }
            this.DataHandlingAdapter.SendInput(requestInfo);
        }

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual void Send(byte[] buffer, int offset, int length)
        {
            this.ThrowIfDisposed();
            if (this.DataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketResource.NullDataAdapter.GetDescription());
            }
            this.DataHandlingAdapter.SendInput(buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        public virtual void Send(IList<ArraySegment<byte>> transferBytes)
        {
            this.ThrowIfDisposed();
            if (this.DataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketResource.NullDataAdapter.GetDescription());
            }
            if (this.DataHandlingAdapter.CanSplicingSend)
            {
                this.DataHandlingAdapter.SendInput(transferBytes);
            }
            else
            {
                var length = 0;
                foreach (var item in transferBytes)
                {
                    length += item.Count;
                }
                using (var byteBlock = new ByteBlock(length))
                {
                    foreach (var item in transferBytes)
                    {
                        byteBlock.Write(item.Array, item.Offset, item.Count);
                    }
                    this.DataHandlingAdapter.SendInput(byteBlock.Buffer, 0, byteBlock.Len);
                }
            }
        }

        #endregion 同步发送

        #region 异步发送

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual Task SendAsync(byte[] buffer, int offset, int length)
        {
            this.ThrowIfDisposed();
            if (this.DataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketResource.NullDataAdapter.GetDescription());
            }
            return this.DataHandlingAdapter.SendInputAsync(buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="requestInfo"></param>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual Task SendAsync(IRequestInfo requestInfo)
        {
            this.ThrowIfDisposed();
            if (this.DataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketResource.NullDataAdapter.GetDescription());
            }
            if (!this.DataHandlingAdapter.CanSendRequestInfo)
            {
                throw new NotSupportedException($"当前适配器不支持对象发送。");
            }
            return this.DataHandlingAdapter.SendInputAsync(requestInfo);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Task SendAsync(IList<ArraySegment<byte>> transferBytes)
        {
            this.ThrowIfDisposed();
            if (this.DataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketResource.NullDataAdapter.GetDescription());
            }
            if (this.DataHandlingAdapter.CanSplicingSend)
            {
                return this.DataHandlingAdapter.SendInputAsync(transferBytes);
            }
            else
            {
                var length = 0;
                foreach (var item in transferBytes)
                {
                    length += item.Count;
                }
                using (var byteBlock = new ByteBlock(length))
                {
                    foreach (var item in transferBytes)
                    {
                        byteBlock.Write(item.Array, item.Offset, item.Count);
                    }
                    return this.DataHandlingAdapter.SendInputAsync(byteBlock.Buffer, 0, byteBlock.Len);
                }
            }
        }

        #endregion 异步发送

        #region Id发送

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="id">用于检索TcpSocketClient</param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public void Send(string id, byte[] buffer, int offset, int length)
        {
            this.Service.Send(id, buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="requestInfo"></param>
        public void Send(string id, IRequestInfo requestInfo)
        {
            this.Service.Send(id, requestInfo);
        }

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="id">用于检索TcpSocketClient</param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public Task SendAsync(string id, byte[] buffer, int offset, int length)
        {
            return this.Service.SendAsync(id, buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="requestInfo"></param>
        public Task SendAsync(string id, IRequestInfo requestInfo)
        {
            return this.Service.SendAsync(id, requestInfo);
        }

        #endregion Id发送

        #endregion 发送

        #region Receiver

        private Receiver m_receiver;

        /// <inheritdoc/>
        public void ClearReceiver()
        {
            this.m_receiver = null;
        }

        /// <inheritdoc/>
        public IReceiver CreateReceiver()
        {
            return this.m_receiver ??= new Receiver(this);
        }

        #endregion
    }
}