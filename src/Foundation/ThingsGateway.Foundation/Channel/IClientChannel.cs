//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.NewLife;

namespace ThingsGateway.Foundation;

/// <summary>
/// 终端通道
/// </summary>
public interface IClientChannel : IChannel, ISender, IClient, IClientSender, IOnlineClient
{
    /// <summary>
    /// 当前通道的数据处理适配器
    /// </summary>
    DataHandlingAdapter ReadOnlyDataHandlingAdapter { get; }

    /// <summary>
    /// 通道等待池
    /// </summary>
    WaitHandlePool<MessageBase> WaitHandlePool { get; }


    /// <summary>
    /// 通讯并发限制
    /// </summary>
    WaitLock WaitLock { get; }

    /// <summary>
    /// 设置数据处理适配器
    /// </summary>
    /// <param name="adapter">适配器</param>
    void SetDataHandlingAdapter(DataHandlingAdapter adapter);
}
