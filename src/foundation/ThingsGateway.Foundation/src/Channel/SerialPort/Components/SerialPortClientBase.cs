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

using System.IO.Ports;

using TouchSocket.Resources;

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 串口客户端基类
    /// </summary>
    public class SerialPortClientBase : SetupConfigObject, ISerialPortClient
    {
        /// <summary>
        /// 串口客户端基类
        /// </summary>
        public SerialPortClientBase()
        {
            this.Protocol = SerialPortUtility.SerialPort;
        }

        /// <summary>
        /// 串口客户端基类
        /// </summary>
        ~SerialPortClientBase()
        {
            Dispose(true);
        }

        #region 变量

        private readonly EasyLock m_semaphore = new();
        private readonly InternalSerialCore m_serialCore = new InternalSerialCore();
        private DelaySender m_delaySender;
        private bool m_online => MainSerialPort?.IsOpen == true;

        #endregion 变量

        #region 事件

        /// <inheritdoc/>
        public ConnectedEventHandler<ISerialPortClient> Connected { get; set; }

        /// <inheritdoc/>
        public SerialConnectingEventHandler<ISerialPortClient> Connecting { get; set; }

        /// <inheritdoc/>
        public DisconnectEventHandler<ISerialPortClient> Disconnected { get; set; }

        /// <inheritdoc/>
        public DisconnectEventHandler<ISerialPortClient> Disconnecting { get; set; }

        /// <summary>
        /// 已经建立连接
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
                await this.PluginManager.RaiseAsync(nameof(ISerialConnectedPlugin.OnSerialConnected), this, e);
            }
            catch (Exception ex)
            {
                this.Logger.Log(LogLevel.Error, this, string.Format(FoundationConst.EventError, nameof(this.Connected)), ex);
            }
        }

        /// <summary>
        /// 准备连接的时候，此时并未建立连接
        /// </summary>
        /// <param name="e"></param>
        protected virtual async Task OnConnecting(SerialConnectingEventArgs e)
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
                await this.PluginManager.RaiseAsync(nameof(ISerialConnectingPlugin.OnSerialConnecting), this, e);
            }
            catch (Exception ex)
            {
                this.Logger.Log(LogLevel.Error, this, string.Format(FoundationConst.EventError, nameof(this.OnConnecting)), ex);
            }
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

                await this.PluginManager.RaiseAsync(nameof(ISerialDisconnectedPlugin.OnSerialDisconnected), this, e).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.Logger.Log(LogLevel.Error, this, string.Format(FoundationConst.EventError, nameof(this.Disconnected)), ex);
            }
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

                await this.PluginManager.RaiseAsync(nameof(ISerialDisconnectingPlugin.OnSerialDisconnecting), this, e).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.Logger.Log(LogLevel.Error, this, string.Format(FoundationConst.EventError, nameof(this.Disconnecting)), ex);
            }
        }

        private Task PrivateOnConnected(ConnectedEventArgs o)
        {
            return this.OnConnected(o);
        }

        private Task PrivateOnConnecting(SerialConnectingEventArgs e)
        {
            if (this.CanSetDataHandlingAdapter)
            {
                this.SetDataHandlingAdapter(this.Config.GetValue(SerialPortConfigExtension.SerialDataHandlingAdapterProperty).Invoke());
            }

            return this.OnConnecting(e);
        }

        private Task PrivateOnDisconnected(object obj)
        {
            this.m_receiver?.TryInputReceive(default, default);
            return this.OnDisconnected((DisconnectEventArgs)obj);
        }

        private Task PrivateOnDisconnecting(object obj)
        {
            return this.OnDisconnecting((DisconnectEventArgs)obj);
        }

        #endregion 事件

        #region 属性

        /// <inheritdoc/>
        public bool CanSend => this.m_online;

        /// <inheritdoc/>
        public virtual bool CanSetDataHandlingAdapter => true;

        /// <inheritdoc/>
        public SingleStreamDataHandlingAdapter DataHandlingAdapter { get; private set; }

        /// <inheritdoc/>
        public DateTime LastReceivedTime => this.GetSerialCore().ReceiveCounter.LastIncrement;

        /// <inheritdoc/>
        public DateTime LastSendTime => this.GetSerialCore().SendCounter.LastIncrement;

        /// <inheritdoc/>
        public SerialPort MainSerialPort { get; private set; }

        /// <inheritdoc/>
        public bool Online { get => this.m_online; }

        /// <inheritdoc/>
        public Protocol Protocol { get; set; }

        #endregion 属性

        #region 断开操作

        /// <inheritdoc/>
        public virtual void Close(string msg = TouchSocketCoreUtility.Empty)
        {
            lock (this.GetSerialCore())
            {
                if (this.m_online)
                {
                    this.PrivateOnDisconnecting(new DisconnectEventArgs(true, msg)).GetFalseAwaitResult();
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
            if (DisposedValue) return;
            lock (this.GetSerialCore())
            {
                if (this.m_online)
                {
                    this.PrivateOnDisconnecting(new DisconnectEventArgs(true, string.Format(FoundationConst.ProactivelyDisconnect, nameof(Dispose)))).GetFalseAwaitResult();
                    this.BreakOut(true, string.Format(FoundationConst.ProactivelyDisconnect, nameof(Dispose)));
                }
            }
            base.Dispose(disposing);
        }

        #endregion 断开操作

        #region Connect

        /// <inheritdoc/>
        public void Connect(int timeout, CancellationToken token)
        {
            this.Open();
        }

        /// <inheritdoc/>
        public Task ConnectAsync(int timeout, CancellationToken token)
        {
            this.Open();
            return EasyTask.CompletedTask;
        }

        /// <summary>
        /// 打开串口
        /// </summary>
        protected void Open()
        {
            try
            {
                this.ThrowIfDisposed();
                this.m_semaphore.Wait();
                if (this.m_online)
                {
                    return;
                }
                if (this.Config == null)
                {
                    throw new ArgumentNullException(nameof(this.Config), FoundationConst.ConfigNotNull);
                }
                var serialPortOption = this.Config.GetValue(SerialPortConfigExtension.SerialPortOptionProperty) ?? throw new ArgumentNullException(FoundationConst.ConfigNotNull);
                this.MainSerialPort?.SafeDispose();
                var serialPort = CreateSerial(serialPortOption);
                this.PrivateOnConnecting(new SerialConnectingEventArgs(serialPort)).ConfigureAwait(false).GetAwaiter().GetResult();

                serialPort.Open();

                this.SetSerialPort(serialPort);
                this.BeginReceive();

                this.PrivateOnConnected(new ConnectedEventArgs()).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            finally
            {
                this.m_semaphore.Release();
            }
        }

        private void BeginReceive()
        {
            _ = this.GetSerialCore().BeginReceive();
        }

        #endregion Connect

        #region TgReceiver

        private TgReceiver m_receiver;

        /// <inheritdoc/>
        public void ClearReceiver()
        {
            this.m_receiver = null;
        }

        /// <inheritdoc/>
        public IReceiver CreateReceiver()
        {
            return this.m_receiver ??= new TgReceiver(this);
        }

        #endregion TgReceiver

        /// <inheritdoc/>
        public virtual void SetDataHandlingAdapter(SingleStreamDataHandlingAdapter adapter)
        {
            if (!this.CanSetDataHandlingAdapter)
            {
                throw new Exception(string.Format(FoundationConst.CannotSet, nameof(SetDataHandlingAdapter)));
            }

            this.SetAdapter(adapter);
        }

        /// <summary>
        /// BreakOut。
        /// </summary>
        /// <param name="manual"></param>
        /// <param name="msg"></param>
        protected void BreakOut(bool manual, string msg)
        {
            lock (this.GetSerialCore())
            {
                if (this.m_online)
                {
                    this.MainSerialPort.SafeDispose();
                    this.m_delaySender.SafeDispose();
                    this.DataHandlingAdapter.SafeDispose();
                    this.PrivateOnDisconnected(new DisconnectEventArgs(manual, msg)).GetFalseAwaitResult();
                }
            }
        }

        /// <summary>
        /// 当收到适配器处理的数据时。
        /// </summary>
        /// <param name="e"></param>
        /// <returns>如果返回<see langword="true"/>则表示数据已被处理，且不会再向下传递。</returns>
        protected virtual async Task ReceivedData(ReceivedDataEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }
            if (PluginManager != null)
                await this.PluginManager.RaiseAsync(nameof(ISerialReceivedPlugin.OnSerialReceived), this, e).ConfigureFalseAwait();
        }

        /// <summary>
        /// 当收到原始数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <returns>如果返回<see langword="true"/>则表示数据已被处理，且不会再向下传递。</returns>
        protected virtual Task<bool> ReceivingData(ByteBlock byteBlock)
        {
            if (this.PluginManager.GetPluginCount(nameof(ISerialReceivingPlugin.OnSerialReceiving)) > 0)
            {
                return this.PluginManager.RaiseAsync(nameof(ISerialReceivingPlugin.OnSerialReceiving), this, new ByteBlockEventArgs(byteBlock));
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
            if (this.PluginManager.GetPluginCount(nameof(ISerialSendingPlugin.OnSerialSending)) > 0)
            {
                var args = new SendingEventArgs(buffer, offset, length);
                await this.PluginManager.RaiseAsync(nameof(ISerialSendingPlugin.OnSerialSending), this, args).ConfigureFalseAwait();
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

        private SerialPort CreateSerial(SerialPortOption serialPortOption)
        {
            var serialPort = new SerialPort(serialPortOption.PortName, serialPortOption.BaudRate, serialPortOption.Parity, serialPortOption.DataBits, serialPortOption.StopBits)
            {
                DtrEnable = serialPortOption.DtrEnable,
                RtsEnable = serialPortOption.RtsEnable,
            };
            return serialPort;
        }

        private SerialCore GetSerialCore()
        {
            this.ThrowIfDisposed();
            return this.m_serialCore ?? throw new ObjectDisposedException(this.GetType().Name);
        }

        private void HandleReceived(SerialCore core, ByteBlock byteBlock)
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
                this.Logger.Log(LogLevel.Error, this, FoundationConst.ReceiveError, ex);
            }
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

        private void SerialCoreBreakOut(SerialCore core, bool manual, string msg)
        {
            this.BreakOut(manual, msg);
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
                throw new NotSupportedException(FoundationConst.CannotSendIRequestInfo);
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
                throw new NotSupportedException(FoundationConst.CannotSendIRequestInfo);
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
                this.GetSerialCore().Send(buffer, offset, length);
            }
        }

        /// <inheritdoc/>
        public async Task DefaultSendAsync(byte[] buffer, int offset, int length)
        {
            if (await this.SendingData(buffer, offset, length))
            {
                await this.GetSerialCore().SendAsync(buffer, offset, length);
            }
        }

        #endregion 发送

        private void SetSerialPort(SerialPort serialPort)
        {
            if (serialPort == null)
            {
                return;
            }

            this.MainSerialPort = serialPort;
            var delaySenderOption = this.Config.GetValue(TouchSocketConfigExtension.DelaySenderProperty);
            if (delaySenderOption != null)
            {
                this.m_delaySender = new DelaySender(delaySenderOption, this.GetSerialCore().Send);
            }
            this.m_serialCore.Reset(serialPort);
            this.m_serialCore.OnReceived = this.HandleReceived;
            this.m_serialCore.OnBreakOut = this.SerialCoreBreakOut;
            if (this.Config.GetValue(TouchSocketConfigExtension.MinBufferSizeProperty) is int minValue)
            {
                this.m_serialCore.MinBufferSize = minValue;
            }

            if (this.Config.GetValue(TouchSocketConfigExtension.MaxBufferSizeProperty) is int maxValue)
            {
                this.m_serialCore.MaxBufferSize = maxValue;
            }
        }

        /// <inheritdoc/>
        public override string? ToString()
        {
            if (MainSerialPort != null)
                return $"{MainSerialPort.PortName}[{MainSerialPort.BaudRate},{MainSerialPort.DataBits},{MainSerialPort.StopBits},{MainSerialPort.Parity}]";
            return base.ToString();
        }
    }
}