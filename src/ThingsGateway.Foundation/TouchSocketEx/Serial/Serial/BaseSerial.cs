#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
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
    /// 通讯基类
    /// </summary>
    public BaseSerial()
    {
        SyncRoot = new object();
    }

    private int m_bufferLength;

    /// <summary>
    /// 数据交互缓存池限制，min=1024 byte
    /// </summary>
    public virtual int BufferLength
    {
        get => m_bufferLength;
        set
        {
            if (value < 1024)
            {
                value = 1024 * 10;
            }
            m_bufferLength = value;
        }
    }

    /// <summary>
    /// 同步根。
    /// </summary>
    protected object SyncRoot;

    /// <summary>
    /// 日志记录器
    /// </summary>
    public ILog Logger { get; set; }
}