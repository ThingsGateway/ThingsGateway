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

/// <summary>
/// 单个通道
/// </summary>
public interface IClientChannel : IChannel, ISender, IClient, IClientSender, IOnlineClient, IAdapterObject
{
    #region 属性

    /// <summary>
    /// 收发等待锁，对于大部分工业主从协议是必须的，一个通道一个实现
    /// </summary>
    AsyncAutoResetEvent WaitLock { get; }

    WaitHandlePool<MessageBase> WaitHandlePool { get; }

    #endregion 属性
}
