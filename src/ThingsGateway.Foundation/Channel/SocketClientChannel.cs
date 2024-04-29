
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------




using System.Diagnostics;

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// SocketClientChannel
    /// </summary>
    [DebuggerDisplay("Id={Id},IPAdress={IP}:{Port}")]
    public class SocketClientChannel : TcpSessionClient, IClientChannel
    {
        /// <summary>
        /// SocketClientChannel
        /// </summary>
        ~SocketClientChannel()
        {
            Dispose(false);
        }

        /// <inheritdoc/>
        public EasyLock WaitLock { get; } = new EasyLock();

        /// <inheritdoc/>
        public ConcurrentList<IProtocol> Collects { get; } = new();

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (DisposedValue) return;
            base.Dispose(disposing);
            PluginManager?.SafeDispose();
        }

        /// <summary>
        /// 接收到数据
        /// </summary>
        public ChannelReceivedEventHandler ChannelReceived { get; set; }

        /// <inheritdoc/>
        public ChannelTypeEnum ChannelType => ChannelTypeEnum.TcpService;

        /// <inheritdoc/>
        public ChannelEventHandler Started { get; set; }

        /// <inheritdoc/>
        public ChannelEventHandler Stoped { get; set; }

        /// <inheritdoc/>
        public ChannelEventHandler Starting { get; set; }
 
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{IP}:{Port}";
        }

        /// <inheritdoc/>
        public Task ConnectAsync(int timeout, CancellationToken token) => throw new NotImplementedException();

        protected override Task OnTcpReceived(ReceivedDataEventArgs e)
        {
            if (this.ChannelReceived != null)
            {
                return this.ChannelReceived.Invoke(this, e);
            }
            return base.OnTcpReceived(e);
        }

        /// <inheritdoc/>
        protected override Task OnTcpConnected(ConnectedEventArgs e)
        {
            //Logger?.Debug($"{ToString()}{FoundationConst.Connected}");
            if (Started != null)
                return Started.Invoke(this);
            return base.OnTcpConnected(e);
        }

        /// <inheritdoc/>
        protected override Task OnTcpConnecting(ConnectingEventArgs e)
        {
            if (Starting != null)
                return Starting.Invoke(this);
            return base.OnTcpConnecting(e);
        }

        /// <inheritdoc/>
        protected override Task OnTcpClosing(ClosingEventArgs e)
        {
            Logger?.Debug($"{ToString()} Disconnecting{(e.Message.IsNullOrEmpty() ? string.Empty : $"-{e.Message}")}");
            return base.OnTcpClosing(e);
        }

        /// <inheritdoc/>
        protected override Task OnTcpClosed(ClosedEventArgs e)
        {
            Logger?.Debug($"{ToString()} Disconnected{(e.Message.IsNullOrEmpty() ? string.Empty : $"-{e.Message}")}");
            return base.OnTcpClosed(e);
        }

        #region

        /// <inheritdoc/>
        public Task SetupAsync(TouchSocketConfig config)
        {
            return EasyTask.CompletedTask;
        }

        #endregion 无
    }
}
