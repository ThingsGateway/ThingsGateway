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

namespace ThingsGateway.Foundation
{
    /// <inheritdoc/>
    public class TgUdpSession : TgUdpSessionBase, IClientChannel
    {
        /// <inheritdoc/>
        ~TgUdpSession()
        {
            Dispose(false);
        }

        /// <inheritdoc/>
        public EasyLock WaitLock { get; } = new EasyLock();

        /// <summary>
        /// 当收到数据时
        /// </summary>
        public TgReceivedEventHandler Received { get; set; }

        /// <inheritdoc/>
        public override string? ToString()
        {
            return RemoteIPHost?.ToString().Replace("tcp", "udp");
        }

        /// <inheritdoc/>
        public ChannelEventHandler Started { get; set; }

        /// <inheritdoc/>
        public ChannelEventHandler Stoped { get; set; }

        /// <inheritdoc/>
        public ChannelEventHandler Starting { get; set; }

        /// <inheritdoc/>
        public ChannelTypeEnum ChannelType => ChannelTypeEnum.UdpSession;

        /// <inheritdoc/>
        public bool Online => this.CanSend;

        /// <inheritdoc/>
        DataHandlingAdapter IClientChannel.DataHandlingAdapter => DataHandlingAdapter;

        /// <inheritdoc/>
        public ConcurrentList<IProtocol> Collects { get; } = new();

        /// <inheritdoc/>
        public void SetDataHandlingAdapter(DataHandlingAdapter adapter)
        {
            if (adapter is UdpDataHandlingAdapter udp)
                base.SetDataHandlingAdapter(udp);
            else
                throw new NotSupportedException(string.Format(FoundationConst.AdapterTypeError, nameof(UdpDataHandlingAdapter)));
        }

        /// <inheritdoc/>
        public void Close(string msg) => this.Stop();

        /// <inheritdoc/>
        public override void Start()
        {
            if (this.ServerState != ServerState.Running)
            {
                base.Start();
                if (this.ServerState == ServerState.Running)
                {
                    Logger.Info($"{Monitor.IPHost}{FoundationConst.ServiceStarted}");
                }
            }
            else
            {
                base.Start();
            }
        }

        /// <inheritdoc/>
        public override async Task StartAsync()
        {
            if (this.ServerState != ServerState.Running)
            {
                await base.StartAsync();
                if (this.ServerState == ServerState.Running)
                {
                    Logger.Info($"{Monitor.IPHost}{FoundationConst.ServiceStarted}");
                }
            }
            else
            {
                await base.StartAsync();
            }
        }

        /// <inheritdoc/>
        public override void Stop()
        {
            if (Monitor != null)
            {
                base.Stop();
                if (Monitor == null)
                    Logger.Info($"{Monitor?.IPHost}{FoundationConst.ServiceStoped}");
            }
            else
            {
                base.Stop();
            }
            if (Stoped != null)
                Stoped.Invoke(this).GetFalseAwaitResult();
        }

        /// <inheritdoc/>
        public override async Task StopAsync()
        {
            if (Monitor != null)
            {
                await base.StopAsync();
                if (Monitor == null)
                    Logger.Info($"{Monitor.IPHost}{FoundationConst.ServiceStoped}");
            }
            else
            {
                await base.StopAsync();
            }
            if (Stoped != null)
                await Stoped.Invoke(this);
        }

        /// <inheritdoc/>
        public void Connect(int timeout, CancellationToken token)
        {
            if (Starting != null)
                Starting.Invoke(this).GetFalseAwaitResult();
            this.Start();
            if (Started != null)
                Started.Invoke(this).GetFalseAwaitResult();
        }

        /// <inheritdoc/>
        public async Task ConnectAsync(int timeout, CancellationToken token)
        {
            if (Starting != null)
                await Starting.Invoke(this);
            await StartAsync();
            if (Started != null)
                await Started.Invoke(this);
        }

        /// <inheritdoc/>
        protected override async Task ReceivedData(UdpReceivedDataEventArgs e)
        {
            if (this.Received != null)
            {
                await this.Received.Invoke(this, e);
                if (e.Handled)
                {
                    return;
                }
            }
            await base.ReceivedData(e);
        }
    }
}