//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Admin.Application;

public class SignalrNoticeService : ISignalrNoticeService
{
    private IEventService<SignalRMessage>? SignalRMessageDispatchService { get; set; }
    private IEventService<string>? StringDispatchService { get; set; }
    public SignalrNoticeService(IEventService<SignalRMessage> signalRMessageDispatchService,
         IEventService<string> stringDispatchService
        )
    {
        SignalRMessageDispatchService = signalRMessageDispatchService;
        StringDispatchService = stringDispatchService;
    }

    /// <inheritdoc/>
    public async Task NewMesage(IEnumerable<long>? clientIds, SignalRMessage message)
    {
        //发送消息给用户
        if (clientIds != null)
        {
            foreach (var clientId in clientIds)
            {
                await SignalRMessageDispatchService.Publish(clientId.ToString(), message);
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
                await StringDispatchService.Publish(clientId.ToString(), message);
            }
        }
    }
}
