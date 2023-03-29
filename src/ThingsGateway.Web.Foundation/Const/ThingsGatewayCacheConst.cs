using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation
{
    /// <summary>
    /// Cache常量
    /// </summary>
    public class ThingsGatewayCacheConst
    {
        /// <summary>
        /// 采集设备
        /// </summary>
        public const string Cache_CollectDevice = CacheConst.Cache_Prefix_Web + "CollectDevice";

        /// <summary>
        /// 设备变量组
        /// </summary>
        public const string Cache_DeviceVariableGroup = CacheConst.Cache_Prefix_Web + "DeviceVariableGroup";

        /// <summary>
        /// 设备变量Id
        /// </summary>
        public const string Cache_DeviceVariableId = CacheConst.Cache_Prefix_Web + "DeviceVariableId";

        /// <summary>
        /// 设备变量名称
        /// </summary>
        public const string Cache_DeviceVariableName = CacheConst.Cache_Prefix_Web + "DeviceVariableName";
        /// <summary>
        /// 插件
        /// </summary>
        public const string Cache_DriverPlugin = CacheConst.Cache_Prefix_Web + "Cache_DriverPlugin";
        /// <summary>
        /// 上传设备
        /// </summary>
        public const string Cache_UploadDevice = CacheConst.Cache_Prefix_Web + "UploadDevice";
    }
}