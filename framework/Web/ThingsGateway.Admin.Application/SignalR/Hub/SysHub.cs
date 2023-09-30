﻿#region copyright
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

using Furion;
using Furion.InstantMessaging;
using Furion.Logging.Extensions;

using Microsoft.AspNetCore.Http.Connections.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

using ThingsGateway.Foundation.Extension.String;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// <inheritdoc cref="ISysHub"/>
/// </summary>
[MapHub(HubConst.SysHubUrl)]
public class SysHub : Hub<ISysHub>
{
    readonly ILogger<ISysHub> _logger;
    /// <inheritdoc cref="ISysHub"/>
    public SysHub(ILogger<ISysHub> logger)
    {
        _logger = logger;
    }
    /// <summary>
    /// 分隔符
    /// </summary>
    public const string Separate = "_s_e_p_a_r_a_t_e_";

    /// <summary>
    /// 连接
    /// </summary>
    /// <returns></returns>
    public override async Task OnConnectedAsync()
    {
        var feature = Context.Features.Get<IHttpContextFeature>();
        var VerificatId = feature.HttpContext.Request.Headers[ClaimConst.VerificatId].FirstOrDefault().ToLong();

        var userIdentifier = Context.UserIdentifier;//自定义的Id
        await UpdateVerificatAsync(userIdentifier, verificat: VerificatId);//更新cache
        await base.OnConnectedAsync();
    }
    /// <summary>
    /// 断开连接
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userIdentifier = Context.UserIdentifier;//自定义的Id
        await UpdateVerificatAsync(userIdentifier, false);//更新cache
        await base.OnDisconnectedAsync(exception);
    }

    #region 方法

    /// <summary>
    /// 更新cache
    /// </summary>
    /// <param name="userIdentifier">用户id</param>
    /// <param name="verificat">上线时的验证id</param>
    /// <param name="isConnect">是否是上线</param>
    private async Task UpdateVerificatAsync(string userIdentifier, bool isConnect = true, long verificat = 0)
    {
        var userId = userIdentifier.Split(Separate)[0].ToLong();//分割取第一个
        if (userId > 0)
        {
            var _verificatService = App.GetService<IVerificatService>();
            //获取cache当前用户的verificat信息列表
            List<VerificatInfo> verificatInfos = await _verificatService.GetVerificatIdAsync(userId);

            if (verificatInfos != null)
            {
                if (isConnect)
                {
                    //获取cache中当前verificat
                    var verificatInfo = verificatInfos.Where(it => it.Id == verificat).FirstOrDefault();
                    if (verificatInfo != null)
                    {
                        verificatInfo.ClientIds.Add(userIdentifier);//添加到客户端列表
                        await _verificatService.SetVerificatIdAsync(userId, verificatInfos);
                    }
                }
                else
                {
                    //获取当前客户端ID所在的verificat信息
                    var verificatInfo = verificatInfos.Where(it => it.ClientIds.Contains(userIdentifier)).FirstOrDefault();
                    if (verificatInfo != null)
                    {
                        verificatInfo.ClientIds.RemoveWhere(it => it == userIdentifier);//从客户端列表删除
                        await _verificatService.SetVerificatIdAsync(userId, verificatInfos);
                    }
                }
            }
        }
        else
        {
            if (isConnect)
                _logger.LogWarning($"未认证SignalR ID：{userIdentifier} 登录");
            else
                _logger.LogWarning($"未认证SignalR ID：{userIdentifier} 注销");
        }
    }

    #endregion 方法
}