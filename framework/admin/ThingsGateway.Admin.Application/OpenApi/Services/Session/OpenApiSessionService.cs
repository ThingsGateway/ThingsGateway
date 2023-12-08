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

using Furion.DependencyInjection;

using SqlSugar;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// <inheritdoc cref="IOpenApiSessionService"/>
/// </summary>
[Injection(Proxy = typeof(OperDispatchProxy))]
public class OpenApiSessionService : DbRepository<OpenApiUser>, IOpenApiSessionService
{
    private readonly IVerificatService _verificatService;

    /// <inheritdoc cref="IOpenApiSessionService"/>
    public OpenApiSessionService(IVerificatService verificatService)
    {
        _verificatService = verificatService;
    }

    /// <inheritdoc/>
    [OperDesc("强退OPENAPI会话")]
    public async Task ExitSessionAsync(long input)
    {
        //从列表中删除
        await _verificatService.SetOpenApiVerificatIdAsync(input, new());
    }

    /// <inheritdoc/>
    [OperDesc("强退OPENAPI令牌")]
    public async Task ExitVerificatAsync(OpenApiExitVerificatInput input)
    {
        //获取该用户的verificat信息
        List<VerificatInfo> verificatInfos = await _verificatService.GetOpenApiVerificatIdAsync(input.Id);
        //当前需要踢掉用户的verificat
        var setVerificats = verificatInfos.Where(it => !input.VerificatIds.Contains(it.Id)).ToList();
        await _verificatService.SetOpenApiVerificatIdAsync(input.Id, setVerificats);//如果还有verificat则更新verificat
    }

    /// <summary>
    /// 获取verificat剩余时间信息
    /// </summary>
    /// <param name="verificatInfos">verificat列表</param>
    private static void GetVerificatInfos(ref List<VerificatInfo> verificatInfos)
    {
        verificatInfos = verificatInfos.ToList();
        verificatInfos.ForEach(it =>
        {
            var now = DateTimeExtensions.CurrentDateTime;
            it.VerificatRemain = now.GetDiffTime(it.VerificatTimeout);//获取时间差
            var verificatSecond = it.VerificatTimeout.AddMinutes(-it.Expire).ToLong();//颁发时间转为时间戳
            var timeoutSecond = it.VerificatTimeout.ToLong();//过期时间转为时间戳
        });
    }

    /// <inheritdoc/>
    public async Task<ISqlSugarPagedList<OpenApiSessionOutput>> PageAsync(OpenApiSessionPageInput input)
    {
        var query = Context.Queryable<OpenApiUser>()
            .WhereIF(!string.IsNullOrEmpty(input.Account), it => it.Account.Contains(input.Account))//根据账号查询
            .WhereIF(!string.IsNullOrEmpty(input.LatestLoginIp), it => it.LatestLoginIp.Contains(input.LatestLoginIp))//根据IP查询
            .OrderBy(it => it.LatestLoginTime, OrderByType.Desc)
            .Select<OpenApiSessionOutput>()
            .Mapper(async it =>
            {
                var verificatInfos = await _verificatService.GetVerificatIdAsync(it.Id);
                if (verificatInfos != null)
                {
                    GetVerificatInfos(ref verificatInfos);//获取剩余时间
                    it.VerificatCount = verificatInfos.Count;//令牌数量
                    it.VerificatSignList = verificatInfos;//令牌列表
                }
                else
                {
                    it.VerificatSignList = new();
                }
            });
        for (int i = input.SortField.Count - 1; i >= 0; i--)
        {
            query = query.OrderByIF(!string.IsNullOrEmpty(input.SortField[i]), $"{input.SortField[i]} {(input.SortDesc[i] ? "desc" : "asc")}");
        }

        var pageInfo = await query.ToPagedListAsync(input.Current, input.Size);//分页
        pageInfo.Records = pageInfo.Records.OrderByDescending(it => it.VerificatCount);
        return pageInfo;
    }
}