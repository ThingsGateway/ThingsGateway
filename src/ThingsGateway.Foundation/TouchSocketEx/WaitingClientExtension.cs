#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

namespace ThingsGateway.Foundation;

/// <summary>
/// WaitingClientExtensions
/// </summary>
public static class WaitingClientExtension
{
    /// <summary>
    /// 获取可等待的客户端。
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <param name="client"></param>
    /// <param name="waitingOptions"></param>
    /// <returns></returns>
    public static IWaitingClient<TClient> GetTGWaitingClient<TClient>(this TClient client, WaitingOptions waitingOptions) where TClient : IClient, IDefaultSender, ISender
    {
        waitingOptions.BreakTrigger = true;
        TGWaitingClient<TClient> waitingClient = new TGWaitingClient<TClient>(client, waitingOptions);
        return waitingClient;
    }
}