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
    /// 内存池
    /// </summary>
    public class BytePool : ArrayPool<byte>
    {
        private readonly Timer m_timer;

        static BytePool()
        {
            Default = new BytePool();
        }

        /// <summary>
        /// 内存池
        /// </summary>
        public BytePool() : this(1024 * 1024 * 10, 100)
        {
        }

        /// <summary>
        /// 内存池
        /// </summary>
        /// <param name="maxArrayLength"></param>
        /// <param name="maxArraysPerBucket"></param>
        public BytePool(int maxArrayLength, int maxArraysPerBucket) : base(maxArrayLength, maxArraysPerBucket)
        {
            this.m_timer = new Timer((o) =>
            {
                this.Clear();
            }, null, 1000 * 60 * 60, 1000 * 60 * 60);
            this.AutoZero = false;
            this.MaxBlockSize = maxArrayLength;
        }

        /// <summary>
        /// 默认的内存池实例
        /// </summary>
        public static BytePool Default { get; private set; }

        /// <summary>
        /// 设置默认内存池实例。
        /// </summary>
        /// <param name="bytePool"></param>
        public static void SetDefault(BytePool bytePool)
        {
            Default = bytePool;
        }

        /// <summary>
        /// 回收内存时，自动归零
        /// </summary>
        public bool AutoZero { get; set; }

        /// <summary>
        /// 单个块最大值
        /// </summary>
        public int MaxBlockSize { get; private set; }

        /// <summary>
        /// 获取ByteBlock
        /// </summary>
        /// <param name="byteSize">长度</param>
        /// <returns></returns>
        public ByteBlock GetByteBlock(int byteSize)
        {
            return new ByteBlock(byteSize, this);
        }

        /// <summary>
        ///  获取ValueByteBlock
        /// </summary>
        /// <param name="byteSize"></param>
        /// <returns></returns>
        public ValueByteBlock GetValueByteBlock(int byteSize)
        {
            return new ValueByteBlock(byteSize, this);
        }
    }
}