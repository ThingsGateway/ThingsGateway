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
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace ThingsGateway.Foundation.Sockets;

/// <summary>
/// 简单TCP客户端
/// </summary>
public class TcpClient : TcpClientBase
{

    /// <summary>
    /// 接收到数据
    /// </summary>
    public ReceivedEventHandler<TcpClient> Received { get; set; }

    /// <summary>
    /// 接收数据
    /// </summary>
    /// <param name="byteBlock"></param>
    /// <param name="requestInfo"></param>
    protected override bool HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
    {
        this.Received?.Invoke(this, byteBlock, requestInfo);
        return false;
    }
}

/// <summary>
/// TCP客户端
/// </summary>
[System.Diagnostics.DebuggerDisplay("{IP}:{Port}")]
public class TcpClientBase : BaseSocket, ITcpClient
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public TcpClientBase()
    {
        this.Protocol = Protocol.Tcp;
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

    #region 变量

    private DelaySender m_delaySender;
    private Stream m_workStream;
    private long m_bufferRate = 1;
    private volatile bool m_online;
    private ValueCounter m_receiveCounter;
    private ValueCounter m_sendCounter;

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

    private void PrivateOnConnected(object o)
    {
        var e = (ConnectedEventArgs)o;
        this.OnConnected(e);
        if (e.Handled)
        {
            return;
        }
        this.PluginsManager.Raise(nameof(ITcpConnectedPlugin.OnTcpConnected), this, e);
    }

    /// <summary>
    /// 已经建立Tcp连接
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnConnected(ConnectedEventArgs e)
    {
        try
        {
            this.Connected?.Invoke(this, e);
        }
        catch (Exception ex)
        {
            this.Logger.Log(LogLevel.Error, this, $"在事件{nameof(this.Connected)}中发生错误。", ex);
        }
    }

    private void PrivateOnConnecting(ConnectingEventArgs e)
    {
        if (this.CanSetDataHandlingAdapter)
        {
            this.SetDataHandlingAdapter(this.Config.GetValue(TouchSocketConfigExtension.TcpDataHandlingAdapterProperty).Invoke());
        }

        this.OnConnecting(e);
        if (e.Handled)
        {
            return;
        }
        this.PluginsManager.Raise(nameof(ITcpConnectingPlugin.OnTcpConnecting), this, e);
    }

    /// <summary>
    /// 准备连接的时候，此时已初始化Socket，但是并未建立Tcp连接
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnConnecting(ConnectingEventArgs e)
    {
        try
        {
            this.Connecting?.Invoke(this, e);
        }
        catch (Exception ex)
        {
            this.Logger.Log(LogLevel.Error, this, $"在事件{nameof(this.OnConnecting)}中发生错误。", ex);
        }
    }

    private void PrivateOnDisconnected(DisconnectEventArgs e)
    {
        this.OnDisconnected(e);
        if (e.Handled)
        {
            return;
        }
        this.PluginsManager.Raise(nameof(ITcpDisconnectedPlugin.OnTcpDisconnected), this, e);
    }

    /// <summary>
    /// 断开连接。在客户端未设置连接状态时，不会触发
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnDisconnected(DisconnectEventArgs e)
    {
        try
        {
            this.Disconnected?.Invoke(this, e);
        }
        catch (Exception ex)
        {
            this.Logger.Log(LogLevel.Error, this, $"在事件{nameof(this.Disconnected)}中发生错误。", ex);
        }
    }

    private void PrivateOnDisconnecting(DisconnectEventArgs e)
    {
        this.OnDisconnecting(e);
        if (e.Handled)
        {
            return;
        }
        this.PluginsManager.Raise(nameof(ITcpDisconnectingPlugin.OnTcpDisconnecting), this, e);
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

    #endregion 事件

    #region 属性

    /// <inheritdoc/>
    public DateTime LastReceivedTime => this.m_receiveCounter.LastIncrement;

    /// <inheritdoc/>
    public DateTime LastSendTime => this.m_sendCounter.LastIncrement;

    /// <inheritdoc/>
    public Func<ByteBlock, bool> OnHandleRawBuffer { get; set; }

    /// <inheritdoc/>
    public Func<ByteBlock, IRequestInfo, bool> OnHandleReceivedData { get; set; }

    /// <inheritdoc/>
    public IContainer Container { get; private set; }

    /// <inheritdoc/>
    public virtual bool CanSetDataHandlingAdapter => true;

    /// <inheritdoc/>
    public TouchSocketConfig Config { get; private set; }

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
    public IPluginsManager PluginsManager { get; private set; }

    /// <inheritdoc/>
    public int Port { get; private set; }

    /// <inheritdoc/>
    public ReceiveType ReceiveType { get; private set; }

    /// <inheritdoc/>
    public bool UseSsl { get; private set; }

    /// <inheritdoc/>
    public Protocol Protocol { get; set; }

    /// <inheritdoc/>
    public IPHost RemoteIPHost { get; private set; }

    /// <inheritdoc/>
    public bool IsClient => true;

    #endregion 属性
    private EasyLock EasyLock { get; set; } = new();

    #region 断开操作

    /// <inheritdoc/>
    public virtual void Close(string msg = TouchSocketCoreUtility.Empty)
    {
        lock (this.SyncRoot)
        {
            if (this.m_online)
            {
                this.PrivateOnDisconnecting(new DisconnectEventArgs(true, msg));

                this.m_online = false;
                this.MainSocket.TryClose();

                this.MainSocket.SafeDispose();
                this.m_delaySender.SafeDispose();
                this.m_workStream.SafeDispose();
                this.DataHandlingAdapter.SafeDispose();
                this.PrivateOnDisconnected(new DisconnectEventArgs(true, msg));
            }
        }
    }

    private void BreakOut(string msg)
    {
        lock (this.SyncRoot)
        {
            if (this.m_online)
            {
                this.m_online = false;
                this.MainSocket.SafeDispose();
                this.m_delaySender.SafeDispose();
                this.m_workStream.SafeDispose();
                this.DataHandlingAdapter.SafeDispose();
                this.PrivateOnDisconnected(new DisconnectEventArgs(false, msg));
            }
        }
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
        lock (this.SyncRoot)
        {
            if (this.m_online)
            {
                this.m_online = false;
                this.MainSocket.TryClose();
                this.PrivateOnDisconnecting(new DisconnectEventArgs(true, $"{nameof(Dispose)}主动断开"));

                this.MainSocket.SafeDispose();
                this.m_delaySender.SafeDispose();
                this.m_workStream.SafeDispose();
                this.DataHandlingAdapter.SafeDispose();
                this.PluginsManager.SafeDispose();
                this.PrivateOnDisconnected(new DisconnectEventArgs(true, $"{nameof(Dispose)}主动断开"));
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
    /// <exception cref="ObjectDisposedException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    /// <exception cref="TimeoutException"></exception>
    protected void TcpConnect(int timeout)
    {
        lock (this.SyncRoot)
        {
            if (this.m_online)
            {
                return;
            }
            if (this.DisposedValue)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (this.Config == null)
            {
                throw new ArgumentNullException(nameof(this.Config), "配置文件不能为空。");
            }
            var iPHost = this.Config.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty) ?? throw new ArgumentNullException(nameof(IPHost), "iPHost不能为空。");
            this.MainSocket.SafeDispose();
            var socket = this.CreateSocket(iPHost);
            this.PrivateOnConnecting(new ConnectingEventArgs(socket));
            if (timeout == 5000)
            {
                socket.Connect(iPHost.Host, iPHost.Port);
            }
            else
            {
                var task = Task.Run(() =>
                {
                    socket.Connect(iPHost.Host, iPHost.Port);
                });
                task.ConfigureAwait(false);
                if (!task.Wait(timeout))
                {
                    socket.SafeDispose();
                    throw new TimeoutException();
                }
            }
            this.m_online = true;
            this.SetSocket(socket);
            this.BeginReceive();
            this.PrivateOnConnected(new ConnectedEventArgs());
        }
    }

    /// <summary>
    /// 异步连接服务器
    /// </summary>
    /// <param name="timeout"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="ObjectDisposedException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    /// <exception cref="TimeoutException"></exception>
    protected virtual async Task TcpConnectAsync(int timeout, CancellationToken cancellationToken = default)
    {
        try
        {
            await EasyLock.WaitAsync();
            if (m_online)
            {
                return;
            }
            if (DisposedValue)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            if (Config == null)
            {
                throw new ArgumentNullException("配置文件不能为空。");
            }
            var iPHost = Config.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty) ?? throw new ArgumentNullException("iPHost不能为空。");
            MainSocket.SafeDispose();
            var socket = CreateSocket(iPHost);
            var args = new ConnectingEventArgs(socket);
            PrivateOnConnecting(args);

#if (NET6_0_OR_GREATER)
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
            {
                success(socket);
            }

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
                    success(socket);
                }
            }
            else
            {
                socket?.SafeDispose();
                socket.Close();
            }
#endif


        }
        finally
        {
            EasyLock.Release();
        }

        void success(Socket socket)
        {
            m_online = true;
            SetSocket(socket);
            BeginReceive();
            PrivateOnConnected(new ConnectedEventArgs());
        }
    }

    /// <inheritdoc/>
    public virtual ITcpClient Connect(int timeout = 5000)
    {
        TcpConnect(timeout);
        return this;
    }

    /// <inheritdoc/>
    public virtual async Task<ITcpClient> ConnectAsync(int timeout = 5000)
    {
        await TcpConnectAsync(timeout);
        return this;
    }
    /// <inheritdoc/>
    public virtual async Task<ITcpClient> ConnectAsync(int timeout, CancellationToken cancellationToken)
    {
        await TcpConnectAsync(timeout, cancellationToken);
        return this;
    }
    #endregion

    /// <inheritdoc/>
    public Stream GetStream()
    {
        this.ThrowIfDisposed();
        this.m_workStream ??= new NetworkStream(this.MainSocket, true);
        return this.m_workStream;
    }

    private void OnReceivePeriod(long value)
    {
        this.ReceiveBufferSize = TouchSocketUtility.HitBufferLength(value);
    }

    private void OnSendPeriod(long value)
    {
        this.SendBufferSize = TouchSocketUtility.HitBufferLength(value);
    }

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

    /// <inheritdoc/>
    public virtual void SetDataHandlingAdapter(SingleStreamDataHandlingAdapter adapter)
    {
        if (!this.CanSetDataHandlingAdapter)
        {
            throw new Exception($"不允许自由调用{nameof(SetDataHandlingAdapter)}进行赋值。");
        }

        this.SetAdapter(adapter);
    }

    /// <inheritdoc/>
    public ITcpClient Setup(string ipHost)
    {
        var config = new TouchSocketConfig();
        config.SetRemoteIPHost(new IPHost(ipHost));
        return this.Setup(config);
    }

    /// <inheritdoc/>
    public ITcpClient Setup(TouchSocketConfig config)
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

        if (this.PluginsManager.Enable)
        {
            var args = new ReceivedDataEventArgs(byteBlock, requestInfo);
            this.PluginsManager.Raise(nameof(ITcpReceivedPlugin.OnTcpReceived), this, args);
        }
    }

    /// <summary>
    /// 处理已接收到的数据。
    /// </summary>
    /// <param name="byteBlock">以二进制流形式传递</param>
    /// <param name="requestInfo">以解析的数据对象传递</param>
    /// <returns>如果返回<see langword="true"/>则表示数据已被处理，且不会再向下传递。</returns>
    protected virtual bool HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
    {
        return false;
    }

    /// <summary>
    /// 当即将发送时，如果覆盖父类方法，则不会触发插件。
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
    /// 加载配置
    /// </summary>
    /// <param name="config"></param>
    protected virtual void LoadConfig(TouchSocketConfig config)
    {
        this.RemoteIPHost = config.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty);
        this.Logger ??= this.Container.Resolve<ILog>();
        this.ReceiveType = config.GetValue(TouchSocketConfigExtension.ReceiveTypeProperty);
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
        this.DataHandlingAdapter = adapter;
    }

    private void BeginReceive()
    {
        if (this.Config.GetValue(TouchSocketConfigExtension.SslOptionProperty) != null)
        {
            this.UseSsl = true;
        }
        if (this.UseSsl)
        {
            var sslOption = (ClientSslOption)this.Config.GetValue(TouchSocketConfigExtension.SslOptionProperty);
            var sslStream = (sslOption.CertificateValidationCallback != null) ? new SslStream(new NetworkStream(this.MainSocket, false), false, sslOption.CertificateValidationCallback) : new SslStream(new NetworkStream(this.MainSocket, false), false);
            if (sslOption.ClientCertificates == null)
            {
                sslStream.AuthenticateAsClient(sslOption.TargetHost);
            }
            else
            {
                sslStream.AuthenticateAsClient(sslOption.TargetHost, sslOption.ClientCertificates, sslOption.SslProtocols, sslOption.CheckCertificateRevocation);
            }
            this.m_workStream = sslStream;
            if (this.ReceiveType != ReceiveType.None)
            {
                this.BeginSsl();
            }
        }
        else
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
            else if (this.ReceiveType == ReceiveType.Bio)
            {
                new Thread(this.BeginBio)
                {
                    IsBackground = true
                }
                .Start();
            }
        }
    }

    private void BeginBio()
    {
        while (true)
        {
            var byteBlock = new ByteBlock(this.ReceiveBufferSize);
            try
            {
                var r = this.MainSocket.Receive(byteBlock.Buffer);
                if (r == 0)
                {
                    this.BreakOut("远程终端主动关闭");
                    return;
                }

                byteBlock.SetLength(r);
                this.HandleBuffer(byteBlock);
            }
            catch (Exception ex)
            {
                this.BreakOut(ex.Message);
                return;
            }
        }
    }

    private void BeginSsl()
    {
        var byteBlock = new ByteBlock(this.ReceiveBufferSize);
        try
        {
            this.m_workStream.BeginRead(byteBlock.Buffer, 0, byteBlock.Capacity, this.EndSsl, byteBlock);
        }
        catch (Exception ex)
        {
            byteBlock.Dispose();
            this.BreakOut(ex.Message);
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
                this.BreakOut("远程终端主动关闭");
            }
            byteBlock.SetLength(r);
            this.HandleBuffer(byteBlock);
            this.BeginSsl();
        }
        catch (Exception ex)
        {
            byteBlock.Dispose();
            this.BreakOut(ex.Message);
        }
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
            this.BreakOut(ex.Message);
        }
    }

    /// <summary>
    /// 处理数据
    /// </summary>
    private void HandleBuffer(ByteBlock byteBlock)
    {
        try
        {
            this.m_receiveCounter.Increment(byteBlock.Length);
            if (this.OnHandleRawBuffer?.Invoke(byteBlock) == false)
            {
                return;
            }
            if (this.DisposedValue)
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
    /// <param name="buffer"><inheritdoc/></param>
    /// <param name="offset"><inheritdoc/></param>
    /// <param name="length"><inheritdoc/></param>
    /// <exception cref="NotConnectedException"><inheritdoc/></exception>
    /// <exception cref="OverlengthException"><inheritdoc/></exception>
    /// <exception cref="Exception"><inheritdoc/></exception>
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
    /// <param name="transferBytes"><inheritdoc/></param>
    /// <exception cref="NotConnectedException"><inheritdoc/></exception>
    /// <exception cref="OverlengthException"><inheritdoc/></exception>
    /// <exception cref="Exception"><inheritdoc/></exception>
    public virtual Task SendAsync(IList<ArraySegment<byte>> transferBytes)
    {
        return Task.Run(() =>
        {
            this.Send(transferBytes);
        });
    }

    #endregion 异步发送

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="buffer"><inheritdoc/></param>
    /// <param name="offset"><inheritdoc/></param>
    /// <param name="length"><inheritdoc/></param>
    /// <exception cref="NotConnectedException"><inheritdoc/></exception>
    /// <exception cref="OverlengthException"><inheritdoc/></exception>
    /// <exception cref="Exception"><inheritdoc/></exception>
    public void DefaultSend(byte[] buffer, int offset, int length)
    {
        if (!this.m_online)
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
            this.m_delaySender = new DelaySender(delaySenderOption, this.MainSocket.AbsoluteSend);
        }
    }

    private void ProcessReceived(SocketAsyncEventArgs e)
    {
        if (!this.m_online)
        {
            e.SafeDispose();
            return;
        }
        if (e.SocketError != SocketError.Success)
        {
            e.SafeDispose();
            this.BreakOut(e.SocketError.ToString());
            return;
        }
        else if (e.BytesTransferred > 0)
        {
            var byteBlock = (ByteBlock)e.UserToken;
            byteBlock.SetLength(e.BytesTransferred);
            this.HandleBuffer(byteBlock);
            try
            {
                var newByteBlock = BytePool.Default.GetByteBlock((int)Math.Min(this.ReceiveBufferSize * this.m_bufferRate, TouchSocketUtility.MaxBufferLength));
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
                e.SafeDispose();
                this.BreakOut(ex.Message);
            }
        }
        else
        {
            e.SafeDispose();
            this.BreakOut("远程终端主动关闭");
        }
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return this.GetIPPort();
    }
}