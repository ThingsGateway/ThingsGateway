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
    /// SocketExtension
    /// </summary>
    public static class SocketExtension
    {
        /// <summary>
        /// 会使用同步锁，保证所有数据上缓存区。
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public static void AbsoluteSend(this Socket socket, byte[] buffer, int offset, int length)
        {
            lock (socket)
            {
                while (length > 0)
                {
                    var r = socket.Send(buffer, offset, length, SocketFlags.None);
                    if (r == 0 && length > 0)
                    {
                        throw new Exception("发送数据不完全");
                    }
                    offset += r;
                    length -= r;
                }
            }
        }

        /// <summary>
        /// 尝试关闭<see cref="Socket"/>。不会抛出异常。
        /// </summary>
        /// <param name="socket"></param>
        public static void TryClose(this Socket socket)
        {
            lock (socket)
            {
                try
                {
                    if (socket.Connected)
                    {
                        socket.Close();
                    }
                }
                catch
                {
                }
            }
        }
    }
}