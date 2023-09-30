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

namespace ThingsGateway.Foundation.Serial;

/// <summary>
/// 通讯基类
/// </summary>
public abstract class BaseSerial : DependencyObject, ISerial
{
    /// <summary>
    /// 同步根。
    /// </summary>
    protected readonly object SyncRoot = new object();
    private int m_receiveBufferSize = 1024 * 64;
    private int m_sendBufferSize = 1024 * 64;

    /// <inheritdoc/>
    public virtual int SendBufferSize
    {
        get => m_sendBufferSize;
        set => m_sendBufferSize = value < 1024 ? 1024 : value;
    }
    /// <inheritdoc/>
    public virtual int ReceiveBufferSize
    {
        get => m_receiveBufferSize;
        set => m_receiveBufferSize = value < 1024 ? 1024 : value;
    }
    /// <inheritdoc/>
    public ILog Logger { get; set; }
}