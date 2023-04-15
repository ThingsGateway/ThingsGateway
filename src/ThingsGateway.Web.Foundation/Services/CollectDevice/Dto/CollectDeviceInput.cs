using System.ComponentModel.DataAnnotations;

using ThingsGateway.Core;
using ThingsGateway.Web.Foundation;

namespace ThingsGateway.Application
{
    /// <summary>
    /// 采集设备添加DTO
    /// </summary>
    public class CollectDeviceAddInput : CollectDeviceEditInput
    {
        /// <inheritdoc/>
        [Required(ErrorMessage = "不能为空")]
        public override string Name { get; set; }
        /// <inheritdoc/>
        [MinValue(1, ErrorMessage = "插件不能为空")]
        public override long PluginId { get; set; }

    }
    /// <summary>
    /// 采集设备编辑DTO
    /// </summary>
    public class CollectDeviceEditInput : CollectDevice
    {

        /// <inheritdoc/>
        [Required(ErrorMessage = "不能为空")]
        public override string Name { get; set; }
        /// <inheritdoc/>
        [MinValue(1, ErrorMessage = "插件不能为空")]
        public override long PluginId { get; set; }

    }
    /// <summary>
    /// 采集设备分页查询DTO
    /// </summary>
    public class CollectDevicePageInput : BasePageInput
    {
        /// <inheritdoc/>
        [Description("设备名称")]
        public string Name { get; set; }
        /// <inheritdoc/>
        [Description("插件名称")]
        public string PluginName { get; set; }
        /// <inheritdoc/>
        [Description("设备组")]
        public string DeviceGroup { get; set; }
    }


}