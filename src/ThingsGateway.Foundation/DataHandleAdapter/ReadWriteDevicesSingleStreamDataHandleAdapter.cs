//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Text;

namespace ThingsGateway.Foundation;

/// <summary>
/// TCP/Serial适配器基类
/// </summary>
public abstract class ReadWriteDevicesSingleStreamDataHandleAdapter<TRequest> : CustomDataHandlingAdapter<TRequest> where TRequest : class, IResultMessage, new()
{
    /// <inheritdoc cref="ReadWriteDevicesSingleStreamDataHandleAdapter{TRequest}"/>
    public ReadWriteDevicesSingleStreamDataHandleAdapter()
    {
        CacheTimeoutEnable = true;
    }

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

    /// <summary>
    /// 非并发协议中，每次交互的对象，会在发送时重新获取
    /// </summary>
    public TRequest Request { get; set; }

    /// <inheritdoc/>
    public override string? ToString()
    {
        return Owner?.ToString();
    }

    protected override FilterResult Filter<TByteBlock>(ref TByteBlock byteBlock, bool beCached, ref TRequest request, ref int tempCapacity)
    {
        if (Logger.LogLevel <= LogLevel.Trace)
            Logger?.Trace($"{ToString()}- Receive:{(IsHexData ? byteBlock.AsSegmentTake().ToHexString() : byteBlock.ToString(byteBlock.Position))}");
        {
            //非并发协议,复用对象
            if (IsSingleThread)
                request = Request == null ? GetInstance() : Request;
            else
            {
                if (!beCached)
                    request = GetInstance();
            }

            var pos = byteBlock.Position;

            if (request.HeadBytesLength > byteBlock.CanReadLength)
            {
                return FilterResult.Cache;//当头部都无法解析时，直接缓存
            }
            ArraySegment<byte>? header = null;
            //传入新的ByteBlock对象，避免影响原有的游标
            //当解析消息设定固定头长度大于0时，获取头部字节
            if (request.HeadBytesLength > 0)
            {
                header = byteBlock.AsSegment(byteBlock.Position, request.HeadBytesLength);
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
                    return FilterResult.GoOn;
                }
                if (request.BodyLength + request.HeadBytesLength > byteBlock.CanReadLength)
                {
                    //body不满足解析，开始缓存，然后保存对象
                    tempCapacity = request.BodyLength + request.HeadBytesLength;
                    return FilterResult.Cache;
                }
                if (request.BodyLength <= 0)
                {
                    //如果body长度无法确定，直接读取全部
                    request.BodyLength = byteBlock.Length;
                }
                var result = UnpackResponse(request, byteBlock);
                if (result.FilterResult == FilterResult.Cache)
                {
                    if (Logger.LogLevel <= LogLevel.Trace)
                        Logger.Trace($"{ToString()}-Received incomplete, cached message, current length:{byteBlock.Length}  {request?.ErrorMessage}");
                    tempCapacity = request.BodyLength + request.HeadBytesLength;
                    request.OperCode = -1;
                }
                else if (result.FilterResult == FilterResult.GoOn)
                {
                    byteBlock.Position += 1;
                    request.OperCode = -1;
                }
                else if (result.FilterResult == FilterResult.Success)
                {
                    byteBlock.Position = request.HeadBytesLength + request.BodyLength + pos;
                    if (request.IsSuccess)
                    {
                        request.Content = result.Content;
                        request.ReceivedBytes = byteBlock;
                    }
                }
                return result.FilterResult;
            }
            else
            {
                byteBlock.Position = byteBlock.Length;//移动游标
                request.OperCode = -1;
                return FilterResult.GoOn;//放弃解析
            }
        }
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
    protected override void OnReceivedSuccess(TRequest request)
    {
    }

    protected override async Task PreviewSendAsync(ReadOnlyMemory<byte> memory)
    {
        if (Logger.LogLevel <= LogLevel.Trace)
            Logger?.Trace($"{ToString()}- Send:{(IsHexData ? memory.Span.ToHexString() : (memory.Span.ToString(Encoding.UTF8)))}");

        //发送
        await this.GoSendAsync(memory).ConfigureFalseAwait();
    }

    /// <inheritdoc/>
    protected override async Task PreviewSendAsync(IRequestInfo requestInfo)
    {
        if (!(requestInfo is ISendMessage sendMessage))
        {
            throw new Exception($"Unable to convert {nameof(requestInfo)} to {nameof(SendMessage)}");
        }
        var sendData = sendMessage.SendBytes;

        if (Logger.LogLevel <= LogLevel.Trace)
            Logger?.Trace($"{ToString()}- Send:{(IsHexData ? sendData.Span.ToHexString() : (sendData.Span.ToString(Encoding.UTF8)))}");
        //非并发主从协议
        if (IsSingleThread)
        {
            SetRequest(sendMessage.Sign, sendData);
        }

        //发送
        await this.GoSendAsync(sendData).ConfigureFalseAwait();
    }

    public void SetRequest(int sign, ReadOnlyMemory<byte> sendData)
    {
        var request = GetInstance();
        request.Sign = sign;
        request.SendInfo(sendData);
        Request = request;
    }

    /// <summary>
    /// 解包获取实际数据
    /// </summary>
    protected abstract AdapterResult UnpackResponse(TRequest request, IByteBlock byteBlock);
}
