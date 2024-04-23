//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Text;

namespace ThingsGateway.Foundation;

/// <summary>
/// TCP/Serial适配器基类
/// </summary>
public abstract class ReadWriteDevicesSingleStreamDataHandleAdapter<TRequest> : CustomDataHandlingAdapter<TRequest> where TRequest : class, IMessage
{
    /// <inheritdoc cref="ReadWriteDevicesSingleStreamDataHandleAdapter{TRequest}"/>
    public ReadWriteDevicesSingleStreamDataHandleAdapter()
    {
        Request = GetInstance();
        CacheTimeoutEnable = true;
    }

    /// <summary>
    /// 是否非并发协议
    /// </summary>
    public virtual bool IsSingleThread { get; } = true;

    /// <summary>
    /// 报文输出时采用字符串还是HexString
    /// </summary>
    public virtual bool IsHexData { get; set; } = true;

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
    /// 发送前，对当前的命令进行打包处理
    /// </summary>
    public abstract byte[] PackCommand(byte[] command, TRequest item);

    /// <inheritdoc/>
    public override string? ToString()
    {
        if (Owner is TgSocketClient client)
        {
            return client.GetIPPort();
        }
        else
        {
            return Owner.ToString();
        }
    }

    /// <inheritdoc/>
    protected override FilterResult Filter(in ByteBlock byteBlock, bool beCached, ref TRequest request, ref int tempCapacity)
    {
        //获取全部内容
        var allBytes = byteBlock.ToArray(0, byteBlock.Len);
        if (Logger.LogLevel <= LogLevel.Trace)
            Logger?.Trace($"{ToString()}- Receive:{(IsHexData ? allBytes.ToHexString(' ') : Encoding.UTF8.GetString(allBytes))}");
        //缓存/不缓存解析一样，因为游标已经归0
        {
            if (IsSingleThread)
                request = Request;
            else
            {
                if (!beCached)
                    request = GetInstance();
            }

            if (request.HeadBytesLength > byteBlock.CanReadLen)
            {
                return FilterResult.Cache;//当头部都无法解析时，直接缓存
            }

            var pos = byteBlock.Pos;//记录初始游标位置，防止本次无法解析时，回退游标。

            byte[] header = new byte[] { };
            if (request.HeadBytesLength > 0)
            {
                //当解析消息设定固定头长度大于0时，获取头部字节
                byteBlock.Read(out header, request.HeadBytesLength);
            }
            //检查头部合法性
            if (request.CheckHeadBytes(header))
            {
                if (request.BodyLength > byteBlock.CanReadLen)
                {
                    //body不满足解析，开始缓存，然后保存对象
                    byteBlock.Pos = pos;//回退游标
                    request.ReceivedBytes = header;
                    return FilterResult.Cache;
                }
                if (request.BodyLength <= 0)
                {
                    //如果body长度无法确定，直接读取全部
                    request.BodyLength = byteBlock.Len;
                }

                byteBlock.Read(out byte[] body, request.BodyLength);

                var bytes = DataTransUtil.SpliceArray(request.HeadBytes, body) ?? body;
                if (bytes.Length > 2048)
                {
                    if (Logger.LogLevel <= LogLevel.Trace)
                        Logger?.Trace($"{ToString()}-Note that the receiving size exceeds 2048, length:{bytes.Length}");
                }
                var result = GetResponse(byteBlock, request, body, bytes);
                if (result == FilterResult.Cache)
                {
                    if (Logger.LogLevel <= LogLevel.Trace)
                        Logger.Trace($"{ToString()}-Received incomplete, cached message, current length:{bytes.Length}  {request?.ErrorMessage}");
                    byteBlock.Pos = pos;//回退游标
                }
                return result;
            }
            else
            {
                byteBlock.Pos = byteBlock.Len;//移动游标
                return FilterResult.GoOn;//放弃解析
            }
        }
    }

    /// <summary>
    /// 获取泛型实例。
    /// </summary>
    /// <returns></returns>
    protected abstract TRequest GetInstance();

    /// <summary>
    /// 解包获取实际数据包
    /// <para>当不满足解析条件时，请返回<see cref="FilterResult.Cache"/>，此时会保存<see cref="ByteBlock.CanReadLen"/>的数据</para>
    /// <para>当数据部分异常时，请移动<see cref="ByteBlock.Pos"/>到指定位置，然后返回<see cref="FilterResult.GoOn"/></para>
    /// <para>当完全满足解析条件时，请返回<see cref="FilterResult.Success"/>最后将<see cref="ByteBlock.Pos"/>移至指定位置。</para>
    /// </summary>
    protected virtual FilterResult GetResponse(ByteBlock byteBlock, TRequest request, byte[] body, byte[] bytes)
    {
        var unpackbytes = UnpackResponse(request, request.SendBytes, body, bytes);
        request.ReceivedBytes = bytes;
        switch (unpackbytes)
        {
            case FilterResult.Cache:
                return FilterResult.Cache;

            case FilterResult.Success:
                return FilterResult.Success;

            case FilterResult.GoOn:
                byteBlock.Pos = byteBlock.Len;
                return FilterResult.GoOn;

            default:
                byteBlock.Pos = byteBlock.Len;
                return FilterResult.GoOn;
        }
    }

    /// <summary>
    /// 发送方法,会重新建立<see cref="Request"/>
    /// </summary>
    protected virtual void GoSend(byte[] item, TRequest request)
    {
        byte[] bytes;
        if (IsSendPackCommand)
            bytes = PackCommand(item, request);
        else
            bytes = item;
        Request = request;
        Request.SendBytes = bytes;
        GoSend(bytes, 0, bytes.Length);
        if (Logger.LogLevel <= LogLevel.Trace)
            Logger?.Trace($"{ToString()}- Send:{(IsHexData ? bytes.ToHexString(' ') : Encoding.UTF8.GetString(bytes))}");
    }

    /// <summary>
    /// 发送方法,会重新建立<see cref="Request"/>
    /// </summary>
    protected virtual async Task GoSendAsync(byte[] item, TRequest request)
    {
        byte[] bytes;
        if (IsSendPackCommand)
            bytes = PackCommand(item, request);
        else
            bytes = item;
        Request = request;
        Request.SendBytes = bytes;
        await GoSendAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
        if (Logger.LogLevel <= LogLevel.Trace)
            Logger?.Trace($"{ToString()}- Send:{(IsHexData ? bytes.ToHexString(' ') : Encoding.UTF8.GetString(bytes))}");
    }

    /// <inheritdoc/>
    protected override void PreviewSend(IRequestInfo requestInfo)
    {
        if (requestInfo == null)
        {
            throw new ArgumentNullException(nameof(requestInfo));
        }
        var message = (ISendMessage)requestInfo;
        var request = GetInstance();
        request.Sign = message.Sign;
        GoSend(message.SendBytes, request);
    }

    /// <inheritdoc/>
    protected override Task PreviewSendAsync(IRequestInfo requestInfo)
    {
        if (requestInfo == null)
        {
            throw new ArgumentNullException(nameof(requestInfo));
        }
        var message = (ISendMessage)requestInfo;
        var request = GetInstance();
        request.Sign = message.Sign;
        return GoSendAsync(message.SendBytes, request);
    }

    /// <inheritdoc/>
    protected override void PreviewSend(byte[] buffer, int offset, int length)
    {
        GoSend(buffer, GetInstance());
    }

    /// <inheritdoc/>
    protected override Task PreviewSendAsync(byte[] buffer, int offset, int length)
    {
        return GoSendAsync(buffer, GetInstance());
    }

    /// <summary>
    /// 报文拆包
    /// </summary>
    protected abstract FilterResult UnpackResponse(TRequest request, byte[]? send, byte[] body, byte[] response);
}
