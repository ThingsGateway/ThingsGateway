namespace ThingsGateway.Core
{
    /// <summary>
    /// 登录设备类型枚举
    /// </summary>
    public enum AuthDeviceTypeEnum
    {
        /// <summary>
        /// PC端
        /// </summary>
        [Description("PC端")]
        PC,

        /// <summary>
        /// 移动端
        /// </summary>
        [Description("移动端")]
        APP,

        /// <summary>
        /// Api
        /// </summary>
        [Description("Api")]
        Api,
    }
}