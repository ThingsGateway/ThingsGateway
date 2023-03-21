using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;

using OfficeOpenXml.Table;

using System.ComponentModel.DataAnnotations;

using ThingsGateway.Core;
using ThingsGateway.Web.Foundation;

namespace ThingsGateway.Application
{
    public class UploadDeviceAddInput : UploadDeviceEditInput
    {

        [Required(ErrorMessage = "不能为空")]
        public override string Name { get; set; }
        [MinValue(1, ErrorMessage = "插件不能为空")]
        public override long PluginId { get; set; }

    }
    public class UploadDeviceEditInput : UploadDevice
    {

        [Required(ErrorMessage = "不能为空")]
        public override string Name { get; set; }
        [MinValue(1, ErrorMessage = "插件不能为空")]
        public override long PluginId { get; set; }

    }
    public class UploadDevicePageInput : BasePageInput
    {
        [Description("设备名称")]
        public string Name { get; set; }
        [Description("插件名称")]
        public string PluginName  {  get;   set;   }
        [Description("设备组")]
        public string DeviceGroup { get; set; }
    }


    #region 导入导出
    public class UploadDeviceImport : ImportPreviewInput
    {

        /// <summary>
        /// 名称
        /// </summary>
        [ImporterHeader(Name = "名称")]
        [Required(ErrorMessage = "名称不能为空")]
        public virtual string Name { get; set; }

        /// <summary>
        /// 插件Id
        /// </summary>
        [ImporterHeader(IsIgnore = true)]
        public virtual long PluginId { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        [ImporterHeader(Name = "描述")]
        public string Description { get; set; }

        /// <summary>
        /// 插件Id
        /// </summary>
        [ImporterHeader(Name = "插件")]
        [Required(ErrorMessage = "插件不能为空")]
        public virtual string PluginName { get; set; }

        /// <summary>
        /// 设备使能
        /// </summary>
        [ImporterHeader(Name = "使能")]
        public virtual bool Enable { get; set; }
        /// <summary>
        /// 设备组
        /// </summary>
        [ImporterHeader(Name = "设备组")]
        public virtual string DeviceGroup { get; set; }
        /// <summary>
        /// 输出日志
        /// </summary>
        [ImporterHeader(Name = "输出日志")]
        public virtual bool IsLogOut { get; set; }
    }

    /// <summary>
    /// 上传设备
    /// </summary>
    [ExcelExporter(Name = "上传设备", TableStyle = TableStyles.Light10, AutoFitAllColumn = true)]
    public class UploadDeviceExport
    {
        /// <summary>
        /// 名称
        /// </summary>
        [ExporterHeader(DisplayName = "名称")]
        public virtual string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [ExporterHeader(DisplayName = "描述")]
        public string Description { get; set; }

        /// <summary>
        /// 插件Id
        /// </summary>
        [ExporterHeader(IsIgnore = true)]
        public virtual long PluginId { get; set; }
        /// <summary>
        /// 插件Id
        /// </summary>
        [ExporterHeader(DisplayName = "插件")]
        public virtual string PluginName { get; set; }

        /// <summary>
        /// 设备使能
        /// </summary>
        [ExporterHeader(DisplayName = "使能")]
        public virtual bool Enable { get; set; }
        /// <summary>
        /// 设备组
        /// </summary>
        [ExporterHeader(DisplayName = "设备组")]
        public virtual string DeviceGroup { get; set; }
        /// <summary>
        /// 输出日志
        /// </summary>
        [ExporterHeader(DisplayName = "输出日志")]
        public virtual bool IsLogOut { get; set; }
    }


    public class UploadDeviceWithPropertyImport
    {
        [ExcelImporter(SheetName = "上传设备")]
        public UploadDeviceImport UploadDeviceExport { get; set; }

        [ExcelImporter(SheetName = "设备附加属性")]
        public DevicePropertyImport DevicePropertyExcel { get; set; }
    }
    #endregion
}