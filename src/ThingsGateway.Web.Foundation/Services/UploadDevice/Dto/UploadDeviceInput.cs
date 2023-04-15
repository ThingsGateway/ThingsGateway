using System.ComponentModel.DataAnnotations;

using ThingsGateway.Core;
using ThingsGateway.Web.Foundation;

namespace ThingsGateway.Application
{
    /// <summary>
    /// 上传设备添加DTO
    /// </summary>
    public class UploadDeviceAddInput : UploadDeviceEditInput
    {
        /// <inheritdoc/>
        [Required(ErrorMessage = "不能为空")]
        public override string Name { get; set; }
        /// <inheritdoc/>
        [MinValue(1, ErrorMessage = "插件不能为空")]
        public override long PluginId { get; set; }

    }
    /// <summary>
    /// 上传设备修改DTO
    /// </summary>
    public class UploadDeviceEditInput : UploadDevice
    {

        /// <inheritdoc/>
        [Required(ErrorMessage = "不能为空")]
        public override string Name { get; set; }
        /// <inheritdoc/>
        [MinValue(1, ErrorMessage = "插件不能为空")]
        public override long PluginId { get; set; }

    }

    /// <summary>
    /// 上传设备分页查询
    /// </summary>
    public class UploadDevicePageInput : BasePageInput
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