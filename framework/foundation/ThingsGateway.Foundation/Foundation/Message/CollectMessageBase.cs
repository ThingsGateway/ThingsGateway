#region copyright

//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

#endregion

namespace ThingsGateway.Foundation.Core;

/// <inheritdoc cref="IMessage"/>
public abstract class MessageBase : OperResult<byte[]>, IMessage
{
    private byte[] sendBytes = new byte[] { };

    /// <inheritdoc/>
    public int BodyLength { get; set; }

    /// <inheritdoc/>
    public byte[] HeadBytes { get; set; }

    /// <inheritdoc/>
    public virtual int HeadBytesLength { get; }

    /// <inheritdoc/>
    public byte[] ReceivedBytes { get; set; }

    /// <inheritdoc/>
    public byte[] SendBytes
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