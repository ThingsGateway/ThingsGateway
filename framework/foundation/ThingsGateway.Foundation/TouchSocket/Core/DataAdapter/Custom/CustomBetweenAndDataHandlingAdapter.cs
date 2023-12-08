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
    /// 区间数据包处理适配器，支持以任意字符、字节数组起始与结尾的数据包。
    /// </summary>
    public abstract class CustomBetweenAndDataHandlingAdapter<TBetweenAndRequestInfo> : CustomDataHandlingAdapter<TBetweenAndRequestInfo> where TBetweenAndRequestInfo : class, IBetweenAndRequestInfo
    {
        /// <summary>
        /// 起始字符，不可以为null，可以为0长度
        /// </summary>
        public abstract byte[] StartCode { get; }

        /// <summary>
        /// 即使找到了终止因子，也不会结束，默认0
        /// </summary>
        public int MinSize { get; set; }

        /// <summary>
        /// 结束字符，不可以为null，不可以为0长度，必须具有有效值。
        /// </summary>
        public abstract byte[] EndCode { get; }

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
        protected override FilterResult Filter(in ByteBlock byteBlock, bool beCached, ref TBetweenAndRequestInfo request, ref int tempCapacity)
        {
            if (beCached)
            {
                int len;
                var pos = byteBlock.Pos;
                while (true)
                {
                    var indexEnd = byteBlock.Buffer.IndexOfFirst(byteBlock.Pos, byteBlock.CanReadLen, this.EndCode);
                    if (indexEnd == -1)
                    {
                        byteBlock.Pos = pos;
                        return FilterResult.Cache;
                    }

                    len = indexEnd - this.EndCode.Length - pos + 1;
                    if (len >= this.MinSize)
                    {
                        break;
                    }
                    byteBlock.Pos += len + this.EndCode.Length;
                }

                byteBlock.Pos = pos;
                request.OnParsingBody(byteBlock.ToArray(pos, len));
                byteBlock.Pos += len;

                if (request.OnParsingEndCode(byteBlock.ToArray(byteBlock.Pos, this.EndCode.Length)))
                {
                    byteBlock.Pos += this.EndCode.Length;
                    return FilterResult.Success;
                }
                else
                {
                    byteBlock.Pos += this.EndCode.Length;
                    return FilterResult.GoOn;
                }
            }
            else
            {
                var requestInfo = this.GetInstance();

                var indexStart = byteBlock.Buffer.IndexOfFirst(byteBlock.Pos, byteBlock.CanReadLen, this.StartCode);
                if (indexStart == -1)
                {
                    return FilterResult.Cache;
                }
                if (!requestInfo.OnParsingStartCode(byteBlock.ToArray(byteBlock.Pos + indexStart + 1 - this.StartCode.Length, this.StartCode.Length)))
                {
                    byteBlock.Pos += indexStart;
                    return FilterResult.GoOn;
                }
                byteBlock.Pos += indexStart + 1;
                request = requestInfo;

                int len;
                var pos = byteBlock.Pos;
                while (true)
                {
                    var indexEnd = byteBlock.Buffer.IndexOfFirst(byteBlock.Pos, byteBlock.CanReadLen, this.EndCode);
                    if (indexEnd == -1)
                    {
                        byteBlock.Pos = pos;
                        return FilterResult.Cache;
                    }

                    len = indexEnd - this.EndCode.Length - pos + 1;
                    if (len >= this.MinSize)
                    {
                        break;
                    }
                    byteBlock.Pos += len + this.EndCode.Length;
                }

                byteBlock.Pos = pos;
                request.OnParsingBody(byteBlock.ToArray(pos, len));
                byteBlock.Pos += len;

                if (request.OnParsingEndCode(byteBlock.ToArray(byteBlock.Pos, this.EndCode.Length)))
                {
                    byteBlock.Pos += this.EndCode.Length;
                    return FilterResult.Success;
                }
                else
                {
                    byteBlock.Pos += this.EndCode.Length;
                    return FilterResult.GoOn;
                }
            }
        }

        /// <summary>
        /// 获取泛型实例。
        /// </summary>
        /// <returns></returns>
        protected abstract TBetweenAndRequestInfo GetInstance();
    }

    /// <summary>
    /// 区间类型的适配器数据模型接口。
    /// </summary>
    public interface IBetweenAndRequestInfo : IRequestInfo
    {
        /// <summary>
        /// 当解析到起始字符时。
        /// </summary>
        /// <param name="startCode"></param>
        /// <returns></returns>
        bool OnParsingStartCode(byte[] startCode);

        /// <summary>
        /// 当解析数据体。
        /// <para>在此方法中，您必须手动保存Body内容</para>
        /// </summary>
        /// <param name="body"></param>
        void OnParsingBody(byte[] body);

        /// <summary>
        /// 当解析到起始字符时。
        /// </summary>
        /// <param name="endCode"></param>
        /// <returns></returns>
        bool OnParsingEndCode(byte[] endCode);
    }
}