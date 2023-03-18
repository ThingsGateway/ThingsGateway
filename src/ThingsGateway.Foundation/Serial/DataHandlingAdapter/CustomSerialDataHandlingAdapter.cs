//------------------------------------------------------------------------------
//  此代码版权归作者若汝棋茗所有
//  源代码使用协议遵循TouchSocket的开源协议及附加协议
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.Serial
{
    /// <summary>
    /// 用户自定义数据处理适配器，使用该适配器时，接收方收到的数据中，<see cref="ByteBlock"/>将为null，
    /// 同时<see cref="IRequestInfo"/>将实现为TRequest，发送数据直接发送。
    /// <para>此处设计思路借鉴SuperSocket。</para>
    /// </summary>
    public abstract class CustomSerialDataHandlingAdapter<TRequest> : SerialDataHandlingAdapter where TRequest : class, IRequestInfo
    {
        /// <summary>
        /// 缓存数据，如果需要手动释放，请先判断，然后到调用<see cref="IDisposable.Dispose"/>后，再置空；
        /// </summary>
        protected ByteBlock TempByteBlock;

        /// <summary>
        /// 缓存对象。
        /// </summary>
        protected TRequest TempRequest;

        private bool m_needReset;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanSendRequestInfo => false;

        /// <summary>
        /// 默认不支持拼接发送
        /// </summary>
        public override bool CanSplicingSend => false;

        /// <summary>
        /// 筛选解析数据。实例化的TRequest会一直保存，直至解析成功，或手动清除。
        /// <para>当不满足解析条件时，请返回<see cref="FilterResult.Cache"/>，此时会保存<see cref="ByteBlock.CanReadLen"/>的数据</para>
        /// <para>当数据部分异常时，请移动<see cref="ByteBlock.Pos"/>到指定位置，然后返回<see cref="FilterResult.GoOn"/></para>
        /// <para>当完全满足解析条件时，请返回<see cref="FilterResult.Success"/>最后将<see cref="ByteBlock.Pos"/>移至指定位置。</para>
        /// </summary>
        /// <param name="byteBlock">字节块</param>
        /// <param name="beCached">是否为上次遗留对象，当该参数为True时，request也将是上次实例化的对象。</param>
        /// <param name="request">对象。</param>
        /// <param name="tempCapacity">缓存容量。当需要首次缓存时，指示申请的ByteBlock的容量。合理的值可避免ByteBlock扩容带来的性能消耗。</param>
        /// <returns></returns>
        protected abstract FilterResult Filter(ByteBlock byteBlock, bool beCached, ref TRequest request, ref int tempCapacity);

        /// <summary>
        /// 成功执行接收以后。
        /// </summary>
        /// <param name="request"></param>
        protected virtual void OnReceivedSuccess(TRequest request)
        {
        }

        /// <summary>
        /// 即将执行<see cref="DataHandlingAdapter.GoReceived(ByteBlock, IRequestInfo)"/>。
        /// </summary>
        /// <param name="request"></param>
        /// <returns>返回值标识是否继续执行</returns>
        protected virtual bool OnReceivingSuccess(TRequest request)
        {
            return true;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        protected override void PreviewReceived(ByteBlock byteBlock)
        {
            if (m_needReset || CacheTimeoutEnable && DateTime.Now - LastCacheTime > CacheTimeout)
            {
                Reset();
                m_needReset = false;
            }
            if (TempByteBlock == null)
            {
                Single(byteBlock, false);
            }
            else
            {
                TempByteBlock.Write(byteBlock.Buffer, 0, byteBlock.Len);
                ByteBlock block = TempByteBlock;
                TempByteBlock = null;
                Single(block, true);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="requestInfo"></param>
        protected override void PreviewSend(IRequestInfo requestInfo)
        {
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer">数据</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        protected override void PreviewSend(byte[] buffer, int offset, int length)
        {
            GoSend(buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        protected override void PreviewSend(IList<ArraySegment<byte>> transferBytes)
        {
            throw new System.NotImplementedException();//因为设置了不支持拼接发送，所以该方法可以不实现。
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Reset()
        {
            TempByteBlock.SafeDispose();
            TempByteBlock = null;
            TempRequest = default;
            m_needReset = true;
        }

        private void Single(ByteBlock byteBlock, bool temp)
        {
            byteBlock.Pos = 0;
            while (byteBlock.Pos < byteBlock.Len)
            {
                if (m_needReset)
                {
                    return;
                }
                int tempCapacity = 1024 * 64;
                FilterResult filterResult = Filter(byteBlock, TempRequest != null, ref TempRequest, ref tempCapacity);
                switch (filterResult)
                {
                    case FilterResult.Success:
                        if (OnReceivingSuccess(TempRequest))
                        {
                            GoReceived(null, TempRequest);
                            OnReceivedSuccess(TempRequest);
                        }
                        TempRequest = default;
                        break;

                    case FilterResult.Cache:
                        if (byteBlock.CanReadLen > 0)
                        {
                            if (temp)
                            {
                                TempByteBlock = new ByteBlock(tempCapacity);
                                TempByteBlock.Write(byteBlock.Buffer, byteBlock.Pos, byteBlock.CanReadLen);
                                byteBlock.Dispose();
                            }
                            else
                            {
                                TempByteBlock = new ByteBlock(tempCapacity);
                                TempByteBlock.Write(byteBlock.Buffer, byteBlock.Pos, byteBlock.CanReadLen);
                            }

                            if (TempByteBlock.Len > MaxPackageSize)
                            {
                                OnError("缓存的数据长度大于设定值的情况下未收到解析信号");
                            }
                        }
                        if (UpdateCacheTimeWhenRev)
                        {
                            LastCacheTime = DateTime.Now;
                        }
                        return;

                    case FilterResult.GoOn:
                        if (UpdateCacheTimeWhenRev)
                        {
                            LastCacheTime = DateTime.Now;
                        }
                        break;
                }
            }
        }
    }
}