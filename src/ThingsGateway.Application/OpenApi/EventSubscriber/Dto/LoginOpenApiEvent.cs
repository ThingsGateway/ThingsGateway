namespace ThingsGateway.Application
{
    /// <summary>
    /// 登录事件参数
    /// </summary>
    public class LoginOpenApiEvent
    {
        /// <summary>
        /// 时间
        /// </summary>
        public DateTime DateTime = DateTime.UtcNow;

        /// <summary>
        /// 登录设备
        /// </summary>
        public AuthDeviceTypeEnum Device { get; set; }

        /// <summary>
        /// 过期时间(分)
        /// </summary>
        public int Expire { get; set; }

        /// <summary>
        /// Ip地址
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// 用户信息
        /// </summary>
        public OpenApiUser OpenApiUser { get; set; }

        /// <summary>
        /// 验证Id
        /// </summary>
        public long VerificatId { get; set; }
    }
}