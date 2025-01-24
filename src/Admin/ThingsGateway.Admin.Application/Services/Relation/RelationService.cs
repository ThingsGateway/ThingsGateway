//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Admin.Application;

internal sealed class RelationService : BaseService<SysRelation>, IRelationService
{
    #region 查询

    /// <summary>
    /// 根据分类获取关系表信息
    /// </summary>
    /// <param name="category">分类名称</param>
    /// <returns>关系表</returns>
    public async Task<List<SysRelation>> GetRelationByCategoryAsync(RelationCategoryEnum category)
    {
        var key = $"{CacheConst.Cache_SysRelation}{category}";
        var sysRelations = App.CacheService.Get<List<SysRelation>>(key);
        if (sysRelations == null)
        {
            using var db = GetDB();
            sysRelations = await db.Queryable<SysRelation>().Where(it => it.Category == category).ToListAsync().ConfigureAwait(false);
            App.CacheService.Set(key, sysRelations ?? new());//赋值空集合
        }

        return sysRelations;
    }

    /// <summary>
    /// 通过对象ID和分类获取关系列表
    /// </summary>
    /// <param name="objectId">对象ID</param>
    /// <param name="category">分类</param>
    /// <returns>关系表</returns>
    public async Task<IEnumerable<SysRelation>> GetRelationListByObjectIdAndCategoryAsync(long objectId, RelationCategoryEnum category)
    {
        var sysRelations = await GetRelationByCategoryAsync(category).ConfigureAwait(false);
        var result = sysRelations.Where(it => it.ObjectId == objectId);//获取关系集合
        return result;
    }

    /// <summary>
    /// 通过对象ID列表和分类获取关系列表
    /// </summary>
    /// <param name="objectIds">对象ID</param>
    /// <param name="category">分类</param>
    /// <returns>关系表</returns>
    public async Task<IEnumerable<SysRelation>> GetRelationListByObjectIdListAndCategoryAsync(IEnumerable<long> objectIds, RelationCategoryEnum category)
    {
        var sysRelations = await GetRelationByCategoryAsync(category).ConfigureAwait(false);
        var result = sysRelations.Where(it => objectIds.Contains(it.ObjectId));//获取关系集合
        return result;
    }

    /// <summary>
    /// 通过目标ID和分类获取关系列表
    /// </summary>
    /// <param name="targetId">目标ID</param>
    /// <param name="category">分类</param>
    /// <returns>关系表</returns>
    public async Task<IEnumerable<SysRelation>> GetRelationListByTargetIdAndCategoryAsync(string targetId, RelationCategoryEnum category)
    {
        var sysRelations = await GetRelationByCategoryAsync(category).ConfigureAwait(false);
        var result = sysRelations.Where(it => it.TargetId == targetId);//获取关系集合
        return result;
    }

    /// <summary>
    /// 通过目标ID列表和分类获取关系列表
    /// </summary>
    /// <param name="targetIds"></param>
    /// <param name="category"></param>
    /// <returns>关系表</returns>
    public async Task<IEnumerable<SysRelation>> GetRelationListByTargetIdListAndCategoryAsync(IEnumerable<string> targetIds, RelationCategoryEnum category)
    {
        var sysRelations = await GetRelationByCategoryAsync(category).ConfigureAwait(false);
        var result = sysRelations.Where(it => targetIds.Contains(it.TargetId));//获取关系集合
        return result;
    }

    /// <summary>
    /// 获取用户模块ID
    /// </summary>
    /// <param name="roleIdList">角色id列表</param>
    /// <param name="userId">用户id</param>
    /// <returns></returns>
    public async Task<IEnumerable<long>> GetUserModuleId(IEnumerable<long> roleIdList, long userId)
    {
        IEnumerable<long>? moduleIds = Enumerable.Empty<long>();
        var roleRelation = await GetRelationByCategoryAsync(RelationCategoryEnum.RoleHasModule).ConfigureAwait(false);//获取角色模块关系集合
        if (roleRelation?.Count > 0)
        {
            moduleIds = roleRelation.Where(it => roleIdList.Contains(it.ObjectId)).Select(it => it.TargetId.ToLong());
        }
        var userRelation = await GetRelationByCategoryAsync(RelationCategoryEnum.UserHasModule).ConfigureAwait(false);//获取用户模块关系集合
        var userModuleIds = userRelation.Where(it => it.ObjectId == userId).Select(it => it.TargetId.ToLong());
        if (userModuleIds.Any())
        {
            moduleIds = (userModuleIds);
        }
        return moduleIds;
    }

    #endregion 查询

    #region 保存

    /// <summary>
    /// 保存关系
    /// </summary>
    /// <param name="category">分类</param>
    /// <param name="objectId">对象ID</param>
    /// <param name="targetId">目标ID</param>
    /// <param name="extJson">拓展信息</param>
    /// <param name="clear">是否清除老的数据</param>
    /// <param name="refreshCache">是否刷新缓存</param>
    public async Task SaveRelationAsync(RelationCategoryEnum category, long objectId, string? targetId,
        string extJson, bool clear, bool refreshCache = true)
    {
        var sysRelation = new SysRelation
        {
            ObjectId = objectId,
            TargetId = targetId,
            Category = category,
            ExtJson = extJson
        };
        using var db = GetDB();
        //事务
        var result = await db.UseTranAsync(async () =>
        {
            if (clear)
                await db.Deleteable<SysRelation>().Where(it => it.ObjectId == objectId && it.Category == category).ExecuteCommandAsync().ConfigureAwait(false);//删除老的
            await db.Insertable(sysRelation).ExecuteCommandAsync().ConfigureAwait(false);//添加新的
        }).ConfigureAwait(false);
        if (result.IsSuccess)//如果成功了
        {
            if (refreshCache)
                RefreshCache(category);
        }
        else
        {
            throw new(result.ErrorMessage, result.ErrorException);
        }
    }

    /// <summary>
    /// 保存关系
    /// </summary>
    /// <param name="category">分类</param>
    /// <param name="objectId">对象ID</param>
    /// <param name="targetIdAndExtJsons">目标ID和拓展信息</param>
    /// <param name="clear">是否清除老的数据</param>
    public async Task SaveRelationBatchAsync(RelationCategoryEnum category, long objectId, IEnumerable<(string targetId, string extJson)> targetIdAndExtJsons, bool clear)
    {
        var sysRelations = targetIdAndExtJsons.Select(a => new SysRelation
        {
            ObjectId = objectId,
            TargetId = a.targetId,
            Category = category,
            ExtJson = a.extJson
        });

        using var db = GetDB();
        //事务
        var result = await db.UseTranAsync(async () =>
        {
            if (clear)
                await db.Deleteable<SysRelation>().Where(it => it.ObjectId == objectId && it.Category == category).ExecuteCommandAsync().ConfigureAwait(false);//删除老的
            await db.Insertable(sysRelations.ToList()).ExecuteCommandAsync().ConfigureAwait(false);//添加新的
        }).ConfigureAwait(false);
        if (result.IsSuccess)//如果成功了
        {
            RefreshCache(category);
        }
        else
        {
            throw new(result.ErrorMessage, result.ErrorException);
        }
    }

    #endregion 保存

    #region 缓存

    /// <summary>
    /// 更新缓存
    /// </summary>
    /// <param name="category">分类</param>
    public void RefreshCache(RelationCategoryEnum category)
    {
        var key = $"{CacheConst.Cache_SysRelation}{category}";
        App.CacheService.Remove(key);
    }

    #endregion 缓存
}
