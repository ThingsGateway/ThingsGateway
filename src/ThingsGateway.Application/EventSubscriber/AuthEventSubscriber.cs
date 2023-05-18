namespace ThingsGateway.Application
{
    /// <summary>
    /// 认证模块事件总线
    /// </summary>
    public class AuthEventSubscriber : IEventSubscriber, ISingleton
    {
        private readonly SqlSugarScope _db;
        private readonly SysCacheService _sysCacheService;
        /// <summary>
        /// <inheritdoc cref="AuthEventSubscriber"/>
        /// </summary>
        /// <param name="sysCacheService"></param>
        /// <param name="services"></param>
        public AuthEventSubscriber(SysCacheService sysCacheService, IServiceProvider services)
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
        [EventSubscribe(EventSubscriberConst.Login)]
        public async Task Login(EventHandlerExecutingContext context)
        {
            var loginEvent = (LoginEvent)context.Source.Payload;//获取参数
            var sysUser = loginEvent.SysUser;

            #region 重新赋值属性,设置本次登录信息为最新的信息

            _db.Tracking(sysUser);//创建跟踪,只更新修改字段
            sysUser.LastLoginDevice = sysUser.LatestLoginDevice;
            sysUser.LastLoginIp = sysUser.LatestLoginIp;
            sysUser.LastLoginTime = sysUser.LatestLoginTime;
            sysUser.LatestLoginDevice = loginEvent.Device.ToString();
            sysUser.LatestLoginIp = loginEvent.Ip;
            sysUser.LatestLoginTime = loginEvent.DateTime;

            #endregion 重新赋值属性,设置本次登录信息为最新的信息

            //更新用户信息
            if (await _db.UpdateableWithAttr(sysUser).ExecuteCommandAsync() > 0)
            {
                _sysCacheService.Set(CacheConst.Cache_SysUser, sysUser.Id.ToString(), sysUser); //更新Cache信息
            }
        }
    }
}