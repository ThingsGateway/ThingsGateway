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

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// CacheDataHandlingAdapter
    /// </summary>
    public abstract class CacheDataHandlingAdapter : SingleStreamDataHandlingAdapter
    {
        /// <summary>
        /// 缓存数据，如果需要手动释放，请先判断，然后到调用<see cref="IDisposable.Dispose"/>后，再置空；
        /// </summary>
        protected ByteBlock m_cacheByteBlock;

        /// <summary>
        /// 将数据缓存起来
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        protected void Cache(byte[] buffer, int offset, int length)
        {
            this.m_cacheByteBlock ??= new ByteBlock(length);
            this.m_cacheByteBlock.Write(buffer, offset, length);
            if (this.UpdateCacheTimeWhenRev)
            {
                this.LastCacheTime = DateTime.Now;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Reset()
        {
            this.m_cacheByteBlock.SafeDispose();
            this.m_cacheByteBlock = null;
            base.Reset();
        }

        /// <summary>
        /// 获取当前缓存，
        /// 如果缓存超时，或者不存在，均会返回false。
        /// 如果获取成功，则会清空内部缓存。
        /// </summary>
        /// <returns></returns>
        protected bool TryGetCache(out byte[] buffer)
        {
            if (this.m_cacheByteBlock == null)
            {
                buffer = null;
                return false;
            }
            if (DateTime.Now - this.LastCacheTime > this.CacheTimeout)
            {
                this.m_cacheByteBlock.SafeDispose();
                this.m_cacheByteBlock = null;
                buffer = null;
                return false;
            }
            buffer = this.m_cacheByteBlock.ToArray();
            this.m_cacheByteBlock.SafeDispose();
            this.m_cacheByteBlock = null;
            return true;
        }

        /// <summary>
        /// 获取缓存，注意：获取的ByteBlock需要手动释放。
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <returns></returns>
        protected bool TryGetCache(out ByteBlock byteBlock)
        {
            if (this.m_cacheByteBlock == null)
            {
                byteBlock = null;
                return false;
            }
            if (DateTime.Now - this.LastCacheTime > this.CacheTimeout)
            {
                this.m_cacheByteBlock.SafeDispose();
                this.m_cacheByteBlock = null;
                byteBlock = null;
                return false;
            }
            byteBlock = this.m_cacheByteBlock;
            return true;
        }
    }
}