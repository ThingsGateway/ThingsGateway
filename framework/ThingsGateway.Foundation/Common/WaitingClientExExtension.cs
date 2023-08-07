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

namespace ThingsGateway.Foundation;

/// <summary>
/// WaitingClientExtensions
/// </summary>
public static class WaitingClientExExtension
{
    /// <summary>
    /// 获取筛选条件的可等待的客户端。
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <param name="client"></param>
    /// <param name="waitingOptions"></param>
    /// <param name="func">当条件成立时返回</param>
    /// <returns></returns>
    public static IWaitingClient<TClient> GetWaitingClientEx<TClient>(this TClient client, WaitingOptions waitingOptions, Func<ResponsedData, bool> func) where TClient : IClient, IDefaultSender, ISender
    {
        return new WaitingClientEx<TClient>(client, waitingOptions, func);
    }

    /// <summary>
    /// 获取可等待的客户端。
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <param name="client"></param>
    /// <param name="waitingOptions"></param>
    /// <returns></returns>
    public static IWaitingClient<TClient> GetWaitingClientEx<TClient>(this TClient client, WaitingOptions waitingOptions) where TClient : IClient, IDefaultSender, ISender
    {
        var waitingClient = new WaitingClientEx<TClient>(client, waitingOptions);
        return waitingClient;
    }
}