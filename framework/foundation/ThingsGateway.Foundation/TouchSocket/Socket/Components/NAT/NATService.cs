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

namespace ThingsGateway.Foundation.Sockets
{
    /// <summary>
    /// Tcp端口转发服务器
    /// </summary>
    public class NATService : TcpService<NATSocketClient>
    {
        /// <inheritdoc/>
        protected override NATSocketClient GetClientInstence(Socket socket, TcpNetworkMonitor monitor)
        {
            var client = base.GetClientInstence(socket, monitor);
            client.m_internalDis = this.OnTargetClientDisconnected;
            client.m_internalTargetClientRev = this.OnTargetClientReceived;
            return client;
        }

        /// <summary>
        /// 在NAT服务器收到数据时。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        /// <returns>需要转发的数据。</returns>
        protected virtual byte[] OnNATReceived(NATSocketClient socketClient, ReceivedDataEventArgs e)
        {
            return e.ByteBlock?.ToArray();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected override sealed async Task OnReceived(NATSocketClient socketClient, ReceivedDataEventArgs e)
        {
            await EasyTask.CompletedTask;
            var data = this.OnNATReceived(socketClient, e);
            if (data != null)
            {
                socketClient.SendToTargetClient(data, 0, data.Length);
            }
        }

        /// <summary>
        /// 当目标客户端断开。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="tcpClient"></param>
        /// <param name="e"></param>
        protected virtual void OnTargetClientDisconnected(NATSocketClient socketClient, ITcpClient tcpClient, DisconnectEventArgs e)
        {
        }

        /// <summary>
        /// 在目标客户端收到数据时。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="tcpClient"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        protected virtual byte[] OnTargetClientReceived(NATSocketClient socketClient, ITcpClient tcpClient, ReceivedDataEventArgs e)
        {
            return e.ByteBlock?.ToArray();
        }
    }
}