namespace ThingsGateway.Application
{
    /// <summary>
    /// 用户模块事件总线
    /// </summary>
    public class UserEventSubscriber : IEventSubscriber, ISingleton
    {
        private readonly IServiceProvider _services;
        /// <summary>
        /// <inheritdoc cref="UserEventSubscriber"/>
        /// </summary>
        /// <param name="services"></param>
        public UserEventSubscriber(IServiceProvider services)
        {
            this._services = services;
        }

        /// <summary>
        /// 根据角色ID列表清除用户缓存
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [EventSubscribe(EventSubscriberConst.ClearUserCache)]
        public async Task DeleteUserCacheByRoleIds(EventHandlerExecutingContext context)
        {
            var roleIds = (List<long>)context.Source.Payload;//获取角色ID
                                                             // 创建新的作用域
            using var scope = _services.CreateScope();
            // 解析角色服务
            var relationService = scope.ServiceProvider.GetRequiredService<IRelationService>();
            //获取用户和角色关系
            var relations = await relationService.GetRelationListByTargetIdListAndCategory(roleIds.Select(it => it.ToString()).ToList(), CateGoryConst.Relation_SYS_USER_HAS_ROLE);
            if (relations.Count > 0)
            {
                var userIds = relations.Select(it => it.ObjectId).ToList();//用户ID列表
                                                                           // 解析用户服务
                var userService = scope.ServiceProvider.GetRequiredService<ISysUserService>();
                //从缓存中删除
                userService.DeleteUserFromCache(userIds);
            }
        }
    }
}