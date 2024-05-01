﻿//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------


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
    /// 发送前，对当前的命令进行打包处理
    /// </summary>
    public abstract void PackCommand(ISendMessage item);

    /// <inheritdoc/>
    public override string? ToString()
    {
        return Owner.ToString();
    }
    /// <inheritdoc/>
    protected override FilterResult Filter(in ByteBlock byteBlock, bool beCached, ref TRequest request, ref int tempCapacity)
    {
        //整个流程都不会改变流的游标位置，所以对于是否缓存的情况都是一样的处理

        if (Logger.LogLevel <= LogLevel.Trace)
            Logger?.Trace($"{ToString()}- Receive:{(IsHexData ? byteBlock.Buffer.ToHexString(byteBlock.Pos, byteBlock.Len, ' ') : byteBlock.ToString())}");
        {
            //非并发协议,复用对象
            if (IsSingleThread)
                request = Request == null ? GetInstance() : Request.DisposedValue ? GetInstance() : Request;
            else
            {
                //并发协议非缓存模式下，重新获取对象
                if (!beCached)
                    request = GetInstance();
            }

            if (request.HeadBytesLength > byteBlock.CanReadLen)
            {
                return FilterResult.Cache;//当头部都无法解析时，直接缓存
            }

            //传入新的ByteBlock对象，避免影响原有的游标
            //当解析消息设定固定头长度大于0时，获取头部字节
            if (request.HeadBytesLength > 0)
            {
                using var header = new ByteBlock(request.HeadBytesLength);
                header.Write(byteBlock.Buffer, byteBlock.Pos, request.HeadBytesLength);
                header.SeekToStart();
                return Check(byteBlock, request, ref tempCapacity, header);
            }
            else
            {
                return Check(byteBlock, request, ref tempCapacity, null);
            }

        }

        FilterResult Check(ByteBlock byteBlock, TRequest request, ref int tempCapacity, ByteBlock? header)
        {
            //检查头部合法性
            if (request.CheckHeadBytes(header))
            {
                if (request.BodyLength > this.MaxPackageSize)
                {
                    this.OnError(default, $"Received BodyLength={request.BodyLength}, greater than the set MaxPackageSize={this.MaxPackageSize}", true, true);
                    return FilterResult.GoOn;
                }

                if (request.BodyLength > byteBlock.CanReadLen)
                {
                    //body不满足解析，开始缓存，然后保存对象
                    tempCapacity = request.BodyLength + request.HeadBytesLength;
                    return FilterResult.Cache;
                }
                if (request.BodyLength <= 0)
                {
                    //如果body长度无法确定，直接读取全部
                    request.BodyLength = byteBlock.Len;
                }

                //传入新的ByteBlock对象，避免影响原有的游标
                using var block = new ByteBlock(byteBlock.Len);
                block.Write(byteBlock.Buffer, byteBlock.Pos, request.BodyLength + request.HeadBytesLength);
                block.SeekToStart();
                request.ReceivedByteBlock = byteBlock;

                using var result = UnpackResponse(block);
                if (result.FilterResult == FilterResult.Cache)
                {
                    if (Logger.LogLevel <= LogLevel.Trace)
                        Logger.Trace($"{ToString()}-Received incomplete, cached message, current length:{byteBlock.Len}  {request?.ErrorMessage}");
                    tempCapacity = request.BodyLength + request.HeadBytesLength;
                    request.OperCode = -1;
                }
                else if (result.FilterResult == FilterResult.GoOn)
                {
                    byteBlock.Pos += 1;
                    request.OperCode = -1;
                }
                else if (result.FilterResult == FilterResult.Success)
                {
                    byteBlock.Pos += request.BodyLength;
                    request.OperCode = null;
                    request.Content = result.ByteBlock;
                }
                return result.FilterResult;
            }
            else
            {
                byteBlock.Pos = byteBlock.Len;//移动游标
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
        request.SafeDispose();
    }

    /// <inheritdoc/>
    protected override void PreviewSend(IRequestInfo requestInfo)
    {
        if (!(requestInfo is ISendMessage message))
        {
            throw new Exception($"Unable to convert {nameof(requestInfo)} to {nameof(ISendMessage)}");
        }
        //发送前打包
        if (IsSendPackCommand)
            PackCommand(message);

        //发送
        this.GoSend(message.SendByteBlock.Buffer, 0, message.SendByteBlock.Len);

        //非并发主从协议
        if (IsSingleThread)
        {
            var request = GetInstance();
            request.Sign = message.Sign;
            request.SendByteBlock = message.SendByteBlock;
            Request = request;
        }
        else
        {
            //并发协议，直接释放内存池
            message.SendByteBlock.SafeDispose();
        }
        if (Logger.LogLevel <= LogLevel.Trace)
            Logger?.Trace($"{ToString()}- Send:{(IsHexData ? message.SendByteBlock.Buffer.ToHexString(message.SendByteBlock.Pos, message.SendByteBlock.Len, ' ') : message.SendByteBlock.ToString())}");
    }

    /// <inheritdoc/>
    protected override void PreviewSend(byte[] buffer, int offset, int length)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc/>
    protected override async Task PreviewSendAsync(IRequestInfo requestInfo)
    {
        if (!(requestInfo is ISendMessage message))
        {
            throw new Exception($"Unable to convert {nameof(requestInfo)} to {nameof(ISendMessage)}");
        }
        //发送前打包
        if (IsSendPackCommand)
            PackCommand(message);

        //发送
        await this.GoSendAsync(message.SendByteBlock.Buffer, 0, message.SendByteBlock.Len).ConfigureAwait(false);

        //非并发主从协议
        if (IsSingleThread)
        {
            var request = GetInstance();
            request.Sign = message.Sign;
            request.SendByteBlock = message.SendByteBlock;
            Request = request;
        }
        else
        {
            //并发协议，直接释放内存池
            message.SendByteBlock.SafeDispose();
        }
        if (Logger.LogLevel <= LogLevel.Trace)
            Logger?.Trace($"{ToString()}- Send:{(IsHexData ? message.SendByteBlock.Buffer.ToHexString(message.SendByteBlock.Pos, message.SendByteBlock.Len, ' ') : message.SendByteBlock.ToString())}");
    }
    /// <inheritdoc/>
    protected override Task PreviewSendAsync(byte[] buffer, int offset, int length)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// 解包获取实际数据包
    /// </summary>
    protected abstract AdapterResult UnpackResponse(ByteBlock bodyBlock);
}


public class AdapterResult : DisposableObject
{
    public FilterResult FilterResult { get; }
    public ByteBlock ByteBlock { get; }

    protected override void Dispose(bool disposing)
    {
        ByteBlock.SafeDispose();
        base.Dispose(disposing);
    }

}
