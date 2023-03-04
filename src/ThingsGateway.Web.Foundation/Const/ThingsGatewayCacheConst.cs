using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation
{
    /// <summary>
    /// Cache常量
    /// </summary>
    public class ThingsGatewayCacheConst
    {
        /// <summary>
        /// 设备变量名称
        /// </summary>
        public const string Cache_DeviceVariableName = CacheConst.Cache_Prefix_Web + "DeviceVariableName";
        /// <summary>
        /// 设备变量Id
        /// </summary>
        public const string Cache_DeviceVariableId = CacheConst.Cache_Prefix_Web + "DeviceVariableId";




        public const string Cache_CollectDevice = CacheConst.Cache_Prefix_Web + "CollectDevice";
        public const string Cache_UploadDevice = CacheConst.Cache_Prefix_Web + "UploadDevice";

        /// <summary>
        /// 插件
        /// </summary>
        public const string Cache_DriverPlugin = CacheConst.Cache_Prefix_Web + "Cache_DriverPlugin";

    }
}