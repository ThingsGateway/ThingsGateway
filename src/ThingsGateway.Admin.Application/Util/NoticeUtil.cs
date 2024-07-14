//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;

namespace ThingsGateway.Admin.Application;

public class NoticeUtil
{
    private static ISignalrNoticeService NoticeService;

    /// <summary>
    /// 通知用户下线事件
    /// </summary>
    /// <param name="userLoginOutEvent"></param>
    /// <returns></returns>
    public static async Task UserLoginOut(UserLoginOutEvent userLoginOutEvent)
    {
        NoticeService ??= App.RootServices!.GetRequiredService<ISignalrNoticeService>();//获取服务
        //遍历verificat列表获取客户端ID列表
        await NoticeService.UserLoginOut(userLoginOutEvent?.ClientIds, userLoginOutEvent!.Message).ConfigureAwait(false);//发送消息
    }
}

/// <summary>
/// 用户登出事件
/// </summary>
public class UserLoginOutEvent
{
    /// <summary>
    /// verificat信息
    /// </summary>

    public List<long>? ClientIds { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    public string Message { get; set; }
}

/// <summary>
/// 新消息事件
/// </summary>
public class NewMessageEvent
{
    /// <summary>
    /// 内容
    /// </summary>
    public SignalRMessage Message { get; set; }

    /// <summary>
    /// 用户Id
    /// </summary>
    public List<long> UserIds { get; set; }
}
