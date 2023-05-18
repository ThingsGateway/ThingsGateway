namespace ThingsGateway.Application
{
    /// <summary>
    /// 认证模块事件总线
    /// </summary>
    public class OpenApiAuthEventSubscriber : IEventSubscriber, ISingleton
    {
        private readonly SqlSugarScope _db;
        private readonly SysCacheService _sysCacheService;
        /// <inheritdoc/>
        public OpenApiAuthEventSubscriber(SysCacheService sysCacheService, IServiceProvider services)
        {
            _db = DbContext.Db;
            this._sysCacheService = sysCacheService;
            this._services = services;
        }

        private IServiceProvider _services { get; }

        /// <summary>
        /// 登录事件
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [EventSubscribe(EventSubscriberConst.LoginOpenApi)]
        public async Task LoginOpenApi(EventHandlerExecutingContext context)
        {
            LoginOpenApiEvent loginEvent = (LoginOpenApiEvent)context.Source.Payload;//获取参数
            OpenApiUser openApiUser = loginEvent.OpenApiUser;

            #region 重新赋值属性,设置本次登录信息为最新的信息

            _db.Tracking(openApiUser);//创建跟踪,只更新修改字段
            openApiUser.LastLoginDevice = openApiUser.LatestLoginDevice;
            openApiUser.LastLoginIp = openApiUser.LatestLoginIp;
            openApiUser.LastLoginTime = openApiUser.LatestLoginTime;
            openApiUser.LatestLoginDevice = loginEvent.Device.ToString();
            openApiUser.LatestLoginIp = loginEvent.Ip;
            openApiUser.LatestLoginTime = loginEvent.DateTime;

            #endregion 重新赋值属性,设置本次登录信息为最新的信息

            //更新用户信息
            if (await _db.UpdateableWithAttr(openApiUser).ExecuteCommandAsync() > 0)
            {
                _sysCacheService.Set(CacheConst.Cache_OpenApiUser, openApiUser.Id.ToString(), openApiUser); //更新Cache信息
            }
        }
    }
}