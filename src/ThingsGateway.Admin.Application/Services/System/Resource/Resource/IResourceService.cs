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

/// <summary>
/// 资源服务
/// </summary>
public interface IResourceService : ISugarService, ITransient
{
    /// <summary>
    /// 获取所有的菜单以及单页面列表，并按分类和排序码排序
    /// </summary>
    /// <returns>所有的菜单和模块以及单页面列表</returns>
    Task<List<SysResource>> GetMenuAndSpaListAsync();

    /// <summary>
    /// 根据资源ID获取所有下级资源
    /// </summary>
    /// <param name="resId">资源ID</param>
    /// <param name="isContainOneself">是否包含自己</param>
    /// <returns>资源列表</returns>
    Task<List<SysResource>> GetChildListByIdAsync(long resId, bool isContainOneself = true);

    /// <summary>
    /// 根据资源ID获取所有下级资源
    /// </summary>
    /// <param name="sysResources">资源列表</param>
    /// <param name="resId">资源ID</param>
    /// <param name="isContainOneself">是否包含自己</param>
    /// <returns>资源列表</returns>
    List<SysResource> GetChildListById(List<SysResource> sysResources, long resId, bool isContainOneself = true);

    /// <summary>
    /// 获取ID获取Code列表
    /// </summary>
    /// <param name="ids">id列表</param>
    /// <param name="category">分类</param>
    /// <returns>Code列表</returns>
    Task<List<string>> GetCodeByIdsAsync(List<long> ids, string category);

    /// <summary>
    /// 获取资源列表
    /// </summary>
    /// <param name="categorys">资源分类列表</param>
    /// <returns></returns>
    Task<List<SysResource>> GetListAsync(List<string>? categorys = null);

    /// <summary>
    /// 根据分类获取资源列表
    /// </summary>
    /// <param name="category">分类名称</param>
    /// <returns>资源列表</returns>
    Task<List<SysResource>> GetListByCategoryAsync(string category);

    /// <summary>
    /// 根据菜单ID获取菜单
    /// </summary>
    /// <param name="menuIds">菜单id列表</param>
    /// <returns>菜单列表</returns>
    Task<List<SysResource>> GetMenuByMenuIdsAsync(List<long> menuIds);

    /// <summary>
    /// 获取权限授权树
    /// </summary>
    /// <param name="routes">路由列表</param>
    /// <returns></returns>
    List<PermissionTreeSelector> PermissionTreeSelector(List<string> routes);

    /// <summary>
    /// 获取API权限授权树
    /// </summary>
    /// <param name="routes">路由列表</param>
    /// <returns></returns>
    List<OpenApiPermissionTreeSelector> ApiPermissionTreeSelector();

    /// <summary>
    /// 刷新缓存
    /// </summary>
    /// <param name="category">分类名称</param>
    /// <returns></returns>
    Task RefreshCacheAsync(string category = null);

    /// <summary>
    /// 角色授权资源树
    /// </summary>
    /// <returns></returns>
    Task<ResTreeSelector> ResourceTreeSelectorAsync();

    /// <summary>
    /// 获取上级
    /// </summary>
    /// <param name="resourceList"></param>
    /// <param name="parentId"></param>
    /// <returns></returns>
    List<SysResource> GetResourceParent(List<SysResource> resourceList, long parentId);
}