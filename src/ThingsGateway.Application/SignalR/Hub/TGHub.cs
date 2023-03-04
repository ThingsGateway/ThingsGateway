using Furion.InstantMessaging;
using Furion.Logging.Extensions;

using Microsoft.AspNetCore.Http.Connections.Features;


namespace ThingsGateway.Application
{
    /// <summary>
    /// <inheritdoc cref="ITGHub"/>
    /// </summary>
    [MapHub(HubConst.HubUrl)]
    public class TGHub : Hub<ITGHub>
    {
        public const string TG_TrackingCircuitHandlerid = nameof(TG_TrackingCircuitHandlerid);
        private readonly SysCacheService _sysCacheService;

        public TGHub(SysCacheService sysCacheService)
        {
            this._sysCacheService = sysCacheService;
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <returns></returns>
        public override Task OnConnectedAsync()
        {
            var feature = Context.Features.Get<IHttpContextFeature>();
            var VerificatId = feature.HttpContext.Request.Headers[ClaimConst.VerificatId].FirstOrDefault().ToLong();

            var userIdentifier = Context.UserIdentifier;//自定义的Id
            UpdateCache(userIdentifier, VerificatId);//更新cache
            return base.OnConnectedAsync();
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userIdentifier = Context.UserIdentifier;//自定义的Id
            UpdateCache(userIdentifier, 0, false);//更新cache
            await base.OnDisconnectedAsync(exception);
        }

        #region 方法

        /// <summary>
        /// 更新cache
        /// </summary>
        /// <param name="userIdentifier">用户id</param>
        /// <param name="verificat">verificat</param>
        /// <param name="ifConnect">是否是上线</param>
        private void UpdateCache(string userIdentifier, long verificat, bool ifConnect = true)
        {
            var userId = userIdentifier.Split(TG_TrackingCircuitHandlerid)[0].ToLong();//分割取第一个
            if (userId > 0)
            {
                //获取cache当前用户的verificat信息列表
                List<VerificatInfo> verificatInfos = _sysCacheService.GetVerificatId(userId);

                if (verificatInfos != null)
                {
                    if (ifConnect)
                    {
                        //获取cache中当前verificat
                        var verificatInfo = verificatInfos.Where(it => it.Id == verificat).FirstOrDefault();
                        if (verificatInfo != null)
                        {
                            verificatInfo.ClientIds.Add(userIdentifier);//添加到客户端列表
                            _sysCacheService.SetVerificatId(userId, verificatInfos);//更新Cache
                        }
                    }
                    else
                    {
                        //获取当前客户端ID所在的verificat信息
                        var verificatInfo = verificatInfos.Where(it => it.ClientIds.Contains(userIdentifier)).FirstOrDefault();
                        if (verificatInfo != null)
                        {
                            verificatInfo.ClientIds.RemoveWhere(it => it == userIdentifier);//从客户端列表删除
                            _sysCacheService.SetVerificatId(userId, verificatInfos);//更新Cache
                        }
                    }
                }
                else
                {
                }
            }
            else
            {
                if (ifConnect)
                    ("未认证SignalR ID：{0} 登录").LogWarning(userIdentifier);
                else
                    ("未认证SignalR ID：{0} 注销").LogWarning(userIdentifier);
            }
        }

        #endregion 方法
    }
}