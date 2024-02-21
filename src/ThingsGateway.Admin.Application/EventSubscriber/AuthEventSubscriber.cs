//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 认证模块事件总线
/// </summary>
public class AuthEventSubscriber : IEventSubscriber, ISingleton
{
    private readonly ISimpleCacheService _simpleCacheService;
    public IServiceProvider _services { get; }
    private readonly SqlSugarScope _db;

    public AuthEventSubscriber(ISimpleCacheService simpleCacheService, IServiceProvider services)
    {
        _db = DbContext.Db;
        _simpleCacheService = simpleCacheService;
        _services = services;
    }

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

        #region 登录/密码策略

        var key = SystemConst.Cache_LoginErrorCount + sysUser.Account;//获取登录错误次数Key值
        _simpleCacheService.Remove(key);//移除登录错误次数

        // 创建新的作用域
        using var scope = _services.CreateScope();
        // 解析服务
        var configService = scope.ServiceProvider.GetRequiredService<IConfigService>();
        var loginPolicy = await configService.GetListByCategoryAsync(CateGoryConst.Config_PWD_POLICY);//获取密码策略
        //获取用户verificat列表
        var tokenInfos = UserTokenCacheUtil.HashGetOne(sysUser.Id);
        var userToken = tokenInfos.Where(it => it.Id == loginEvent.VerificatId).FirstOrDefault();

        #endregion 登录/密码策略

        #region 重新赋值属性,设置本次登录信息为最新的信息

        sysUser.LastLoginDevice = sysUser.LatestLoginDevice;
        sysUser.LastLoginIp = sysUser.LatestLoginIp;
        sysUser.LastLoginTime = sysUser.LatestLoginTime;
        sysUser.LatestLoginDevice = loginEvent.Device.ToString();
        sysUser.LatestLoginIp = loginEvent.Ip;
        sysUser.LatestLoginTime = loginEvent.DateTime;

        #endregion 重新赋值属性,设置本次登录信息为最新的信息

        //更新用户登录信息
        if (await _db.UpdateableWithAttr(sysUser).UpdateColumns(it => new
        {
            it.LastLoginDevice,
            it.LastLoginIp,
            it.LastLoginTime,
            it.LatestLoginDevice,
            it.LatestLoginIp,
            it.LatestLoginTime,
        }).ExecuteCommandAsync() > 0)
            _simpleCacheService.HashAdd(SystemConst.Cache_SysUser, sysUser.Id.ToString(), sysUser);//更新Redis信息

        await Task.CompletedTask;
    }

    /// <summary>
    /// 登出事件
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [EventSubscribe(EventSubscriberConst.LoginOut)]
    public async Task LoginOut(EventHandlerExecutingContext context)
    {
        _ = (LoginEvent)context.Source.Payload;//获取参数
        await Task.CompletedTask;
    }

    /// <summary>
    /// 获取通知服务
    /// </summary>
    /// <returns></returns>
    private INoticeService GetNoticeService()
    {
        var noticeService = _services.GetService<INoticeService>();//获取服务
        return noticeService;
    }

    /// <summary>
    ///   延迟执行
    /// </summary>
    /// <param name="millisecondsDelay">毫秒</param>
    /// <param name="actionToExecute">方法</param>
    private async Task DelayedExecutionAsync(int millisecondsDelay, Action actionToExecute)
    {
        // 延迟指定的时间
        await Task.Delay(millisecondsDelay);

        // 执行目标方法
        actionToExecute.Invoke();
    }
}