namespace ThingsGateway.Application
{
    /// <summary>
    /// 会话分页查询
    /// </summary>
    public class OpenApiSessionPageInput : BasePageInput
    {
        /// <summary>
        /// 账号
        /// </summary>
        [Description("账号")]
        public string Account { get; set; }
        /// <summary>
        /// 最新登录IP
        /// </summary>
        [Description("最新登录IP")]
        public string LatestLoginIp { get; set; }
    }

    /// <summary>
    /// 退出参数
    /// </summary>
    public class OpenApiExitVerificatInput : BaseIdInput
    {
        /// <summary>
        /// 验证ID列表
        /// </summary>
        [Required(ErrorMessage = "VerificatIds不能为空")]
        public List<long> VerificatIds { get; set; }
    }
}