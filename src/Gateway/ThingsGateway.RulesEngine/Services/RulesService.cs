//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using SqlSugar;

using System.Data;

using ThingsGateway.Admin.Application;
using ThingsGateway.Extension.Generic;
using ThingsGateway.Gateway.Application;

using TouchSocket.Core;

namespace ThingsGateway.RulesEngine;

internal sealed class RulesService : BaseService<Rules>, IRulesService
{

    private IRulesEngineHostedService _rulesEngineHostedService;
    private IRulesEngineHostedService RulesEngineHostedService
    {
        get
        {
            if (_rulesEngineHostedService == null)
            {
                _rulesEngineHostedService = App.GetService<IRulesEngineHostedService>();
            }
            return _rulesEngineHostedService;
        }
    }
    [OperDesc("ClearRules", localizerType: typeof(Rules))]
    public async Task ClearRulesAsync()
    {
        var dataScope = await GlobalData.SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);

        using var db = GetDB();

        var data = GetAll()
              .WhereIf(dataScope != null && dataScope?.Count > 0, u => dataScope.Contains(u.CreateOrgId))//在指定机构列表查询
            .WhereIf(dataScope?.Count == 0, u => u.CreateUserId == UserManager.UserId)
            .Select(a => a.Id).ToList();
        await db.Deleteable<Rules>(data).ExecuteCommandAsync().ConfigureAwait(false);

        DeleteRulesFromCache();
        await RulesEngineHostedService.Delete(data).ConfigureAwait(false);

    }

    [OperDesc("DeleteRules", localizerType: typeof(Rules))]
    public async Task<bool> DeleteRulesAsync(IEnumerable<long> ids)
    {
        using var db = GetDB();
        var dataScope = await GlobalData.SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        await db.Deleteable<Rules>().Where(a => ids.ToHashSet().Contains(a.Id))
                          .WhereIF(dataScope != null && dataScope?.Count > 0, u => dataScope.Contains(u.CreateOrgId))//在指定机构列表查询
             .WhereIF(dataScope?.Count == 0, u => u.CreateUserId == UserManager.UserId)

.ExecuteCommandAsync().ConfigureAwait(false);

        DeleteRulesFromCache();
        await RulesEngineHostedService.Delete(ids).ConfigureAwait(false);
        return true;

    }
    private const string Cache_Rules = "ThingsGateway:Cache_Rules:";
    /// <inheritdoc />
    public void DeleteRulesFromCache()
    {
        App.CacheService.Remove(Cache_Rules);//删除通道缓存
    }

    /// <summary>
    /// 从缓存/数据库获取全部信息
    /// </summary>
    /// <returns>列表</returns>
    public List<Rules> GetAll()
    {
        var key = Cache_Rules;
        var channels = App.CacheService.Get<List<Rules>>(key);
        if (channels == null)
        {
            using var db = GetDB();
            channels = db.Queryable<Rules>().ToList();
            App.CacheService.Set(key, channels);
        }
        return channels;
    }

    public Rules? GetRulesById(long id)
    {
        var data = GetAll();
        return data?.FirstOrDefault(x => x.Id == id);
    }

    /// <summary>
    /// 报表查询
    /// </summary>
    /// <param name="option">查询条件</param>
    /// <param name="filterKeyValueAction">查询条件</param>
    public async Task<QueryData<Rules>> PageAsync(QueryPageOptions option, FilterKeyValueAction filterKeyValueAction = null)
    {
        var dataScope = await GlobalData.SysUserService.GetCurrentUserDataScopeAsync().ConfigureAwait(false);
        return await QueryAsync(option, a => a
        .WhereIF(!option.SearchText.IsNullOrWhiteSpace(), a => a.Name.Contains(option.SearchText!))
                 .WhereIF(dataScope != null && dataScope?.Count > 0, u => dataScope.Contains(u.CreateOrgId))//在指定机构列表查询
         .WhereIF(dataScope?.Count == 0, u => u.CreateUserId == UserManager.UserId)
       , filterKeyValueAction).ConfigureAwait(false);
    }

    /// <summary>
    /// 保存通道
    /// </summary>
    /// <param name="input">通道</param>
    /// <param name="type">保存类型</param>
    [OperDesc("SaveRules", localizerType: typeof(Rules))]
    public async Task<bool> SaveRulesAsync(Rules input, ItemChangedType type)
    {

        //验证
        CheckInput(input);
        if (type == ItemChangedType.Update)
            await GlobalData.SysUserService.CheckApiDataScopeAsync(input.CreateOrgId, input.CreateUserId).ConfigureAwait(false);

        if (await base.SaveAsync(input, type).ConfigureAwait(false))
        {
            DeleteRulesFromCache();
            await RulesEngineHostedService.Edit(input).ConfigureAwait(false);
            return true;
        }
        return false;
    }

    private static void CheckInput(Rules input)
    {

    }






}

