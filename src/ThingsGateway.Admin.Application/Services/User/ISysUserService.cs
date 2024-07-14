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
/// 用户服务接口，提供用户相关操作方法。
/// </summary>
public interface ISysUserService
{
    /// <summary>
    /// 获取用户拥有的OpenAPI权限。
    /// </summary>
    /// <param name="id">用户ID。</param>
    /// <returns>用户拥有的OpenAPI权限。</returns>
    Task<GrantPermissionData> ApiOwnPermissionAsync(long id);

    /// <summary>
    /// 删除用户。
    /// </summary>
    /// <param name="ids">用户ID列表。</param>
    /// <returns>是否删除成功。</returns>
    Task<bool> DeleteUserAsync(IEnumerable<long> ids);

    /// <summary>
    /// 从缓存中删除用户信息。
    /// </summary>
    /// <param name="userId">用户ID。</param>
    void DeleteUserFromCache(long userId);

    /// <summary>
    /// 从缓存中删除多个用户信息。
    /// </summary>
    /// <param name="ids">用户ID列表。</param>
    void DeleteUserFromCache(IEnumerable<long> ids);

    /// <summary>
    /// 获取用户拥有的按钮编码。
    /// </summary>
    /// <param name="userId">用户ID。</param>
    /// <returns>以菜单链接为键，按钮编码列表为值的字典。</returns>
    Task<Dictionary<string, List<string>>> GetButtonCodeListAsync(long userId);

    /// <summary>
    /// 根据账号获取用户ID。
    /// </summary>
    /// <param name="account">账号。</param>
    /// <returns>用户ID，如果用户不存在则返回0。</returns>
    Task<long> GetIdByAccountAsync(string account);

    /// <summary>
    /// 获取用户拥有的权限。
    /// </summary>
    /// <param name="userId">用户ID。</param>
    /// <returns>权限列表。</returns>
    Task<IEnumerable<DataScope>> GetPermissionListByUserIdAsync(long userId);

    /// <summary>
    /// 根据账号获取用户信息。
    /// </summary>
    /// <param name="account">账号。</param>
    /// <returns>用户信息，如果用户不存在则返回null。</returns>
    Task<SysUser?> GetUserByAccountAsync(string account);

    /// <summary>
    /// 根据用户ID获取用户信息。
    /// </summary>
    /// <param name="userId">用户ID。</param>
    /// <returns>用户信息，如果用户不存在则返回null。</returns>
    Task<SysUser?> GetUserByIdAsync(long userId);

    /// <summary>
    /// 根据用户ID列表获取用户列表。
    /// </summary>
    /// <param name="input">用户ID列表。</param>
    /// <returns>用户列表。</returns>
    Task<List<UserSelectorOutput>> GetUserListByIdListAsync(IEnumerable<long> input);

    /// <summary>
    /// 授予用户OpenAPI权限。
    /// </summary>
    /// <param name="input">授权信息。</param>
    /// <returns>异步任务。</returns>
    Task GrantApiPermissionAsync(GrantPermissionData input);

    /// <summary>
    /// 授予用户资源。
    /// </summary>
    /// <param name="input">授权信息。</param>
    /// <returns>异步任务。</returns>
    Task GrantResourceAsync(GrantResourceData input);

    /// <summary>
    /// 授予用户角色。
    /// </summary>
    /// <param name="input">授权信息。</param>
    /// <returns>异步任务。</returns>
    Task GrantRoleAsync(GrantUserOrRoleInput input);

    /// <summary>
    /// 获取用户拥有的资源。
    /// </summary>
    /// <param name="id">用户ID。</param>
    /// <returns>用户拥有的资源数据。</returns>
    Task<GrantResourceData> OwnResourceAsync(long id);

    /// <summary>
    /// 获取用户拥有的角色ID列表。
    /// </summary>
    /// <param name="id">用户ID。</param>
    /// <returns>角色ID列表。</returns>
    Task<IEnumerable<long>> OwnRoleAsync(long id);

    /// <summary>
    /// 表格查询用户信息。
    /// </summary>
    /// <param name="option">查询选项。</param>
    /// <returns>用户信息列表。</returns>
    Task<QueryData<SysUser>> PageAsync(QueryPageOptions option);

    /// <summary>
    /// 重置用户密码。
    /// </summary>
    /// <param name="id">用户ID。</param>
    /// <returns>异步任务。</returns>
    Task ResetPasswordAsync(long id);

    /// <summary>
    /// 保存用户信息。
    /// </summary>
    /// <param name="input">用户信息。</param>
    /// <param name="changedType">变更类型。</param>
    /// <returns>是否保存成功。</returns>
    Task<bool> SaveUserAsync(SysUser input, ItemChangedType changedType);
}
