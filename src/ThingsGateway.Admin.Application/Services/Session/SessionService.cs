//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using NewLife.Extension;

using SqlSugar;

using ThingsGateway.Core.Extension;

namespace ThingsGateway.Admin.Application;

public class SessionService : BaseService<SysUser>, ISessionService
{
    private readonly IVerificatInfoCacheService _verificatInfoCacheService;

    public SessionService(IVerificatInfoCacheService verificatInfoCacheService)
    {
        _verificatInfoCacheService = verificatInfoCacheService;
    }

    #region 查询

    /// <summary>
    /// 表格查询
    /// </summary>
    /// <param name="option">查询条件</param>
    public async Task<QueryData<SessionOutput>> PageAsync(QueryPageOptions option)
    {
        //获取verificat列表
        var bTokenInfoDic = GetTokenDicFromCache();
        //获取用户ID列表
        var userIds = bTokenInfoDic.Keys.Select(it => it.ToLong());
        var ret = new QueryData<SessionOutput>()
        {
            IsSorted = option.SortOrder != SortOrder.Unset,
            IsFiltered = option.Filters.Any(),
            IsAdvanceSearch = option.AdvanceSearches.Any() || option.CustomerSearches.Any(),
            IsSearch = option.Searches.Any()
        };

        using var db = GetDB();
        var query = db.GetQuery<SysUser>(option).WhereIF(!option.SearchText.IsNullOrWhiteSpace(), a => a.Account.Contains(option.SearchText!)).Select<SessionOutput>()
            .Mapper(it =>
            {
                if (bTokenInfoDic.TryGetValue(it.Id, out var verificatInfos))
                {
                    GetTokenInfos(ref verificatInfos);//获取剩余时间
                    it.VerificatCount = verificatInfos.Count;//令牌数量
                    it.VerificatSignList = verificatInfos;//令牌列表

                    //如果有mqtt客户端ID就是在线
                    it.Online = verificatInfos.Any(it => it.ClientIds.Count > 0);
                }
            });
        if (option.IsPage)
        {
            RefAsync<int> totalCount = 0;

            var items = await query
                .ToPageListAsync(option.PageIndex, option.PageItems, totalCount);

            ret.TotalCount = totalCount;
            ret.Items = items;
        }
        else if (option.IsVirtualScroll)
        {
            RefAsync<int> totalCount = 0;

            var items = await query
                .ToPageListAsync(option.StartIndex, option.PageItems, totalCount);

            ret.TotalCount = totalCount;
            ret.Items = items;
        }
        else
        {
            var items = await query
                .ToListAsync();
            ret.TotalCount = items.Count;
            ret.Items = items;
        }
        return ret;
    }

    #endregion 查询

    #region 修改

    /// <summary>
    /// 强退会话
    /// </summary>
    /// <param name="userId">用户id</param>
    [OperDesc("ExitSession")]
    public async Task ExitSession(long userId)
    {
        //verificat列表
        var verificatInfos = _verificatInfoCacheService.HashGetOne(userId);
        //从列表中删除
        _verificatInfoCacheService.HashDel(userId);
        await NoticeUserLoginOut(userId, verificatInfos);
    }

    /// <summary>
    /// 强退令牌
    /// </summary>
    /// <param name="input">参数</param>
    /// <returns></returns>
    [OperDesc("ExitVerificat")]
    public async Task ExitVerificat(ExitVerificatInput input)
    {
        var userId = input.Id;
        //获取该用户的verificat信息
        var verificatInfos = _verificatInfoCacheService.HashGetOne(userId);
        //当前需要踢掉用户的verificat
        var deleteVerificats = verificatInfos.Where(it => input.VerificatIds.Contains(it.Id));
        //踢掉包含verificat列表的verificat信息
        verificatInfos = verificatInfos.Where(it => !input.VerificatIds.Contains(it.Id)).ToList();
        if (verificatInfos.Count > 0)
            _verificatInfoCacheService.HashAdd(userId, verificatInfos);//如果还有verificat则更新verificat
        else
            _verificatInfoCacheService.HashDel(userId);//否则直接删除key
        await NoticeUserLoginOut(userId, deleteVerificats);
    }

    #endregion 修改

    #region 方法

    /// <summary>
    /// 获取cache中verificat信息列表
    /// </summary>
    /// <returns></returns>
    private Dictionary<long, List<VerificatInfo>> GetTokenDicFromCache()
    {
        //cache获取verificat信息hash集合,并转成字典
        var bTokenDic = _verificatInfoCacheService.GetAll();
        if (bTokenDic != null)
        {
            foreach (var it in bTokenDic)
            {
                var verificats = it.Value.Where(it => it.VerificatTimeout.AddSeconds(30) > DateTime.Now).ToList();//去掉登录超时的
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
                _verificatInfoCacheService.HashSet(bTokenDic);
            }
            else
            {
                _verificatInfoCacheService.Remove();
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
    private void GetTokenInfos(ref List<VerificatInfo> verificatInfos)
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
    private async Task NoticeUserLoginOut(long userId, IEnumerable<VerificatInfo> verificatInfos)
    {
        await NoticeUtil.UserLoginOut(new UserLoginOutEvent
        {
            Message = Localizer["ExitVerificat"],
            VerificatInfos = verificatInfos,
        });//通知用户下线
    }

    #endregion 方法
}
