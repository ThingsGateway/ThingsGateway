//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Collections.Concurrent;
using System.Diagnostics;

namespace ThingsGateway.Foundation
{
    [DebuggerDisplay("Count={Count}")]
    internal class SocketClientCollection : ConcurrentDictionary<string, ISocketClient>, ISocketClientCollection
    {
        public IEnumerable<ISocketClient> GetClients()
        {
            return this.Values;
        }

        public IEnumerable<string> GetIds()
        {
            return this.Keys;
        }

        public bool SocketClientExist(string id)
        {
            return string.IsNullOrEmpty(id) ? false : this.ContainsKey(id);
        }

        public bool TryGetSocketClient(string id, out ISocketClient socketClient)
        {
            if (string.IsNullOrEmpty(id))
            {
                socketClient = null;
                return false;
            }

            return this.TryGetValue(id, out socketClient);
        }

        public bool TryGetSocketClient<TClient>(string id, out TClient socketClient) where TClient : ISocketClient
        {
            if (string.IsNullOrEmpty(id))
            {
                socketClient = default;
                return false;
            }

            if (this.TryGetValue(id, out var client))
            {
                socketClient = (TClient)client;
                return true;
            }
            socketClient = default;
            return false;
        }

        internal bool TryAdd(ISocketClient socketClient)
        {
            return this.TryAdd(socketClient.Id, socketClient);
        }

        internal bool TryRemove<TClient>(string id, out TClient socketClient) where TClient : ISocketClient
        {
            if (string.IsNullOrEmpty(id))
            {
                socketClient = default;
                return false;
            }

            if (this.TryRemove(id, out var client))
            {
                socketClient = (TClient)client;
                return true;
            }
            socketClient = default;
            return false;
        }
    }
}