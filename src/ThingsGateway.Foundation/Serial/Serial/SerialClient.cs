#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using System.IO.Ports;

using TouchSocket.Resources;

namespace ThingsGateway.Foundation.Serial
{
    /// <inheritdoc cref="SerialClientBase"/>
    public class SerialClient : SerialClientBase
    {
        /// <summary>
        /// 接收到数据
        /// </summary>
        public ReceivedEventHandler<SerialClient> Received { get; set; }
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
            Received?.Invoke(this, byteBlock, requestInfo);
            base.HandleReceivedData(byteBlock, requestInfo);
        }
    }

    /// <summary>
    /// 串口管理
    /// </summary>
    public class SerialClientBase : BaseSerial, ISerialClient
    {
        static readonly Protocol SerialPortProtocol = new("SerialPort");
        /// <summary>
        /// 构造函数
        /// </summary>
        public SerialClientBase()
        {
            this.Protocol = SerialClientBase.SerialPortProtocol;
        }

        #region 变量

        private SerialDelaySender m_delaySender;
        private bool m_useDelaySender;

        #endregion 变量

        #region 事件

        /// <summary>
        /// 已关闭事件
        /// </summary>
        public CloseEventHandler<ISerialClientBase> Closed { get; set; }
        /// <summary>
        /// 关闭中事件
        /// </summary>
        public CloseEventHandler<ISerialClientBase> Closing { get; set; }


        /// <summary>
        /// 成功连接到串口
        /// </summary>
        public MessageEventHandler<ISerialClient> Opened { get; set; }
        /// <summary>
        /// 连接中
        /// </summary>
        public OpeningEventHandler<ISerialClient> Opening { get; set; }
        /// <summary>
        /// 已连接
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnOpened(MsgEventArgs e)
        {
            try
            {
                Opened?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                Logger.Log(LogType.Error, this, $"在事件{nameof(Opened)}中发生错误。", ex);
            }
        }

        /// <summary>
        /// 断开连接。在客户端未设置连接状态时，不会触发
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnClosed(CloseEventArgs e)
        {
            try
            {
                Closed?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                Logger.Log(LogType.Error, this, $"在事件{nameof(Closed)}中发生错误。", ex);
            }
        }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// <para>
        /// 当主动调用Close断开时，可通过<see cref="TouchSocketEventArgs.IsPermitOperation"/>终止断开行为。
        /// </para>
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnClosing(CloseEventArgs e)
        {
            try
            {
                Closing?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                Logger.Log(LogType.Error, this, $"在事件{nameof(Closing)}中发生错误。", ex);
            }
        }
        private void PrivateOnOpening(OpeningEventArgs e)
        {
            this.LastReceivedTime = DateTime.UtcNow;
            this.LastSendTime = DateTime.UtcNow;
            if (this.CanSetDataHandlingAdapter)
            {
                this.SetDataHandlingAdapter(this.Config.GetValue<Func<SerialDataHandlingAdapter>>(SerialConfigExtension.DataHandlingAdapterProperty).Invoke());
            }
            if (this.UsePlugin)
            {
                this.PluginsManager.Raise<IOpeningPlugin>(nameof(IOpeningPlugin.OnOpening), this, e);
                if (e.Handled)
                {
                    return;
                }
            }
            this.OnOpening(e);
        }

        /// <summary>
        /// 准备连接的时候，此时已初始化Socket，但是并未建立Tcp连接
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnOpening(OpeningEventArgs e)
        {
            try
            {
                Opening?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                Logger.Log(LogType.Error, this, $"在事件{nameof(this.OnOpening)}中发生错误。", ex);
            }
        }

        private void PrivateOnBeforeOpen(OpeningEventArgs e)
        {
            LastReceivedTime = DateTime.UtcNow;
            LastSendTime = DateTime.UtcNow;
            if (CanSetDataHandlingAdapter)
            {
                this.SetDataHandlingAdapter(Config.GetValue(SerialConfigExtension.DataHandlingAdapterProperty).Invoke());
            }
            if (UsePlugin)
            {
                PluginsManager.Raise<IOpeningPlugin>(nameof(IOpeningPlugin.OnOpening), this, e);
                if (e.Handled)
                {
                    return;
                }
            }
            OnOpening(e);
        }

        private void PrivateOnOpened(MsgEventArgs e)
        {
            if (UsePlugin)
            {
                PluginsManager.Raise<IOpenedPlugin>(nameof(IOpenedPlugin.OnOpened), this, e);
                if (e.Handled)
                {
                    return;
                }
            }
            OnOpened(e);
        }
        private void PrivateOnClosed(CloseEventArgs e)
        {
            if (UsePlugin && PluginsManager.Raise<IClosedPlguin>(nameof(IClosedPlguin.OnClosed), this, e))
            {
                return;
            }
            OnClosed(e);
        }
        private void PrivateOnClosing(CloseEventArgs e)
        {
            if (UsePlugin && PluginsManager.Raise<IClosingPlugin>(nameof(IClosingPlugin.OnClosing), this, e))
            {
                return;
            }
            OnClosing(e);
        }
        #endregion 事件

        #region 属性
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Protocol Protocol { get; set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool CanSend { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual bool CanSetDataHandlingAdapter => true;

        /// <summary>
        /// 客户端配置
        /// </summary>
        public TouchSocketConfig Config { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IContainer Container => Config?.Container;

        /// <summary>
        /// 数据处理适配器
        /// </summary>
        public SerialDataHandlingAdapter SerialDataHandlingAdapter { get; private set; }

        /// <inheritdoc/>
        public bool IsClient => true;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public DateTime LastReceivedTime { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public DateTime LastSendTime { get; private set; }
        /// <summary>
        /// 串口对象
        /// </summary>
        public SerialPort MainSerialPort { get; private set; }

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
        public bool Online => CanSend;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IPluginsManager PluginsManager { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ReceiveType ReceiveType { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public SerialProperty SerialProperty { get; private set; }

        /// <summary>
        /// 是否已启用插件
        /// </summary>
        public bool UsePlugin { get; private set; }


        #endregion 属性

        #region 断开操作

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual void Close()
        {
            Close($"{nameof(Close)}主动断开");
        }

        /// <summary>
        /// 中断终端，传递中断消息。
        /// </summary>
        /// <param name="msg"></param>
        public virtual void Close(string msg)
        {
            if (CanSend)
            {
                var args = new CloseEventArgs(true, msg)
                {
                    IsPermitOperation = true
                };
                PrivateOnClosing(args);
                if (DisposedValue || args.IsPermitOperation)
                {
                    BreakOut(msg, true);
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (CanSend)
            {
                var args = new CloseEventArgs(true, $"{nameof(Dispose)}主动断开");
                PrivateOnClosing(args);
            }
            PluginsManager?.Clear();
            Config = default;
            SerialDataHandlingAdapter.SafeDispose();
            SerialDataHandlingAdapter = default;
            BreakOut($"{nameof(Dispose)}主动断开", true);
            base.Dispose(disposing);
        }

        private void BreakOut(string msg, bool manual)
        {
            lock (SyncRoot)
            {
                if (CanSend)
                {
                    CanSend = false;
                    this.TryShutdown();
                    MainSerialPort.SafeDispose();
                    m_delaySender.SafeDispose();
                    SerialDataHandlingAdapter.SafeDispose();
                    PrivateOnClosed(new CloseEventArgs(manual, msg));
                }
            }
        }
        #endregion 断开操作

        /// <summary>
        /// 配置服务器
        /// </summary>
        /// <param name="config"></param>
        /// <exception cref="Exception"></exception>
        public ISerialClient Setup(TouchSocketConfig config)
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
        /// <summary>
        /// 请求连接到服务器。
        /// </summary>
        public virtual ISerialClient Open()
        {
            this.SerialOpen();
            return this;
        }
        /// <summary>
        /// 异步连接服务器
        /// </summary>
        public Task<ISerialClient> OpenAsync()
        {
            return EasyTask.Run(() =>
            {
                return this.Open();
            });
        }
        /// <summary>
        /// 设置数据处理适配器
        /// </summary>
        /// <param name="adapter"></param>
        public virtual void SetDataHandlingAdapter(SerialDataHandlingAdapter adapter)
        {
            if (!this.CanSetDataHandlingAdapter)
            {
                throw new Exception($"不允许自由调用{nameof(SetDataHandlingAdapter)}进行赋值。");
            }

            this.SetAdapter(adapter);
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
                this.PluginsManager.Raise<ISerialPlugin>(nameof(ISerialPlugin.OnSendingData), this, args);
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
            this.SerialProperty = config.GetValue(SerialConfigExtension.SerialProperty);
            this.BufferLength = config.GetValue(TouchSocketConfigExtension.BufferLengthProperty);
            this.ReceiveType = config.GetValue(TouchSocketConfigExtension.ReceiveTypeProperty);
            this.UsePlugin = config.IsUsePlugin;
            this.Logger = this.Container.Resolve<ILog>();

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
        protected void SetAdapter(SerialDataHandlingAdapter adapter)
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
            this.SerialDataHandlingAdapter = adapter;
        }
        /// <summary>
        /// 打开串口方法
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        protected void SerialOpen()
        {
            lock (SyncRoot)
            {
                if (CanSend)
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
                var serialProperty = Config.GetValue(SerialConfigExtension.SerialProperty);
                if (serialProperty == null)
                {
                    throw new ArgumentNullException("serialProperty不能为空。");
                }
                if (MainSerialPort != null)
                {
                    MainSerialPort.Dispose();
                }

                MainSerialPort = this.CreateSerial(serialProperty);

                OpeningEventArgs args = new OpeningEventArgs(MainSerialPort);
                PrivateOnOpening(args);
                MainSerialPort.Open();
                CanSend = true;

                if (Config.GetValue(TouchSocketConfigExtension.DelaySenderProperty) is DelaySenderOption senderOption)
                {
                    m_useDelaySender = true;
                    m_delaySender.SafeDispose();
                    m_delaySender = new SerialDelaySender(MainSerialPort, senderOption.QueueLength, this.OnDelaySenderError)
                    {
                        DelayLength = senderOption.DelayLength
                    };
                }

                this.BeginReceive();
                PrivateOnOpened(new MsgEventArgs("连接成功"));
                return;
            }

        }
        private void BeginReceive()
        {
            if (this.ReceiveType == ReceiveType.Auto)
            {
                MainSerialPort.DataReceived += this.EventArgs_DataReceived;
                SerialReceivedEventArgs eventArgs = new();
                ByteBlock byteBlock = BytePool.Default.GetByteBlock(this.BufferLength);
                byteBlock.SetLength(0);
                eventArgs.UserToken = byteBlock;
                if (this.MainSerialPort.BytesToRead > 0)
                {
                    this.ProcessReceived(eventArgs);
                }
            }
        }


        private SerialPort CreateSerial(SerialProperty serialProperty)
        {
            SerialPort serialPort = new SerialPort(serialProperty.PortName, serialProperty.BaudRate, serialProperty.Parity, serialProperty.DataBits, serialProperty.StopBits);
            serialPort.DtrEnable = true;
            serialPort.RtsEnable = true;
            return serialPort;
        }

        private void EventArgs_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                SerialReceivedEventArgs eventArgs = new();
                ByteBlock byteBlock = BytePool.Default.GetByteBlock(this.BufferLength);
                byteBlock.SetLength(0);
                eventArgs.UserToken = byteBlock;
                if (this.MainSerialPort.BytesToRead > 0)
                {
                    this.ProcessReceived(eventArgs);
                }
            }
            catch (Exception ex)
            {
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
                if (this.UsePlugin && this.PluginsManager.Raise<ISerialPlugin>(nameof(ISerialPlugin.OnReceivingData), this, new ByteBlockEventArgs(byteBlock)))
                {
                    return;
                }
                if (this.SerialDataHandlingAdapter == null)
                {
                    this.Logger.Error(this, TouchSocketStatus.NullDataAdapter.GetDescription());
                    return;
                }
                this.SerialDataHandlingAdapter.ReceivedInput(byteBlock);
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

        private void PrivateHandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            if (this.OnHandleReceivedData?.Invoke(byteBlock, requestInfo) == false)
            {
                return;
            }
            if (this.UsePlugin)
            {
                ReceivedDataEventArgs args = new ReceivedDataEventArgs(byteBlock, requestInfo);
                this.PluginsManager.Raise<ISerialPlugin>(nameof(ISerialPlugin.OnReceivedData), this, args);
                if (args.Handled)
                {
                    return;
                }
            }

            this.HandleReceivedData(byteBlock, requestInfo);
        }
        #region 发送

        #region 同步发送

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="requestInfo"></param>
        /// <exception cref="NotOpenedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public void Send(IRequestInfo requestInfo)
        {
            if (this.DisposedValue)
            {
                return;
            }
            if (this.SerialDataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.SerialDataHandlingAdapter), TouchSocketStatus.NullDataAdapter.GetDescription());
            }
            if (!this.SerialDataHandlingAdapter.CanSendRequestInfo)
            {
                throw new NotSupportedException($"当前适配器不支持对象发送。");
            }
            this.SerialDataHandlingAdapter.SendInput(requestInfo);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"><inheritdoc/></param>
        /// <param name="offset"><inheritdoc/></param>
        /// <param name="length"><inheritdoc/></param>
        /// <exception cref="NotOpenedException"><inheritdoc/></exception>
        /// <exception cref="OverlengthException"><inheritdoc/></exception>
        /// <exception cref="Exception"><inheritdoc/></exception>
        public virtual void Send(byte[] buffer, int offset, int length)
        {
            if (this.SerialDataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.SerialDataHandlingAdapter), TouchSocketStatus.NullDataAdapter.GetDescription());
            }
            this.SerialDataHandlingAdapter.SendInput(buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"><inheritdoc/></param>
        /// <exception cref="NotOpenedException"><inheritdoc/></exception>
        /// <exception cref="OverlengthException"><inheritdoc/></exception>
        /// <exception cref="Exception"><inheritdoc/></exception>
        public virtual void Send(IList<ArraySegment<byte>> transferBytes)
        {
            if (this.SerialDataHandlingAdapter == null)
            {
                throw new ArgumentNullException(nameof(this.SerialDataHandlingAdapter), TouchSocketStatus.NullDataAdapter.GetDescription());
            }

            if (this.SerialDataHandlingAdapter.CanSplicingSend)
            {
                this.SerialDataHandlingAdapter.SendInput(transferBytes);
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
                    this.SerialDataHandlingAdapter.SendInput(byteBlock.Buffer, 0, byteBlock.Len);
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
        /// <exception cref="NotOpenedException"><inheritdoc/></exception>
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
        /// <exception cref="NotOpenedException"></exception>
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
        /// <exception cref="NotOpenedException"><inheritdoc/></exception>
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
        /// <exception cref="NotOpenedException"><inheritdoc/></exception>
        /// <exception cref="OverlengthException"><inheritdoc/></exception>
        /// <exception cref="Exception"><inheritdoc/></exception>
        public void DefaultSend(byte[] buffer, int offset, int length)
        {
            if (!this.CanSend)
            {
                throw new NotOpenedException("未连接串口");
            }
            if (this.HandleSendingData(buffer, offset, length))
            {
                if (this.m_useDelaySender && length < TouchSocketUtility.BigDataBoundary)
                {
                    this.m_delaySender.Send(new QueueDataBytes(buffer, offset, length));
                }
                else
                {
                    this.MainSerialPort.Write(buffer, offset, length);
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
        /// <exception cref="NotOpenedException"><inheritdoc/></exception>
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
        private void ProcessReceived(SerialReceivedEventArgs e)
        {
            if (!this.CanSend)
            {
                return;
            }
            if (MainSerialPort.BytesToRead > 0)
            {
                byte[] buffer = new byte[2048];
                int offset = 0;
                int num = MainSerialPort.Read(buffer, offset, MainSerialPort.BytesToRead);
                ByteBlock byteBlock = e.UserToken;
                byteBlock.Write(buffer, byteBlock.Len, num);
                this.HandleBuffer(byteBlock);

                try
                {
                    ByteBlock newByteBlock = BytePool.Default.GetByteBlock(this.BufferLength);
                    newByteBlock.SetLength(num);
                    e.UserToken = newByteBlock;

                    if (MainSerialPort.BytesToRead > 0)
                    {
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
                this.BreakOut("串口关闭", false);
            }
        }
    }
}