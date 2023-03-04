namespace ThingsGateway.Core
{
    /// <summary>
    /// 跳转类型
    /// </summary>
    public enum TargetTypeEnum
    {
        None = 0,

        /// <summary>
        /// 目录
        /// </summary>
        CATALOG,

        /// <summary>
        /// 当前页面
        /// </summary>
        SELF,

        /// <summary>
        /// 新建页面
        /// </summary>
        BLANK,

        /// <summary>
        /// 内置调用
        /// </summary>
        CALLBACK,
    }
}