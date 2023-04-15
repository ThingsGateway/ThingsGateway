namespace ThingsGateway.Web.Foundation
{
    /// <summary>
    /// 插件分组
    /// </summary>
    public class DriverPluginCategory
    {
        /// <summary>
        /// 插件子组
        /// </summary>
        public List<DriverPluginCategory> Children { get; set; }

        /// <summary>
        /// 插件ID
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 插件名称
        /// </summary>
        public string Name { get; set; }
    }

}