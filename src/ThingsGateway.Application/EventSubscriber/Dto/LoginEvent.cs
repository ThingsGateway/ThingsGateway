namespace ThingsGateway.Application
{
    /// <summary>
    /// 登录事件参数
    /// </summary>
    public class LoginEvent
    {
        /// <summary>
        /// 时间
        /// </summary>
        public DateTime DateTime = DateTime.Now;

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
        public SysUser SysUser { get; set; }

        /// <summary>
        /// 验证Id
        /// </summary>
        public long VerificatId { get; set; }
    }
}