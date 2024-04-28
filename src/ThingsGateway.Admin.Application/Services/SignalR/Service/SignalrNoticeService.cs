
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------




using Microsoft.AspNetCore.SignalR;

namespace ThingsGateway.Admin.Application;

public class SignalrNoticeService : ISignalrNoticeService
{
    private IHubContext<SysHub, ISysHub> HubContext;

    public SignalrNoticeService(IHubContext<SysHub, ISysHub> hubContext)
    {
        HubContext = hubContext;
    }

    /// <inheritdoc/>
    public async Task NewMesage(IEnumerable<long>? clientIds, SignalRMessage message)
    {
        //发送消息给用户
        if (clientIds != null)
            await HubContext.Clients.Users(clientIds.Select(a => a.ToString())).NewMessage(message);
    }

    /// <inheritdoc/>
    public async Task UserLoginOut(IEnumerable<long>? clientIds, string message)
    {
        //发送消息给用户
        if (clientIds != null)
            await HubContext.Clients.Users(clientIds.Select(a => a.ToString())).LoginOut(message);
    }
}