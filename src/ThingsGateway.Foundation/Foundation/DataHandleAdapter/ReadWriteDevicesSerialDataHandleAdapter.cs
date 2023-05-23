#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using ThingsGateway.Foundation.Serial;

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 串口适配器基类
    /// </summary>
    public abstract class ReadWriteDevicesSerialDataHandleAdapter<TRequest> : CustomSerialDataHandlingAdapter<TRequest> where TRequest : class, IMessage
    {
        /// <inheritdoc cref="ReadWriteDevicesSerialDataHandleAdapter{TRequest}"/>
        public ReadWriteDevicesSerialDataHandleAdapter()
        {
            Request = GetInstance();
        }

        /// <inheritdoc/>
        public override bool CanSendRequestInfo => false;

        /// <inheritdoc/>
        public override bool CanSplicingSend => false;

        /// <inheritdoc/>
        public virtual bool IsSendPackCommand { get; set; } = true;

        /// <inheritdoc/>
        public TRequest Request { get; set; }

        /// <summary>
        /// 发送前，对当前的命令进行打包处理
        /// </summary>
        public abstract byte[] PackCommand(byte[] command);

        /// <summary>
        /// 筛选解析数据。实例化的request会一直保存，直至解析成功，或手动清除。
        /// <para>当不满足解析条件时，请返回<see cref="FilterResult.Cache"/>，此时会保存<see cref="ByteBlock.CanReadLen"/>的数据</para>
        /// <para>当数据部分异常时，请移动<see cref="ByteBlock.Pos"/>到指定位置，然后返回<see cref="FilterResult.GoOn"/></para>
        /// <para>当完全满足解析条件时，请返回<see cref="FilterResult.Success"/>最后将<see cref="ByteBlock.Pos"/>移至指定位置。</para>
        /// </summary>
        /// <param name="byteBlock">字节块</param>
        /// <param name="beCached">是否为上次遗留对象，当该参数为True时，request也将是上次实例化的对象。</param>
        /// <param name="request">对象。</param>
        /// <param name="tempCapacity">缓存容量。当需要首次缓存时，指示申请的ByteBlock的容量。合理的值可避免ByteBlock扩容带来的性能消耗。</param>
        /// <returns></returns>
        protected override FilterResult Filter(ByteBlock byteBlock, bool beCached, ref TRequest request, ref int tempCapacity)
        {
            var allBytes = (byteBlock.ToArray(0, byteBlock.Len));
            Client.Logger?.Trace("报文-" + Client.SerialProperty.ToString() + "-" + ThingsGateway.Foundation.Resources.Resource.Received + ":" + allBytes.ToHexString(" "));

            //if (Request?.SendBytes == null)
            //{
            //    request = default;//放弃所有解析
            //    return FilterResult.GoOn;
            //}
            if (beCached)
            {
                byteBlock.Read(out byte[] body, byteBlock.Len);
                var bytes = request.HeadBytes.SpliceArray(body);
                return GetResponse(byteBlock, request, request.ReceivedBytes.SpliceArray(allBytes), bytes);
            }
            else
            {
                request = Request;
                if (request.HeadBytesLength > byteBlock.CanReadLen)
                {
                    return FilterResult.Cache;
                }
                byte[] header = new byte[] { };
                if (request.HeadBytesLength > 0)
                {
                    byteBlock.Read(out header, request.HeadBytesLength);
                }
                if (request.CheckHeadBytes(header))
                {
                    if (request.BodyLength > byteBlock.CanReadLen)//body不满足解析，开始缓存，然后保存对象
                    {
                        return FilterResult.Cache;
                    }
                    if (request.BodyLength <= 0)
                    {
                        request.BodyLength = byteBlock.Len;
                    }
                    byteBlock.Read(out byte[] body, request.BodyLength);
                    var bytes = request.HeadBytes.SpliceArray(body);
                    return GetResponse(byteBlock, request, allBytes, bytes);
                }
                else
                {
                    return FilterResult.GoOn;
                }
            }
        }
        /// <summary>
        /// 解包获取实际数据包
        /// </summary>
        protected virtual FilterResult GetResponse(ByteBlock byteBlock, TRequest request, byte[] allBytes, byte[] bytes)
        {
            var unpackbytes = UnpackResponse(request.SendBytes, allBytes);
            request.Message = unpackbytes.Message;
            request.ResultCode = unpackbytes.ResultCode;
            if (unpackbytes.IsSuccess)
            {
                request.Content = unpackbytes.Content;
                request.ReceivedBytes = bytes;
                return FilterResult.Success;
            }
            else
            {
                byteBlock.Pos = byteBlock.Len;
                request.ReceivedBytes = allBytes;
                Client.Logger?.Warning(Client.SerialProperty.ToString() + unpackbytes.Message);
                return FilterResult.Success;
            }
        }

        /// <summary>
        /// 获取泛型实例。
        /// </summary>
        /// <returns></returns>
        protected abstract TRequest GetInstance();

        /// <summary>
        /// 发送方法,会重新建立<see cref="Request"/>
        /// </summary>
        protected void GoSend(byte[] item)
        {
            byte[] bytes = null;
            if (IsSendPackCommand)
                bytes = PackCommand(item);
            else
                bytes = item;
            Request = GetInstance();
            Request.SendBytes = bytes;
            GoSend(bytes, 0, bytes.Length);
            Client.Logger?.Trace("报文-" + Client.SerialProperty.ToString() + "-" + ThingsGateway.Foundation.Resources.Resource.Send + ":" + Request.SendBytes.ToHexString(" "));
        }

        /// <inheritdoc/>
        protected override void PreviewSend(byte[] buffer, int offset, int length)
        {
            GoSend(buffer);
        }

        /// <summary>
        /// 报文拆包
        /// </summary>
        protected abstract OperResult<byte[]> UnpackResponse(byte[] send, byte[] response);
    }
}