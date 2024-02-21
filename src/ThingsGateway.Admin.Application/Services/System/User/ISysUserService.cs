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
/// 用户服务
/// </summary>
public partial interface ISysUserService : ISugarService, ITransient
{
    #region 查询

    /// <summary>
    /// 根据用户ID获取按钮ID集合
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<List<string>> GetButtonCodeListAsync(long userId);

    /// <summary>
    /// 根据账号获取用户信息
    /// </summary>
    /// <param name="account">用户名</param>
    /// <returns>用户信息</returns>
    Task<SysUser> GetUserByAccountAsync(string account);

    /// <summary>
    /// 根据用户ID获取角色权限
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="orgId"></param>
    /// <returns></returns>
    Task<List<DataScope>> GetPermissionListByUserIdAsync(long userId);

    /// <summary>
    /// 根据手机号获取用户账号
    /// </summary>
    /// <param name="phone">手机号</param>
    /// <returns>用户账号名称</returns>
    Task<long> GetIdByPhoneAsync(string phone);

    /// <summary>
    /// 用户选择器
    /// </summary>
    /// <param name="input">查询参数</param>
    /// <returns></returns>
    Task<SqlSugarPagedList<UserSelectorOutput>> UserSelectorAsync(UserSelectorInput input);

    /// <summary>
    /// 用户分页查询
    /// </summary>
    /// <param name="input">查询参数</param>
    /// <returns>用户分页列表</returns>
    Task<SqlSugarPagedList<SysUser>> PageAsync(UserPageInput input);

    /// <summary>
    /// 用户列表
    /// </summary>
    /// <param name="input">查询</param>
    /// <returns></returns>
    Task<List<SysUser>> ListAsync(UserPageInput input);

    /// <summary>
    /// 根据用户Id获取用户信息
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>用户信息</returns>
    Task<SysUser> GetUserByIdAsync(long userId);

    /// <summary>
    /// 根据用户Id获取用户信息
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <typeparam name="T">转换的实体</typeparam>
    /// <returns></returns>
    Task<T> GetUserByIdAsync<T>(long userId);

    /// <summary>
    ///根据用户账号获取用户ID
    /// </summary>
    /// <param name="account">用户账号</param>
    /// <returns></returns>
    Task<long> GetIdByAccountAsync(string account);

    /// <summary>
    /// 根据用户手机获取用户信息
    /// </summary>
    /// <param name="phone">手机号</param>
    /// <returns>用户信息</returns>
    Task<SysUser> GetUserByPhoneAsync(string phone);

    /// <summary>
    /// 获取用户拥有角色
    /// </summary>
    /// <param name="input">用户ID</param>
    /// <returns></returns>
    Task<List<long>> OwnRoleAsync(BaseIdInput input);

    /// <summary>
    /// 获取用户拥有的资源
    /// </summary>
    /// <param name="input">用户id</param>
    /// <returns>资源列表</returns>
    Task<RoleOwnResourceOutput> OwnResourceAsync(BaseIdInput input);

    /// <summary>
    /// 获取用户拥有的权限
    /// </summary>
    /// <param name="input">用户id</param>
    /// <returns>权限列表</returns>
    Task<RoleOwnPermissionOutput> OwnPermissionAsync(BaseIdInput input);

    /// <summary>
    /// 获取用户拥有的OPENAPI权限
    /// </summary>
    /// <param name="input">用户id</param>
    /// <returns>权限列表</returns>
    Task<RoleOwnPermissionOutput> ApiOwnPermissionAsync(BaseIdInput input);

    /// <summary>
    /// 用户权限树选择
    /// </summary>
    /// <param name="input">用户id</param>
    /// <returns>权限列表</returns>
    Task<List<string>> UserPermissionTreeSelectorAsync(BaseIdInput input);

    /// <summary>
    /// 根据id集合获取用户集合
    /// </summary>
    /// <param name="input">Id集合</param>
    /// <returns></returns>
    Task<List<UserSelectorOutput>> GetUserListByIdListAsync(IdListInput input);

    #endregion 查询

    #region 新增

    /// <summary>
    /// 添加用户
    /// </summary>
    /// <param name="input">添加参数</param>
    /// <returns></returns>
    Task AddAsync(UserAddInput input);

    #endregion 新增

    #region 编辑

    /// <summary>
    /// 编辑
    /// </summary>
    /// <param name="input">编辑参数</param>
    /// <returns></returns>
    Task EditAsync(UserEditInput input);

    /// <summary>
    /// 启用用户
    /// </summary>
    /// <param name="input">用户Id</param>
    /// <returns></returns>
    Task EnableUserAsync(BaseIdInput input);

    /// <summary>
    /// 禁用用户
    /// </summary>
    /// <param name="input">用户Id</param>
    /// <returns></returns>
    Task DisableUserAsync(BaseIdInput input);

    /// <summary>
    /// 重置密码
    /// </summary>
    /// <param name="input">用户Id</param>
    /// <returns></returns>
    Task ResetPasswordAsync(BaseIdInput input);

    /// <summary>
    /// 给授权用户角色
    /// </summary>
    /// <param name="input">授权参数</param>
    /// <returns></returns>
    Task GrantRoleAsync(UserGrantRoleInput input);

    /// <summary>
    /// 给授权用户资源
    /// </summary>
    /// <param name="input">授权参数</param>
    /// <returns></returns>
    Task GrantResourceAsync(UserGrantResourceInput input);

    /// <summary>
    /// 给授权用户权限
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task GrantPermissionAsync(GrantPermissionInput input);

    /// <summary>
    /// 给授权用户OPENAPI权限
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task ApiGrantPermissionAsync(GrantPermissionInput input);

    /// <summary>
    /// 设置用户默认值
    /// </summary>
    /// <param name="sysUsers"></param>
    /// <returns></returns>
    Task SetUserDefaultAsync(List<SysUser> sysUsers);

    #endregion 编辑

    #region 删除

    /// <summary>
    /// 删除用户
    /// </summary>
    /// <param name="input">Id列表</param>
    /// <returns></returns>
    Task DeleteAsync(List<BaseIdInput> input);

    /// <summary>
    /// 从redis中删除用户信息
    /// </summary>
    /// <param name="ids">用户ID列表</param>
    void DeleteUserFromRedis(List<long> ids);

    /// <summary>
    /// 从redis中删除用户信息
    /// </summary>
    /// <param name="userId">用户ID</param>
    void DeleteUserFromRedis(long userId);

    #endregion 删除
}