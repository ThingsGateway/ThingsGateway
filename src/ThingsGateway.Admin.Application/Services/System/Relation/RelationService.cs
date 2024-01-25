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

namespace ThingsGateway.Admin.Application;

/// <inheritdoc cref="IRelationService"/>
public class Relationservice : DbRepository<SysRelation>, IRelationService
{
    private readonly ILogger<Relationservice> _logger;
    private readonly ISimpleCacheService _simpleCacheService;

    public Relationservice(ILogger<Relationservice> logger, ISimpleCacheService simpleCacheService)
    {
        _logger = logger;
        _simpleCacheService = simpleCacheService;
    }

    /// <inheritdoc/>
    public async Task<List<SysRelation>> GetRelationByCategoryAsync(string category)
    {
        var key = SystemConst.Cache_SysRelation + category;
        //先从Redis拿
        var sysRelations = _simpleCacheService.Get<List<SysRelation>>(key);
        if (sysRelations == null)
        {
            //redis没有就去数据库拿
            sysRelations = await base.GetListAsync(it => it.Category == category);
            if (sysRelations.Count > 0)
            {
                //插入Redis
                _simpleCacheService.Set(key, sysRelations);
            }
        }
        return sysRelations;
    }

    /// <inheritdoc/>
    public async Task<List<SysRelation>> GetRelationListByObjectIdAndCategoryAsync(long objectId, string category)
    {
        var sysRelations = await GetRelationByCategoryAsync(category);
        var result = sysRelations.Where(it => it.ObjectId == objectId).ToList();//获取关系集合
        return result;
    }

    /// <inheritdoc/>
    public async Task<SysRelation> GetWorkbenchAsync(long userId)
    {
        var sysRelations = await GetRelationByCategoryAsync(CateGoryConst.Relation_SYS_USER_WORKBENCH_DATA);
        var result = sysRelations.Where(it => it.ObjectId == userId).FirstOrDefault();//获取个人工作台
        return result;
    }

    /// <inheritdoc/>
    public async Task<SysRelation> GetDefaultRazorAsync(long userId)
    {
        var sysRelations = await GetRelationByCategoryAsync(CateGoryConst.Relation_SYS_USER_DEFAULT_RAZOR);
        var result = sysRelations.Where(it => it.ObjectId == userId).FirstOrDefault();//获取个人主页
        return result;
    }

    /// <inheritdoc/>
    public async Task<List<SysRelation>> GetRelationListByObjectIdListAndCategoryAsync(List<long> objectIds, string category)
    {
        var sysRelations = await GetRelationByCategoryAsync(category);
        var result = sysRelations.Where(it => objectIds.Contains(it.ObjectId)).ToList();//获取关系集合
        return result;
    }

    /// <inheritdoc/>
    public async Task<List<SysRelation>> GetRelationListByTargetIdAndCategoryAsync(string targetId, string category)
    {
        var sysRelations = await GetRelationByCategoryAsync(category);
        var result = sysRelations.Where(it => it.TargetId == targetId).ToList();//获取关系集合
        return result;
    }

    /// <inheritdoc/>
    public async Task<List<SysRelation>> GetRelationListByTargetIdListAndCategoryAsync(List<string> targetIds, string category)
    {
        var sysRelations = await GetRelationByCategoryAsync(category);
        var result = sysRelations.Where(it => targetIds.Contains(it.TargetId)).ToList();//获取关系集合
        return result;
    }

    /// <inheritdoc/>
    public async Task RefreshCacheAsync(string category)
    {
        var key = SystemConst.Cache_SysRelation + category;//key
        _simpleCacheService.Remove(key);//删除redis
        await GetRelationByCategoryAsync(category);//更新缓存
    }

    /// <inheritdoc/>
    public async Task SaveRelationBatchAsync(string category, long objectId, List<string> targetIds,
        List<string> extJsons, bool clear)
    {
        var sysRelations = new List<SysRelation>();//要添加的列表
        for (var i = 0; i < targetIds.Count; i++)
        {
            sysRelations.Add(new SysRelation
            {
                ObjectId = objectId,
                TargetId = targetIds[i],
                Category = category,
                ExtJson = extJsons == null ? null : extJsons[i]
            });
        }
        //事务
        var result = await Context.AsTenant().UseTranAsync(async () =>
        {
            if (clear)
                await DeleteAsync(it => it.ObjectId == objectId && it.Category == category);//删除老的
            await InsertRangeAsync(sysRelations);//添加新的
        });
        if (result.IsSuccess)//如果成功了
        {
            await RefreshCacheAsync(category);
        }
        else
        {
            throw new(result.ErrorMessage, result.ErrorException);
        }
    }

    /// <inheritdoc/>
    public async Task SaveRelationAsync(string category, long objectId, string targetId,
        string extJson, bool clear, bool refreshCache = true)
    {
        var sysRelation = new SysRelation
        {
            ObjectId = objectId,
            TargetId = targetId,
            Category = category,
            ExtJson = extJson
        };
        //事务
        var result = await Context.AsTenant().UseTranAsync(async () =>
        {
            if (clear)
                await DeleteAsync(it => it.ObjectId == objectId && it.Category == category);//删除老的
            await InsertAsync(sysRelation);//添加新的
        });
        if (result.IsSuccess)//如果成功了
        {
            if (refreshCache)
                await RefreshCacheAsync(category);
        }
        else
        {
            //写日志
            throw new(result.ErrorMessage, result.ErrorException);
        }
    }
}