using Microsoft.AspNetCore.Http.Connections.Features;

namespace ThingsGateway.Application
{
    /// <summary>
    /// 用户ID提供器
    /// </summary>
    public class UserIdProvider : IUserIdProvider
    {
        /// <inheritdoc/>
        public string GetUserId(HubConnectionContext connection)
        {
            var feature = connection.Features.Get<IHttpContextFeature>();
            var UserId = feature.HttpContext.Request.Headers[ClaimConst.UserId].FirstOrDefault()?.ToLong();

            if (UserId > 0)
            {
                return $"{UserId}{TGHub.TG_TrackingCircuitHandlerid}{YitIdHelper.NextId()}";//返回用户ID
            }

            return connection.ConnectionId;
        }
    }
}