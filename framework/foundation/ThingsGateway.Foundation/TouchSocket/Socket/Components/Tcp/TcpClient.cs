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
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace ThingsGateway.Foundation.Sockets
{
    /// <summary>
    /// 简单Tcp客户端
    /// </summary>
    public class TcpClient : TcpClientBase
    {
        /// <summary>
        /// 接收到数据
        /// </summary>
        public ReceivedEventHandler<TcpClient> Received { get; set; }

        /// <inheritdoc/>
        protected override Task ReceivedData(ReceivedDataEventArgs e)
        {
            if (this.Received != null)
            {
                return this.Received.Invoke(this, e);
            }
            return base.ReceivedData(e);
        }
    }

    /// <summary>
    /// Tcp客户端
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{IP}:{Port}")]
    public class TcpClientBase : SetupConfigObject, ITcpClient
    {
        /// <summary>
        /// Tcp客户端
        /// </summary>
        public TcpClientBase()
        {
            this.Protocol = Protocol.Tcp;
        }

        #region 变量

        private DelaySender m_delaySender;
        private volatile bool m_online;
        private readonly SemaphoreSlim m_semaphoreForConnect = new SemaphoreSlim(1, 1);
        private readonly InternalTcpCore m_tcpCore = new InternalTcpCore();
        #endregion 变量

        #region 事件

        /// <inheritdoc/>
        public ConnectedEventHandler<ITcpClient> Connected { get; set; }

        /// <inheritdoc/>
        public ConnectingEventHandler<ITcpClient> Connecting { get; set; }

        /// <inheritdoc/>
        public DisconnectEventHandler<ITcpClientBase> Disconnected { get; set; }

        /// <inheritdoc/>
        public DisconnectEventHandler<ITcpClientBase> Disconnecting { get; set; }

        private Task PrivateOnConnected(object o)
        {
            return this.OnConnected((ConnectedEventArgs)o);
        }

        /// <summary>
        /// 已经建立Tcp连接
        /// </summary>
        /// <param name="e"></param>
        protected virtual async Task OnConnected(ConnectedEventArgs e)
        {
            try
            {
                if (this.Connected != null)
                {
                    await this.Connected.Invoke(this, e);
                    if (e.Handled)
                    {
                        return;
                    }
                }
                await this.PluginManager.RaiseAsync(nameof(ITcpConnectedPlugin.OnTcpConnected), this, e);
            }
            catch (Exception ex)
            {
                this.Logger.Log(LogLevel.Error, this, $"在事件{nameof(this.Connected)}中发生错误。", ex);
            }
        }

        private Task PrivateOnConnecting(ConnectingEventArgs e)
        {
            if (this.CanSetDataHandlingAdapter)
            {
                this.SetDataHandlingAdapter(this.Config.GetValue(TouchSocketConfigExtension.TcpDataHandlingAdapterProperty).Invoke());
            }

            return this.OnConnecting(e);
        }

        /// <summary>
        /// 准备连接的时候，此时已初始化Socket，但是并未建立Tcp连接
        /// </summary>
        /// <param name="e"></param>
        protected virtual async Task OnConnecting(ConnectingEventArgs e)
        {
            try
            {
                if (this.Connecting != null)
                {
                    await this.Connecting.Invoke(this, e);
                    if (e.Handled)
                    {
                        return;
                    }
                }
                await this.PluginManager.RaiseAsync(nameof(ITcpConnectingPlugin.OnTcpConnecting), this, e);
            }
            catch (Exception ex)
            {
                this.Logger.Log(LogLevel.Error, this, $"在事件{nameof(this.OnConnecting)}中发生错误。", ex);
            }
        }

        private Task PrivateOnDisconnected(object obj)
        {
            this.m_receiver?.TryInputReceive(default, default);
            return this.OnDisconnected((DisconnectEventArgs)obj);
        }

        /// <summary>
        /// 断开连接。在客户端未设置连接状态时，不会触发
        /// </summary>
        /// <param name="e"></param>
        protected virtual async Task OnDisconnected(DisconnectEventArgs e)
        {
            try
            {
                if (this.Disconnected != null)
                {
                    await this.Disconnected.Invoke(this, e).ConfigureAwait(false);
                    if (e.Handled)
                    {
                        return;
                    }
                }

                await this.PluginManager.RaiseAsync(nameof(ITcpDisconnectedPlugin.OnTcpDisconnected), this, e).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.Logger.Log(LogLevel.Error, this, $"在事件{nameof(this.Disconnected)}中发生错误。", ex);
            }
        }

        private Task PrivateOnDisconnecting(object obj)
        {
            return this.OnDisconnecting((DisconnectEventArgs)obj);
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
                    await this.Disconnecting.Invoke(this, e).ConfigureAwait(false);
                    if (e.Handled)
                    {
                        return;
                    }
                }

                await this.PluginManager.RaiseAsync(nameof(ITcpDisconnectingPlugin.OnTcpDisconnecting), this, e).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.Logger.Log(LogLevel.Error, this, $"在事件{nameof(this.Disconnecting)}中发生错误。", ex);
            }
        }

        #endregion 事件

        #region 属性

        /// <inheritdoc/>
        public DateTime LastReceivedTime => this.GetTcpCore().ReceiveCounter.LastIncrement;

        /// <inheritdoc/>
        public DateTime LastSendTime => this.GetTcpCore().SendCounter.LastIncrement;


        /// <inheritdoc/>
        public virtual bool CanSetDataHandlingAdapter => true;


        /// <inheritdoc/>
        public SingleStreamDataHandlingAdapter DataHandlingAdapter { get; private set; }

        /// <inheritdoc/>
        public string IP { get; private set; }

        /// <inheritdoc/>
        public Socket MainSocket { get; private set; }

        /// <inheritdoc/>
        public bool Online { get => this.m_online; }

        /// <inheritdoc/>
        public bool CanSend => this.m_online;


        /// <inheritdoc/>
        public int Port { get; private set; }



        /// <inheritdoc/>
        public bool UseSsl => this.GetTcpCore().UseSsl;

        /// <inheritdoc/>
        public Protocol Protocol { get; set; }

        /// <inheritdoc/>
        public IPHost RemoteIPHost { get; private set; }

        /// <inheritdoc/>
        public bool IsClient => true;

        #endregion 属性

        #region 断开操作

        /// <inheritdoc/>
        public virtual void Close(string msg = TouchSocketCoreUtility.Empty)
        {
            lock (this.GetTcpCore())
            {
                if (this.m_online)
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
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            lock (this.GetTcpCore())
            {
                if (this.m_online)
                {
                    this.PrivateOnDisconnecting(new DisconnectEventArgs(true, $"{nameof(Dispose)}主动断开")).GetFalseAwaitResult();
                    this.BreakOut(true, $"{nameof(Dispose)}主动断开");
                }
            }
            base.Dispose(disposing);
        }

        #endregion 断开操作

        #region Connect

        /// <summary>
        /// 建立Tcp的连接。
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="token"></param>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        /// <exception cref="TimeoutException"></exception>
        protected void TcpConnect(int timeout, CancellationToken token = default)
        {
            try
            {
                this.ThrowIfDisposed();
                this.m_semaphoreForConnect.Wait(token);
                if (this.m_online)
                {
                    return;
                }

                if (this.Config == null)
                {
                    throw new ArgumentNullException(nameof(this.Config), "配置文件不能为空。");
                }
                var iPHost = this.Config.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty) ?? throw new ArgumentNullException(nameof(IPHost), "iPHost不能为空。");
                this.MainSocket.SafeDispose();
                var socket = this.CreateSocket(iPHost);
                this.PrivateOnConnecting(new ConnectingEventArgs(socket)).GetFalseAwaitResult();

                var task = Task.Run(() =>
                {
                    socket.Connect(iPHost.Host, iPHost.Port);
                }, token);
                task.ConfigureFalseAwait();
                if (!task.Wait(timeout, token))
                {
                    socket.SafeDispose();
                    throw new TimeoutException();
                }
                this.m_online = true;
                this.SetSocket(socket);
                this.BeginReceive();
                this.PrivateOnConnected(new ConnectedEventArgs()).GetFalseAwaitResult();
                //_ = Task.Factory.StartNew(this.PrivateOnConnected, new ConnectedEventArgs());
            }
            finally
            {
                this.m_semaphoreForConnect.Release();
            }
        }

        private void BeginReceive()
        {
            if (this.Config.GetValue(TouchSocketConfigExtension.SslOptionProperty) is ClientSslOption sslOption)
            {
                this.GetTcpCore().Authenticate(sslOption);
                _ = this.GetTcpCore().BeginSslReceive();
            }
            else
            {
                this.GetTcpCore().BeginIocpReceive();
            }
        }


        /// <summary>
        /// 异步连接服务器
        /// </summary>
        protected async Task TcpConnectAsync(int timeout, CancellationToken cancellationToken = default)
        {
            try
            {
                ThrowIfDisposed();
                await this.m_semaphoreForConnect.WaitAsync();
                if (this.m_online)
                {
                    return;
                }

                if (this.Config == null)
                {
                    throw new ArgumentNullException(nameof(this.Config), "配置文件不能为空。");
                }
                var iPHost = this.Config.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty) ?? throw new ArgumentNullException(nameof(IPHost), "iPHost不能为空。");
                this.MainSocket.SafeDispose();
                var socket = this.CreateSocket(iPHost);
                await this.PrivateOnConnecting(new ConnectingEventArgs(socket));

#if NET6_0_OR_GREATER
                using CancellationTokenSource cancellationTokenSource = new(timeout);
                using CancellationTokenSource stoppingToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, cancellationToken);
                try
                {
                    await socket.ConnectAsync(iPHost.EndPoint, stoppingToken.Token);
                }
                catch (OperationCanceledException)
                {
                    throw new TimeoutException("连接超时");
                }
                await Success(socket);
#else

                using CancellationTokenSource cancellationTokenSource = new();
                using CancellationTokenSource stoppingToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, cancellationToken);
                var task = Task.Factory.FromAsync(socket.BeginConnect(iPHost.EndPoint, null, null), socket.EndConnect);
                var result = await Task.WhenAny(task, Task.Delay(timeout, stoppingToken.Token));
                if (result == task)
                {
                    cancellationTokenSource.Cancel();
                    if (task.Exception != null)
                    {
                        socket?.SafeDispose();
                        throw task.Exception;
                    }
                    else
                    {
                       await Success(socket);
                    }
                }
                else
                {
                    socket?.SafeDispose();
                    throw new TimeoutException("连接超时");
                }




#endif
                async Task Success(Socket socket)
                {
                    this.m_online = true;
                    this.SetSocket(socket);
                    this.BeginReceive();
                    await this.PrivateOnConnected(new ConnectedEventArgs());
                    //_ = Task.Factory.StartNew(this.PrivateOnConnected, new ConnectedEventArgs());
                }
            }
            finally
            {
                this.m_semaphoreForConnect.Release();
            }
        }


        /// <inheritdoc/>
        public virtual void Connect(int timeout, CancellationToken cancellationToken)
        {
            this.TcpConnect(timeout, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task ConnectAsync(int timeout, CancellationToken cancellationToken)
        {
            await this.TcpConnectAsync(timeout, cancellationToken);
        }

        #endregion Connect

        #region Receiver

        private Receiver m_receiver;

        /// <inheritdoc/>
        public IReceiver CreateReceiver()
        {
            return this.m_receiver ??= new Receiver(this);
        }

        /// <inheritdoc/>
        public void ClearReceiver()
        {
            this.m_receiver = null;
        }

        #endregion

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.GetIPPort();
        }

        private void TcpCoreBreakOut(TcpCore core, bool manual, string msg)
        {
            this.BreakOut(manual, msg);
        }
        /// <summary>
        /// BreakOut。
        /// </summary>
        /// <param name="manual"></param>
        /// <param name="msg"></param>
        protected void BreakOut(bool manual, string msg)
        {
            lock (this.GetTcpCore())
            {
                if (this.m_online)
                {
                    this.m_online = false;
                    this.MainSocket.SafeDispose();
                    this.m_delaySender.SafeDispose();
                    this.DataHandlingAdapter.SafeDispose();
                    this.PrivateOnDisconnected(new DisconnectEventArgs(manual, msg)).GetFalseAwaitResult();
                }
            }
        }

        private TcpCore GetTcpCore()
        {
            this.ThrowIfDisposed();
            return this.m_tcpCore ?? throw new ObjectDisposedException(this.GetType().Name);
        }


        /// <inheritdoc/>
        public virtual void SetDataHandlingAdapter(SingleStreamDataHandlingAdapter adapter)
        {
            if (!this.CanSetDataHandlingAdapter)
            {
                throw new Exception($"不允许自由调用{nameof(SetDataHandlingAdapter)}进行赋值。");
            }

            this.SetAdapter(adapter);
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

        /// <summary>
        /// 当收到适配器处理的数据时。
        /// </summary>
        /// <param name="e"></param>
        /// <returns>如果返回<see langword="true"/>则表示数据已被处理，且不会再向下传递。</returns>
        protected virtual Task ReceivedData(ReceivedDataEventArgs e)
        {
            return this.PluginManager.RaiseAsync(nameof(ITcpReceivedPlugin.OnTcpReceived), this, e);
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
                await this.PluginManager.RaiseAsync(nameof(ITcpSendingPlugin.OnTcpSending), this, args).ConfigureFalseAwait();
                return args.IsPermitOperation;
            }
            return true;
        }

        /// <inheritdoc/>
        protected override void LoadConfig(TouchSocketConfig config)
        {
            this.RemoteIPHost = config.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty);
        }

        /// <summary>
        /// 设置适配器，该方法不会检验<see cref="CanSetDataHandlingAdapter"/>的值。
        /// </summary>
        /// <param name="adapter"></param>
        protected void SetAdapter(SingleStreamDataHandlingAdapter adapter)
        {
            this.ThrowIfDisposed();
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

        private Socket CreateSocket(IPHost iPHost)
        {
            Socket socket;
            if (iPHost.HostNameType == UriHostNameType.Dns)
            {
                socket = new Socket(SocketType.Stream, ProtocolType.Tcp)
                {
                    SendTimeout = this.Config.GetValue(TouchSocketConfigExtension.SendTimeoutProperty)
                };
            }
            else
            {
                socket = new Socket(iPHost.EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
                {
                    SendTimeout = this.Config.GetValue(TouchSocketConfigExtension.SendTimeoutProperty)
                };
            }
            if (this.Config.GetValue(TouchSocketConfigExtension.KeepAliveValueProperty) is KeepAliveValue keepAliveValue)
            {
#if NET45_OR_GREATER

                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                socket.IOControl(IOControlCode.KeepAliveValues, keepAliveValue.KeepAliveTime, null);
#else
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    socket.IOControl(IOControlCode.KeepAliveValues, keepAliveValue.KeepAliveTime, null);
                }
#endif
            }

            var noDelay = this.Config.GetValue(TouchSocketConfigExtension.NoDelayProperty);
            if (noDelay != null)
            {
                socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, noDelay);
            }

            if (this.Config.GetValue(TouchSocketConfigExtension.BindIPHostProperty) != null)
            {
                if (this.Config.GetValue(TouchSocketConfigExtension.ReuseAddressProperty))
                {
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                }
                socket.Bind(this.Config.GetValue(TouchSocketConfigExtension.BindIPHostProperty).EndPoint);
            }
            return socket;
        }

        #region 发送

        #region 同步发送

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="requestInfo"></param>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public void Send(IRequestInfo requestInfo)
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
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"><inheritdoc/></param>
        /// <param name="offset"><inheritdoc/></param>
        /// <param name="length"><inheritdoc/></param>
        /// <exception cref="NotConnectedException"><inheritdoc/></exception>
        /// <exception cref="OverlengthException"><inheritdoc/></exception>
        /// <exception cref="Exception"><inheritdoc/></exception>
        public virtual void Send(byte[] buffer, int offset, int length)
        {
            if (this.DataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketResource.NullDataAdapter.GetDescription());
            }
            this.DataHandlingAdapter.SendInput(buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"><inheritdoc/></param>
        /// <exception cref="NotConnectedException"><inheritdoc/></exception>
        /// <exception cref="OverlengthException"><inheritdoc/></exception>
        /// <exception cref="Exception"><inheritdoc/></exception>
        public virtual void Send(IList<ArraySegment<byte>> transferBytes)
        {
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

        #endregion 发送

        private void SetSocket(Socket socket)
        {
            if (socket == null)
            {
                this.IP = null;
                this.Port = -1;
                return;
            }

            this.IP = socket.RemoteEndPoint.GetIP();
            this.Port = socket.RemoteEndPoint.GetPort();
            this.MainSocket = socket;
            var delaySenderOption = this.Config.GetValue(TouchSocketConfigExtension.DelaySenderProperty);
            if (delaySenderOption != null)
            {
                this.m_delaySender = new DelaySender(delaySenderOption, this.GetTcpCore().Send);
            }
            this.m_tcpCore.Reset(socket);
            this.m_tcpCore.OnReceived = this.HandleReceived;
            this.m_tcpCore.OnBreakOut = this.TcpCoreBreakOut;
            if (this.Config.GetValue(TouchSocketConfigExtension.MinBufferSizeProperty) is int minValue)
            {
                this.m_tcpCore.MinBufferSize = minValue;
            }

            if (this.Config.GetValue(TouchSocketConfigExtension.MaxBufferSizeProperty) is int maxValue)
            {
                this.m_tcpCore.MaxBufferSize = maxValue;
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
                if (this.ReceivingData(byteBlock).GetFalseAwaitResult())
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


    }
}