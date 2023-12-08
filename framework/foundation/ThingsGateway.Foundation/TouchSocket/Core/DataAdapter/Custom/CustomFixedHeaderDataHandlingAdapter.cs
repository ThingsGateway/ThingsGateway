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

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// 用户自定义固定包头解析器，使用该适配器时，接收方收到的数据中，<see cref="ByteBlock"/>将为null，同时<see cref="IRequestInfo"/>将实现为TFixedHeaderRequestInfo。
    /// </summary>
    public abstract class CustomFixedHeaderDataHandlingAdapter<TFixedHeaderRequestInfo> : CustomDataHandlingAdapter<TFixedHeaderRequestInfo> where TFixedHeaderRequestInfo : class, IFixedHeaderRequestInfo
    {
        /// <summary>
        /// 固定包头的长度。
        /// </summary>
        public abstract int HeaderLength { get; }

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
        protected override FilterResult Filter(in ByteBlock byteBlock, bool beCached, ref TFixedHeaderRequestInfo request, ref int tempCapacity)
        {
            if (beCached)
            {
                if (request.BodyLength > byteBlock.CanReadLen)//body不满足解析，开始缓存，然后保存对象
                {
                    tempCapacity = request.BodyLength + this.HeaderLength;
                    return FilterResult.Cache;
                }

                byteBlock.Read(out var body, request.BodyLength);
                if (request.OnParsingBody(body))
                {
                    return FilterResult.Success;
                }
                else
                {
                    request = default;//放弃所有解析
                    return FilterResult.GoOn;
                }
            }
            else
            {
                if (this.HeaderLength > byteBlock.CanReadLen)
                {
                    return FilterResult.Cache;
                }

                var requestInfo = this.GetInstance();
                byteBlock.Read(out var header, this.HeaderLength);
                if (requestInfo.OnParsingHeader(header))
                {
                    request = requestInfo;
                    if (request.BodyLength > byteBlock.CanReadLen)//body不满足解析，开始缓存，然后保存对象
                    {
                        tempCapacity = request.BodyLength + this.HeaderLength;
                        return FilterResult.Cache;
                    }

                    byteBlock.Read(out var body, request.BodyLength);
                    if (request.OnParsingBody(body))
                    {
                        return FilterResult.Success;
                    }
                    else
                    {
                        request = default;//放弃所有解析
                        return FilterResult.GoOn;
                    }
                }
                else
                {
                    return FilterResult.GoOn;
                }
            }
        }

        /// <summary>
        /// 获取泛型实例。
        /// </summary>
        /// <returns></returns>
        protected abstract TFixedHeaderRequestInfo GetInstance();
    }

    /// <summary>
    /// 用户自定义固定包头请求
    /// </summary>
    public interface IFixedHeaderRequestInfo : IRequestInfo
    {
        /// <summary>
        /// 数据体长度
        /// </summary>
        int BodyLength { get; }

        /// <summary>
        /// 当收到数据，由框架封送固定协议头。
        /// <para>您需要在此函数中，解析自己的固定包头，并且对<see cref="BodyLength"/>赋值后续数据的长度，然后返回True。</para>
        /// <para>如果返回false，则意味着放弃本次解析</para>
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        bool OnParsingHeader(byte[] header);

        /// <summary>
        /// 当收到数据，由框架封送有效载荷数据。
        /// <para>如果返回false，意味着放弃本次解析的所有数据，包括已经解析完成的Header</para>
        /// </summary>
        /// <param name="body">载荷数据</param>
        /// <returns>是否成功有效</returns>
        bool OnParsingBody(byte[] body);
    }
}