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
using System.Net.Security;
using System.Net.Sockets;

namespace ThingsGateway.Foundation.Sockets
{
    /// <summary>
    /// SocketClient
    /// </summary>
    [DebuggerDisplay("Id={Id},IPAdress={IP}:{Port}")]
    public class SocketClient : BaseSocket, ISocketClient
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public SocketClient()
        {
            this.Protocol = Protocol.Tcp;
            this.m_receiveCounter = new ValueCounter
            {
                Period = TimeSpan.FromSeconds(1),
                OnPeriod = this.OnReceivePeriod
            };
            m_sendCounter = new ValueCounter
            {
                Period = TimeSpan.FromSeconds(1),
                OnPeriod = this.OnSendPeriod
            };
        }

        #region 变量
        private ValueCounter m_receiveCounter;
        private ValueCounter m_sendCounter;
        private long m_bufferRate = 1;
        private DelaySender m_delaySender;
        private Stream m_workStream;

        #endregion 变量

        #region 属性

        /// <inheritdoc/>
        public bool CanSend => this.Online;

        /// <inheritdoc/>
        public virtual bool CanSetDataHandlingAdapter => true;

        /// <inheritdoc/>
        public TouchSocketConfig Config { get; private set; }

        /// <inheritdoc/>
        public IContainer Container { get; private set; }

        /// <inheritdoc/>
        public SingleStreamDataHandlingAdapter DataHandlingAdapter { get; private set; }

        /// <inheritdoc/>
        public string Id { get; private set; }

        /// <inheritdoc/>
        public string IP { get; private set; }

        /// <inheritdoc/>
        public bool IsClient => false;

        /// <inheritdoc/>
        public Socket MainSocket { get; private set; }

        /// <inheritdoc/>
        public bool Online { get; private set; }

        /// <inheritdoc/>
        public IPluginsManager PluginsManager { get; private set; }

        /// <inheritdoc/>
        public int Port { get; private set; }

        /// <inheritdoc/>
        public Protocol Protocol { get; set; }

        /// <inheritdoc/>
        public TcpListenOption ListenOption { get; private set; }

        /// <inheritdoc/>
        public ReceiveType ReceiveType { get; private set; }

        /// <inheritdoc/>
        public TcpServiceBase Service { get; private set; }

        /// <inheritdoc/>
        public bool UseSsl { get; private set; }

        /// <inheritdoc/>
        public DateTime LastReceivedTime => this.m_receiveCounter.LastIncrement;

        /// <inheritdoc/>
        public DateTime LastSendTime => this.m_sendCounter.LastIncrement;

        /// <inheritdoc/>
        public Func<ByteBlock, bool> OnHandleRawBuffer { get; set; }

        /// <inheritdoc/>
        public Func<ByteBlock, IRequestInfo, bool> OnHandleReceivedData { get; set; }

        /// <inheritdoc/>
        public string ServiceIP { get; private set; }

        /// <inheritdoc/>
        public int ServicePort { get; private set; }

        #endregion 属性

        #region Internal

        internal void BeginReceive()
        {
            try
            {
                if (this.ReceiveType == ReceiveType.Iocp)
                {
                    var eventArgs = new SocketAsyncEventArgs();
                    eventArgs.Completed += this.EventArgs_Completed;
                    var byteBlock = BytePool.Default.GetByteBlock(this.ReceiveBufferSize);
                    eventArgs.UserToken = byteBlock;
                    eventArgs.SetBuffer(byteBlock.Buffer, 0, byteBlock.Capacity);
                    if (!this.MainSocket.ReceiveAsync(eventArgs))
                    {
                        this.ProcessReceived(eventArgs);
                    }
                }
            }
            catch (Exception ex)
            {
                this.BreakOut(ex.Message, false);
            }
        }

        internal void BeginReceiveSsl(ServiceSslOption sslOption)
        {
            var sslStream = (sslOption.CertificateValidationCallback != null) ? new SslStream(new NetworkStream(this.MainSocket, false), false, sslOption.CertificateValidationCallback) : new SslStream(new NetworkStream(this.MainSocket, false), false);
            sslStream.AuthenticateAsServer(sslOption.Certificate);
            this.m_workStream = sslStream;
            this.UseSsl = true;
            if (this.ReceiveType == ReceiveType.Iocp)
            {
                this.BeginSsl();
            }
        }

        internal void InternalConnected(ConnectedEventArgs e)
        {
            this.Online = true;

            this.OnConnected(e);

            if (e.Handled)
            {
                return;
            }


            if (this.PluginsManager.Enable && this.PluginsManager.Raise(nameof(ITcpConnectedPlugin.OnTcpConnected), this, e))
            {
                return;
            }
        }

        internal void InternalConnecting(ConnectingEventArgs e)
        {
            this.OnConnecting(e);
            if (e.Handled)
            {
                return;
            }
            if (this.PluginsManager.Enable && this.PluginsManager.Raise(nameof(ITcpConnectingPlugin.OnTcpConnecting), this, e))
            {
                return;
            }
        }

        internal void InternalInitialized()
        {
            this.OnInitialized();
        }

        internal void InternalSetConfig(TouchSocketConfig config)
        {
            this.Config = config;
        }

        internal void InternalSetContainer(IContainer container)
        {
            this.Container = container;
            this.Logger ??= container.Resolve<ILog>();
        }

        internal void InternalSetId(string id)
        {
            this.Id = id;
        }

        internal void InternalSetPluginsManager(IPluginsManager pluginsManager)
        {
            this.PluginsManager = pluginsManager;
        }

        internal void InternalSetListenOption(TcpListenOption option)
        {
            this.ListenOption = option;
            this.ReceiveType = option.ReceiveType;
        }

        internal void InternalSetService(TcpServiceBase serviceBase)
        {
            this.Service = serviceBase;
        }

        internal void InternalSetSocket(Socket mainSocket)
        {
            this.MainSocket = mainSocket ?? throw new ArgumentNullException(nameof(mainSocket));
            this.IP = mainSocket.RemoteEndPoint.GetIP();
            this.Port = mainSocket.RemoteEndPoint.GetPort();
            this.ServiceIP = mainSocket.LocalEndPoint.GetIP();
            this.ServicePort = mainSocket.LocalEndPoint.GetPort();

            if (this.Config.GetValue(TouchSocketConfigExtension.DelaySenderProperty) is DelaySenderOption senderOption)
            {
                this.m_delaySender = new DelaySender(senderOption, this.MainSocket.AbsoluteSend);
            }
        }

        #endregion Internal

        #region 事件&委托

        /// <inheritdoc/>
        public DisconnectEventHandler<ITcpClientBase> Disconnected { get; set; }

        /// <inheritdoc/>
        public DisconnectEventHandler<ITcpClientBase> Disconnecting { get; set; }

        /// <summary>
        /// 当客户端完整建立TCP连接。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnConnected(ConnectedEventArgs e)
        {
            this.Service.OnInternalConnected(this, e);
        }

        /// <summary>
        /// 客户端正在连接。
        /// </summary>
        protected virtual void OnConnecting(ConnectingEventArgs e)
        {
            this.Service.OnInternalConnecting(this, e);
        }

        /// <summary>
        /// 客户端已断开连接。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDisconnected(DisconnectEventArgs e)
        {
            this.Disconnected?.Invoke(this, e);
        }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// <para>
        /// 当主动调用Close断开时。
        /// </para>
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDisconnecting(DisconnectEventArgs e)
        {
            try
            {
                this.Disconnecting?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                this.Logger.Log(LogLevel.Error, this, $"在事件{nameof(this.Disconnecting)}中发生错误。", ex);
            }
        }

        /// <summary>
        /// 当初始化完成时，执行在<see cref="OnConnecting(ConnectingEventArgs)"/>之前。
        /// </summary>
        protected virtual void OnInitialized()
        {
        }

        private void PrivateOnDisconnected(DisconnectEventArgs e)
        {
            this.OnDisconnected(e);
            if (e.Handled)
            {
                return;
            }

            if (this.PluginsManager.Enable && this.PluginsManager.Raise(nameof(ITcpDisconnectedPlugin.OnTcpDisconnected), this, e))
            {
                return;
            }

            if (!e.Handled)
            {
                this.Service.OnInternalDisconnected(this, e);
            }
        }

        private void PrivateOnDisconnecting(DisconnectEventArgs e)
        {
            this.OnDisconnecting(e);
            if (e.Handled)
            {
                return;
            }

            if (this.PluginsManager.Enable && this.PluginsManager.Raise(nameof(ITcpDisconnectingPlugin.OnTcpDisconnecting), this, e))
            {
                return;
            }
            this.Service.OnInternalDisconnecting(this, e);
        }

        #endregion 事件&委托

        /// <inheritdoc/>
        public override int ReceiveBufferSize
        {
            get => base.ReceiveBufferSize;
            set
            {
                base.ReceiveBufferSize = value;
                if (this.MainSocket != null)
                {
                    this.MainSocket.ReceiveBufferSize = base.ReceiveBufferSize;
                }
            }
        }

        /// <inheritdoc/>
        public override int SendBufferSize
        {
            get => base.SendBufferSize;
            set
            {
                base.SendBufferSize = value;
                if (this.MainSocket != null)
                {
                    this.MainSocket.SendBufferSize = base.SendBufferSize;
                }
            }
        }

        private void OnSendPeriod(long value)
        {
            this.SendBufferSize = TouchSocketUtility.HitBufferLength(value);
        }
        private void OnReceivePeriod(long value)
        {
            this.ReceiveBufferSize = TouchSocketUtility.HitBufferLength(value);
        }

        /// <inheritdoc/>
        public virtual void Close(string msg = TouchSocketCoreUtility.Empty)
        {

            lock (this.SyncRoot)
            {
                if (this.Online)
                {
                    this.MainSocket.TryClose();
                    this.PrivateOnDisconnecting(new DisconnectEventArgs(true, msg));
                }
                this.BreakOut(msg, true);
            }
        }

        /// <inheritdoc/>
        public Stream GetStream()
        {
            this.m_workStream ??= new NetworkStream(this.MainSocket, true);
            return this.m_workStream;
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
                    if (this.PluginsManager.Enable)
                    {
                        var e = new IdChangedEventArgs(oldId, newId);
                        this.PluginsManager.Raise(nameof(IIdChangedPlugin.OnIdChanged), socketClient, e);
                    }
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
            lock (this.SyncRoot)
            {
                if (this.Online)
                {
                    this.PrivateOnDisconnecting(new DisconnectEventArgs(true, $"{nameof(Dispose)}主动断开"));
                }
                this.BreakOut($"{nameof(Dispose)}主动断开", true);
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// 处理已接收到的数据。
        /// <para>根据不同的数据处理适配器，会传递不同的数据</para>
        /// </summary>
        /// <param name="byteBlock">以二进制流形式传递</param>
        /// <param name="requestInfo">以解析的数据对象传递</param>
        /// <returns>如果返回<see langword="true"/>则表示数据已被处理，且不会再向下传递。</returns>
        protected virtual bool HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            return false;
        }

        /// <summary>
        /// 当即将发送时。
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <returns>返回值表示是否允许发送</returns>
        protected virtual bool HandleSendingData(byte[] buffer, int offset, int length)
        {
            if (this.PluginsManager.Enable)
            {
                var args = new SendingEventArgs(buffer, offset, length);
                this.PluginsManager.Raise(nameof(ITcpSendingPlugin.OnTcpSending), this, args);
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
            this.DataHandlingAdapter = adapter;
        }

        private void BeginSsl()
        {
            if (!this.DisposedValue)
            {
                var byteBlock = new ByteBlock(this.ReceiveBufferSize);
                try
                {
                    this.m_workStream.BeginRead(byteBlock.Buffer, 0, byteBlock.Capacity, this.EndSsl, byteBlock);
                }
                catch (Exception ex)
                {
                    byteBlock.SafeDispose();
                    this.BreakOut(ex.Message, false);
                }
            }
        }

        private void BreakOut(string msg, bool manual)
        {
            lock (this.SyncRoot)
            {
                if (this.GetSocketCliectCollection().TryRemove(this.Id, out _))
                {
                    if (this.Online)
                    {
                        this.Online = false;
                        this.MainSocket.SafeDispose();
                        this.m_delaySender.SafeDispose();
                        this.DataHandlingAdapter.SafeDispose();
                        this.PrivateOnDisconnected(new DisconnectEventArgs(manual, msg));
                    }
                }

                base.Dispose(true);
            }
        }

        private void EndSsl(IAsyncResult result)
        {
            var byteBlock = (ByteBlock)result.AsyncState;
            try
            {
                var r = this.m_workStream.EndRead(result);
                if (r == 0)
                {
                    this.BreakOut("远程终端主动关闭", false);
                    return;
                }
                byteBlock.SetLength(r);

                this.HandleBuffer(byteBlock);
                this.BeginSsl();
            }
            catch (Exception ex)
            {
                byteBlock.SafeDispose();
                this.BreakOut(ex.Message, false);
            }
        }

        private void EventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                this.m_bufferRate = 1;
                this.ProcessReceived(e);
            }
            catch (Exception ex)
            {
                e.SafeDispose();
                this.BreakOut(ex.Message, false);
            }
        }

        private SocketClientCollection GetSocketCliectCollection()
        {
            return this.Service?.SocketClients as SocketClientCollection;
        }

        private void HandleBuffer(ByteBlock byteBlock)
        {
            try
            {
                if (this.DisposedValue)
                {
                    return;
                }
                this.m_receiveCounter.Increment(byteBlock.Length);
                if (this.OnHandleRawBuffer?.Invoke(byteBlock) == false)
                {
                    return;
                }
                if (this.PluginsManager.Enable && this.PluginsManager.Raise(nameof(ITcpReceivingPlugin.OnTcpReceiving), this, new ByteBlockEventArgs(byteBlock)))
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
            finally
            {
                byteBlock.Dispose();
            }
        }

        private void PrivateHandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            if (this.OnHandleReceivedData?.Invoke(byteBlock, requestInfo) == false)
            {
                return;
            }

            if (this.HandleReceivedData(byteBlock, requestInfo))
            {
                return;
            }

            if (this.PluginsManager.GetPluginCount(nameof(ITcpReceivedPlugin.OnTcpReceived)) > 0 && this.PluginsManager.Raise(nameof(ITcpReceivedPlugin.OnTcpReceived), this, new ReceivedDataEventArgs(byteBlock, requestInfo)))
            {
                return;
            }

            this.Service.OnInternalReceivedData(this, byteBlock, requestInfo);
        }

        private void ProcessReceived(SocketAsyncEventArgs e)
        {
            if (this.DisposedValue)
            {
                e.SafeDispose();
                return;
            }

            if (e.SocketError != SocketError.Success)
            {
                e.SafeDispose();
                this.BreakOut(e.SocketError.ToString(), false);
            }
            else if (e.BytesTransferred > 0)
            {
                var byteBlock = (ByteBlock)e.UserToken;
                byteBlock.SetLength(e.BytesTransferred);
                this.HandleBuffer(byteBlock);
                try
                {
                    var newByteBlock = new ByteBlock((int)Math.Min(this.ReceiveBufferSize * this.m_bufferRate, TouchSocketUtility.MaxBufferLength));
                    e.UserToken = newByteBlock;
                    e.SetBuffer(newByteBlock.Buffer, 0, newByteBlock.Capacity);

                    if (!this.MainSocket.ReceiveAsync(e))
                    {
                        this.m_bufferRate += 2;
                        this.ProcessReceived(e);
                    }
                }
                catch (Exception ex)
                {
                    this.BreakOut(ex.Message, false);
                }
            }
            else
            {
                e.SafeDispose();
                this.BreakOut("远程主机主动断开连接", false);
            }
        }

        #region 发送

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public void DefaultSend(byte[] buffer, int offset, int length)
        {
            if (!this.CanSend)
            {
                throw new NotConnectedException(TouchSocketResource.NotConnected.GetDescription());
            }
            if (this.HandleSendingData(buffer, offset, length))
            {
                if (this.UseSsl)
                {
                    this.m_workStream.Write(buffer, offset, length);
                }
                else
                {
                    if (this.m_delaySender != null && length < this.m_delaySender.DelayLength)
                    {
                        this.m_delaySender.Send(QueueDataBytes.CreateNew(buffer, offset, length));
                    }
                    else
                    {
                        this.MainSocket.AbsoluteSend(buffer, offset, length);
                    }
                }
                this.m_sendCounter.Increment(length);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"><inheritdoc/></param>
        /// <param name="offset"><inheritdoc/></param>
        /// <param name="length"><inheritdoc/></param>
        /// <exception cref="NotConnectedException"><inheritdoc/></exception>
        /// <exception cref="OverlengthException"><inheritdoc/></exception>
        /// <exception cref="Exception"><inheritdoc/></exception>
        public Task DefaultSendAsync(byte[] buffer, int offset, int length)
        {
            return Task.Run(() =>
            {
                this.DefaultSend(buffer, offset, length);
            });
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
            if (this.DisposedValue)
            {
                return;
            }
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
            if (this.DisposedValue)
            {
                return;
            }
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
            if (this.DisposedValue)
            {
                return;
            }
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
        /// IOCP发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual Task SendAsync(byte[] buffer, int offset, int length)
        {
            return Task.Run(() =>
            {
                this.Send(buffer, offset, length);
            });
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
            return Task.Run(() =>
            {
                this.Send(requestInfo);
            });
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        public virtual Task SendAsync(IList<ArraySegment<byte>> transferBytes)
        {
            return Task.Run(() =>
            {
                this.Send(transferBytes);
            });
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

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.GetIPPort();
        }
    }
}