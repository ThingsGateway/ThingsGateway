//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Http.Connections.Features;
using Microsoft.AspNetCore.SignalR;

using ThingsGateway.Core.Extension;

namespace ThingsGateway.Admin.Application;

public class SysHub : Hub<ISysHub>
{
    private readonly IVerificatInfoService _verificatInfoService;

    public SysHub(IVerificatInfoService verificatInfoService)
    {
        _verificatInfoService = verificatInfoService;
    }

    /// <summary>
    /// 连接
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var feature = Context.Features.Get<IHttpContextFeature>();
        var VerificatId = feature!.HttpContext!.Request.Headers[ClaimConst.VerificatId].FirstOrDefault().ToLong();
        if (VerificatId > 0)
        {
            var userIdentifier = Context.UserIdentifier;//自定义的Id
            UpdateVerificat(userIdentifier.ToLong(), verificat: VerificatId);//更新cache
        }
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userIdentifier = Context.UserIdentifier.ToLong();//自定义的Id
        var feature = Context.Features.Get<IHttpContextFeature>();
        var VerificatId = feature!.HttpContext!.Request.Headers[ClaimConst.VerificatId].FirstOrDefault().ToLong();
        UpdateVerificat(userIdentifier, verificat: VerificatId, isConnect: false);//更新cache
        await base.OnDisconnectedAsync(exception);
    }

    #region 方法

    /// <summary>
    /// 更新cache
    /// </summary>
    /// <param name="userId">用户id</param>
    /// <param name="verificat">上线时的验证id</param>
    /// <param name="isConnect">上线</param>
    private void UpdateVerificat(long userId, long verificat = 0, bool isConnect = true)
    {
        if (userId != 0)
        {
            //获取cache当前用户的verificat信息列表
            if (isConnect)
            {
                //获取cache中当前verificat
                var verificatInfo = _verificatInfoService.GetOne(verificat);
                if (verificatInfo != null)
                {
                    verificatInfo.ClientIds.Add(userId);//添加到客户端列表
                    _verificatInfoService.Update(verificatInfo);//更新Cache
                }
            }
            else
            {
                //获取当前客户端ID所在的verificat信息
                var verificatInfo = _verificatInfoService.GetOne(verificat);
                if (verificatInfo != null)
                {
                    verificatInfo.ClientIds.RemoveWhere(it => it == userId);//从客户端列表删除
                    _verificatInfoService.Update(verificatInfo);//更新Cache
                }
            }
        }
    }

    #endregion 方法
}
