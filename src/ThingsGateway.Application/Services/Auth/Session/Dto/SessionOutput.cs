namespace ThingsGateway.Application
{
    /// <summary>
    /// 会话输出
    /// </summary>
    public class SessionOutput : PrimaryKeyEntity
    {
        /// <summary>
        /// 账号
        ///</summary>
        [Description("账号")]
        public virtual string Account { get; set; }

        /// <summary>
        /// 姓名
        ///</summary>

        /// <summary>
        /// 最新登录ip
        ///</summary>
        [Description("最新登录ip")]
        public string LatestLoginIp { get; set; }

        /// <summary>
        /// 最新登录时间
        ///</summary>
        [Description("最新登录时间")]
        public DateTime? LatestLoginTime { get; set; }

        [Description("姓名")]
        public virtual string Name { get; set; }

        /// <summary>
        /// 在线状态
        /// </summary>
        [Description("在线状态")]
        public bool OnlineStatus { get; set; }

        /// <summary>
        /// 令牌数量
        /// </summary>
        [Description("令牌数量")]
        public int VerificatCount { get; set; }

        /// <summary>
        /// 令牌信息集合
        /// </summary>
        [Description("令牌列表")]
        public List<VerificatInfo> VerificatSignList { get; set; }
    }
}