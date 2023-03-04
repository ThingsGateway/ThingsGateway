namespace ThingsGateway.Core
{
    /// <summary>
    /// 会话表
    ///</summary>
    [SugarTable("sys_verificat", TableDescription = "会话表")]
    [Tenant(SqlsugarConst.DB_Default)]
    public class SysVerificat : PrimaryIdEntity
    {
        /// <summary>
        /// UserId
        /// </summary>
        [SugarColumn(ColumnDescription = "UserId", IsPrimaryKey = true)]
        public override long Id { get; set; }

        [SugarColumn(IsJson = true)]
        public List<VerificatInfo> VerificatInfos { get; set; }
    }

    /// <summary>
    /// Verificat信息
    /// </summary>
    public class VerificatInfo : PrimaryIdEntity
    {
        /// <summary>
        /// MQTT客户端ID列表
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public List<string> ClientIds { get; set; } = new List<string>();

        /// <summary>
        /// 设备
        /// </summary>
        [Description("设备")]
        public string Device { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        [Description("过期时间")]
        public int Expire { get; set; }

        [Description("VerificatId")]
        public override long Id { get; set; }

        /// <summary>
        /// 在线状态
        /// </summary>
        [Description("在线状态")]
        public bool IsOnline => ClientIds.Count > 0;

        /// <summary>
        /// 连接数量
        /// </summary>
        [Description("连接数量")]
        public int OnlineNum => ClientIds.Count;

        [Description("UserId")]
        public long UserId { get; set; }

        /// <summary>
        /// verificat剩余有效期
        /// </summary>
        [Description("剩余有效期")]
        public string VerificatRemain { get; set; }

        /// <summary>
        /// verificat剩余有效期百分比
        /// </summary>
        [Description("剩余百分比")]
        public double VerificatRemainPercent { get; set; }

        /// <summary>
        /// 超时时间
        /// </summary>
        [Description("超时时间")]
        public DateTime VerificatTimeout { get; set; }
    }
}