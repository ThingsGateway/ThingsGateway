namespace ThingsGateway.Application
{
    /// <summary>
    /// 登录输入参数
    /// </summary>
    public class LoginInput : ValidCodeInput
    {
        /// <summary>
        /// 账号
        ///</summary>
        [Required(ErrorMessage = "账号不能为空"), MinLength(3, ErrorMessage = "账号不能少于4个字符")]
        public string Account { get; set; }

        /// <summary>
        /// 设备类型，默认PC
        /// </summary>
        /// <example>0</example>
        public AuthDeviceTypeEnum Device { get; set; } = AuthDeviceTypeEnum.PC;

        /// <summary>
        /// 密码
        ///</summary>
        [Required(ErrorMessage = "密码不能为空"), MinLength(3, ErrorMessage = "密码不能少于3个字符")]
        public string Password { get; set; }
    }

    public class ValidCodeInput
    {
        /// <summary>
        /// 验证码
        /// </summary>
        public string ValidCode { get; set; }

        /// <summary>
        /// 请求号
        /// </summary>
        public string ValidCodeReqNo { get; set; }
    }
}