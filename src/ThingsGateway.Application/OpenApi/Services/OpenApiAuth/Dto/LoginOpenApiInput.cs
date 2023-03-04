namespace ThingsGateway.Application
{
    /// <summary>
    /// 登录输入参数
    /// </summary>
    public class LoginOpenApiInput
    {
        /// <summary>
        /// 账号
        ///</summary>
        [Required(ErrorMessage = "账号不能为空"), MinLength(3, ErrorMessage = "账号不能少于4个字符")]
        public string Account { get; set; }

        /// <summary>
        /// 密码
        ///</summary>
        [Required(ErrorMessage = "密码不能为空"), MinLength(3, ErrorMessage = "密码不能少于3个字符")]
        public string Password { get; set; }
    }
}