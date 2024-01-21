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

using System.Net.Sockets;

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// Tcp端口转发服务器
    /// </summary>
    public class TgNATService : TgTcpServiceBase<TgNATSocketClient>
    {
        /// <inheritdoc/>
        protected override TgNATSocketClient GetClientInstence(Socket socket, TcpNetworkMonitor monitor)
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
        protected virtual byte[]? OnNATReceived(TgNATSocketClient socketClient, ReceivedDataEventArgs e)
        {
            return e.ByteBlock?.ToArray();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected override sealed async Task OnReceived(TgNATSocketClient socketClient, ReceivedDataEventArgs e)
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
        protected virtual void OnTargetClientDisconnected(TgNATSocketClient socketClient, ITcpClient tcpClient, DisconnectEventArgs e)
        {
        }

        /// <summary>
        /// 在目标客户端收到数据时。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="tcpClient"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        protected virtual byte[]? OnTargetClientReceived(TgNATSocketClient socketClient, ITcpClient tcpClient, ReceivedDataEventArgs e)
        {
            return e.ByteBlock?.ToArray();
        }
    }
}