
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




using System.Net;
using System.Text;

namespace ThingsGateway.Foundation;

/// <summary>
/// UDP适配器基类
/// </summary>
public abstract class ReadWriteDevicesUdpDataHandleAdapter<TRequest> : UdpDataHandlingAdapter where TRequest : class, IMessage
{
    /// <summary>
    /// 报文输出时采用字符串还是HexString
    /// </summary>
    public virtual bool IsHexData { get; set; } = true;

    /// <inheritdoc cref="ReadWriteDevicesUdpDataHandleAdapter{TRequest}"/>
    public ReadWriteDevicesUdpDataHandleAdapter()
    {
        Request = GetInstance();
    }

    /// <inheritdoc/>
    public override bool CanSendRequestInfo => true;

    /// <inheritdoc/>
    public override bool CanSplicingSend => false;

    /// <inheritdoc/>
    public virtual bool IsSendPackCommand { get; set; } = true;

    /// <summary>
    /// 非并发协议中，每次交互的对象，会在发送时重新获取
    /// </summary>
    public TRequest Request { get; set; }

    /// <summary>
    /// 发送前，对当前的命令进行打包处理<br />
    /// </summary>
    public abstract byte[] PackCommand(byte[] command, TRequest item);

    /// <inheritdoc/>
    public override string? ToString()
    {
        return (Owner as UdpSession)?.RemoteIPHost?.ToString() ?? Owner.ToString();
    }

    /// <summary>
    /// 获取泛型实例。
    /// </summary>
    /// <returns></returns>
    protected abstract TRequest GetInstance();

    /// <summary>
    /// 预发送方法，会对命令重新打包并发送字节数组
    /// </summary>
    protected void GoSend(EndPoint endPoint, byte[] item, TRequest request)
    {
        byte[] bytes;
        if (IsSendPackCommand)
            bytes = PackCommand(item, request);
        else
            bytes = item;
        Request = request;
        Request.SendBytes = bytes;
        GoSend(endPoint, bytes, 0, bytes.Length);
        if (Logger.LogLevel <= LogLevel.Trace)
            Logger?.Trace($"{endPoint}- Send:{(IsHexData ? Request.SendBytes.ToHexString(' ') : Encoding.UTF8.GetString(Request.SendBytes))}");
    }

    /// <inheritdoc/>
    protected override void PreviewSend(EndPoint endPoint, byte[] buffer, int offset, int length)
    {
        GoSend(endPoint, buffer, GetInstance());
    }

    /// <inheritdoc/>
    protected override Task PreviewSendAsync(EndPoint endPoint, byte[] buffer, int offset, int length)
    {
        return GoSendAsync(endPoint, buffer, GetInstance());
    }

    /// <summary>
    /// 预发送方法，会对命令重新打包并发送字节数组
    /// </summary>
    protected async Task GoSendAsync(EndPoint endPoint, byte[] item, TRequest request)
    {
        byte[] bytes;
        if (IsSendPackCommand)
            bytes = PackCommand(item, request);
        else
            bytes = item;
        Request = request;
        Request.SendBytes = bytes;
        await GoSendAsync(endPoint, bytes, 0, bytes.Length);
        if (Logger.LogLevel <= LogLevel.Trace)
            Logger?.Trace($"{endPoint}- Send:{(IsHexData ? Request.SendBytes.ToHexString(' ') : Encoding.UTF8.GetString(Request.SendBytes))}");
    }

    /// <inheritdoc/>
    protected override void PreviewSend(EndPoint endPoint, IRequestInfo requestInfo)
    {
        if (requestInfo == null)
        {
            throw new ArgumentNullException(nameof(requestInfo));
        }
        var message = (ISendMessage)requestInfo;
        var request = GetInstance();
        request.Sign = message.Sign;
        GoSend(endPoint, message.SendBytes, request);
    }

    /// <inheritdoc/>
    protected override Task PreviewSendAsync(EndPoint endPoint, IRequestInfo requestInfo)
    {
        if (requestInfo == null)
        {
            throw new ArgumentNullException(nameof(requestInfo));
        }
        var message = (ISendMessage)requestInfo;
        var request = GetInstance();
        request.Sign = message.Sign;
        return GoSendAsync(endPoint, message.SendBytes, request);
    }

    /// <inheritdoc/>
    protected override void PreviewReceived(EndPoint remoteEndPoint, ByteBlock byteBlock)
    {
        var allBytes = byteBlock.ToArray(0, byteBlock.Len);
        if (Logger.LogLevel <= LogLevel.Trace)
            Logger?.Trace($"{remoteEndPoint}- Receive:{(IsHexData ? allBytes.ToHexString(' ') : Encoding.UTF8.GetString(allBytes))}");

        //if (Request?.SendBytes == null)
        //{
        //    GoReceived(remoteEndPoint, byteBlock, null);
        //    return;
        //}
        byte[] header = new byte[] { };
        if (Request.HeadBytesLength > 0)
        {
            //当解析消息设定固定头长度大于0时，获取头部字节
            byteBlock.Read(out header, Request.HeadBytesLength);
        }
        //检查头部合法性
        if (Request.CheckHeadBytes(header))
        {
            if (Request.BodyLength <= 0)
            {
                Request.BodyLength = byteBlock.Len;
            }
            byteBlock.Read(out byte[] body, Request.BodyLength);
            var bytes = DataTransUtil.SpliceArray(Request.HeadBytes, body);
            var unpackbytes = UnpackResponse(Request, Request.SendBytes, bytes);
            Request.ErrorMessage = unpackbytes.ErrorMessage;
            Request.OperCode = unpackbytes.OperCode;
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
                Logger?.Warning(unpackbytes.ErrorMessage);
                GoReceived(remoteEndPoint, null, Request);
                return;
            }
        }
    }

    /// <inheritdoc/>
    protected override void Reset()
    {
    }

    /// <summary>
    /// 根据对方返回的报文命令，对命令进行基本的拆包<br />
    /// </summary>
    /// <remarks>
    /// 在实际解包的操作过程中，通常对状态码，错误码等消息进行判断，如果校验不通过，将携带错误消息返回<br />
    /// </remarks>
    /// <param name="request">消息类</param>
    /// <param name="send">发送的原始报文数据</param>
    /// <param name="response">设备方反馈的原始报文内容</param>
    /// <returns>返回拆包之后的报文信息，默认不进行任何的拆包操作</returns>
    protected abstract OperResult<byte[]> UnpackResponse(TRequest request, byte[]? send, byte[] response);
}