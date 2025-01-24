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
public class DeviceSingleStreamDataHandleAdapter<TRequest> : CustomDataHandlingAdapter<TRequest> where TRequest : MessageBase, new()
{
    /// <inheritdoc cref="DeviceSingleStreamDataHandleAdapter{TRequest}"/>
    public DeviceSingleStreamDataHandleAdapter()
    {
        CacheTimeoutEnable = true;
        SurLength = int.MaxValue;
    }

    /// <inheritdoc/>
    public override bool CanSendRequestInfo => true;

    /// <inheritdoc/>
    public override bool CanSplicingSend => false;

    /// <summary>
    /// 报文输出时采用字符串还是HexString
    /// </summary>
    public virtual bool IsHexLog { get; set; } = true;

    public virtual bool IsSingleThread { get; set; } = true;

    /// <summary>
    /// 非并发协议中，每次交互的对象，会在发送时重新获取
    /// </summary>
    public TRequest Request { get; set; }

    /// <inheritdoc />
    public void SetRequest(ISendMessage sendMessage, ref ValueByteBlock byteBlock)
    {
        var request = GetInstance();
        request.Sign = sendMessage.Sign;
        request.SendInfo(sendMessage, ref byteBlock);
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
            Logger?.Trace($"{ToString()}- Receive:{(IsHexLog ? byteBlock.AsSegmentTake().ToHexString() : byteBlock.ToString(byteBlock.Position))}");

        try
        {
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
                    request.OperCode = -1;
                    request.ErrorMessage = $"Received BodyLength={request.BodyLength}, greater than the set MaxPackageSize={MaxPackageSize}";
                    OnError(default, request.ErrorMessage, true, true);
                    SetResult(request);
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
                    byteBlock.Position = pos + request.BodyLength + request.HeaderLength;
                    Logger?.Trace($"{ToString()}-{request?.ToString()}");
                    request.OperCode = -1;
                    SetResult(request);
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
                SetResult(request);
                return FilterResult.GoOn;//放弃解析
            }
        }
        catch (Exception ex)
        {
            Logger?.LogWarning(ex, $"{ToString()} Received parsing error");
            byteBlock.Position = byteBlock.Length;//移动游标
            request.Exception = ex;
            request.OperCode = -1;
            SetResult(request);
            return FilterResult.GoOn;//放弃解析
        }
    }

    private void SetResult(TRequest request)
    {
        if ((Owner as IClientChannel)?.WaitHandlePool?.TryGetDataAsync(request.Sign, out var waitDataAsync) == true)
        {
            waitDataAsync.SetResult(request);
        }
    }

    /// <summary>
    /// 获取泛型实例。
    /// </summary>
    /// <returns></returns>
    protected virtual TRequest GetInstance()
    {
        return new TRequest() { OperCode = -1, Sign = -1 };
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
            Logger?.Trace($"{ToString()}- Send:{(IsHexLog ? memory.Span.ToHexString() : (memory.Span.ToString(Encoding.UTF8)))}");

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

        var byteBlock = new ValueByteBlock(sendMessage.MaxLength);
        try
        {
            sendMessage.Build(ref byteBlock);
            if (Logger?.LogLevel <= LogLevel.Trace)
                Logger?.Trace($"{ToString()}- Send:{(IsHexLog ? byteBlock.Span.ToHexString() : (byteBlock.Span.ToString(Encoding.UTF8)))}");
            //非并发主从协议
            if (IsSingleThread)
            {
                SetRequest(sendMessage, ref byteBlock);
            }
            await GoSendAsync(byteBlock.Memory).ConfigureAwait(false);
        }
        finally
        {
            byteBlock.SafeDispose();
        }
    }
}
