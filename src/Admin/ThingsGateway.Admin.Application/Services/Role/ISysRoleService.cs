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

using SqlSugar;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 角色服务接口
/// </summary>
public interface ISysRoleService
{
    /// <summary>
    /// 获取角色拥有的OpenApi权限
    /// </summary>
    /// <param name="id">角色id</param>
    Task<GrantPermissionData> ApiOwnPermissionAsync(long id);

    /// <summary>
    /// 获取角色树
    /// </summary>
    Task<List<RoleTreeOutput>> TreeAsync();

    /// <summary>
    /// 删除角色
    /// </summary>
    /// <param name="ids">id列表</param>
    Task<bool> DeleteRoleAsync(IEnumerable<long> ids);

    /// <summary>
    /// 从缓存/数据库获取全部角色信息
    /// </summary>
    /// <returns>角色列表</returns>
    Task<List<SysRole>> GetAllAsync();

    /// <summary>
    /// 根据角色id获取角色列表
    /// </summary>
    /// <param name="input">角色id列表</param>
    /// <returns>角色列表</returns>
    Task<IEnumerable<SysRole>> GetRoleListByIdListAsync(IEnumerable<long> input);

    /// <summary>
    /// 根据用户id获取角色列表
    /// </summary>
    /// <param name="userId">用户id</param>
    /// <returns>角色列表</returns>
    Task<IEnumerable<SysRole>> GetRoleListByUserIdAsync(long userId);

    /// <summary>
    /// 授权OpenApi权限
    /// </summary>
    /// <param name="input">授权信息</param>
    Task GrantApiPermissionAsync(GrantPermissionData input);

    /// <summary>
    /// 授权资源
    /// </summary>
    /// <param name="input">授权信息</param>
    Task GrantResourceAsync(GrantResourceData input);

    /// <summary>
    /// 授权用户
    /// </summary>
    /// <param name="input">授权参数</param>
    Task GrantUserAsync(GrantUserOrRoleInput input);

    /// <summary>
    /// 获取拥有的资源
    /// </summary>
    /// <param name="id">id</param>
    /// <param name="category">类型</param>
    Task<GrantResourceData> OwnResourceAsync(long id, RelationCategoryEnum category = RelationCategoryEnum.RoleHasResource);

    /// <summary>
    /// 获取角色的用户id列表
    /// </summary>
    /// <param name="id">角色id</param>
    /// <returns></returns>
    Task<IEnumerable<long>> OwnUserAsync(long id);

    /// <summary>
    /// 报表查询
    /// </summary>
    /// <param name="option">查询条件</param>
    /// <param name="queryFunc">查询条件</param>
    Task<QueryData<SysRole>> PageAsync(QueryPageOptions option, Func<ISugarQueryable<SysRole>, ISugarQueryable<SysRole>>? queryFunc = null);

    /// <summary>
    /// 刷新缓存
    /// </summary>
    void RefreshCache();

    /// <summary>
    /// 保存角色
    /// </summary>
    /// <param name="input">角色</param>
    /// <param name="type">保存类型</param>
    Task<bool> SaveRoleAsync(SysRole input, ItemChangedType type);
}
