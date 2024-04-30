
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------




using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ThingsGateway.Foundation;

/// <inheritdoc cref="IMessage"/>
public abstract class MessageBase : IOperResult<byte[]>, IMessage, IWaitHandle
{
    #region Result
    /// <summary>
    /// 异常堆栈
    /// </summary>
#if NET6_0_OR_GREATER
    [System.Text.Json.Serialization.JsonIgnore]
#endif

    [JsonIgnore]
    public Exception? Exception { get; set; }

    /// <summary>
    /// 默认构造，操作结果会是成功
    /// </summary>
    public MessageBase()
    {
    }

    /// <summary>
    /// 从另一个操作对象中赋值信息
    /// </summary>
    public MessageBase(IOperResult operResult)
    {
        OperCode = operResult.OperCode;
        ErrorMessage = operResult.ErrorMessage;
        Exception = operResult.Exception;
        ErrorCode = operResult.ErrorCode;
    }

    /// <summary>
    /// 传入错误信息
    /// </summary>
    /// <param name="msg"></param>
    public MessageBase(string msg)
    {
        OperCode = 500;
        ErrorMessage = msg;
    }

    /// <summary>
    /// 传入异常堆栈
    /// </summary>
    public MessageBase(Exception ex)
    {
        OperCode = 500;
        Exception = ex;
        ErrorMessage = ex.Message;
        //指定Timeout或OperationCanceled为超时取消
        if (ex is TimeoutException || ex is OperationCanceledException)
        {
            ErrorCode = ErrorCodeEnum.Canceled;
        }
        else if (ex is ReturnErrorException)
        {
            ErrorCode = ErrorCodeEnum.RetuenError;
        }
        else
        {
            ErrorCode = ErrorCodeEnum.InvokeFail;
        }
    }

    /// <summary>
    /// 传入错误信息与异常堆栈
    /// </summary>
    public MessageBase(string msg, Exception ex) : this(ex)
    {
        ErrorMessage = msg;
    }

    /// <inheritdoc/>
    public int? OperCode { get; set; }

    /// <inheritdoc/>
    public bool IsSuccess => OperCode == null || OperCode == 0;

    /// <inheritdoc/>
    public string? ErrorMessage { get; set; }

    /// <inheritdoc/>
#if NET6_0_OR_GREATER
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
#endif

    [JsonConverter(typeof(StringEnumConverter))]
    public ErrorCodeEnum? ErrorCode { get; private set; } = ErrorCodeEnum.RetuenError;

    /// <inheritdoc/>
    public byte[] Content { get; set; }

    #endregion

    /// <inheritdoc/>
    public virtual long Sign { get; set; }

    private byte[]? sendBytes;

    /// <inheritdoc/>
    public int BodyLength { get; set; }

    /// <inheritdoc/>
    public byte[] HeadBytes { get; set; }

    /// <inheritdoc/>
    public virtual int HeadBytesLength { get; }

    /// <inheritdoc/>
    public byte[] ReceivedBytes { get; set; }

    /// <inheritdoc/>
    public byte[]? SendBytes
    {
        get
        {
            return sendBytes;
        }
        set
        {
            sendBytes = value;
            SendBytesThen();
        }
    }

    /// <inheritdoc/>
    public abstract bool CheckHeadBytes(byte[] heads);

    /// <summary>
    /// 写入<see cref="SendBytes"/>后触发此方法
    /// </summary>
    protected virtual void SendBytesThen()
    {
    }
}

/// <inheritdoc/>
public class SendMessage : ISendMessage
{
    /// <inheritdoc/>
    public SendMessage(byte[] sendBytes)
    {
        SendBytes = sendBytes;
    }

    /// <inheritdoc/>
    public virtual long Sign { get; set; }

    /// <inheritdoc/>
    public byte[] SendBytes { get; set; }
}
