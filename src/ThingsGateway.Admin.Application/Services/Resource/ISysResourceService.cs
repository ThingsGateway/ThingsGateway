
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
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
    /// 从缓存/数据库读取全部资源列表
    /// </summary>
    /// <returns>全部资源列表</returns>
    Task<List<SysResource>> GetAllAsync();

    /// <summary>
    /// 表格查询
    /// </summary>
    /// <param name="options">查询条件</param>
    /// <param name="searchModel">查询条件</param>
    /// <returns></returns>
    Task<QueryData<SysResource>> PageAsync(QueryPageOptions options, ResourceSearchInput searchModel);

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
    /// 保存资源
    /// </summary>
    /// <param name="input">资源</param>
    /// <param name="type">保存类型</param>
    Task<bool> SaveResourceAsync(SysResource input, ItemChangedType type);

    /// <summary>
    /// 删除资源
    /// </summary>
    /// <param name="ids">id列表</param>
    /// <returns></returns>
    Task<bool> DeleteResourceAsync(IEnumerable<long> ids);

    /// <summary>
    /// 刷新缓存
    /// </summary>
    void RefreshCache();
}