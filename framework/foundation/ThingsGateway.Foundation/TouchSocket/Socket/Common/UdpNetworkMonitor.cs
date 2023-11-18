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

namespace ThingsGateway.Foundation.Sockets
{
    /// <summary>
    /// Udp监听器
    /// </summary>
    public class UdpNetworkMonitor
    {
        /// <summary>
        /// Udp监听器
        /// </summary>
        /// <param name="iPHost"></param>
        /// <param name="socket"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public UdpNetworkMonitor(IPHost iPHost, Socket socket)
        {
            this.IPHost = iPHost;
            this.Socket = socket ?? throw new ArgumentNullException(nameof(socket));

        }

        /// <summary>
        /// IPHost
        /// </summary>
        public IPHost IPHost { get; }

        /// <summary>
        /// Socket组件
        /// </summary>
        public Socket Socket { get; private set; }
    }
}
