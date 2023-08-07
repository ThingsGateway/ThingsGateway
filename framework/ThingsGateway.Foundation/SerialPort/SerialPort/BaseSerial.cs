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
    protected readonly object SyncRoot;

    /// <summary>
    /// 通讯基类
    /// </summary>
    public BaseSerial()
    {
        this.SyncRoot = new object();
    }

    /// <summary>
    /// 数据交互缓存池限制，min=1024 byte
    /// </summary>
    public int BufferLength { get; private set; } = 64 * 1024;

    /// <summary>
    /// 日志记录器
    /// </summary>
    public ILog Logger { get; set; }

    /// <summary>
    /// 设置数据交互缓存池尺寸，min=1024 byte。
    /// 一般情况下该值用于三个方面，包括：socket的发送、接收缓存，及内存池的默认申请。
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public virtual int SetBufferLength(int value)
    {
        if (value < 1024)
        {
            value = 1024;
        }
        this.BufferLength = value;
        return this.BufferLength;
    }
}