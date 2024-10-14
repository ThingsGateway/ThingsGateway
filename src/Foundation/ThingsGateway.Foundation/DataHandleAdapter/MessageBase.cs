//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation;

/// <inheritdoc cref="IResultMessage"/>
public class MessageBase : OperResultClass<byte[]>, IResultMessage, IWaitHandle
{
    #region 构造

    /// <inheritdoc />
    public MessageBase() : base()
    {
    }

    /// <inheritdoc />
    public MessageBase(IOperResult operResult) : base(operResult)
    {
    }

    /// <inheritdoc />
    public MessageBase(Exception ex) : base(ex)
    {
    }

    #endregion 构造

    /// <inheritdoc/>
    public int BodyLength { get; set; }

    /// <inheritdoc/>
    public virtual int HeaderLength { get; set; }

    /// <inheritdoc/>
    public virtual int Sign { get; set; } = -1;

    /// <inheritdoc />
    public virtual FilterResult CheckBody<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
    {
        return FilterResult.Success;
    }

    /// <inheritdoc/>
    public virtual bool CheckHead<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
    {
        return true;
    }

    /// <inheritdoc/>
    public virtual void SendInfo(ISendMessage sendMessage)
    {
    }
}
