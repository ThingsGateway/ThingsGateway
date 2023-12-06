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
using System.Collections.Concurrent;
using System.Net;

namespace ThingsGateway.Foundation.Dmtp.Rpc
{
    /// <summary>
    /// UdpDmtpService
    /// </summary>
    public partial class UdpDmtp : UdpSessionBase, IUdpDmtp
    {
        private readonly Timer m_timer;
        private readonly ConcurrentDictionary<EndPoint, UdpDmtpClient> m_udpDmtpClients = new ConcurrentDictionary<EndPoint, UdpDmtpClient>();

        /// <summary>
        /// 构造函数
        /// </summary>
        public UdpDmtp()
        {
            this.m_timer = new Timer((obj) =>
            {
                //this.m_udpDmtpClients.RemoveWhen((kv) =>
                //{
                //    if (DateTime.Now - kv.Value.LastActiveTime > TimeSpan.FromMinutes(1))
                //    {
                //        return true;
                //    }
                //    return false;
                //});
            }, null, 1000 * 10, 1000 * 10);
        }

        /// <inheritdoc/>
        public IDmtpActor DmtpActor => this.PrivateGetUdpDmtpClient().DmtpActor;

        /// <summary>
        /// 通过终结点获取<see cref="IUdpDmtpClient"/>
        /// </summary>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        public IUdpDmtpClient GetUdpDmtpClient(EndPoint endPoint)
        {
            return this.PrivateGetUdpDmtpClient(endPoint);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            this.m_timer.SafeDispose();
            this.m_udpDmtpClients.Clear();
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override async Task ReceivedData(UdpReceivedDataEventArgs e)
        {
            var client = this.PrivateGetUdpDmtpClient(e.EndPoint);
            if (client == null)
            {
                return;
            }

            var message = DmtpMessage.CreateFrom(e.ByteBlock);
            if (!await client.InputReceivedData(message))
            {
                if (this.PluginManager.Enable)
                {
                    await this.PluginManager.RaiseAsync(nameof(IDmtpReceivedPlugin.OnDmtpReceived), client, new DmtpMessageEventArgs(message));
                }
            }
        }

        /// <inheritdoc/>
        protected override void LoadConfig(TouchSocketConfig config)
        {
            base.LoadConfig(config);
        }

        private UdpDmtpClient PrivateGetUdpDmtpClient(EndPoint endPoint)
        {
            if (!this.m_udpDmtpClients.TryGetValue(endPoint, out var udpRpcActor))
            {
                udpRpcActor = new UdpDmtpClient(this, endPoint, this.Resolver.Resolve<ILog>())
                {
                    Client = this,
                };
                if (udpRpcActor.Created(this.PluginManager))
                {
                    this.m_udpDmtpClients.TryAdd(endPoint, udpRpcActor);
                }
            }
            return udpRpcActor;
        }

        private IUdpDmtpClient PrivateGetUdpDmtpClient()
        {
            if (this.RemoteIPHost == null)
            {
                throw new ArgumentNullException(nameof(this.RemoteIPHost));
            }
            return this.PrivateGetUdpDmtpClient(this.RemoteIPHost.EndPoint);
        }
    }
}