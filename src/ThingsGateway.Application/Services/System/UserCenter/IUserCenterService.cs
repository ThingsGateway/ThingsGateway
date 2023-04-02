namespace ThingsGateway.Application
{
    /// <summary>
    /// 个人信息中心服务
    /// </summary>
    public interface IUserCenterService : ITransient
    {
        /// <summary>
        /// 获取个人菜单
        /// </summary>
        /// <returns></returns>
        Task<List<SysResource>> GetOwnMenu(string UserAccount = null);

        /// <summary>
        /// 更新个人信息
        /// </summary>
        /// <param name="input">信息参数</param>
        /// <returns></returns>
        Task UpdateUserInfo(UpdateInfoInput input);


        /// <summary>
        /// 获取个人首页快捷方式
        /// </summary>
        /// <returns></returns>
        Task<List<long>> GetLoginWorkbench();

        /// <summary>
        /// 编辑个人工作台
        /// </summary>
        /// <param name="input">工作台字符串</param>
        /// <returns></returns>
        Task UpdateWorkbench(List<long> input);
        /// <summary>
        /// 更改密码
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task EditPassword(PasswordInfoInput input);
    }
}