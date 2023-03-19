using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;

using OfficeOpenXml.Table;

using System.ComponentModel.DataAnnotations;

using ThingsGateway.Core;
using ThingsGateway.Web.Foundation;

namespace ThingsGateway.Application
{
    public class CollectDeviceAddInput : CollectDeviceEditInput
    {

        [Required(ErrorMessage = "不能为空")]
        public override string Name { get; set; }
        [MinValue(1, ErrorMessage = "插件不能为空")]
        public override long PluginId { get; set; }

    }
    public class CollectDeviceEditInput : CollectDevice
    {

        [Required(ErrorMessage = "不能为空")]
        public override string Name { get; set; }
        [MinValue(1, ErrorMessage = "插件不能为空")]
        public override long PluginId { get; set; }

    }
    public class CollectDevicePageInput : BasePageInput
    {
        [Description("设备名称")]
        public string Name { get; set; }
        [Description("插件名称")]
        public string PluginName { get; set; }
        [Description("设备组")]
        public string DeviceGroup { get; set; }
    }


    #region 导入导出
    public class CollectDeviceImport : UploadDeviceImport
    {


    }

    /// <summary>
    /// 采集设备
    /// </summary>
    [ExcelExporter(Name = "采集设备", TableStyle = TableStyles.Light10, AutoFitAllColumn = true)]
    public class CollectDeviceExport : UploadDeviceExport
    {



    }

    public class DevicePropertyImport : ImportPreviewInput
    {

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

    public class CollectDeviceWithPropertyImport
    {
        [ExcelImporter(SheetName = "采集设备")]
        public CollectDeviceImport CollectDeviceExport { get; set; }

        [ExcelImporter(SheetName = "设备附加属性")]
        public DevicePropertyImport DevicePropertyExcel { get; set; }
    }
    #endregion
}