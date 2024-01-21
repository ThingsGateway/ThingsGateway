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

using System.Diagnostics;

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// TgSocketClient
    /// </summary>
    [DebuggerDisplay("Id={Id},IPAdress={IP}:{Port}")]
    public class TgSocketClient : TgSocketClientBase, IClientChannel
    {
        /// <summary>
        /// TgSocketClient
        /// </summary>
        ~TgSocketClient()
        {
            Dispose(false);
        }

        /// <inheritdoc/>
        public EasyLock WaitLock { get; } = new EasyLock();

        /// <inheritdoc/>
        public List<IProtocol> Collects { get; } = new();

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
        public TgReceivedEventHandler Received { get; set; }

        /// <inheritdoc/>
        public ChannelTypeEnum ChannelType => ChannelTypeEnum.TcpService;

        /// <inheritdoc/>
        DataHandlingAdapter IClientChannel.DataHandlingAdapter => DataHandlingAdapter;

        /// <inheritdoc/>
        public ChannelEventHandler Started { get; set; }

        /// <inheritdoc/>
        public ChannelEventHandler Stoped { get; set; }

        /// <inheritdoc/>
        public ChannelEventHandler Starting { get; set; }

        /// <inheritdoc/>
        public void SetDataHandlingAdapter(DataHandlingAdapter adapter)
        {
            if (adapter is SingleStreamDataHandlingAdapter single)
                base.SetDataHandlingAdapter(single);
            else
                throw new NotSupportedException(string.Format(FoundationConst.AdapterTypeError, nameof(SingleStreamDataHandlingAdapter)));
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{IP}:{Port}";
        }

        /// <inheritdoc/>
        public void Connect(int timeout, CancellationToken token) => throw new NotImplementedException();

        /// <inheritdoc/>
        public Task ConnectAsync(int timeout, CancellationToken token) => throw new NotImplementedException();

        /// <inheritdoc/>
        protected override Task ReceivedData(ReceivedDataEventArgs e)
        {
            if (this.Received != null)
            {
                return this.Received.Invoke(this, e);
            }
            return base.ReceivedData(e);
        }

        /// <inheritdoc/>
        protected override async Task OnConnected(ConnectedEventArgs e)
        {
            //Logger?.Debug($"{ToString()}{FoundationConst.Connected}");
            if (Started != null)
                await Started.Invoke(this);
            await base.OnConnected(e);
        }

        /// <inheritdoc/>
        protected override async Task OnConnecting(ConnectingEventArgs e)
        {
            //Logger?.Debug($"{ToString()}{FoundationConst.Connecting}{(e.Message.IsNullOrEmpty() ? string.Empty : $"-{e.Message}")}");
            if (Starting != null)
                await Starting.Invoke(this);
            await base.OnConnecting(e);
        }

        /// <inheritdoc/>
        protected override Task OnDisconnecting(DisconnectEventArgs e)
        {
            Logger?.Debug($"{ToString()} {FoundationConst.Disconnecting}{(e.Message.IsNullOrEmpty() ? string.Empty : $"-{e.Message}")}");
            return base.OnDisconnecting(e);
        }

        /// <inheritdoc/>
        protected override Task OnDisconnected(DisconnectEventArgs e)
        {
            Logger?.Debug($"{ToString()} {FoundationConst.Disconnected}{(e.Message.IsNullOrEmpty() ? string.Empty : $"-{e.Message}")}");
            return base.OnDisconnected(e);
        }

        #region 无

        /// <inheritdoc/>
        public void Setup(TouchSocketConfig config)
        {
        }

        /// <inheritdoc/>
        public Task SetupAsync(TouchSocketConfig config)
        {
            return EasyTask.CompletedTask;
        }

        #endregion 无
    }
}