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

using Furion.FriendlyException;

using Microsoft.Extensions.DependencyInjection;

namespace ThingsGateway.Admin.Application;

/// <inheritdoc cref="IRelationService"/>
public class RelationService : DbRepository<SysRelation>, IRelationService
{
    private readonly IServiceScope _serviceScope;
    public RelationService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScope = serviceScopeFactory.CreateScope();
    }
    /// <inheritdoc/>
    public async Task<List<SysRelation>> GetRelationByCategoryAsync(string category)
    {
        //先从Cache拿，需要获取新的对象，避免操作导致缓存中对象改变
        var sysRelations = _serviceScope.ServiceProvider.GetService<MemoryCache>().Get<List<SysRelation>>(CacheConst.CACHE_SYSRELATION + category, true);
        if (sysRelations == null)
        {
            //cache没有就去数据库拿
            sysRelations = await GetListAsync(it => it.Category == category);
            if (sysRelations.Count > 0)
            {
                //插入Cache
                _serviceScope.ServiceProvider.GetService<MemoryCache>().Set(CacheConst.CACHE_SYSRELATION + category, sysRelations, true);
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
    public async Task<SysRelation> GetWorkbenchAsync(long userId)
    {
        var sysRelations = await GetRelationByCategoryAsync(CateGoryConst.Relation_SYS_USER_WORKBENCH_DATA);
        var result = sysRelations.FirstOrDefault(it => it.ObjectId == userId);//获取个人工作台
        return result;
    }

    /// <inheritdoc/>
    public void RefreshCache(string category)
    {
        _serviceScope.ServiceProvider.GetService<MemoryCache>().Remove(CacheConst.CACHE_SYSRELATION + category);//删除cache
    }

    /// <inheritdoc/>
    public async Task SaveRelationAsync(string category, long objectId, string targetId, string extJson, bool clear, bool refreshCache = true)
    {
        var sysRelation = new SysRelation
        {
            ObjectId = objectId,
            TargetId = targetId,
            Category = category,
            ExtJson = extJson
        };
        //事务
        var result = await itenant.UseTranAsync(async () =>
        {
            if (clear)
                await DeleteAsync(it => it.ObjectId == objectId && it.Category == category);//删除老的
            await InsertAsync(sysRelation);//添加新的
        });
        if (result.IsSuccess)//如果成功了
        {
            if (refreshCache)
                RefreshCache(category);
        }
        else
        {
            //写日志
            throw Oops.Oh(result.ErrorMessage);
        }
    }

    /// <inheritdoc/>
    public async Task SaveRelationBatchAsync(string category, long objectId, List<string> targetIds, List<string> extJsons, bool clear)
    {
        var sysRelations = new List<SysRelation>();//要添加的列表
        for (int i = 0; i < targetIds.Count; i++)
        {
            sysRelations.Add(new SysRelation
            {
                ObjectId = objectId,
                TargetId = targetIds[i],
                Category = category,
                ExtJson = extJsons?[i]
            });
        }

        var result = await itenant.UseTranAsync(async () =>
        {
            if (clear)
                await DeleteAsync(it => it.ObjectId == objectId && it.Category == category);//删除老的
            await InsertRangeAsync(sysRelations);//添加新的
        });
        if (result.IsSuccess)//如果成功了
        {
            RefreshCache(category);
        }
        else
        {
            throw Oops.Oh(result.ErrorMessage);
        }
    }
}