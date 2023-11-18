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
    /// Client工厂
    /// </summary>
    public abstract class ClientFactory<TClient> : DependencyObject where TClient : IClient
    {
        /// <summary>
        /// Client工厂
        /// </summary>
        public ClientFactory()
        {
            this.MainConfig = new TouchSocketConfig();
        }

        /// <summary>
        /// 已创建的客户端安全列表，一般不要直接操作。
        /// </summary>
        public ConcurrentList<TClient> CreatedClients { get; } = new ConcurrentList<TClient>();

        /// <summary>
        /// 空闲客户端的安全队列，一般不要直接操作。
        /// </summary>
        public ConcurrentQueue<TClient> FreeClients { get; } = new ConcurrentQueue<TClient>();

        /// <summary>
        /// 主通信客户端。
        /// </summary>
        public abstract TClient MainClient { get; }

        /// <summary>
        /// 主客户端配置
        /// </summary>
        /// <returns></returns>
        public virtual TouchSocketConfig MainConfig { get; }

        /// <summary>
        /// 最大客户端数量。默认10。
        /// </summary>
        public int MaxCount { get; set; } = 10;

        /// <summary>
        /// 池中维护的最小客户端数量。默认0。
        /// </summary>
        public int MinCount { get; set; }

        /// <summary>
        /// 检验主通信状态。最好在每次操作时都调用。
        /// </summary>
        /// <param name="tryInit">如果状态异常，是否进行再次初始化</param>
        /// <returns></returns>
        public abstract Result CheckStatus(bool tryInit = true);

        /// <summary>
        /// 检验主通信状态。最好在每次操作时都调用。
        /// </summary>
        /// <param name="tryInit">如果状态异常，是否进行再次初始化</param>
        /// <returns></returns>
        public virtual Task<Result> CheckStatusAsync(bool tryInit = true)
        {
            return Task.Run(() =>
              {
                  return this.CheckStatus(tryInit);
              });
        }

        /// <summary>
        /// 清理池中的所有客户端。
        /// </summary>
        /// <returns></returns>
        public virtual Task<int> ClearAsync()
        {
            return Task.Run(() =>
             {
                 return this.Clear();
             });
        }
        /// <summary>
        /// 清理池中的所有客户端。
        /// </summary>
        /// <returns></returns>
        public virtual int Clear()
        {
            var count = 0;
            foreach (var item in this.CreatedClients)
            {
                count++;
                this.DisposeClient(item);
            }
            this.FreeClients.Clear();
            return count;
        }

        /// <summary>
        /// 释放客户端最后的调用。
        /// </summary>
        /// <param name="client"></param>
        public abstract void DisposeClient(TClient client);

        /// <summary>
        /// 释放客户端最后的调用。
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public virtual Task DisposeClientAsync(TClient client)
        {
            return Task.Run(() =>
             {
                 this.DisposeClient(client);
             });
        }

        /// <summary>
        /// 获取空闲可用的客户端数量。
        /// </summary>
        public abstract int GetAvailableCount();

        /// <summary>
        /// 获取用于传输的客户端。在此处返回的结果，必须完成基本初始化，例如连接等。
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public abstract TClient GetTransferClient(TimeSpan waitTime);

        /// <summary>
        /// 判断客户端是不是存活状态。
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public abstract bool IsAlive(TClient client);

        /// <summary>
        /// 释放使用完成的客户端
        /// </summary>
        /// <param name="client"></param>
        public abstract void ReleaseTransferClient(TClient client);

        /// <summary>
        /// 释放使用完成的客户端
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public virtual Task ReleaseTransferClientAsync(TClient client)
        {
            return Task.Run(() =>
              {
                  this.ReleaseTransferClient(client);
              });
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.DisposeClient(this.MainClient);
            this.Clear();
        }

        /// <summary>
        /// 获取用于传输的客户端配置
        /// </summary>
        /// <returns></returns>
        protected abstract TouchSocketConfig GetTransferConfig();
    }
}