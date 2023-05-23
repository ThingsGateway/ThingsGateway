#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

namespace ThingsGateway.Application
{
    /// <summary>
    /// 角色服务
    /// </summary>
    public interface IRoleService : ITransient
    {
        /// <summary>
        /// 添加角色
        /// </summary>
        /// <param name="input">添加参数</param>
        /// <returns></returns>
        Task Add(RoleAddInput input);

        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="input">删除参数</param>
        /// <returns></returns>
        Task Delete(List<BaseIdInput> input);

        /// <summary>
        /// 编辑角色
        /// </summary>
        /// <param name="input">编辑角色</param>
        /// <returns></returns>
        Task Edit(RoleEditInput input);

        /// <summary>
        /// 根据用户ID获取用户角色Id集合
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        Task<List<long>> GetRoleIdListByUserId(long userId);

        /// <summary>
        /// 根据用户ID获取用户角色集合
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        Task<List<SysRole>> GetRoleListByUserId(long userId);

        /// <summary>
        /// 给角色授权资源
        /// </summary>
        /// <param name="input">授权参数</param>
        /// <returns></returns>
        Task GrantResource(GrantResourceInput input);

        /// <summary>
        /// 给角色授权用户
        /// </summary>
        /// <param name="input">授权信息</param>
        /// <returns></returns>
        Task GrantUser(GrantUserInput input);
        /// <summary>
        /// 角色刷新资源
        /// </summary>
        Task RefreshResource(long? menuId = null);
        /// <summary>
        /// 角色拥有资源
        /// </summary>
        /// <param name="input">角色id</param>
        /// <returns>角色拥有资源信息</returns>
        Task<RoleOwnResourceOutput> OwnResource(BaseIdInput input);

        /// <summary>
        /// 获取角色下的用户
        /// </summary>
        /// <param name="input">角色ID</param>
        /// <returns></returns>
        Task<List<long>> OwnUser(BaseIdInput input);

        /// <summary>
        /// 分页查询角色
        /// </summary>
        /// <param name="input">查询参数</param>
        /// <returns></returns>
        Task<SqlSugarPagedList<SysRole>> Page(RolePageInput input);

        /// <summary>
        /// 刷新缓存
        /// </summary>
        /// <returns></returns>
        Task RefreshCache();

        /// <summary>
        /// 角色选择器
        /// </summary>
        Task<List<SysRole>> RoleSelector(string searchKey = null);
    }
}