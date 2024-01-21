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

using ThingsGateway.Core.Extension;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// <inheritdoc cref="ISessionService"/>
/// </summary>
[Injection(Proxy = typeof(OperDispatchProxy))]
public class SessionService : DbRepository<SysUser>, ISessionService
{
    private readonly ISimpleCacheService _simpleCacheService;
    private readonly IEventPublisher _eventPublisher;

    public SessionService(ISimpleCacheService simpleCacheService, IEventPublisher eventPublisher)
    {
        _simpleCacheService = simpleCacheService;
        _eventPublisher = eventPublisher;
    }

    /// <inheritdoc/>
    public async Task<SqlSugarPagedList<SessionOutput>> PageAsync(SessionPageInput input)
    {
        //获取b端verificat列表
        var bTokenInfoDic = GetTokenDicFromRedis();

        //获取用户ID列表
        var userIds = bTokenInfoDic.Keys.Select(it => it.ToLong()).ToList();
        var query = Context.Queryable<SysUser>().Where(it => userIds.Contains(it.Id))//根据ID查询
            .WhereIF(!string.IsNullOrEmpty(input.Account), it => it.Account.Contains(input.Account))//根据账号查询
            .OrderBy(it => it.LatestLoginTime, OrderByType.Desc)
            .Select<SessionOutput>()
            .Mapper(it =>
            {
                var verificatInfos = bTokenInfoDic[it.Id];//获取用户verificat信息
                GetTokenInfos(ref verificatInfos);//获取剩余时间
                it.VerificatCount = verificatInfos.Count;//令牌数量
                it.VerificatSignList = verificatInfos;//令牌列表
                //如果有mqtt客户端ID就是在线
                it.Online = verificatInfos.Any(it => it.ClientIds.Count > 0);
            });

        var pageInfo = await query.ToPagedListAsync(input.Current, input.Size);//分页
        pageInfo.Records.OrderByDescending(it => it.VerificatCount);
        return pageInfo;
    }

    /// <inheritdoc/>
    public SessionAnalysisOutput Analysis()
    {
        var tokenDic = GetTokenDicFromRedis();//redistoken会话字典信息
        var tokenInfosList = tokenDic.Values.ToList();//端verificat列表
        var dicB = new Dictionary<long, List<VerificatInfo>>();
        var onLineCount = 0;
        foreach (var verificat in tokenDic)
        {
            var b = verificat.Value.ToList();//获取该用户B端verificat
            if (b.Count > 0)
                dicB.Add(verificat.Key, b);
            var count = verificat.Value.Count(it => it.ClientIds.Count > 0);//计算在线用户
            onLineCount += count;
        }
        var verificatB = dicB.Values.ToList();//b端verificat列表
        int maxCountB = 0;
        if (verificatB.Count > 0)
            maxCountB = verificatB.OrderByDescending(it => it.Count).Take(1).First().Count();//b端最大会话数

        return new SessionAnalysisOutput()
        {
            OnLineCount = onLineCount,
            CurrentSessionTotalCount = verificatB.Count,
            MaxVerificatCount = maxCountB,
        };
    }

    /// <inheritdoc/>
    [OperDesc("强退会话")]
    public async Task ExitSessionAsync(BaseIdInput input)
    {
        var userId = input.Id;
        //verificat列表
        var verificatInfos = UserTokenCacheUtil.HashGetOne(userId);
        //从列表中删除
        UserTokenCacheUtil.HashDel(userId);
        await NoticeUserLoginOut(userId, verificatInfos);
    }

    /// <inheritdoc/>
    [OperDesc("强退令牌")]
    public async Task ExitVerificatAsync(ExitVerificatInput input)
    {
        var userId = input.Id;
        //获取该用户的verificat信息
        var verificatInfos = UserTokenCacheUtil.HashGetOne(userId);
        //当前需要踢掉用户的verificat
        var deleteVerificats = verificatInfos.Where(it => input.Verificats.Contains(it.Id)).ToList();
        //踢掉包含verificat列表的verificat信息
        verificatInfos = verificatInfos.Where(it => !input.Verificats.Contains(it.Id)).ToList();
        if (verificatInfos.Count > 0)
            UserTokenCacheUtil.HashAdd(userId, verificatInfos);//如果还有verificat则更新verificat
        else
            UserTokenCacheUtil.HashDel(userId);//否则直接删除key
        await NoticeUserLoginOut(userId, deleteVerificats);
    }

    #region 方法

    /// <summary>
    /// 获取redis中verificat信息列表
    /// </summary>
    /// <returns></returns>
    public Dictionary<long, List<VerificatInfo>> GetTokenDicFromRedis()
    {
        var clockSkew = App.GetConfig<int>("JWTSettings:ClockSkew");//获取过期时间容错值(秒)
        //redis获取verificat信息hash集合,并转成字典
        var bTokenDic = UserTokenCacheUtil.HashGetAll();
        if (bTokenDic != null)
        {
            foreach (var it in bTokenDic)
            {
                var verificats = it.Value.Where(it => it.VerificatTimeout.AddSeconds(clockSkew) > DateTime.Now).ToList();//去掉登录超时的
                if (verificats.Count == 0)
                {
                    //表示都过期了
                    bTokenDic.Remove(it.Key);
                }
                else
                {
                    bTokenDic[it.Key] = verificats;//重新赋值verificat
                }
            }
            if (bTokenDic.Count > 0)
            {
                UserTokenCacheUtil.HashSet(bTokenDic);
            }
            else
            {
                UserTokenCacheUtil.Remove();
            }
            return bTokenDic;
        }
        else
        {
            return new Dictionary<long, List<VerificatInfo>>();
        }
    }

    /// <summary>
    /// 获取verificat剩余时间信息
    /// </summary>
    /// <param name="verificatInfos">verificat列表</param>
    /// <param name="loginClientType">登录类型</param>
    public void GetTokenInfos(ref List<VerificatInfo> verificatInfos)
    {
        verificatInfos.ForEach(it =>
        {
            var now = DateTime.Now;
            it.VerificatRemain = now.GetDiffTime(it.VerificatTimeout);//获取时间差
        });
    }

    /// <summary>
    /// 通知用户下线
    /// </summary>
    /// <returns></returns>
    private async Task NoticeUserLoginOut(long userId, List<VerificatInfo> verificatInfos)
    {
        await _eventPublisher.PublishAsync(EventSubscriberConst.UserLoginOut, new UserLoginOutEvent
        {
            Message = "您已被强制下线!",
            VerificatInfos = verificatInfos,
            UserId = userId
        });//通知用户下线
    }

    #endregion 方法
}