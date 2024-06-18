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

    /// <summary>
    /// 是否非并发协议
    /// </summary>
    public virtual bool IsSingleThread { get; } = true;

    /// <inheritdoc/>
    public virtual bool IsSendPackCommand { get; set; } = false;

    /// <summary>
    /// 在解析时发生错误。
    /// </summary>
    /// <param name="ex">异常</param>
    /// <param name="error">错误异常</param>
    /// <param name="reset">是否调用Reset/></param>
    /// <param name="log">是否记录日志</param>
    protected override void OnError(Exception ex, string error, bool reset, bool log)
    {
        if (reset)
        {
            this.Reset();
        }
        if (log)
        {
            this.Logger?.LogError(ex, error);
        }
    }

    /// <summary>
    /// 发送前，对当前的命令进行打包处理
    /// </summary>
    public virtual byte[] PackCommand(ISendMessage item)
    {
        return item.SendBytes;
    }

    /// <summary>
    /// 非并发协议中，每次交互的对象，会在发送时重新获取
    /// </summary>
    public TRequest Request { get; set; }

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
    protected override async Task PreviewSendAsync(EndPoint endPoint, ReadOnlyMemory<byte> memory)
    {
        if (Logger.LogLevel <= LogLevel.Trace)
            Logger?.Trace($"{ToString()}- Send:{(IsHexData ? memory.Span.ToHexString() : (memory.Span.ToString(Encoding.UTF8)))}");

        //发送
        await this.GoSendAsync(endPoint, memory).ConfigureFalseAwait();
    }

    /// <inheritdoc/>
    protected override async Task PreviewReceived(EndPoint remoteEndPoint, ByteBlock byteBlock)
    {
        if (Logger.LogLevel <= LogLevel.Trace)
            Logger?.Trace($"{ToString()}- Receive:{(IsHexData ? byteBlock.AsSegmentTake().ToHexString() : byteBlock.ToString(byteBlock.Position))}");

        TRequest request = null;
        if (IsSingleThread)
            request = Request == null ? GetInstance() : Request;
        else
        {
            request = GetInstance();
        }
        ArraySegment<byte>? header = null;
        //传入新的ByteBlock对象，避免影响原有的游标
        //当解析消息设定固定头长度大于0时，获取头部字节
        if (request.HeadBytesLength > 0)
        {
            header = byteBlock.AsSegment(0, request.HeadBytesLength);
        }
        else
        {
        }
        //检查头部合法性
        if (request.CheckHeadBytes(header?.Array))
        {
            if (request.BodyLength > this.MaxPackageSize)
            {
                this.OnError(default, $"Received BodyLength={request.BodyLength}, greater than the set MaxPackageSize={this.MaxPackageSize}", true, true);
                await GoReceived(remoteEndPoint, null, request);
            }

            if (request.BodyLength + request.HeadBytesLength > byteBlock.CanReadLength)
            {
                await GoReceived(remoteEndPoint, null, request);
                return;
            }
            if (request.BodyLength <= 0)
            {
                //如果body长度无法确定，直接读取全部
                request.BodyLength = byteBlock.Length;
            }

            var result = UnpackResponse(request, byteBlock);

            byteBlock.Position = byteBlock.Length;
            if (request.IsSuccess)
            {
                request.Content = result.Content;
                request.ReceivedBytes = byteBlock;
            }
            await GoReceived(remoteEndPoint, null, request);
            return;
        }
        else
        {
            byteBlock.Position = byteBlock.Length;//移动游标
            request.OperCode = -1;
            await GoReceived(remoteEndPoint, null, request);
            return;
        }
    }

    /// <inheritdoc/>
    protected override async Task PreviewSendAsync(EndPoint endPoint, IRequestInfo requestInfo)
    {
        if (!(requestInfo is ISendMessage sendMessage))
        {
            throw new Exception($"Unable to convert {nameof(requestInfo)} to {nameof(ISendMessage)}");
        }
        var sendData = sendMessage.SendBytes;

        if (IsSendPackCommand)
            sendData = PackCommand(sendMessage);

        if (Logger.LogLevel <= LogLevel.Trace)
            Logger?.Trace($"{ToString()}- Send:{(IsHexData ? sendData.ToHexString(' ') : Encoding.UTF8.GetString(sendData))}");
        //非并发主从协议
        if (IsSingleThread)
        {
            var request = GetInstance();
            request.Sign = sendMessage.Sign;
            request.SendInfo(sendData);
            Request = request;
        }

        //发送
        await this.GoSendAsync(endPoint, sendData).ConfigureFalseAwait();
    }

    /// <summary>
    /// 解包获取实际数据包
    /// </summary>
    protected abstract AdapterResult UnpackResponse(TRequest request, IByteBlock byteBlock);
}
