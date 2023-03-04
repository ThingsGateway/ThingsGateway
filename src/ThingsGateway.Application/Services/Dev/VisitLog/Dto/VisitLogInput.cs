namespace ThingsGateway.Application
{
    /// <summary>
    /// 访问日志分页输入
    /// </summary>
    public class VisitLogPageInput : BasePageInput
    {
        /// <summary>
        /// 账号
        /// </summary>
        [Description("账号")]
        public string Account { get; set; }

        /// <summary>
        /// 分类
        /// </summary>
        [Description("分类")]
        public string Category { get; set; }

        /// <summary>
        /// 执行状态
        ///</summary>
        [Description("执行状态")]
        public string ExeStatus { get; set; }
    }
}