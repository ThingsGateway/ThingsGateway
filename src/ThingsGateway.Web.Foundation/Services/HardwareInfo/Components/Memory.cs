namespace ThingsGateway.Web.Foundation
{
    /// <summary>
    /// WMI 类：Win32 物理内存
    /// </summary>
    public class Memory
    {
        /// <summary>
        /// 总容量（以字节为单位）
        /// </summary>
        [Description("总容量")]
        public UInt64 Capacity { get; set; }

        /// <summary>
        /// 制造商
        /// </summary>
        [Description("制造商")]
        public string Manufacturer { get; set; } = string.Empty;

        /// <summary>
        /// 此设备的最大工作电压，以毫伏为单位，如果电压未知，则为 0。
        /// </summary>
        [Description("最大工作电压")]
        public UInt32 MaxVoltage { get; set; }

        /// <summary>
        /// 此设备的最小工作电压，以毫伏为单位，如果电压未知，则为 0。
        /// </summary>
        [Description("最小工作电压")]
        public UInt32 MinVoltage { get; set; }

        /// <summary>
        /// 物理内存的速度（以纳秒为单位）。
        /// </summary>
        [Description("速度(ns)")]
        public UInt32 Speed { get; set; }

    }
}
