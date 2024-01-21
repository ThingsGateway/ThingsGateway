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
/// 角色服务
/// </summary>
public interface IRoleService : ISugarService, ITransient
{
    /// <summary>
    /// 获取所有角色
    /// </summary>
    /// <returns></returns>
    Task<List<SysRole>> GetListAsync();

    /// <summary>
    /// 添加角色
    /// </summary>
    /// <param name="input">添加参数</param>
    /// <returns></returns>
    Task AddAsync(RoleAddInput input);

    /// <summary>
    /// 删除角色
    /// </summary>
    /// <param name="input">删除参数</param>
    /// <returns></returns>
    Task DeleteAsync(List<BaseIdInput> input);

    /// <summary>
    /// 编辑角色
    /// </summary>
    /// <param name="input">编辑参数</param>
    /// <returns></returns>
    Task EditAsync(RoleEditInput input);

    /// <summary>
    /// 根据用户ID获取用户角色集合
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns></returns>
    Task<List<SysRole>> GetRoleListByUserIdAsync(long userId);

    /// <summary>
    /// 给角色授权权限
    /// </summary>
    /// <param name="input">授权信息</param>
    /// <returns></returns>
    Task GrantPermissionAsync(GrantPermissionInput input);

    /// <summary>
    /// 给角色授权OPENAPI权限
    /// </summary>
    /// <param name="input">授权信息</param>
    /// <returns></returns>
    Task ApiGrantPermissionAsync(GrantPermissionInput input);

    /// <summary>
    /// 给角色授权
    /// </summary>
    /// <param name="input">授权参数</param>
    /// <returns></returns>
    Task GrantResourceAsync(GrantResourceInput input);

    /// <summary>
    /// 给角色授权用户
    /// </summary>
    /// <param name="input">授权信息</param>
    /// <returns></returns>
    Task GrantUserAsync(GrantUserInput input);

    /// <summary>
    /// 获取角色拥有权限
    /// </summary>
    /// <param name="input">角色ID</param>
    /// <returns></returns>
    Task<RoleOwnPermissionOutput> OwnPermissionAsync(BaseIdInput input);

    /// <summary>
    /// 获取角色拥有OPENAPI权限
    /// </summary>
    /// <param name="input">角色ID</param>
    /// <returns></returns>
    Task<RoleOwnPermissionOutput> ApiOwnPermissionAsync(BaseIdInput input);

    /// <summary>
    /// 角色拥有资源
    /// </summary>
    /// <param name="input">角色id</param>
    /// <param name="category">资源类型</param>
    /// <returns>角色拥有资源信息</returns>
    Task<RoleOwnResourceOutput> OwnResourceAsync(BaseIdInput input, string category = CateGoryConst.Relation_SYS_ROLE_HAS_RESOURCE);

    /// <summary>
    /// 获取角色下的用户
    /// </summary>
    /// <param name="input">角色ID</param>
    /// <returns></returns>
    Task<List<long>> OwnUserAsync(BaseIdInput input);

    /// <summary>
    /// 分页查询角色
    /// </summary>
    /// <param name="input">查询参数</param>
    /// <returns></returns>
    Task<SqlSugarPagedList<SysRole>> PageAsync(RolePageInput input);

    /// <summary>
    /// 刷新缓存
    /// </summary>
    /// <returns></returns>
    Task RefreshCacheAsync();

    /// <summary>
    /// 获取角色授权权限选择器
    /// </summary>
    /// <param name="input">角色ID</param>
    /// <returns></returns>
    Task<List<PermissionTreeSelector>> RolePermissionTreeSelectorAsync(BaseIdInput input);

    /// <summary>
    /// 角色选择器
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<SqlSugarPagedList<SysRole>> RoleSelectorAsync(RoleSelectorInput input);

    /// <summary>
    /// 根据id集合获取角色集合
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<List<SysRole>> GetRoleListByIdListAsync(IdListInput input);
}