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

namespace ThingsGateway.Foundation.Sockets
{
    /// <summary>
    /// Tcp服务器基类
    /// </summary>
    public abstract class TcpServiceBase : SetupConfigObject, ITcpService
    {
        private readonly ConcurrentStack<TcpCore> m_tcpCores = new ConcurrentStack<TcpCore>();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int Count => this.SocketClients.Count;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract int MaxCount { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract IEnumerable<TcpNetworkMonitor> Monitors { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract string ServerName { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract ServerState ServerState { get; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract ISocketClientCollection SocketClients { get; }

        /// <summary>
        /// 添加一个地址监听。支持在服务器运行过程中动态添加。
        /// </summary>
        /// <param name="options"></param>
        public abstract void AddListen(TcpListenOption options);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetIds()
        {
            return this.SocketClients.GetIds();
        }

        /// <summary>
        /// 移除一个地址监听。支持在服务器运行过程中动态移除。
        /// </summary>
        /// <param name="monitor">监听器</param>
        /// <returns>返回是否已成功移除</returns>
        public abstract bool RemoveListen(TcpNetworkMonitor monitor);

        /// <summary>
        /// 租用TcpCore
        /// </summary>
        /// <returns></returns>
        public TcpCore RentTcpCore()
        {
            if (this.m_tcpCores.TryPop(out var tcpCore))
            {
                return tcpCore;
            }

            return new InternalTcpCore();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="oldId"></param>
        /// <param name="newId"></param>
        public abstract void ResetId(string oldId, string newId);

        /// <summary>
        /// 归还TcpCore
        /// </summary>
        /// <param name="tcpCore"></param>
        public void ReturnTcpCore(TcpCore tcpCore)
        {
            if (this.DisposedValue)
            {
                tcpCore.SafeDispose();
                return;
            }
            this.m_tcpCores.Push(tcpCore);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public abstract bool SocketClientExist(string id);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public abstract IService Start();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public abstract IService Stop();

        internal Task OnInternalConnected(ISocketClient socketClient, ConnectedEventArgs e)
        {
            return this.OnClientConnected(socketClient, e);
        }

        internal Task OnInternalConnecting(ISocketClient socketClient, ConnectingEventArgs e)
        {
            return this.OnClientConnecting(socketClient, e);
        }

        internal Task OnInternalDisconnected(ISocketClient socketClient, DisconnectEventArgs e)
        {
            return this.OnClientDisconnected(socketClient, e);
        }

        internal Task OnInternalDisconnecting(ISocketClient socketClient, DisconnectEventArgs e)
        {
            return this.OnClientDisconnecting(socketClient, e);
        }

        internal Task OnInternalReceivedData(ISocketClient socketClient, ReceivedDataEventArgs e)
        {
            return this.OnClientReceivedData(socketClient, e);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            while (this.m_tcpCores.TryPop(out var tcpCore))
            {
                tcpCore.SafeDispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// 客户端连接完成
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected abstract Task OnClientConnected(ISocketClient socketClient, ConnectedEventArgs e);

        /// <summary>
        /// 客户端请求连接
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected abstract Task OnClientConnecting(ISocketClient socketClient, ConnectingEventArgs e);

        /// <summary>
        /// 客户端断开连接
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected abstract Task OnClientDisconnected(ISocketClient socketClient, DisconnectEventArgs e);

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected abstract Task OnClientDisconnecting(ISocketClient socketClient, DisconnectEventArgs e);

        /// <summary>
        /// 收到数据时
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected abstract Task OnClientReceivedData(ISocketClient socketClient, ReceivedDataEventArgs e);


        #region Id发送

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="id">用于检索TcpSocketClient</param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public void Send(string id, byte[] buffer, int offset, int length)
        {
            if (this.SocketClients.TryGetSocketClient(id, out var client))
            {
                client.Send(buffer, offset, length);
            }
            else
            {
                throw new ClientNotFindException(TouchSocketResource.ClientNotFind.GetDescription(id));
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="requestInfo"></param>
        public void Send(string id, IRequestInfo requestInfo)
        {
            if (this.SocketClients.TryGetSocketClient(id, out var client))
            {
                client.Send(requestInfo);
            }
            else
            {
                throw new ClientNotFindException(TouchSocketResource.ClientNotFind.GetDescription(id));
            }
        }

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="id">用于检索TcpSocketClient</param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="NotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public Task SendAsync(string id, byte[] buffer, int offset, int length)
        {
            return this.SocketClients.TryGetSocketClient(id, out var client)
                ? client.SendAsync(buffer, offset, length)
                : throw new ClientNotFindException(TouchSocketResource.ClientNotFind.GetDescription(id));
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="requestInfo"></param>
        public Task SendAsync(string id, IRequestInfo requestInfo)
        {
            return this.SocketClients.TryGetSocketClient(id, out var client)
                ? client.SendAsync(requestInfo)
                : throw new ClientNotFindException(TouchSocketResource.ClientNotFind.GetDescription(id));
        }

        #endregion Id发送

    }
}