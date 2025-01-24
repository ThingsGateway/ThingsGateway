//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 资源服务接口，定义了资源相关操作的接口方法
/// </summary>
public interface ISysResourceService
{
    /// <summary>
    /// 更改父级
    /// </summary>
    /// <param name="id"></param>
    /// <param name="parentMenuId"></param>
    /// <returns></returns>
    Task ChangeParentAsync(long id, long parentMenuId);

    /// <summary>
    /// 构造树形
    /// </summary>
    /// <param name="resourceList">资源列表</param>
    /// <param name="parentId">父ID</param>
    /// <returns></returns>
    IEnumerable<SysResource> ConstructMenuTrees(IEnumerable<SysResource> resourceList, long parentId = 0);


    /// <summary>
    /// 复制资源到其他模块
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="moduleId"></param>
    /// <returns></returns>
    Task CopyAsync(IEnumerable<long> ids, long moduleId);

    /// <summary>
    /// 删除资源
    /// </summary>
    /// <param name="ids">id列表</param>
    /// <returns></returns>
    Task<bool> DeleteResourceAsync(IEnumerable<long> ids);

    /// <summary>
    /// 从缓存/数据库读取全部资源列表
    /// </summary>
    /// <returns>全部资源列表</returns>
    Task<List<SysResource>> GetAllAsync();

    /// <summary>
    /// 根据菜单Id获取菜单列表
    /// </summary>
    /// <param name="menuIds">菜单id列表</param>
    /// <returns>菜单列表</returns>
    Task<IEnumerable<SysResource>> GetMenuByMenuIdsAsync(IEnumerable<long> menuIds);

    /// <summary>
    /// 根据模块Id获取模块列表
    /// </summary>
    /// <param name="moduleIds">模块id列表</param>
    /// <returns>菜单列表</returns>
    Task<IEnumerable<SysResource>> GetMuduleByMuduleIdsAsync(IEnumerable<long> moduleIds);

    /// <summary>
    /// 获取父菜单集合
    /// </summary>
    /// <param name="allMenuList">所有菜单列表</param>
    /// <param name="myMenus">我的菜单列表</param>
    /// <returns></returns>
    IEnumerable<SysResource> GetMyParentResources(IEnumerable<SysResource> allMenuList, IEnumerable<SysResource> myMenus);

    /// <summary>
    /// 获取资源所有下级，结果不会转为树形
    /// </summary>
    /// <param name="resourceList">资源列表</param>
    /// <param name="parentId">父Id</param>
    /// <returns></returns>
    IEnumerable<SysResource> GetResourceChilden(IEnumerable<SysResource> resourceList, long parentId);

    /// <summary>
    /// 获取资源所有父级，结果不会转为树形
    /// </summary>
    /// <param name="resourceList">资源列表</param>
    /// <param name="resourceId">Id</param>
    /// <returns></returns>
    IEnumerable<SysResource> GetResourceParent(IEnumerable<SysResource> resourceList, long resourceId);

    /// <summary>
    /// 表格查询
    /// </summary>
    /// <param name="options">查询条件</param>
    /// <param name="searchModel">查询条件</param>
    /// <returns></returns>
    Task<QueryData<SysResource>> PageAsync(QueryPageOptions options, ResourceTableSearchModel searchModel);

    /// <summary>
    /// 刷新缓存
    /// </summary>
    void RefreshCache();

    /// <summary>
    /// 保存资源
    /// </summary>
    /// <param name="input">资源</param>
    /// <param name="type">保存类型</param>
    Task<bool> SaveResourceAsync(SysResource input, ItemChangedType type);
}
