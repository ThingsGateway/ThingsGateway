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

namespace ThingsGateway.Application
{
    /// <summary>
    /// <inheritdoc cref="IOpenApiSessionService"/>
    /// </summary>
    [Injection(Proxy = typeof(OperDispatchProxy))]
    public class OpenApiSessionService : DbRepository<OpenApiUser>, IOpenApiSessionService
    {
        #region Private Fields

        private readonly SysCacheService _sysCacheService;

        #endregion Private Fields

        #region Public Constructors

        /// <inheritdoc cref="IOpenApiSessionService"/>
        public OpenApiSessionService(SysCacheService sysCacheService)
        {
            this._sysCacheService = sysCacheService;
        }

        #endregion Public Constructors

        #region Public Methods

        /// <inheritdoc/>
        [OperDesc("强退OPENAPI会话")]
        public async Task ExitSession(BaseIdInput input)
        {
            //verificat列表
            List<VerificatInfo> verificatInfos = _sysCacheService.GetOpenApiVerificatId(input.Id);
            //从列表中删除
            _sysCacheService.SetOpenApiVerificatId(input.Id, new());
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        [OperDesc("强退OPENAPI令牌")]
        public async Task ExitVerificat(OpenApiExitVerificatInput input)
        {
            //获取该用户的verificat信息
            List<VerificatInfo> verificatInfos = _sysCacheService.GetOpenApiVerificatId(input.Id);
            //当前需要踢掉用户的verificat
            List<VerificatInfo> deleteVerificats = new();
            if (input.Id == 0 || verificatInfos.Count == 0)
            {
                var sysverificats = _sysCacheService.GetAllOpenApiVerificatId();
                verificatInfos = sysverificats.SelectMany(x => x.VerificatInfos).ToList();
                if (verificatInfos.Count > 0)
                {
                    input.Id = verificatInfos.FirstOrDefault().UserId;
                }
            }
            if (input.Id <= 0) return;
            deleteVerificats = verificatInfos.Where(it => input.VerificatIds.Contains(it.Id)).ToList();

            //踢掉包含verificat列表的verificat信息
            verificatInfos = verificatInfos.Where(it => !input.VerificatIds.Contains(it.Id)).ToList();
            _sysCacheService.SetOpenApiVerificatId(input.Id, verificatInfos);//如果还有verificat则更新verificat
            await Task.CompletedTask;
        }

        /// <summary>
        /// 获取cache中verificat信息列表
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, List<VerificatInfo>> GetVerificatDicFromCache()
        {
            List<SysVerificat> bVerificat = _sysCacheService.GetAllOpenApiVerificatId();
            return bVerificat.ToDictionary(it => it.Id.ToString(), a => a.VerificatInfos);
        }

        /// <summary>
        /// 获取verificat剩余时间信息
        /// </summary>
        /// <param name="verificatInfos">verificat列表</param>
        public void GetVerificatInfos(ref List<VerificatInfo> verificatInfos)
        {
            verificatInfos = verificatInfos.ToList();
            verificatInfos.ForEach(it =>
            {
                var now = DateTime.UtcNow;
                it.VerificatRemain = now.GetDiffTime(it.VerificatTimeout);//获取时间差
                var verificatSecond = it.VerificatTimeout.AddMinutes(-it.Expire).ToLong();//颁发时间转为时间戳
                var timeoutSecond = it.VerificatTimeout.ToLong();//过期时间转为时间戳
                var verificatRemainPercent = 1 - ((now.ToLong() - verificatSecond) * 1.0 / (timeoutSecond - verificatSecond));//求百分比,用现在时间-verificat颁布时间除以超时时间-verificat颁布时间
                it.VerificatRemainPercent = verificatRemainPercent;
            });
        }

        /// <inheritdoc/>
        public async Task<SqlSugarPagedList<OpenApiSessionOutput>> Page(OpenApiSessionPageInput input)
        {
            //获取b端verificat列表
            var bVerificatInfoDic = GetVerificatDicFromCache();

            //获取用户ID列表
            var userIds = bVerificatInfoDic.Keys.Select(it => it.ToLong()).ToList();
            var query = Context.Queryable<OpenApiUser>().Where(it => userIds.Contains(it.Id))//根据ID查询
                  .WhereIF(!string.IsNullOrEmpty(input.Account), it => it.Account.Contains(input.Account))//根据账号查询
                  .WhereIF(!string.IsNullOrEmpty(input.LatestLoginIp), it => it.LatestLoginIp.Contains(input.LatestLoginIp))//根据IP查询
                  .OrderByIF(!string.IsNullOrEmpty(input.SortField), $"{input.SortField} {input.SortOrder}")
                  .OrderBy(it => it.LatestLoginTime, OrderByType.Desc)
                  .Select<OpenApiSessionOutput>()
                  .Mapper(it =>
                  {
                      var verificatInfos = bVerificatInfoDic[it.Id.ToString()];//获取用户verificat信息
                      GetVerificatInfos(ref verificatInfos);//获取剩余时间
                      it.VerificatCount = verificatInfos.Count;//令牌数量
                      it.VerificatSignList = verificatInfos;//令牌列表
                  });

            var pageInfo = await query.ToPagedListAsync(input.Current, input.Size);//分页
            pageInfo.Records.OrderByDescending(it => it.VerificatCount);
            return pageInfo;
        }

        #endregion Public Methods
    }
}