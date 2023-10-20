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


namespace ThingsGateway.Foundation.Dmtp
{
    /// <summary>
    /// DmtpChannelExtension
    /// </summary>
    public static class DmtpChannelExtension
    {
        /// <summary>
        /// 写入通道
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="data"></param>
        public static void Write(this IDmtpChannel channel, byte[] data)
        {
            channel.Write(data, 0, data.Length);
        }

        /// <summary>
        /// 写入通道
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="data"></param>
        public static Task WriteAsync(this IDmtpChannel channel, byte[] data)
        {
            return channel.WriteAsync(data, 0, data.Length);
        }

        /// <summary>
        /// 尝试写入。
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static bool TryWrite(this IDmtpChannel channel, byte[] data, int offset, int length)
        {
            if (channel.CanWrite)
            {
                try
                {
                    channel.Write(data, offset, length);
                    return true;
                }
                catch
                {
                }
            }
            return false;
        }

        /// <summary>
        /// 尝试写入
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool TryWrite(this IDmtpChannel channel, byte[] data)
        {
            return TryWrite(channel, data, 0, data.Length);
        }

        /// <summary>
        /// 异步尝试写入
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static async Task<bool> TryWriteAsync(this IDmtpChannel channel, byte[] data, int offset, int length)
        {
            if (channel.CanWrite)
            {
                try
                {
                    await channel.WriteAsync(data, offset, length);
                    return true;
                }
                catch
                {
                }
            }
            return false;
        }

        /// <summary>
        /// 异步尝试写入
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Task<bool> TryWriteAsync(this IDmtpChannel channel, byte[] data)
        {
            return TryWriteAsync(channel, data, 0, data.Length);
        }
    }
}