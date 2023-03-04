namespace ThingsGateway.Application
{
    /// <summary>
    /// 会话分页查询
    /// </summary>
    public class SessionPageInput : BasePageInput
    {
        /// <summary>
        /// 账号
        /// </summary>
        [Description("账号")]
        public string Account { get; set; }

        [Description("最新登录IP")]
        public string LatestLoginIp { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        [Description("姓名")]
        public string Name { get; set; }
    }

    /// <summary>
    /// 退出参数
    /// </summary>
    public class ExitVerificatInput : BaseIdInput
    {
        [Required(ErrorMessage = "VerificatIds不能为空")]
        public List<long> VerificatIds { get; set; }
    }
}