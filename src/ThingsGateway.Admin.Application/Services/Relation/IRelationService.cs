//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Admin.Application;

public interface IRelationService
{
    /// <summary>
    /// 根据分类获取关系表信息
    /// </summary>
    /// <param name="category">分类名称</param>
    /// <returns>关系表</returns>
    Task<List<SysRelation>> GetRelationByCategoryAsync(RelationCategoryEnum category);

    /// <summary>
    /// 通过对象ID和分类获取关系列表
    /// </summary>
    /// <param name="objectId">对象ID</param>
    /// <param name="category">分类</param>
    /// <returns>关系表</returns>
    Task<IEnumerable<SysRelation>> GetRelationListByObjectIdAndCategoryAsync(long objectId, RelationCategoryEnum category);

    /// <summary>
    /// 通过对象ID列表和分类获取关系列表
    /// </summary>
    /// <param name="objectIds">对象ID</param>
    /// <param name="category">分类</param>
    /// <returns>关系表</returns>
    Task<IEnumerable<SysRelation>> GetRelationListByObjectIdListAndCategoryAsync(IEnumerable<long> objectIds, RelationCategoryEnum category);

    /// <summary>
    /// 通过目标ID和分类获取关系列表
    /// </summary>
    /// <param name="targetId">目标ID</param>
    /// <param name="category">分类</param>
    /// <returns>关系表</returns>
    Task<IEnumerable<SysRelation>> GetRelationListByTargetIdAndCategoryAsync(string targetId, RelationCategoryEnum category);

    /// <summary>
    /// 通过目标ID列表和分类获取关系列表
    /// </summary>
    /// <param name="targetIds"></param>
    /// <param name="category"></param>
    /// <returns>关系表</returns>
    Task<IEnumerable<SysRelation>> GetRelationListByTargetIdListAndCategoryAsync(IEnumerable<string> targetIds, RelationCategoryEnum category);

    /// <summary>
    /// 获取用户模块ID
    /// </summary>
    /// <param name="roleIdList">角色id列表</param>
    /// <param name="userId">用户id</param>
    /// <returns></returns>
    Task<IEnumerable<long>> GetUserModuleId(IEnumerable<long> roleIdList, long userId);

    /// <summary>
    /// 更新缓存
    /// </summary>
    /// <param name="category">分类</param>
    void RefreshCache(RelationCategoryEnum category);

    /// <summary>
    /// 保存关系
    /// </summary>
    /// <param name="category">分类</param>
    /// <param name="objectId">对象ID</param>
    /// <param name="targetId">目标ID</param>
    /// <param name="extJson">拓展信息</param>
    /// <param name="clear">是否清除老的数据</param>
    /// <param name="refreshCache">是否刷新缓存</param>
    Task SaveRelationAsync(RelationCategoryEnum category, long objectId, string? targetId, string extJson, bool clear, bool refreshCache = true);

    /// <summary>
    /// 保存关系
    /// </summary>
    /// <param name="category">分类</param>
    /// <param name="objectId">对象ID</param>
    /// <param name="targetIdAndExtJsons">目标ID和拓展信息</param>
    /// <param name="clear">是否清除老的数据</param>
    Task SaveRelationBatchAsync(RelationCategoryEnum category, long objectId, IEnumerable<(string targetId, string extJson)> targetIdAndExtJsons, bool clear);
}
