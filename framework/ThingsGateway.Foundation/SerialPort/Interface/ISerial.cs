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
/// 串口基接口
/// </summary>
public interface ISerial : IDisposable
{
    /// <summary>
    /// 数据交互缓存池限制
    /// </summary>
    int BufferLength { get; }

    /// <summary>
    /// 日志记录器
    /// </summary>
    ILog Logger { get; }

    /// <summary>
    /// 设置数据交互缓存池尺寸，min=1024 byte。
    /// 一般情况下该值用于三个方面，包括：发送、接收缓存，及内存池的默认申请。
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    int SetBufferLength(int value);

}