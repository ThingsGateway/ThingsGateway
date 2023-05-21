//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.InteropServices;

using TouchSocket.Resources;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 简单TCP客户端
    /// </summary>
    public class TGTcpClient : TGTcpClientBase
    {
        /// <summary>
        /// 接收到数据
        /// </summary>
        public ReceivedEventHandler<TGTcpClient> Received { get; set; }
        /// <summary>
        /// 自定义锁
        /// </summary>
        public EasyLock EasyLock { get; set; } = new();

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        protected override void HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            this.Received?.Invoke(this, byteBlock, requestInfo);
            base.HandleReceivedData(byteBlock, requestInfo);
        }
    }

    /// <summary>
    /// TCP客户端
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{IP}:{Port}")]
    public class TGTcpClientBase : BaseSocket, ITcpClient
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public TGTcpClientBase()
        {
            this.Protocol = Protocol.TCP;
        }

        #region 变量

        private DelaySender m_delaySender;
        private bool m_useDelaySender;
        private Stream m_workStream;

        #endregion 变量

        #region 事件

        /// <summary>
        /// 成功连接到服务器
        /// </summary>
        public MessageEventHandler<ITcpClient> Connected { get; set; }

        /// <summary>
        /// 准备连接的时候，此时已初始化Socket，但是并未建立Tcp连接
        /// </summary>
        public ConnectingEventHandler<ITcpClient> Connecting { get; set; }

        /// <summary>
        /// 断开连接。在客户端未设置连接状态时，不会触发
        /// </summary>
        public DisconnectEventHandler<ITcpClientBase> Disconnected { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public DisconnectEventHandler<ITcpClientBase> Disconnecting { get; set; }

        private void PrivateOnConnected(MsgEventArgs e)
        {
            if (this.UsePlugin)
            {
                this.PluginsManager.Raise<IConnectedPlugin>(nameof(IConnectedPlugin.OnConnected), this, e);
                if (e.Handled)
                {
                    return;
                }
            }
            this.OnConnected(e);
        }

        /// <summary>
        /// 已经建立Tcp连接
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnConnected(MsgEventArgs e)
        {
            try
            {
                this.Connected?.Invoke(this, e);
            }
            catch (System.Exception ex)
            {
                this.Logger.Log(LogType.Error, this, $"在事件{nameof(this.Connected)}中发生错误。", ex);
            }
        }

        private void PrivateOnConnecting(ConnectingEventArgs e)
        {
            this.LastReceivedTime = DateTime.UtcNow;
            this.LastSendTime = DateTime.UtcNow;
            if (this.CanSetDataHandlingAdapter)
            {
                this.SetDataHandlingAdapter(this.Config.GetValue<Func<DataHandlingAdapter>>(TouchSocketConfigExtension.DataHandlingAdapterProperty).Invoke());
            }
            if (this.UsePlugin)
            {
                this.PluginsManager.Raise<IConnectingPlugin>(nameof(IConnectingPlugin.OnConnecting), this, e);
                if (e.Handled)
                {
                    return;
                }
            }
            this.OnConnecting(e);
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
                this.Logger.Log(LogType.Error, this, $"在事件{nameof(this.OnConnecting)}中发生错误。", ex);
            }
        }

        private void PrivateOnDisconnected(DisconnectEventArgs e)
        {
            if (this.UsePlugin && this.PluginsManager.Raise<IDisconnectedPlguin>(nameof(IDisconnectedPlguin.OnDisconnected), this, e))
            {
                return;
            }
            this.OnDisconnected(e);
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
                this.Logger.Log(LogType.Error, this, $"在事件{nameof(this.Disconnected)}中发生错误。", ex);
            }
        }

        private void PrivateOnDisconnecting(DisconnectEventArgs e)
        {
            if (this.UsePlugin && this.PluginsManager.Raise<IDisconnectingPlugin>(nameof(IDisconnectingPlugin.OnDisconnecting), this, e))
            {
                return;
            }
            this.OnDisconnecting(e);
        }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// <para>
        /// 当主动调用Close断开时，可通过<see cref="TouchSocketEventArgs.IsPermitOperation"/>终止断开行为。
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
                this.Logger.Log(LogType.Error, this, $"在事件{nameof(this.Disconnecting)}中发生错误。", ex);
            }
        }

        #endregion 事件

        #region 属性

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public DateTime LastReceivedTime { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public DateTime LastSendTime { get; private set; }

        /// <summary>
        /// 处理未经过适配器的数据。返回值表示是否继续向下传递。
        /// </summary>
        public Func<ByteBlock, bool> OnHandleRawBuffer { get; set; }

        /// <summary>
        /// 处理经过适配器后的数据。返回值表示是否继续向下传递。
        /// </summary>
        public Func<ByteBlock, IRequestInfo, bool> OnHandleReceivedData { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IContainer Container => this.Config?.Container;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual bool CanSetDataHandlingAdapter => true;

        /// <summary>
        /// 客户端配置
        /// </summary>
        public TouchSocketConfig Config { get; private set; }

        /// <summary>
        /// 数据处理适配器
        /// </summary>
        public DataHandlingAdapter DataHandlingAdapter { get; private set; }

        /// <summary>
        /// IP地址
        /// </summary>
        public string IP { get; private set; }

        /// <summary>
        /// 主通信器
        /// </summary>
        public Socket MainSocket { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool CanSend { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool Online => this.CanSend;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IPluginsManager PluginsManager { get; private set; }

        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ReceiveType ReceiveType { get; private set; }

        /// <summary>
        /// 是否已启用插件
        /// </summary>
        public bool UsePlugin { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool UseSsl { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Protocol Protocol { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IPHost RemoteIPHost { get; private set; }

        /// <inheritdoc/>
        public bool IsClient => true;

        #endregion 属性

        #region 断开操作

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual void Close()
        {
            this.Close($"{nameof(Close)}主动断开");
        }

        /// <summary>
        /// 中断终端，传递中断消息。
        /// </summary>
        /// <param name="msg"></param>
        public virtual void Close(string msg)
        {
            if (this.CanSend)
            {
                var args = new DisconnectEventArgs(true, msg)
                {
                    IsPermitOperation = true
                };
                this.PrivateOnDisconnecting(args);
                if (this.DisposedValue || args.IsPermitOperation)
                {
                    this.BreakOut(msg, true);
                }
            }
        }

        private  void BreakOut(string msg, bool manual)
        {
            try
            {
                privateEasyLock.Lock();
                if (this.CanSend)
                {
                    this.CanSend = false;
                    this.TryShutdown();
                    this.MainSocket.SafeDispose();
                    this.m_delaySender.SafeDispose();
                    this.m_workStream.SafeDispose();
                    this.DataHandlingAdapter.SafeDispose();
                    this.PrivateOnDisconnected(new DisconnectEventArgs(manual, msg));
                }
            }
            finally
            {
                privateEasyLock.UnLock();
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (this.CanSend)
            {
                var args = new DisconnectEventArgs(true, $"{nameof(Dispose)}主动断开");
                this.PrivateOnDisconnecting(args);
            }
            this.PluginsManager?.Clear();
            this.Config = default;
            this.DataHandlingAdapter.SafeDispose();
            this.DataHandlingAdapter = default;
            this.BreakOut($"{nameof(Dispose)}主动断开", true);
            base.Dispose(disposing);
        }

        #endregion 断开操作

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
            try
            {
                 privateEasyLock.Lock();

                if (this.CanSend)
                {
                    return;
                }
                if (this.DisposedValue)
                {
                    throw new ObjectDisposedException(this.GetType().FullName);
                }
                if (this.Config == null)
                {
                    throw new ArgumentNullException("配置文件不能为空。");
                }
                IPHost iPHost = this.Config.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty);
                if (iPHost == null)
                {
                    throw new ArgumentNullException("iPHost不能为空。");
                }
                if (this.MainSocket != null)
                {
                    this.MainSocket.Dispose();
                }
                this.MainSocket = this.CreateSocket(iPHost);

                ConnectingEventArgs args = new ConnectingEventArgs(this.MainSocket);
                this.PrivateOnConnecting(args);
                if (timeout == 5000)
                {
                    this.MainSocket.Connect(iPHost.EndPoint);
                    this.CanSend = true;
                    this.LoadSocketAndReadIpPort();

                    if (this.Config.GetValue(TouchSocketConfigExtension.DelaySenderProperty) is DelaySenderOption senderOption)
                    {
                        this.m_useDelaySender = true;
                        this.m_delaySender.SafeDispose();
                        this.m_delaySender = new DelaySender(this.MainSocket, senderOption.QueueLength, this.OnDelaySenderError)
                        {
                            DelayLength = senderOption.DelayLength
                        };
                    }

                    this.BeginReceive();
                    this.PrivateOnConnected(new MsgEventArgs("连接成功"));
                }
                else
                {
                    var result = this.MainSocket.BeginConnect(iPHost.EndPoint, null, null);
                    if (result.AsyncWaitHandle.WaitOne(timeout))
                    {
                        if (this.MainSocket.Connected)
                        {
                            this.MainSocket.EndConnect(result);
                            this.CanSend = true;
                            this.LoadSocketAndReadIpPort();

                            if (this.Config.GetValue(TouchSocketConfigExtension.DelaySenderProperty) is DelaySenderOption senderOption)
                            {
                                this.m_useDelaySender = true;
                                this.m_delaySender.SafeDispose();
                                this.m_delaySender = new DelaySender(this.MainSocket, senderOption.QueueLength, this.OnDelaySenderError)
                                {
                                    DelayLength = senderOption.DelayLength
                                };
                            }

                            this.BeginReceive();
                            this.PrivateOnConnected(new MsgEventArgs("连接成功"));
                            return;
                        }
                        else
                        {
                            this.MainSocket.SafeDispose();
                            throw new Exception("异步已完成，但是socket并未在连接状态，可能发生了绑定端口占用的错误。");
                        }
                    }
                    this.MainSocket.SafeDispose();
                    throw new TimeoutException();
                }
            }

            finally
            {
                privateEasyLock.UnLock();

            }

        }

        /// <summary>
        /// 请求连接到服务器。
        /// </summary>
        public virtual ITcpClient Connect(int timeout = 5000)
        {
            this.TcpConnect(timeout);
            return this;
        }

        private EasyLock privateEasyLock { get; set; } = new();

        /// <summary>
        /// 异步连接服务器
        /// </summary>
        public async Task<ITcpClient> ConnectAsync(int timeout = 5000)
        {
            try
            {
                await privateEasyLock.LockAsync();

                if (this.CanSend)
                {
                    return this;
                }
                if (this.DisposedValue)
                {
                    throw new ObjectDisposedException(this.GetType().FullName);
                }
                if (this.Config == null)
                {
                    throw new ArgumentNullException("配置文件不能为空。");
                }
                IPHost iPHost = this.Config.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty);
                if (iPHost == null)
                {
                    throw new ArgumentNullException("iPHost不能为空。");
                }
                if (this.MainSocket != null)
                {
                    this.MainSocket.Dispose();
                }
                this.MainSocket = this.CreateSocket(iPHost);

                ConnectingEventArgs args = new ConnectingEventArgs(this.MainSocket);
                this.PrivateOnConnecting(args);
                {
#if (NET6_0_OR_GREATER)
                    var nowTime = DateTime.UtcNow;
                    using CancellationTokenSource cancellationTokenSource = new();
                    var task = this.MainSocket.ConnectAsync(iPHost.EndPoint, cancellationTokenSource.Token).AsTask();
                    var result = await Task.WhenAny(task, Task.Delay(timeout, cancellationTokenSource.Token));
                    if (DateTime.UtcNow - nowTime < TimeSpan.FromMilliseconds(timeout))
                    {
                        await Task.Delay(500);
                    }
                    if (result == task)
                    {
                        cancellationTokenSource.Cancel();
                        if (task.Exception != null)
                        {
                            if (DateTime.UtcNow - nowTime < TimeSpan.FromMilliseconds(timeout))
                            {
                                await Task.Delay(timeout);
                            }
                            throw task.Exception;
                        }
                    }
                    else
                    {
                        cancellationTokenSource.Cancel();
                    }
#else

                    var nowTime = DateTime.UtcNow;
                    using CancellationTokenSource cancellationTokenSource = new();
                    var task = Task.Factory.FromAsync(this.MainSocket.BeginConnect(iPHost.EndPoint, null, null), this.MainSocket.EndConnect);
                    var result = await Task.WhenAny(task, Task.Delay(timeout, cancellationTokenSource.Token));
                    if (DateTime.UtcNow - nowTime < TimeSpan.FromMilliseconds(timeout))
                    {
                        await Task.Delay(500);
                    }
                    if (result == task)
                    {
                        cancellationTokenSource.Cancel();
                        if (task.Exception != null)
                        {
                            if (DateTime.UtcNow - nowTime < TimeSpan.FromMilliseconds(timeout))
                            {
                                await Task.Delay(timeout);
                            }
                            throw task.Exception;
                        }
                    }
                    else
                    {
                        this.MainSocket.Close();
                    }

#endif


                    if (this.MainSocket.Connected)
                    {
                        this.CanSend = true;
                        this.LoadSocketAndReadIpPort();

                        if (this.Config.GetValue(TouchSocketConfigExtension.DelaySenderProperty) is DelaySenderOption senderOption)
                        {
                            this.m_useDelaySender = true;
                            this.m_delaySender.SafeDispose();
                            this.m_delaySender = new DelaySender(this.MainSocket, senderOption.QueueLength, this.OnDelaySenderError)
                            {
                                DelayLength = senderOption.DelayLength
                            };
                        }

                        this.BeginReceive();
                        this.PrivateOnConnected(new MsgEventArgs("连接成功"));
                    }
                    else
                    {
                        this.MainSocket.SafeDispose();
                        throw new Exception("连接失败");
                    }
                }
            }
            finally
            {
                privateEasyLock.UnLock();
            }
            return this;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public Stream GetStream()
        {
            if (this.m_workStream == null)
            {
                this.m_workStream = new NetworkStream(this.MainSocket, true);
            }
            return this.m_workStream;
        }

        /// <summary>
        /// 设置数据处理适配器
        /// </summary>
        /// <param name="adapter"></param>
        public virtual void SetDataHandlingAdapter(DataHandlingAdapter adapter)
        {
            if (!this.CanSetDataHandlingAdapter)
            {
                throw new Exception($"不允许自由调用{nameof(SetDataHandlingAdapter)}进行赋值。");
            }

            this.SetAdapter(adapter);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="ipHost"></param>
        /// <returns></returns>
        public ITcpClient Setup(string ipHost)
        {
            TouchSocketConfig config = new TouchSocketConfig();
            config.SetRemoteIPHost(new IPHost(ipHost));
            return this.Setup(config);
        }

        /// <summary>
        /// 配置服务器
        /// </summary>
        /// <param name="config"></param>
        /// <exception cref="Exception"></exception>
        public ITcpClient Setup(TouchSocketConfig config)
        {
            this.Config = config;
            this.PluginsManager = config.PluginsManager;

            if (config.IsUsePlugin)
            {
                this.PluginsManager.Raise<IConfigPlugin>(nameof(IConfigPlugin.OnLoadingConfig), this, new ConfigEventArgs(config));
            }
            this.LoadConfig(this.Config);
            if (this.UsePlugin)
            {
                this.PluginsManager.Raise<IConfigPlugin>(nameof(IConfigPlugin.OnLoadedConfig), this, new ConfigEventArgs(config));
            }
            return this;
        }

        private void PrivateHandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            if (this.OnHandleReceivedData?.Invoke(byteBlock, requestInfo) == false)
            {
                return;
            }
            if (this.UsePlugin)
            {
                ReceivedDataEventArgs args = new ReceivedDataEventArgs(byteBlock, requestInfo);
                this.PluginsManager.Raise<ITcpPlugin>(nameof(ITcpPlugin.OnReceivedData), this, args);
                if (args.Handled)
                {
                    return;
                }
            }

            this.HandleReceivedData(byteBlock, requestInfo);
        }

        /// <summary>
        /// 处理已接收到的数据。
        /// </summary>
        /// <param name="byteBlock">以二进制流形式传递</param>
        /// <param name="requestInfo">以解析的数据对象传递</param>
        protected virtual void HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
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
            if (this.UsePlugin)
            {
                SendingEventArgs args = new SendingEventArgs(buffer, offset, length);
                this.PluginsManager.Raise<ITcpPlugin>(nameof(ITcpPlugin.OnSendingData), this, args);
                if (args.IsPermitOperation)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="config"></param>
        protected virtual void LoadConfig(TouchSocketConfig config)
        {
            if (config == null)
            {
                throw new Exception("配置文件为空");
            }
            this.RemoteIPHost = config.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty);
            this.BufferLength = config.GetValue(TouchSocketConfigExtension.BufferLengthProperty);
            this.ReceiveType = config.GetValue(TouchSocketConfigExtension.ReceiveTypeProperty);
            this.UsePlugin = config.IsUsePlugin;
            this.Logger = this.Container.Resolve<ILog>();

            if (config.GetValue(TouchSocketConfigExtension.SslOptionProperty) != null)
            {
                this.UseSsl = true;
            }
        }

        /// <summary>
        /// 在延迟发生错误
        /// </summary>
        /// <param name="ex"></param>
        protected virtual void OnDelaySenderError(Exception ex)
        {
            this.Logger.Log(LogType.Error, this, "发送错误", ex);
        }

        /// <summary>
        /// 设置适配器，该方法不会检验<see cref="CanSetDataHandlingAdapter"/>的值。
        /// </summary>
        /// <param name="adapter"></param>
        protected void SetAdapter(DataHandlingAdapter adapter)
        {
            if (adapter is null)
            {
                throw new ArgumentNullException(nameof(adapter));
            }

            if (this.Config != null)
            {
                if (this.Config.GetValue(TouchSocketConfigExtension.MaxPackageSizeProperty) is int v1)
                {
                    adapter.MaxPackageSize = v1;
                }
                if (this.Config.GetValue(TouchSocketConfigExtension.CacheTimeoutProperty) != TimeSpan.Zero)
                {
                    adapter.CacheTimeout = this.Config.GetValue(TouchSocketConfigExtension.CacheTimeoutProperty);
                }
                if (this.Config.GetValue(TouchSocketConfigExtension.CacheTimeoutEnableProperty) is bool v2)
                {
                    adapter.CacheTimeoutEnable = v2;
                }
                if (this.Config.GetValue(TouchSocketConfigExtension.UpdateCacheTimeWhenRevProperty) is bool v3)
                {
                    adapter.UpdateCacheTimeWhenRev = v3;
                }
            }

            adapter.OnLoaded(this);
            adapter.ReceivedCallBack = this.PrivateHandleReceivedData;
            adapter.SendCallBack = this.DefaultSend;
            this.DataHandlingAdapter = adapter;
        }

        private void BeginReceive()
        {
            this.m_workStream.SafeDispose();
            if (this.UseSsl)
            {
                ClientSslOption sslOption = (ClientSslOption)this.Config.GetValue(TouchSocketConfigExtension.SslOptionProperty);
                SslStream sslStream = (sslOption.CertificateValidationCallback != null) ? new SslStream(new NetworkStream(this.MainSocket, false), false, sslOption.CertificateValidationCallback) : new SslStream(new NetworkStream(this.MainSocket, false), false);
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
                if (this.ReceiveType == ReceiveType.Auto)
                {
                    SocketAsyncEventArgs eventArgs = new SocketAsyncEventArgs();
                    eventArgs.Completed += this.EventArgs_Completed;

                    ByteBlock byteBlock = BytePool.Default.GetByteBlock(this.BufferLength);
                    eventArgs.UserToken = byteBlock;
                    eventArgs.SetBuffer(byteBlock.Buffer, 0, byteBlock.Buffer.Length);
                    if (!this.MainSocket.ReceiveAsync(eventArgs))
                    {
                        this.ProcessReceived(eventArgs);
                    }
                }
            }
        }

        private void BeginSsl()
        {
            ByteBlock byteBlock = new ByteBlock(this.BufferLength);
            try
            {
                this.m_workStream.BeginRead(byteBlock.Buffer, 0, byteBlock.Capacity, this.EndSsl, byteBlock);
            }
            catch (Exception ex)
            {
                byteBlock.Dispose();
                this.BreakOut(ex.Message, false);
            }
        }

        private void EndSsl(IAsyncResult result)
        {
            ByteBlock byteBlock = (ByteBlock)result.AsyncState;
            try
            {
                int r = this.m_workStream.EndRead(result);
                if (r == 0)
                {
                    this.BreakOut("远程终端主动关闭", false);
                }
                byteBlock.SetLength(r);
                this.HandleBuffer(byteBlock);
                this.BeginSsl();
            }
            catch (System.Exception ex)
            {
                byteBlock.Dispose();
                this.BreakOut(ex.Message, false);
            }
        }

        private Socket CreateSocket(IPHost iPHost)
        {
            Socket socket = new Socket(iPHost.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.ReceiveBufferSize = this.BufferLength;
            socket.SendBufferSize = this.BufferLength;
#if NET45_OR_GREATER
            KeepAliveValue keepAliveValue = this.Config.GetValue<KeepAliveValue>(TouchSocketConfigExtension.KeepAliveValueProperty);
            if (keepAliveValue.Enable)
            {
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                socket.IOControl(IOControlCode.KeepAliveValues, keepAliveValue.KeepAliveTime, null);
            }
#else
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                KeepAliveValue keepAliveValue = Config.GetValue<KeepAliveValue>(TouchSocketConfigExtension.KeepAliveValueProperty);
                if (keepAliveValue.Enable)
                {
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    socket.IOControl(IOControlCode.KeepAliveValues, keepAliveValue.KeepAliveTime, null);
                }
            }
#endif
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, this.Config.GetValue<bool>(TouchSocketConfigExtension.NoDelayProperty));
            if (this.Config.GetValue<IPHost>(TouchSocketConfigExtension.BindIPHostProperty) != null)
            {
                if (this.Config.GetValue<bool>(TouchSocketConfigExtension.ReuseAddressProperty))
                {
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                }
                socket.Bind(this.Config.GetValue<IPHost>(TouchSocketConfigExtension.BindIPHostProperty).EndPoint);
            }
            return socket;
        }

        private void EventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                this.ProcessReceived(e);
            }
            catch (System.Exception ex)
            {
                e.Dispose();
                this.BreakOut(ex.Message, false);
            }
        }

        /// <summary>
        /// 处理数据
        /// </summary>
        private void HandleBuffer(ByteBlock byteBlock)
        {
            try
            {
                this.LastReceivedTime = DateTime.UtcNow;
                if (this.OnHandleRawBuffer?.Invoke(byteBlock) == false)
                {
                    return;
                }
                if (this.DisposedValue)
                {
                    return;
                }
                if (this.UsePlugin && this.PluginsManager.Raise<ITcpPlugin>(nameof(ITcpPlugin.OnReceivingData), this, new ByteBlockEventArgs(byteBlock)))
                {
                    return;
                }
                if (this.DataHandlingAdapter == null)
                {
                    this.Logger.Error(this, TouchSocketStatus.NullDataAdapter.GetDescription());
                    return;
                }
                this.DataHandlingAdapter.ReceivedInput(byteBlock);
            }
            catch (Exception ex)
            {
                this.Logger.Log(LogType.Error, this, "在处理数据时发生错误", ex);
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
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketStatus.NullDataAdapter.GetDescription());
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
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketStatus.NullDataAdapter.GetDescription());
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
                throw new ArgumentNullException(nameof(this.DataHandlingAdapter), TouchSocketStatus.NullDataAdapter.GetDescription());
            }

            if (this.DataHandlingAdapter.CanSplicingSend)
            {
                this.DataHandlingAdapter.SendInput(transferBytes);
            }
            else
            {
                ByteBlock byteBlock = BytePool.Default.GetByteBlock(this.BufferLength);
                try
                {
                    foreach (var item in transferBytes)
                    {
                        byteBlock.Write(item.Array, item.Offset, item.Count);
                    }
                    this.DataHandlingAdapter.SendInput(byteBlock.Buffer, 0, byteBlock.Len);
                }
                finally
                {
                    byteBlock.Dispose();
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
            return EasyTask.Run(() =>
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
            return EasyTask.Run(() =>
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
            return EasyTask.Run(() =>
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
            if (!this.CanSend)
            {
                throw new NotConnectedException(TouchSocketStatus.NotConnected.GetDescription());
            }
            if (this.HandleSendingData(buffer, offset, length))
            {
                if (this.UseSsl)
                {
                    this.m_workStream.Write(buffer, offset, length);
                }
                else
                {
                    if (this.m_useDelaySender && length < TouchSocketUtility.BigDataBoundary)
                    {
                        this.m_delaySender.Send(new QueueDataBytes(buffer, offset, length));
                    }
                    else
                    {
                        this.MainSocket.AbsoluteSend(buffer, offset, length);
                    }
                }

                this.LastSendTime = DateTime.UtcNow;
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
            return EasyTask.Run(() =>
            {
                this.DefaultSend(buffer, offset, length);
            });
        }

        #endregion 发送

        private void LoadSocketAndReadIpPort()
        {
            if (this.MainSocket == null)
            {
                this.IP = null;
                this.Port = -1;
                return;
            }

            string ipport;
            if (this.MainSocket.Connected && this.MainSocket.RemoteEndPoint != null)
            {
                ipport = this.MainSocket.RemoteEndPoint.ToString();
            }
            else if (this.MainSocket.IsBound && this.MainSocket.LocalEndPoint != null)
            {
                ipport = this.MainSocket.LocalEndPoint.ToString();
            }
            else
            {
                return;
            }

            int r = ipport.LastIndexOf(":");
            this.IP = ipport.Substring(0, r);
            this.Port = Convert.ToInt32(ipport.Substring(r + 1, ipport.Length - (r + 1)));
        }

        private void ProcessReceived(SocketAsyncEventArgs e)
        {
            if (!this.CanSend)
            {
                e.Dispose();
                return;
            }
            if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
            {
                ByteBlock byteBlock = (ByteBlock)e.UserToken;
                byteBlock.SetLength(e.BytesTransferred);
                this.HandleBuffer(byteBlock);
                try
                {
                    ByteBlock newByteBlock = BytePool.Default.GetByteBlock(this.BufferLength);
                    e.UserToken = newByteBlock;
                    e.SetBuffer(newByteBlock.Buffer, 0, newByteBlock.Buffer.Length);

                    if (!this.MainSocket.ReceiveAsync(e))
                    {
                        this.ProcessReceived(e);
                    }
                }
                catch (System.Exception ex)
                {
                    e.Dispose();
                    this.BreakOut(ex.Message, false);
                }
            }
            else
            {
                e.Dispose();
                this.BreakOut("远程终端主动关闭", false);
            }
        }
    }
}