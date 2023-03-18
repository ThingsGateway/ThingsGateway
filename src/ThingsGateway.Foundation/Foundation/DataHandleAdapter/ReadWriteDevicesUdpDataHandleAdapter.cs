using System.Net;

using TouchSocket.Sockets;

namespace ThingsGateway.Foundation
{
    public abstract class ReadWriteDevicesUdpDataHandleAdapter<TRequest> : UdpDataHandlingAdapter where TRequest : class, IMessage
    {
        public ReadWriteDevicesUdpDataHandleAdapter()
        {
            Request = GetInstance();
        }
        public override bool CanSendRequestInfo => false;
        public override bool CanSplicingSend => false;
        public virtual bool IsSendPackCommand { get; set; } = true;

        /// <summary>
        /// 读写接口返回的实际消息对象，每次发送数据前会清空字节数组
        /// </summary>
        public TRequest Request { get; set; }

        /// <summary>
        /// 发送前，对当前的命令进行打包处理<br />
        /// </summary>
        public abstract byte[] PackCommand(byte[] command);

        /// <summary>
        /// 获取泛型实例。
        /// </summary>
        /// <returns></returns>
        protected abstract TRequest GetInstance();

        /// <summary>
        /// 预发送方法，会对命令重新打包并发送字节数组
        /// </summary>
        /// <param name="item"></param>
        /// <param name="isAsync"></param>
        protected void GoSend(EndPoint endPoint, byte[] item)
        {
            byte[] bytes = null;
            if (IsSendPackCommand)
                bytes = PackCommand(item);
            else
                bytes = item;
            Request = GetInstance();
            Request.SendBytes = bytes;
            GoSend(endPoint, bytes, 0, bytes.Length);
            Owner.Logger?.Debug(ToString() + Environment.NewLine + ThingsGateway.Foundation.Resources.Resource.Send + " : " + Request.SendBytes.ToHexString(" "));
        }

        protected override void PreviewReceived(EndPoint remoteEndPoint, ByteBlock byteBlock)
        {
            var allBytes = byteBlock.ToArray(0, byteBlock.Len);
            Owner.Logger?.Debug(ToString() + Environment.NewLine + ThingsGateway.Foundation.Resources.Resource.Received + " : " + allBytes.ToHexString(" "));

            if (Request?.SendBytes == null)
            {
                GoReceived(remoteEndPoint, byteBlock, null);
                return;
            }
            byte[] header = new byte[] { };
            if (Request.HeadBytesLength > 0)
            {
                byteBlock.Read(out header, Request.HeadBytesLength);
            }
            if (Request.CheckHeadBytes(header))
            {
                if (Request.BodyLength <= 0)
                {
                    Request.BodyLength = byteBlock.Len;
                }
                byteBlock.Read(out byte[] body, Request.BodyLength);
                var bytes = Request.HeadBytes.SpliceArray(body);
                var unpackbytes = UnpackResponse(Request.SendBytes, bytes);
                Request.Message = unpackbytes.Message;
                Request.ResultCode = unpackbytes.ResultCode;
                if (unpackbytes.IsSuccess)
                {
                    Request.Content = unpackbytes.Content;
                    Request.ReceivedBytes = bytes;
                    GoReceived(remoteEndPoint, null, Request);
                    return;
                }
                else
                {
                    byteBlock.Pos = byteBlock.Len;
                    Request.ReceivedBytes = byteBlock.ToArray(0, byteBlock.Len);
                    Owner.Logger?.Warning(ToString() + unpackbytes.Message);
                    GoReceived(remoteEndPoint, null, Request);
                    return;
                }
            }
        }

        protected override void PreviewSend(EndPoint endPoint, byte[] buffer, int offset, int length)
        {
            GoSend(endPoint, buffer);
        }

        protected override void PreviewSend(EndPoint endPoint, IList<ArraySegment<byte>> transferBytes)
        {
            throw new System.NotImplementedException();//因为设置了不支持拼接发送，所以该方法可以不实现。
        }

        protected override void PreviewSend(IRequestInfo requestInfo)
        {
            throw new System.NotImplementedException();//因为设置了不支持，所以该方法可以不实现。
        }

        protected override void Reset()
        {
        }

        /// <summary>
        /// 根据对方返回的报文命令，对命令进行基本的拆包<br />
        /// </summary>
        /// <remarks>
        /// 在实际解包的操作过程中，通常对状态码，错误码等消息进行判断，如果校验不通过，将携带错误消息返回<br />
        /// </remarks>
        /// <param name="send">发送的原始报文数据</param>
        /// <param name="response">设备方反馈的原始报文内容</param>
        /// <returns>返回拆包之后的报文信息，默认不进行任何的拆包操作</returns>
        protected abstract OperResult<byte[]> UnpackResponse(byte[] send, byte[] response);
    }
}