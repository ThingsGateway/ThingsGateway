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
public interface IOpenApiUserService : ITransient
{
    /// <summary>
    /// 添加用户
    /// </summary>
    /// <param name="input">添加参数</param>
    /// <returns></returns>
    Task AddAsync(OpenApiUserAddInput input);

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
    Task EditAsync(OpenApiUserEditInput input);


    /// <summary>
    /// 启用用户
    /// </summary>
    /// <param name="input">用户Id</param>
    /// <returns></returns>
    Task EnableUserAsync(long input);

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
    Task<OpenApiUser> GetUserByAccountAsync(string account);
    /// <summary>
    /// 根据ID获取用户信息
    /// </summary>
    /// <param name="Id"></param>
    /// <returns></returns>
    Task<OpenApiUser> GetUsertByIdAsync(long Id);

    /// <summary>
    /// 给用户授权
    /// </summary>
    /// <param name="input">授权参数</param>
    /// <returns></returns>
    Task GrantRoleAsync(OpenApiUserGrantPermissionInput input);

    /// <summary>
    /// 获取用户拥有权限，返回的是服务方法名称
    /// </summary>
    /// <param name="input">用户ID</param>
    /// <returns></returns>
    Task<List<string>> OwnPermissionsAsync(BaseIdInput input);

    /// <summary>
    /// 用户分页查询
    /// </summary>
    /// <param name="input">查询参数</param>
    /// <returns>用户分页列表</returns>
    Task<ISqlSugarPagedList<OpenApiUser>> PageAsync(OpenApiUserPageInput input);
}