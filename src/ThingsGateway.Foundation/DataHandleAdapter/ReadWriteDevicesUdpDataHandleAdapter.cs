
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------




using System.Net;
using System.Text;

namespace ThingsGateway.Foundation;

/// <summary>
/// UDP适配器基类
/// </summary>
public abstract class ReadWriteDevicesUdpDataHandleAdapter<TRequest> : UdpDataHandlingAdapter where TRequest : class, IResultMessage, new()
{
    /// <inheritdoc/>
    public override bool CanSendRequestInfo => true;

    /// <inheritdoc/>
    public override bool CanSplicingSend => false;

    /// <summary>
    /// 报文输出时采用字符串还是HexString
    /// </summary>
    public virtual bool IsHexData { get; set; } = true;
    /// <inheritdoc/>
    public virtual bool IsSendPackCommand { get; set; } = true;
    /// <summary>
    /// 是否非并发协议
    /// </summary>
    public virtual bool IsSingleThread { get; } = true;
    /// <summary>
    /// 非并发协议中，每次交互的对象，会在发送时重新获取
    /// </summary>
    public TRequest Request { get; set; }

    /// <summary>
    /// 发送前，对当前的命令进行打包处理<br />
    /// </summary>
    public abstract void PackCommand(ISendMessage item);

    /// <inheritdoc/>
    public override string? ToString()
    {
        return Owner.ToString();
    }

    /// <summary>
    /// 获取泛型实例。
    /// </summary>
    /// <returns></returns>
    protected virtual TRequest GetInstance()
    {
        return new TRequest();
    }

    /// <inheritdoc/>
    protected override Task PreviewReceived(EndPoint remoteEndPoint, ByteBlock byteBlock)
    {
        if (Logger.LogLevel <= LogLevel.Trace)
            Logger?.Trace($"{ToString()}- Receive:{(IsHexData ? byteBlock.Buffer.ToHexString(0, byteBlock.Len, ' ') : byteBlock.ToString())}");

        TRequest request = null;
        //非并发协议,复用对象
        if (IsSingleThread)
            request = request == null ? GetInstance() : request.DisposedValue ? GetInstance() : request;
        else
        {
            //并发协议非缓存模式下，重新获取对象
            request = GetInstance();
        }

        //传入新的ByteBlock对象，避免影响原有的游标
        //当解析消息设定固定头长度大于0时，获取头部字节
        if (request.HeadBytesLength > 0)
        {
            var header = byteBlock.ToArrayTake(request.HeadBytesLength);
            Check(byteBlock, header);
            request.SafeDispose();
            return GoReceived(remoteEndPoint, null, request);
        }
        else
        {
            Check(byteBlock, null);
            request.SafeDispose();
            return GoReceived(remoteEndPoint, null, request);
        }

        void Check(ByteBlock byteBlock, byte[]? header)
        {
            //检查头部合法性
            if (request.CheckHeadBytes(header))
            {
                if (request.BodyLength > this.MaxPackageSize)
                {
                    this.OnError(default, $"Received BodyLength={request.BodyLength}, greater than the set MaxPackageSize={this.MaxPackageSize}", true, true);
                    return;
                }

                if (request.BodyLength + request.HeadBytesLength > byteBlock.CanReadLen)
                {
                    return;
                }
                if (request.BodyLength <= 0)
                {
                    //如果body长度无法确定，直接读取全部
                    request.BodyLength = byteBlock.Len;
                }

                request.ReceivedByteBlock = byteBlock;

                var result = UnpackResponse(request);

                byteBlock.Pos += request.BodyLength;
                if (request.IsSuccess)
                    request.Content = result;
                return;
            }
            else
            {
                byteBlock.Pos = byteBlock.Len;//移动游标
                request.OperCode = -1;
                return;
            }
        }

    }

    /// <inheritdoc/>
    protected override void PreviewSend(EndPoint endPoint, byte[] buffer, int offset, int length)
    {
        var send = new SendMessage(buffer, offset, length);
        //发送前打包
        if (IsSendPackCommand)
            PackCommand(send);
        if (Logger.LogLevel <= LogLevel.Trace)
            Logger?.Trace($"{ToString()}- Send:{(IsHexData ? send.SendBytes.ToHexString(send.Offset, send.Length, ' ') : Encoding.UTF8.GetString(send.SendBytes, send.Offset, send.Length))}");
        //发送
        this.GoSend(endPoint, send.SendBytes, send.Offset, send.Length);
    }

    /// <inheritdoc/>
    protected override void PreviewSend(EndPoint endPoint, IRequestInfo requestInfo)
    {
        if (!(requestInfo is ISendMessage message))
        {
            throw new Exception($"Unable to convert {nameof(requestInfo)} to {nameof(ISendMessage)}");
        }
        //发送前打包
        if (IsSendPackCommand)
            PackCommand(message);

        if (Logger.LogLevel <= LogLevel.Trace)
            Logger?.Trace($"{ToString()}- Send:{(IsHexData ? message.SendBytes.ToHexString(message.Offset, message.Length, ' ') : Encoding.UTF8.GetString(message.SendBytes, message.Offset, message.Length))}");
        //非并发主从协议
        if (IsSingleThread)
        {
            var request = GetInstance();
            request.Sign = message.Sign;
            if (message.Offset != 0 || message.Length != message.SendBytes.Length)
                request.SendBytes = message.SendBytes;
            else
            {
                byte[] bytes = new byte[message.Length];
                Array.Copy(message.SendBytes, message.Offset, bytes, 0, message.Length);
                request.SendBytes = bytes;
            }

            Request = request;
        }
        //发送
        this.GoSend(endPoint, message.SendBytes, message.Offset, message.Length);


    }

    /// <inheritdoc/>
    protected override async Task PreviewSendAsync(EndPoint endPoint, byte[] buffer, int offset, int length)
    {
        var send = new SendMessage(buffer, offset, length);
        //发送前打包
        if (IsSendPackCommand)
            PackCommand(send);
        if (Logger.LogLevel <= LogLevel.Trace)
            Logger?.Trace($"{ToString()}- Send:{(IsHexData ? send.SendBytes.ToHexString(send.Offset, send.Length, ' ') : Encoding.UTF8.GetString(send.SendBytes, send.Offset, send.Length))}");
        //发送
        await this.GoSendAsync(endPoint, send.SendBytes, send.Offset, send.Length).ConfigureAwait(false);
    }
    /// <inheritdoc/>
    protected override async Task PreviewSendAsync(EndPoint endPoint, IRequestInfo requestInfo)
    {
        if (!(requestInfo is ISendMessage message))
        {
            throw new Exception($"Unable to convert {nameof(requestInfo)} to {nameof(ISendMessage)}");
        }
        //发送前打包
        if (IsSendPackCommand)
            PackCommand(message);

        if (Logger.LogLevel <= LogLevel.Trace)
            Logger?.Trace($"{ToString()}- Send:{(IsHexData ? message.SendBytes.ToHexString(message.Offset, message.Length, ' ') : Encoding.UTF8.GetString(message.SendBytes, message.Offset, message.Length))}");

        //非并发主从协议
        if (IsSingleThread)
        {
            var request = GetInstance();
            request.Sign = message.Sign;
            if (message.Offset != 0 || message.Length != message.SendBytes.Length)
                request.SendBytes = message.SendBytes;
            else
            {
                byte[] bytes = new byte[message.Length];
                Array.Copy(message.SendBytes, message.Offset, bytes, 0, message.Length);
                request.SendBytes = bytes;
            }
            Request = request;
        }
        //发送
        await this.GoSendAsync(endPoint, message.SendBytes, message.Offset, message.Length).ConfigureAwait(false);


    }

    /// <summary>
    /// 解包获取实际数据包
    /// </summary>
    protected abstract byte[] UnpackResponse(TRequest request);
}
