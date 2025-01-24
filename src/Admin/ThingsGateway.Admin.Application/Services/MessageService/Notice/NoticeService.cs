//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Admin.Application;
internal sealed class NoticeService : INoticeService
{
    private IEventService<AppMessage>? MessageDispatchService { get; set; }
    private IEventService<UserLoginOut>? UserLoginOutDispatchService { get; set; }
    private IEventService<NavigationUri>? NavigationMesageDispatchService { get; set; }
    public NoticeService(IEventService<AppMessage> signalRMessageDispatchService,
         IEventService<UserLoginOut> userLoginOutDispatchService,
         IEventService<NavigationUri> navigationMesageDispatchService
        )
    {
        MessageDispatchService = signalRMessageDispatchService;
        UserLoginOutDispatchService = userLoginOutDispatchService;
        NavigationMesageDispatchService = navigationMesageDispatchService;
    }


    /// <inheritdoc/>
    public async Task NewMesage(IEnumerable<long>? clientIds, AppMessage message)
    {
        //发送消息给用户
        if (clientIds != null)
        {
            foreach (var clientId in clientIds)
            {
                await MessageDispatchService.Publish(clientId.ToString(), message).ConfigureAwait(false);
            }
        }
    }

    /// <inheritdoc/>
    public async Task UserLoginOut(IEnumerable<long>? clientIds, string message)
    {
        //发送消息给用户
        if (clientIds != null)
        {
            foreach (var clientId in clientIds)
            {
                await UserLoginOutDispatchService.Publish(clientId.ToString(), new(message)).ConfigureAwait(false);
            }
        }
    }



    /// <inheritdoc/>
    public async Task NavigationMesage(IEnumerable<long>? clientIds, string uri, string message)
    {
        //发送消息给用户
        if (clientIds != null)
        {
            foreach (var clientId in clientIds)
            {
                await NavigationMesageDispatchService.Publish(clientId.ToString(), new(uri, message)).ConfigureAwait(false);
            }
        }
    }
}
