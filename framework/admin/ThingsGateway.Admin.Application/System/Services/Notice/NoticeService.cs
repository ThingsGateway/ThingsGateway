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

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// <inheritdoc cref="INoticeService"/>
/// </summary>
public class NoticeService : INoticeService
{
    private readonly IServiceScope _serviceScope;

    public NoticeService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScope = serviceScopeFactory.CreateScope();
    }

    /// <inheritdoc/>
    public async Task LogoutAsync(long userId, List<VerificatInfo> verificatInfos, string message)
    {
        //客户端ID列表
        var clientIds = new List<string>();
        //遍历cancellationToken列表获取客户端ID列表
        verificatInfos.ForEach(it =>
        {
            clientIds.AddRange(it.ClientIds);
        });
        //获取signalr实例
        var signalr = _serviceScope.ServiceProvider.GetService<IHubContext<SysHub, ISysHub>>();
        //发送其他客户端登录消息
        await signalr.Clients.Users(clientIds).Logout(message);
    }
}