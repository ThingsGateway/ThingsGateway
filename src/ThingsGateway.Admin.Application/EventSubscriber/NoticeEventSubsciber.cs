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

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 通知事件总线
/// </summary>
public class NoticeEventSubsciber : IEventSubscriber, ISingleton
{
    private readonly ISimpleCacheService _simpleCacheService;

    public IServiceProvider _service { get; }
    private readonly SqlSugarScope _db;

    public NoticeEventSubsciber(ISimpleCacheService simpleCacheService, IServiceProvider service)
    {
        _db = DbContext.Db;
        _simpleCacheService = simpleCacheService;
        _service = service;
    }

    /// <summary>
    /// 通知用户下线事件
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [EventSubscribe(EventSubscriberConst.UserLoginOut)]
    public async Task UserLoginOut(EventHandlerExecutingContext context)
    {
        var loginEvent = (UserLoginOutEvent)context.Source.Payload;//获取参数
        //客户端ID列表
        var clientIds = new List<string>();
        //遍历verificat列表获取客户端ID列表
        loginEvent?.VerificatInfos?.ForEach(it =>
        {
            clientIds.AddRange(it.ClientIds);
        });
        await GetNoticeService().UserLoginOut(loginEvent.UserId, clientIds, loginEvent.Message);//发送消息
    }

    /// <summary>
    /// 有新的消息
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [EventSubscribe(EventSubscriberConst.NewMessage)]
    public async Task NewMessage(EventHandlerExecutingContext context)
    {
        var newMessageEvent = (NewMessageEvent)context.Source.Payload;//获取参数

        var clientIds = new List<string>();
        //获取用户verificat列表
        var verificatInfos = UserTokenCacheUtil.HashGet(newMessageEvent.UserIds.ToArray());
        verificatInfos.ForEach(it =>
        {
            if (it != null)
            {
                it = it.Where(it => it.VerificatTimeout > DateTime.Now).ToList();//去掉登录超时的
                //遍历verificat
                it.ForEach(it =>
                {
                    clientIds.AddRange(it.ClientIds);//添加到客户端ID列表
                });
            }
        });
        await GetNoticeService().NewMesage(newMessageEvent.UserIds, clientIds, newMessageEvent.Message);//发送消息
    }

    /// <summary>
    /// 获取通知服务
    /// </summary>
    /// <returns></returns>
    private INoticeService GetNoticeService()
    {
        var noticeService = _service.CreateScope().ServiceProvider.GetService<INoticeService>();//获取服务
        return noticeService;
    }
}