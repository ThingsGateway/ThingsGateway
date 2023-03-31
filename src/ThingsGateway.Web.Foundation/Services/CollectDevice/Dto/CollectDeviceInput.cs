using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;

using OfficeOpenXml.Table;

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


    #region 导入导出
    /// <inheritdoc/>
    public class CollectDeviceImport : UploadDeviceImport
    {


    }

    /// <summary>
    /// 采集设备导出DTO
    /// </summary>
    [ExcelExporter(Name = "采集设备", TableStyle = TableStyles.Light10, AutoFitAllColumn = true)]
    public class CollectDeviceExport : UploadDeviceExport
    {



    }

    /// <summary>
    /// 采集设备导入DTO
    /// </summary>
    public class DevicePropertyImport : ImportPreviewInput
    {
        /// <summary>
        /// 设备ID，已忽略
        /// </summary>
        [ImporterHeader(IsIgnore = true)]
        public virtual long DeviceId { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        [ImporterHeader(Name = "设备名称")]
        public string DeviceName { get; set; }

        /// <summary>
        /// 属性名称
        /// </summary>
        [ImporterHeader(Name = "名称")]
        public string PropertyName { get; set; }
        /// <summary>
        /// 属性描述
        /// </summary>
        [ImporterHeader(Name = "描述")]
        public string Description { get; set; }
        /// <summary>
        /// 属性值
        /// </summary>
        [ImporterHeader(Name = "属性值")]
        public string Value { get; set; }


    }
    /// <summary>
    /// 设备附加属性表
    /// </summary>
    [ExcelExporter(Name = "设备附加属性", TableStyle = TableStyles.Light10, AutoFitAllColumn = true)]
    public class DevicePropertyExport
    {
        /// <summary>
        /// 设备ID，已忽略
        /// </summary>
        [ExporterHeader(IsIgnore = true)]
        public virtual long DeviceId { get; set; }
        /// <summary>
        /// 设备名称
        /// </summary>
        [ExporterHeader(DisplayName = "设备名称")]
        public string DeviceName { get; set; }

        /// <summary>
        /// 属性名称
        /// </summary>
        [ExporterHeader(DisplayName = "名称")]
        public string PropertyName { get; set; }
        /// <summary>
        /// 属性描述
        /// </summary>
        [ExporterHeader(DisplayName = "描述")]
        public string Description { get; set; }
        /// <summary>
        /// 属性值
        /// </summary>
        [ExporterHeader(DisplayName = "属性值")]
        public string Value { get; set; }


    }
    /// <summary>
    /// 采集设备Excel导入表示类
    /// </summary>
    public class CollectDeviceWithPropertyImport
    {
        /// <summary>
        /// 采集设备基本属性
        /// </summary>
        [ExcelImporter(SheetName = "采集设备")]
        public CollectDeviceImport CollectDeviceExport { get; set; }
        /// <summary>
        /// 采集设备附加属性
        /// </summary>
        [ExcelImporter(SheetName = "设备附加属性")]
        public DevicePropertyImport DevicePropertyExcel { get; set; }
    }
    #endregion
}