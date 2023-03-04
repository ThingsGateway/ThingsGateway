namespace ThingsGateway.Application
{
    /// <summary>
    /// 权限校验服务
    /// </summary>
    public interface IAuthService : ITransient
    {
        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <returns></returns>
        ValidCodeOutPut GetCaptchaInfo();

        /// <summary>
        /// 获取登录用户信息
        /// </summary>
        /// <returns></returns>
        Task<SysUser> GetLoginUser();

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="input">登录参数</param>
        /// <returns>Token信息</returns>
        Task<LoginOutPut> Login(LoginInput input);

        /// <summary>
        /// 退出登录
        /// </summary>
        /// <returns></returns>
        Task LoginOut();
    }
}