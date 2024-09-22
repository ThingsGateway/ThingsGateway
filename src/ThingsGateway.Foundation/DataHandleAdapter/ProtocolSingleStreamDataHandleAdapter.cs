//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Text;

namespace ThingsGateway.Foundation;

/// <summary>
/// TCP/Serial适配器基类
/// </summary>
public class ProtocolSingleStreamDataHandleAdapter<TRequest> : CustomDataHandlingAdapter<TRequest> where TRequest : MessageBase, new()
{
    /// <inheritdoc cref="ProtocolSingleStreamDataHandleAdapter{TRequest}"/>
    public ProtocolSingleStreamDataHandleAdapter()
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

    /// <inheritdoc />
    public void SetRequest(int sign, ISendMessage sendMessage)
    {
        var request = GetInstance();
        request.Sign = sign;
        request.SendInfo(sendMessage);
        Request = request;
    }

    /// <inheritdoc/>
    public override string? ToString()
    {
        return Owner?.ToString();
    }

    /// <inheritdoc />
    protected override FilterResult Filter<TByteBlock>(ref TByteBlock byteBlock, bool beCached, ref TRequest request, ref int tempCapacity)
    {
        if (Logger?.LogLevel <= LogLevel.Trace)
            Logger?.Trace($"{ToString()}- Receive:{(IsHexData ? byteBlock.AsSegmentTake().ToHexString() : byteBlock.ToString(byteBlock.Position))}");

        try
        {
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

                if (request.HeaderLength > byteBlock.CanReadLength)
                {
                    return FilterResult.Cache;//当头部都无法解析时，直接缓存
                }

                //检查头部合法性
                if (request.CheckHead(ref byteBlock))
                {
                    byteBlock.Position = pos;
                    if (request.BodyLength > MaxPackageSize)
                    {
                        OnError(default, $"Received BodyLength={request.BodyLength}, greater than the set MaxPackageSize={MaxPackageSize}", true, true);
                        return FilterResult.GoOn;
                    }
                    if (request.BodyLength + request.HeaderLength > byteBlock.CanReadLength)
                    {
                        //body不满足解析，开始缓存，然后保存对象
                        tempCapacity = request.BodyLength + request.HeaderLength;
                        return FilterResult.Cache;
                    }
                    //if (request.BodyLength <= 0)
                    //{
                    //    //如果body长度无法确定，直接读取全部
                    //    request.BodyLength = byteBlock.Length;
                    //}
                    var headPos = pos + request.HeaderLength;
                    byteBlock.Position = headPos;
                    var result = request.CheckBody(ref byteBlock);
                    if (result == FilterResult.Cache)
                    {
                        if (Logger?.LogLevel <= LogLevel.Trace)
                            Logger?.Trace($"{ToString()}-Received incomplete, cached message, current length:{byteBlock.Length}  {request?.ErrorMessage}");
                        tempCapacity = request.BodyLength + request.HeaderLength;
                        request.OperCode = -1;
                    }
                    else if (result == FilterResult.GoOn)
                    {
                        byteBlock.Position = pos + 1;
                        request.OperCode = -1;
                    }
                    else if (result == FilterResult.Success)
                    {
                        byteBlock.Position = request.HeaderLength + request.BodyLength + pos;
                    }
                    return result;
                }
                else
                {
                    byteBlock.Position = pos + 1;//移动游标
                    request.OperCode = -1;
                    return FilterResult.GoOn;//放弃解析
                }
            }
        }
        catch (Exception ex)
        {
            Logger?.LogWarning(ex, $"{ToString()} Received parsing error");
            byteBlock.Position = byteBlock.Length;//移动游标
            return FilterResult.GoOn;//放弃解析
        }
    }

    /// <summary>
    /// 获取泛型实例。
    /// </summary>
    /// <returns></returns>
    protected virtual TRequest GetInstance()
    {
        return new TRequest() { OperCode = -1 };
    }

    /// <inheritdoc/>
    protected override void OnReceivedSuccess(TRequest request)
    {
        Request = null;
    }

    /// <inheritdoc />
    protected override async Task PreviewSendAsync(ReadOnlyMemory<byte> memory)
    {
        if (Logger?.LogLevel <= LogLevel.Trace)
            Logger?.Trace($"{ToString()}- Send:{(IsHexData ? memory.Span.ToHexString() : (memory.Span.ToString(Encoding.UTF8)))}");

        //发送
        await GoSendAsync(memory).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task PreviewSendAsync(IRequestInfo requestInfo)
    {
        if (!(requestInfo is ISendMessage sendMessage))
        {
            throw new Exception($"Unable to convert {nameof(requestInfo)} to {nameof(ISendMessage)}");
        }

        var requestInfoBuilder = (ISendMessage)requestInfo;

        var byteBlock = new ValueByteBlock(requestInfoBuilder.MaxLength);
        try
        {
            requestInfoBuilder.Build(ref byteBlock);
            if (Logger?.LogLevel <= LogLevel.Trace)
                Logger?.Trace($"{ToString()}- Send:{(IsHexData ? byteBlock.Span.ToHexString() : (byteBlock.Span.ToString(Encoding.UTF8)))}");
            //非并发主从协议
            if (IsSingleThread)
            {
                SetRequest(sendMessage.Sign, requestInfoBuilder);
            }
            await GoSendAsync(byteBlock.Memory).ConfigureAwait(false);
        }
        finally
        {
            byteBlock.SafeDispose();
        }
    }
}
