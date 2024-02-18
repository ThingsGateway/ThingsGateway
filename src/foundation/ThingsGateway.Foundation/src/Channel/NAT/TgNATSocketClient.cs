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
    /// <summary>
    /// 端口转发辅助
    /// </summary>
    public class TgNATSocketClient : TgSocketClient
    {
        internal Action<TgNATSocketClient, ITcpClient, DisconnectEventArgs> m_internalDis;
        internal Func<TgNATSocketClient, ITcpClient, ReceivedDataEventArgs, byte[]?> m_internalTargetClientRev;
        private readonly ConcurrentList<ITcpClient> m_targetClients = new ConcurrentList<ITcpClient>();

        /// <summary>
        /// 添加转发客户端。
        /// </summary>
        /// <param name="config">配置文件</param>
        /// <param name="setupAction">当完成配置，但是还未连接时回调。</param>
        /// <returns></returns>
        public ITcpClient AddTargetClient(TouchSocketConfig config, Action<ITcpClient> setupAction = default)
        {
            var tcpClient = new TcpClient();
            tcpClient.Disconnected += this.TcpClient_Disconnected;
            tcpClient.Received += this.TcpClient_Received;
            tcpClient.Setup(config);
            setupAction?.Invoke(tcpClient);
            tcpClient.Connect();

            this.m_targetClients.Add(tcpClient);
            return tcpClient;
        }

        /// <summary>
        /// 添加转发客户端。
        /// </summary>
        /// <param name="config">配置文件</param>
        /// <param name="setupAction">当完成配置，但是还未连接时回调。</param>
        /// <returns></returns>
        public Task<ITcpClient> AddTargetClientAsync(TouchSocketConfig config, Action<ITcpClient> setupAction = default)
        {
            return Task.FromResult(this.AddTargetClient(config, setupAction));
        }

        /// <summary>
        /// 获取所有目标客户端
        /// </summary>
        /// <returns></returns>
        public ITcpClient[] GetTargetClients()
        {
            return this.m_targetClients.ToArray();
        }

        /// <summary>
        /// 发送数据到全部转发端。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void SendToTargetClient(byte[] buffer, int offset, int length)
        {
            foreach (var socket in this.m_targetClients)
            {
                try
                {
                    socket.Send(buffer, offset, length);
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="e"></param>
        protected override Task OnDisconnected(DisconnectEventArgs e)
        {
            foreach (var client in this.m_targetClients)
            {
                client.TryShutdown();
                client.SafeDispose();
            }
            return base.OnDisconnected(e);
        }

        private async Task TcpClient_Disconnected(ITcpClientBase client, DisconnectEventArgs e)
        {
            await EasyTask.CompletedTask;
            foreach (var item in client.PluginManager.Plugins)
            {
                if (typeof(ReconnectionPlugin<>) == item.GetType().GetGenericTypeDefinition())
                {
                    this.m_internalDis?.Invoke(this, (ITcpClient)client, e);
                    return;
                }
            }
            client.Dispose();
            this.m_targetClients.Remove((ITcpClient)client);
            this.m_internalDis?.Invoke(this, (ITcpClient)client, e);
        }

        private async Task TcpClient_Received(TcpClient client, ReceivedDataEventArgs e)
        {
            await EasyTask.CompletedTask;
            if (this.DisposedValue)
            {
                return;
            }

            try
            {
                var data = this.m_internalTargetClientRev?.Invoke(this, client, e);
                if (data != null)
                {
                    if (this.CanSend)
                    {
                        await this.SendAsync(data);
                    }
                }
            }
            catch
            {
            }
        }
    }
}