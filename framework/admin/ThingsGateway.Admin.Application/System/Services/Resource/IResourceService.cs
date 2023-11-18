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

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 资源服务
/// </summary>
public interface IResourceService : ITransient
{
    /// <summary>
    /// 获取所有的菜单和模块以及单页面列表，并按分类和排序码排序,不会形成树列表
    /// </summary>
    /// <returns>所有的菜单和模块以及单页面列表</returns>
    Task<List<SysResource>> GetaMenuAndSpaListAsync();
    /// <summary>
    /// 获取子资源
    /// </summary>
    /// <param name="sysResources"></param>
    /// <param name="resId"></param>
    /// <param name="isContainOneself"></param>
    /// <returns></returns>
    List<SysResource> GetChildListById(List<SysResource> sysResources, long resId, bool isContainOneself = true);

    /// <summary>
    /// 获取ID获取Code列表
    /// </summary>
    /// <param name="ids">id列表</param>
    /// <param name="category">分类</param>
    /// <returns>Code列表</returns>
    Task<List<string>> GetCodeByIdsAsync(List<long> ids, ResourceCategoryEnum category);

    /// <summary>
    /// 根据分类获取资源列表
    /// </summary>
    /// <param name="category">分类名称</param>
    /// <returns>资源列表</returns>
    Task<List<SysResource>> GetListByCategoryAsync(ResourceCategoryEnum category);

    /// <summary>
    /// 资源分类列表,如果是空的则获取全部资源
    /// </summary>
    /// <param name="categorys">资源分类列表</param>
    /// <returns></returns>
    Task<List<SysResource>> GetListByCategorysAsync(List<ResourceCategoryEnum> categorys = null);
    /// <summary>
    /// 获取资源所有下级
    /// </summary>
    /// <param name="resourceList">资源列表</param>
    /// <param name="parentId">父ID</param>
    /// <returns></returns>
    List<SysResource> GetResourceChilden(List<SysResource> resourceList, long parentId);

    /// <summary>
    /// 获取上级
    /// </summary>
    /// <returns></returns>
    List<SysResource> GetResourceParent(List<SysResource> resourceList, long parentId);

    /// <summary>
    /// 获取授权菜单
    /// </summary>
    /// <returns></returns>
    Task<List<RoleGrantResourceMenu>> GetRoleGrantResourceMenusAsync();

    /// <summary>
    /// 刷新缓存
    /// </summary>
    /// <param name="category">分类名称</param>
    /// <returns></returns>
    void RefreshCache(ResourceCategoryEnum category);

    /// <summary>
    /// 构建菜单树形结构
    /// </summary>
    /// <param name="resourceList">菜单列表</param>
    /// <param name="parentId">父ID</param>
    /// <returns>菜单形结构</returns>
    /// <inheritdoc/>
    List<SysResource> ResourceListToTree(List<SysResource> resourceList, long parentId = 0);
    /// <summary>
    /// 多个树转列表
    /// </summary>
    /// <param name="data"></param>
    List<SysResource> ResourceTreeToList(List<SysResource> data);

}