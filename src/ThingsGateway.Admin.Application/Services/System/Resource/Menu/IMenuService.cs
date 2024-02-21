//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 菜单服务
/// </summary>
public interface IMenuService : ISugarService, ITransient
{
    /// <summary>
    /// 添加菜单
    /// </summary>
    /// <param name="input">添加参数</param>
    /// <returns></returns>
    Task AddAsync(MenuAddInput input);

    /// <summary>
    /// 构建菜单树形结构
    /// </summary>
    /// <param name="resourceList">菜单列表</param>
    /// <param name="parentId">父ID</param>
    /// <returns>菜单形结构</returns>
    List<SysResource> ConstructMenuTrees(List<SysResource> resourceList, long parentId = 0);

    /// <summary>
    /// 获取菜单树
    /// </summary>
    /// <param name="input">菜单树查询参数</param>
    /// <returns>菜单树列表</returns>
    Task<List<SysResource>> GetListAsync(MenuPageInput input);

    /// <summary>
    /// 编辑菜单
    /// </summary>
    /// <param name="input">菜单编辑参数</param>
    /// <returns></returns>
    Task EditAsync(MenuEditInput input);

    /// <summary>
    /// 删除菜单
    /// </summary>
    /// <param name="input">删除菜单参数</param>
    /// <returns></returns>
    Task DeleteAsync(List<BaseIdInput> input);
}