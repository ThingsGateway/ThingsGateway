namespace ThingsGateway.Application.Services.Auth
{
    /// <summary>
    /// 登录服务
    /// </summary>
    public interface IOpenApiAuthService : ITransient
    {
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="input">登录参数</param>
        /// <returns>Token信息</returns>
        Task<LoginOpenApiOutPut> LoginOpenApi(LoginOpenApiInput input);
        /// <summary>
        /// 登出
        /// </summary>
        /// <returns></returns>
        Task LoginOut();
    }
}