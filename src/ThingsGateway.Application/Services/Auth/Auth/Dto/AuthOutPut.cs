namespace ThingsGateway.Application
{
    /// <summary>
    /// 登录返回参数
    /// </summary>
    public class BaseLoginOutPut
    {
        public string Account { get; set; }
        public long VerificatId { get; set; }
    }

    /// <summary>
    /// 验证码值返回
    /// </summary>
    public class ValidCodeOutPut
    {
        /// <summary>
        /// 验证码值
        /// </summary>
        public string CodeValue { get; set; }

        /// <summary>
        /// 验证码请求号
        /// </summary>
        public string ValidCodeReqNo { get; set; }
    }

    /// <summary>
    /// 登出返回参数
    /// </summary>
    public class LoginOutPut : BaseLoginOutPut
    {
    }
}