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
/// 用户服务
/// </summary>
public interface ISysUserService : ITransient
{
    /// <summary>
    /// 添加用户
    /// </summary>
    /// <param name="input">添加参数</param>
    /// <returns></returns>
    Task AddAsync(UserAddInput input);

    /// <summary>
    /// 删除用户
    /// </summary>
    /// <param name="input">Id列表</param>
    /// <returns></returns>
    Task DeleteAsync(params long[] input);

    /// <summary>
    /// 从cache中删除用户信息
    /// </summary>
    /// <param name="ids">用户ID列表</param>
    void DeleteUserFromCache(params long[] ids);


    /// <summary>
    /// 禁用用户
    /// </summary>
    /// <param name="input">用户Id</param>
    /// <returns></returns>
    Task DisableUserAsync(long input);

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
    Task EnableUserAsync(long input);

    /// <summary>
    /// 根据用户ID获取按钮ID集合
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<List<string>> GetButtonCodeListAsync(long userId);

    /// <summary>
    ///根据用户账号获取用户ID
    /// </summary>
    /// <param name="account">用户账号</param>
    /// <returns></returns>
    Task<long> GetIdByAccountAsync(string account);

    /// <summary>
    /// 根据账号获取用户信息
    /// </summary>
    /// <param name="account">用户名</param>
    /// <returns>用户信息</returns>
    Task<SysUser> GetUserByAccountAsync(string account);
    /// <summary>
    /// 根据ID获取用户信息
    /// </summary>
    /// <param name="Id">用户ID</param>
    /// <returns>用户信息</returns>
    Task<SysUser> GetUserByIdAsync(long Id);

    /// <summary>
    /// 给用户授权角色
    /// </summary>
    /// <param name="input">授权参数</param>
    /// <returns></returns>
    Task GrantRoleAsync(UserGrantRoleInput input);

    /// <summary>
    /// 获取用户拥有角色
    /// </summary>
    /// <param name="input">用户ID</param>
    /// <returns></returns>
    Task<List<long>> OwnRoleAsync(BaseIdInput input);

    /// <summary>
    /// 用户分页查询
    /// </summary>
    /// <param name="input">查询参数</param>
    /// <returns>用户分页列表</returns>
    Task<ISqlSugarPagedList<SysUser>> PageAsync(UserPageInput input);

    /// <summary>
    /// 重置密码
    /// </summary>
    /// <param name="input">用户Id</param>
    /// <returns></returns>
    Task ResetPasswordAsync(long input);

    /// <summary>
    /// 用户选择器
    /// </summary>
    /// <returns></returns>
    Task<List<UserSelectorOutput>> UserSelectorAsync(string searchKey);
}