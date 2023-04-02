namespace ThingsGateway.Application
{
    /// <summary>
    /// 登录返回参数
    /// </summary>
    public class LoginOpenApiOutPut : BaseLoginOutPut
    {
        /// <summary>
        /// TOKEN
        /// </summary>
        public string Token { get; set; }
    }
}