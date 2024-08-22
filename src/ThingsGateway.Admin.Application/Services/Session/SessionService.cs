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

using Mapster;

using SqlSugar;

using ThingsGateway.Core.Extension;
using ThingsGateway.NewLife.X.Extension;

namespace ThingsGateway.Admin.Application;

public class SessionService : BaseService<SysUser>, ISessionService
{
    private readonly IVerificatInfoService _verificatInfoService;

    public SessionService(IVerificatInfoService verificatInfoService)
    {
        _verificatInfoService = verificatInfoService;
    }

    #region 查询

    /// <summary>
    /// 表格查询
    /// </summary>
    /// <param name="option">查询条件</param>
    public async Task<QueryData<SessionOutput>> PageAsync(QueryPageOptions option)
    {
        var ret = new QueryData<SessionOutput>()
        {
            IsSorted = option.SortOrder != SortOrder.Unset,
            IsFiltered = option.Filters.Any(),
            IsAdvanceSearch = option.AdvanceSearches.Any() || option.CustomerSearches.Any(),
            IsSearch = option.Searches.Any()
        };

        using var db = GetDB();
        var query = db.GetQuery<SysUser>(option).WhereIF(!option.SearchText.IsNullOrWhiteSpace(), a => a.Account.Contains(option.SearchText!));

        if (option.IsPage)
        {
            RefAsync<int> totalCount = 0;

            var items = await query.ToPageListAsync(option.PageIndex, option.PageItems, totalCount).ConfigureAwait(false);

            var verificatInfoDicts = _verificatInfoService.GetListByUserIds(items.Select(a => a.Id).ToList()).GroupBy(a => a.UserId).ToDictionary(a => a.Key, a => a.ToList());

            var r = items.Select((it) =>
            {
                var reuslt = it.Adapt<SessionOutput>();
                if (verificatInfoDicts.TryGetValue(it.Id, out var verificatInfos))
                {
                    GetTokenInfos(verificatInfos);//获取剩余时间
                    reuslt.VerificatCount = verificatInfos.Count;//令牌数量
                    reuslt.VerificatSignList = verificatInfos;//令牌列表

                    //如果有mqtt客户端ID就是在线
                    reuslt.Online = verificatInfos.Any(it => it.ClientIds.Count > 0);
                }

                return reuslt;
            }).ToList();

            ret.TotalCount = totalCount;
            ret.Items = r;
        }
        else if (option.IsVirtualScroll)
        {
            RefAsync<int> totalCount = 0;

            var items = await query.ToPageListAsync(option.StartIndex, option.PageItems, totalCount).ConfigureAwait(false);
            var verificatInfoDicts = _verificatInfoService.GetListByUserIds(items.Select(a => a.Id).ToList()).GroupBy(a => a.UserId).ToDictionary(a => a.Key, a => a.ToList());

            var r = items.Select((it) =>
            {
                var reuslt = it.Adapt<SessionOutput>();
                if (verificatInfoDicts.TryGetValue(it.Id, out var verificatInfos))
                {
                    GetTokenInfos(verificatInfos);//获取剩余时间
                    reuslt.VerificatCount = verificatInfos.Count;//令牌数量
                    reuslt.VerificatSignList = verificatInfos;//令牌列表

                    //如果有mqtt客户端ID就是在线
                    reuslt.Online = verificatInfos.Any(it => it.ClientIds.Count > 0);
                }

                return reuslt;
            }).ToList();
            ret.TotalCount = totalCount;
            ret.Items = r;
        }
        else
        {
            var items = await query.ToListAsync().ConfigureAwait(false);

            var verificatInfoDicts = _verificatInfoService.GetListByUserIds(items.Select(a => a.Id).ToList()).GroupBy(a => a.UserId).ToDictionary(a => a.Key, a => a.ToList());

            var r = items.Select((it) =>
            {
                var reuslt = it.Adapt<SessionOutput>();
                if (verificatInfoDicts.TryGetValue(it.Id, out var verificatInfos))
                {
                    GetTokenInfos(verificatInfos);//获取剩余时间
                    reuslt.VerificatCount = verificatInfos.Count;//令牌数量
                    reuslt.VerificatSignList = verificatInfos;//令牌列表

                    //如果有mqtt客户端ID就是在线
                    reuslt.Online = verificatInfos.Any(it => it.ClientIds.Count > 0);
                }

                return reuslt;
            }).ToList();
            ret.TotalCount = items.Count;
            ret.Items = r;
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
        var verificatInfoIds = _verificatInfoService.GetListByUserId(userId);
        //verificat列表
        _verificatInfoService.Delete(verificatInfoIds.Select(a => a.Id).ToList());
        await NoticeUserLoginOut(userId, verificatInfoIds.SelectMany(a => a.ClientIds).ToList()).ConfigureAwait(false);
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
        var data = input.VerificatIds.ToList();
        if (data.Any())
        {
            var data1 = _verificatInfoService.GetListByIds(data).SelectMany(a => a.ClientIds).ToList();
            _verificatInfoService.Delete(data);//如果还有verificat则更新verificat
            await NoticeUserLoginOut(userId, data1).ConfigureAwait(false);
        }
    }

    #endregion 修改

    #region 方法

    /// <summary>
    /// 获取verificat剩余时间信息
    /// </summary>
    /// <param name="verificatInfos">verificat列表</param>
    private void GetTokenInfos(List<VerificatInfo> verificatInfos)
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
    private async Task NoticeUserLoginOut(long userId, List<long> clientIds)
    {
        await NoticeUtil.UserLoginOut(new UserLoginOutEvent
        {
            Message = Localizer["ExitVerificat"],
            ClientIds = clientIds,
        }).ConfigureAwait(false);//通知用户下线
    }

    #endregion 方法
}
