//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation;

/// <inheritdoc cref="IResultMessage"/>
public class MessageBase : OperResultClass<byte[]>, IResultMessage, IWaitHandle
{
    public MessageBase()
    {
    }

    public MessageBase(IOperResult operResult) : base(operResult)
    {
    }

    public MessageBase(string msg) : base(msg)
    {
    }

    public MessageBase(Exception ex) : base(ex)
    {
    }

    public MessageBase(string msg, Exception ex) : base(msg, ex)
    {
    }

    /// <inheritdoc/>
    public int BodyLength { get; set; }

    /// <inheritdoc/>
    public virtual int HeadBytesLength { get; }

    /// <inheritdoc/>
    public virtual int Sign { get; set; }

    /// <inheritdoc/>
    public IByteBlock ReceivedBytes { get; set; }

    /// <inheritdoc/>
    public virtual bool CheckHeadBytes(byte[]? headBytes)
    {
        return true;
    }

    /// <inheritdoc/>
    public virtual void SendInfo(ReadOnlyMemory<byte> sendBytes)
    {
    }
}
