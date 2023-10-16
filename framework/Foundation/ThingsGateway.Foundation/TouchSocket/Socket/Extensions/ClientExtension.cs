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
    /// 客户端扩展类
    /// </summary>
    public static class ClientExtension
    {
        /// <summary>
        /// 获取相关信息。格式：
        ///<para>IPPort=IP:Port,Id=id,Protocol=Protocol</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <returns></returns>
        public static string GetInfo<T>(this T client) where T : ISocketClient
        {
            return $"IP&Port={client.IP}:{client.Port},Id={client.Id},Protocol={client.Protocol}";
        }

        /// <summary>
        /// 获取IP和端口。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <returns></returns>
        public static string GetIPPort<T>(this T client) where T : ITcpClientBase
        {
            return $"{client.IP}:{client.Port}";
        }

        /// <summary>
        /// 获取最后活动时间。即<see cref="IClient.LastReceivedTime"/>与<see cref="IClient.LastSendTime"/>的最近值。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <returns></returns>
        public static DateTime GetLastActiveTime<T>(this T client) where T : IClient
        {
            return client.LastSendTime > client.LastReceivedTime ? client.LastSendTime : client.LastReceivedTime;
        }

        /// <summary>
        /// 获取服务器中，除自身以外的所有客户端id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetOtherIds<T>(this T client) where T : ISocketClient
        {
            return client.Service.GetIds().Where(id => id != client.Id);
        }

        /// <summary>
        /// 安全性关闭。不会抛出异常。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        public static void SafeClose<T>(this T client, string msg) where T : ITcpClientBase
        {
            try
            {
                client.Close(msg);
            }
            catch
            {
            }
        }

        /// <summary>
        /// 安全性发送关闭报文
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="how"></param>
        public static bool TryShutdown<T>(this T client, SocketShutdown how = SocketShutdown.Both) where T : ITcpClientBase
        {
            try
            {
                if (!client.MainSocket.Connected)
                {
                    return false;
                }
                client?.MainSocket?.Shutdown(how);
                return true;
            }
            catch
            {
            }

            return false;
        }

        #region 连接

        /// <inheritdoc cref="ITcpClient.Connect(int)"/>
        public static TClient Connect<TClient>(this TClient client, string ipHost, int timeout = 5000) where TClient : ITcpClient
        {
            TouchSocketConfig config;
            if (client.Config == null)
            {
                config = new TouchSocketConfig();
                config.SetRemoteIPHost(ipHost);
                client.Setup(config);
            }
            else
            {
                config = client.Config;
                config.SetRemoteIPHost(ipHost);
            }
            client.Connect(timeout);
            return client;
        }

        /// <inheritdoc cref="ITcpClient.ConnectAsync(int)"/>
        public static async Task<TClient> ConnectAsync<TClient>(this TClient client, string ipHost, int timeout = 5000) where TClient : ITcpClient
        {
            TouchSocketConfig config;
            if (client.Config == null)
            {
                config = new TouchSocketConfig();
                config.SetRemoteIPHost(ipHost);
                client.Setup(config);
            }
            else
            {
                config = client.Config;
                config.SetRemoteIPHost(ipHost);
            }
            await client.ConnectAsync(timeout);
            return client;
        }

        /// <summary>
        /// 尝试连接。不会抛出异常。
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="client"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static Result TryConnect<TClient>(this TClient client, int timeout = 5000) where TClient : ITcpClient
        {
            try
            {
                client.Connect(timeout);
                return new Result(ResultCode.Success);
            }
            catch (Exception ex)
            {
                return new Result(ResultCode.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// 尝试连接。不会抛出异常。
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="client"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<Result> TryConnectAsync<TClient>(this TClient client, int timeout = 5000) where TClient : ITcpClient
        {
            try
            {
                await client.ConnectAsync(timeout);
                return new Result(ResultCode.Success);
            }
            catch (Exception ex)
            {
                return new Result(ResultCode.Exception, ex.ToString());
            }
        }

        #endregion 连接
    }
}