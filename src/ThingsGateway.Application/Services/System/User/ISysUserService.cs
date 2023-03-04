namespace ThingsGateway.Application
{
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
        Task Add(UserAddInput input);

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="input">Id列表</param>
        /// <returns></returns>
        Task Delete(List<BaseIdInput> input);

        /// <summary>
        /// 从cache中删除用户信息
        /// </summary>
        /// <param name="ids">用户ID列表</param>
        void DeleteUserFromCache(List<long> ids);

        /// <summary>
        /// 从cache中删除用户信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        void DeleteUserFromCache(long userId);

        /// <summary>
        /// 禁用用户
        /// </summary>
        /// <param name="input">用户Id</param>
        /// <returns></returns>
        Task DisableUser(BaseIdInput input);

        /// <summary>
        /// 编辑
        /// </summary>
        /// <param name="input">编辑参数</param>
        /// <returns></returns>
        Task Edit(UserEditInput input);

        /// <summary>
        /// 启用用户
        /// </summary>
        /// <param name="input">用户Id</param>
        /// <returns></returns>
        Task EnableUser(BaseIdInput input);

        /// <summary>
        /// 根据用户ID获取按钮ID集合
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<string>> GetButtonCodeList(long userId);

        /// <summary>
        ///根据用户账号获取用户ID
        /// </summary>
        /// <param name="account">用户账号</param>
        /// <returns></returns>
        Task<long> GetIdByAccount(string account);

        /// <summary>
        /// 根据账号获取用户信息
        /// </summary>
        /// <param name="account">用户名</param>
        /// <returns>用户信息</returns>
        Task<SysUser> GetUserByAccount(string account);

        Task<SysUser> GetUsertById(long Id);

        /// <summary>
        /// 给用户授权角色
        /// </summary>
        /// <param name="input">授权参数</param>
        /// <returns></returns>
        Task GrantRole(UserGrantRoleInput input);

        /// <summary>
        /// 获取用户拥有角色
        /// </summary>
        /// <param name="input">用户ID</param>
        /// <returns></returns>
        Task<List<long>> OwnRole(BaseIdInput input);

        /// <summary>
        /// 用户分页查询
        /// </summary>
        /// <param name="input">查询参数</param>
        /// <returns>用户分页列表</returns>
        Task<SqlSugarPagedList<SysUser>> Page(UserPageInput input);

        /// <summary>
        /// 重置密码
        /// </summary>
        /// <param name="input">用户Id</param>
        /// <returns></returns>
        Task ResetPassword(BaseIdInput input);

        /// <summary>
        /// 用户选择器
        /// </summary>
        /// <param name="input">查询参数</param>
        /// <returns></returns>
        Task<List<UserSelectorOutPut>> UserSelector(string searchKey);
    }
}