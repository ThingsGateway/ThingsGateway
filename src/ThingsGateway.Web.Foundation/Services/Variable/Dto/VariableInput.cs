using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;

using OfficeOpenXml.Table;

using System.ComponentModel.DataAnnotations;

using ThingsGateway.Core;
using ThingsGateway.Foundation;
using ThingsGateway.Web.Foundation;

namespace ThingsGateway.Application
{
    /// <summary>
    /// 添加变量DTO
    /// </summary>
    public class VariableAddInput : VariableEditInput
    {
        /// <inheritdoc/>
        [MinValue(100, ErrorMessage = "低于最小值")]
        public override int IntervalTime { get; set; } = 1000;
        /// <inheritdoc/>
        public override long DeviceId { get; set; }
    }
    /// <summary>
    /// 修改变量DTO
    /// </summary>
    public class VariableEditInput : CollectDeviceVariable, IValidatableObject
    {

        /// <inheritdoc/>
        [Required(ErrorMessage = "不能为空")]
        public override string Name { get; set; }

        /// <inheritdoc/>
        [MinValue(1, ErrorMessage = "不能为空")]
        public override long DeviceId  { get;   set;   }

        /// <inheritdoc/>
        [MinValue(100, ErrorMessage = "低于最小值")]
        public override int IntervalTime { get; set; }

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(VariableAddress) && string.IsNullOrEmpty(OtherMethod))
                yield return new ValidationResult("变量地址或特殊方法不能同时为空", new[] { nameof(VariableAddress) });
        }
    }

    /// <summary>
    /// 变量分页查询参数
    /// </summary>
    public class VariablePageInput : BasePageInput
    {
        /// <inheritdoc/>
        [Description("变量名称")]
        public string Name { get; set; }
        /// <inheritdoc/>
        [Description("设备名称")]
        public string DeviceName { get; set; }
        /// <inheritdoc/>
        [Description("变量地址")]
        public string VariableAddress { get; set; }



    }


    #region 导入导出
    /// <summary>
    /// 变量导入DTO
    /// </summary>
    public class CollectDeviceVariableImport : ImportPreviewInput, IValidatableObject
    {
        /// <inheritdoc/>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(VariableAddress) && string.IsNullOrEmpty(OtherMethod))
                yield return new ValidationResult("变量地址或特殊方法不能同时为空", new[] { nameof(VariableAddress) });
        }
        /// <summary>
        /// 设备
        /// </summary>
        [ImporterHeader(IsIgnore = true)]
        public virtual long DeviceId { get; set; }

        /// <summary>
        /// 设备
        /// </summary>
        [ImporterHeader(Name = "设备")]
        public virtual string DeviceName { get; set; }

        /// <summary>
        /// 变量值表达式
        /// </summary>
        [ImporterHeader(Name = "读取表达式")]
        public string ReadExpressions { get; set; }
        /// <summary>
        /// 变量值表达式
        /// </summary>
        [ImporterHeader(Name = "写入表达式")]
        public string WriteExpressions { get; set; }

        /// <summary>
        /// 执行间隔
        /// </summary>
        [ImporterHeader(Name = "执行间隔")]
        [MinValue(100, ErrorMessage = "执行间隔不能低于100")]
        public virtual int IntervalTime { get; set; }

        /// <summary>
        /// 特殊方法，若不为空，此时Address为方法参数
        /// </summary>
        [ImporterHeader(Name = "特殊方法")]
        public string OtherMethod { get; set; }

        /// <summary>
        /// 变量地址，可能带有额外的信息，比如<see cref="DataFormat"/> ，以;分割
        /// </summary>
        [ImporterHeader(Name = "变量地址")]
        public string VariableAddress { get; set; }


        /// <summary>
        /// 名称
        /// </summary>
        [ImporterHeader(Name = "名称")]
        [Required(ErrorMessage = "名称不能为空")]
        public virtual string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [ImporterHeader(Name = "描述")]
        public string Description { get; set; }


        /// <summary>
        /// 读写权限
        /// </summary>
        [ImporterHeader(Name = "读写权限")]
        public ProtectTypeEnum ProtectTypeEnum { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        [ImporterHeader(Name = "数据类型")]
        public DataTypeEnum DataTypeEnum { get; set; }

        /// <summary>
        /// 是否允许远程Rpc写入，不包含Blazor Web页
        /// </summary>
        [ImporterHeader(Name = "允许远程写入")]
        public bool RpcWriteEnable { get; set; }


        #region 报警
        ///// <summary>
        ///// 报警组
        ///// </summary>
        //[ImporterHeader(Name = "报警组")]
        //[ExporterHeader(DisplayName = "报警组")]
        //public string AlarmGroup { get; set; } = "";
        ///// <summary>
        ///// 报警死区
        ///// </summary>
        //[ImporterHeader(Name = "报警死区")]
        //[ExporterHeader(DisplayName = "报警死区")]
        //public int AlarmDeadZone { get; set; }
        ///// <summary>
        ///// 报警延时
        ///// </summary>
        //[ImporterHeader(Name = "报警延时")]
        //[ExporterHeader(DisplayName = "报警延时")]
        //public int AlarmDelayTime { get; set; }
        /// <summary>
        /// 布尔开报警使能
        /// </summary>
        [ImporterHeader(Name = "布尔开报警使能")]
        [ExporterHeader(DisplayName = "布尔开报警使能")]
        public bool BoolOpenAlarmEnable { get; set; }
        /// <summary>
        /// 布尔开报警约束
        /// </summary>
        [ImporterHeader(Name = "布尔开报警约束")]
        [ExporterHeader(DisplayName = "布尔开报警约束")]
        public string BoolOpenRestrainExpressions { get; set; } = "";
        /// <summary>
        /// 布尔开报警文本
        /// </summary>
        [ImporterHeader(Name = "布尔开报警文本")]
        [ExporterHeader(DisplayName = "布尔开报警文本")]
        public string BoolOpenAlarmText { get; set; } = "";


        /// <summary>
        /// 布尔关报警使能
        /// </summary>
        [ImporterHeader(Name = "布尔关报警使能")]
        [ExporterHeader(DisplayName = "布尔关报警使能")]
        public bool BoolCloseAlarmEnable { get; set; }
        /// <summary>
        /// 布尔关报警约束
        /// </summary>
        [ImporterHeader(Name = "布尔关报警约束")]
        [ExporterHeader(DisplayName = "布尔关报警约束")]
        public string BoolCloseRestrainExpressions { get; set; } = "";
        /// <summary>
        /// 布尔关报警文本
        /// </summary>
        [ImporterHeader(Name = "布尔关报警文本")]
        [ExporterHeader(DisplayName = "布尔关报警文本")]
        public string BoolCloseAlarmText { get; set; } = "";

        /// <summary>
        /// 高报使能
        /// </summary>
        [ImporterHeader(Name = "高报使能")]
        [ExporterHeader(DisplayName = "高报使能")]
        public bool HAlarmEnable { get; set; }
        /// <summary>
        /// 高报约束
        /// </summary>
        [ImporterHeader(Name = "高报约束")]
        [ExporterHeader(DisplayName = "高报约束")]
        public string HRestrainExpressions { get; set; } = "";
        /// <summary>
        /// 高报文本
        /// </summary>
        [ImporterHeader(Name = "高报文本")]
        [ExporterHeader(DisplayName = "高报文本")]
        public string HAlarmText { get; set; } = "";
        /// <summary>
        /// 高限值
        /// </summary>
        [ImporterHeader(Name = "高限值")]
        [ExporterHeader(DisplayName = "高限值")]
        public string HAlarmCode { get; set; } = "";


        /// <summary>
        /// 高高报使能
        /// </summary>
        [ImporterHeader(Name = "高高报使能")]
        [ExporterHeader(DisplayName = "高高报使能")]
        public bool HHAlarmEnable { get; set; }
        /// <summary>
        /// 高高报约束
        /// </summary>
        [ImporterHeader(Name = "高高报约束")]
        [ExporterHeader(DisplayName = "高高报约束")]
        public string HHRestrainExpressions { get; set; } = "";
        /// <summary>
        /// 高高报文本
        /// </summary>
        [ImporterHeader(Name = "高高报文本")]
        [ExporterHeader(DisplayName = "高高报文本")]
        public string HHAlarmText { get; set; } = "";
        /// <summary>
        /// 高高限值
        /// </summary>
        [ImporterHeader(Name = "高高限值")]
        [ExporterHeader(DisplayName = "高高限值")]
        public string HHAlarmCode { get; set; } = "";

        /// <summary>
        /// 低报使能
        /// </summary>
        [ImporterHeader(Name = "低报使能")]
        [ExporterHeader(DisplayName = "低报使能")]
        public bool LAlarmEnable { get; set; }
        /// <summary>
        /// 低报约束
        /// </summary>
        [ImporterHeader(Name = "低报约束")]
        [ExporterHeader(DisplayName = "低报约束")]
        public string LRestrainExpressions { get; set; } = "";
        /// <summary>
        /// 低报文本
        /// </summary>
        [ImporterHeader(Name = "低报文本")]
        [ExporterHeader(DisplayName = "低报文本")]
        public string LAlarmText { get; set; } = "";
        /// <summary>
        /// 低限值
        /// </summary>
        [ImporterHeader(Name = "低限值")]
        [ExporterHeader(DisplayName = "低限值")]
        public string LAlarmCode { get; set; } = "";

        /// <summary>
        /// 低低报使能
        /// </summary>
        [ImporterHeader(Name = "低低报使能")]
        [ExporterHeader(DisplayName = "低低报使能")]
        public bool LLAlarmEnable { get; set; }
        /// <summary>
        /// 低低报约束
        /// </summary>
        [ImporterHeader(Name = "低低报约束")]
        [ExporterHeader(DisplayName = "低低报约束")]
        public string LLRestrainExpressions { get; set; } = "";
        /// <summary>
        /// 低低报文本
        /// </summary>
        [ImporterHeader(Name = "低低报文本")]
        [ExporterHeader(DisplayName = "低低报文本")]
        public string LLAlarmText { get; set; } = "";
        /// <summary>
        /// 低低限值
        /// </summary>
        [ImporterHeader(Name = "低低限值")]
        [ExporterHeader(DisplayName = "低低限值")]
        public string LLAlarmCode { get; set; } = "";

        #endregion

        #region 历史
        /// <summary>
        /// 存储类型
        /// </summary>
        [ImporterHeader(Name = "存储类型")]
        [ExporterHeader(DisplayName = "存储类型")]
        public HisType HisType { get; set; }

        /// <summary>
        /// 启用
        /// </summary>
        [ImporterHeader(Name = "历史启用")]
        [ExporterHeader(DisplayName = "历史启用")]
        public bool HisEnable { get; set; }
        #endregion
    }

    /// <summary>
    /// 变量导出DTO
    /// </summary>
    [ExcelExporter(Name = "变量", TableStyle = TableStyles.Light10, AutoFitAllColumn = true)]
    public class CollectDeviceVariableExport
    {
        /// <summary>
        /// 设备
        /// </summary>
        [ExporterHeader(IsIgnore = true)]
        public virtual long DeviceId { get; set; }


        /// <summary>
        /// 名称
        /// </summary>
        [ExporterHeader(DisplayName = "名称")]
        [Required(ErrorMessage = "名称不能为空")]
        public virtual string Name { get; set; }
        /// <summary>
        /// 设备
        /// </summary>
        [ExporterHeader(DisplayName = "设备")]
        public virtual string DeviceName { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [ExporterHeader(DisplayName = "描述")]
        public string Description { get; set; }



        /// <summary>
        /// 变量地址，可能带有额外的信息，比如<see cref="DataFormat"/> ，以;分割
        /// </summary>
        [ExporterHeader(DisplayName = "变量地址")]
        public string VariableAddress { get; set; }

        /// <summary>
        /// 读写权限
        /// </summary>
        [ExporterHeader(DisplayName = "读写权限")]
        public ProtectTypeEnum ProtectTypeEnum { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        [ExporterHeader(DisplayName = "数据类型")]
        public DataTypeEnum DataTypeEnum { get; set; }

        /// <summary>
        /// 执行间隔
        /// </summary>
        [ExporterHeader(DisplayName = "执行间隔")]
        [MinValue(100, ErrorMessage = "执行间隔不能低于100")]
        public virtual int IntervalTime { get; set; }
        /// <summary>
        /// 变量值表达式
        /// </summary>
        [ExporterHeader(DisplayName = "读取表达式")]
        public string ReadExpressions { get; set; }
        /// <summary>
        /// 变量值表达式
        /// </summary>
        [ExporterHeader(DisplayName = "写入表达式")]
        public string WriteExpressions { get; set; }
        /// <summary>
        /// 特殊方法，若不为空，此时Address为方法参数
        /// </summary>
        [ExporterHeader(DisplayName = "特殊方法")]
        public string OtherMethod { get; set; }

        /// <summary>
        /// 是否允许远程Rpc写入，不包含Blazor Web页
        /// </summary>
        [ExporterHeader(DisplayName = "允许远程写入")]
        public bool RpcWriteEnable { get; set; }
        #region 报警
        ///// <summary>
        ///// 报警组
        ///// </summary>
        //[ImporterHeader(Name = "报警组")]
        //[ExporterHeader(DisplayName = "报警组")]
        //public string AlarmGroup { get; set; } = "";
        ///// <summary>
        ///// 报警死区
        ///// </summary>
        //[ImporterHeader(Name = "报警死区")]
        //[ExporterHeader(DisplayName = "报警死区")]
        //public int AlarmDeadZone { get; set; }
        ///// <summary>
        ///// 报警延时
        ///// </summary>
        //[ImporterHeader(Name = "报警延时")]
        //[ExporterHeader(DisplayName = "报警延时")]
        //public int AlarmDelayTime { get; set; }
        /// <summary>
        /// 布尔开报警使能
        /// </summary>
        [ImporterHeader(Name = "布尔开报警使能")]
        [ExporterHeader(DisplayName = "布尔开报警使能")]
        public bool BoolOpenAlarmEnable { get; set; }
        /// <summary>
        /// 布尔开报警约束
        /// </summary>
        [ImporterHeader(Name = "布尔开报警约束")]
        [ExporterHeader(DisplayName = "布尔开报警约束")]
        public string BoolOpenRestrainExpressions { get; set; } = "";
        /// <summary>
        /// 布尔开报警文本
        /// </summary>
        [ImporterHeader(Name = "布尔开报警文本")]
        [ExporterHeader(DisplayName = "布尔开报警文本")]
        public string BoolOpenAlarmText { get; set; } = "";


        /// <summary>
        /// 布尔关报警使能
        /// </summary>
        [ImporterHeader(Name = "布尔关报警使能")]
        [ExporterHeader(DisplayName = "布尔关报警使能")]
        public bool BoolCloseAlarmEnable { get; set; }
        /// <summary>
        /// 布尔关报警约束
        /// </summary>
        [ImporterHeader(Name = "布尔关报警约束")]
        [ExporterHeader(DisplayName = "布尔关报警约束")]
        public string BoolCloseRestrainExpressions { get; set; } = "";
        /// <summary>
        /// 布尔关报警文本
        /// </summary>
        [ImporterHeader(Name = "布尔关报警文本")]
        [ExporterHeader(DisplayName = "布尔关报警文本")]
        public string BoolCloseAlarmText { get; set; } = "";

        /// <summary>
        /// 高报使能
        /// </summary>
        [ImporterHeader(Name = "高报使能")]
        [ExporterHeader(DisplayName = "高报使能")]
        public bool HAlarmEnable { get; set; }
        /// <summary>
        /// 高报约束
        /// </summary>
        [ImporterHeader(Name = "高报约束")]
        [ExporterHeader(DisplayName = "高报约束")]
        public string HRestrainExpressions { get; set; } = "";
        /// <summary>
        /// 高报文本
        /// </summary>
        [ImporterHeader(Name = "高报文本")]
        [ExporterHeader(DisplayName = "高报文本")]
        public string HAlarmText { get; set; } = "";
        /// <summary>
        /// 高限值
        /// </summary>
        [ImporterHeader(Name = "高限值")]
        [ExporterHeader(DisplayName = "高限值")]
        public string HAlarmCode { get; set; } = "";


        /// <summary>
        /// 高高报使能
        /// </summary>
        [ImporterHeader(Name = "高高报使能")]
        [ExporterHeader(DisplayName = "高高报使能")]
        public bool HHAlarmEnable { get; set; }
        /// <summary>
        /// 高高报约束
        /// </summary>
        [ImporterHeader(Name = "高高报约束")]
        [ExporterHeader(DisplayName = "高高报约束")]
        public string HHRestrainExpressions { get; set; } = "";
        /// <summary>
        /// 高高报文本
        /// </summary>
        [ImporterHeader(Name = "高高报文本")]
        [ExporterHeader(DisplayName = "高高报文本")]
        public string HHAlarmText { get; set; } = "";
        /// <summary>
        /// 高高限值
        /// </summary>
        [ImporterHeader(Name = "高高限值")]
        [ExporterHeader(DisplayName = "高高限值")]
        public string HHAlarmCode { get; set; } = "";

        /// <summary>
        /// 低报使能
        /// </summary>
        [ImporterHeader(Name = "低报使能")]
        [ExporterHeader(DisplayName = "低报使能")]
        public bool LAlarmEnable { get; set; }
        /// <summary>
        /// 低报约束
        /// </summary>
        [ImporterHeader(Name = "低报约束")]
        [ExporterHeader(DisplayName = "低报约束")]
        public string LRestrainExpressions { get; set; } = "";
        /// <summary>
        /// 低报文本
        /// </summary>
        [ImporterHeader(Name = "低报文本")]
        [ExporterHeader(DisplayName = "低报文本")]
        public string LAlarmText { get; set; } = "";
        /// <summary>
        /// 低限值
        /// </summary>
        [ImporterHeader(Name = "低限值")]
        [ExporterHeader(DisplayName = "低限值")]
        public string LAlarmCode { get; set; } = "";

        /// <summary>
        /// 低低报使能
        /// </summary>
        [ImporterHeader(Name = "低低报使能")]
        [ExporterHeader(DisplayName = "低低报使能")]
        public bool LLAlarmEnable { get; set; }
        /// <summary>
        /// 低低报约束
        /// </summary>
        [ImporterHeader(Name = "低低报约束")]
        [ExporterHeader(DisplayName = "低低报约束")]
        public string LLRestrainExpressions { get; set; } = "";
        /// <summary>
        /// 低低报文本
        /// </summary>
        [ImporterHeader(Name = "低低报文本")]
        [ExporterHeader(DisplayName = "低低报文本")]
        public string LLAlarmText { get; set; } = "";
        /// <summary>
        /// 低低限值
        /// </summary>
        [ImporterHeader(Name = "低低限值")]
        [ExporterHeader(DisplayName = "低低限值")]
        public string LLAlarmCode { get; set; } = "";

        #endregion

        #region 历史
        /// <summary>
        /// 存储类型
        /// </summary>
        [ImporterHeader(Name = "存储类型")]
        [ExporterHeader(DisplayName = "存储类型")]
        public HisType HisType { get; set; }

        /// <summary>
        /// 启用
        /// </summary>
        [ImporterHeader(Name = "历史启用")]
        [ExporterHeader(DisplayName = "历史启用")]
        public bool HisEnable { get; set; }
        #endregion
    }
    /// <summary>
    /// 变量上传属性导入DTO
    /// </summary>
    public class VariablePropertyImport : ImportPreviewInput
    {
        /// <summary>
        /// 设备ID
        /// </summary>
        [ImporterHeader(IsIgnore = true)]
        public virtual long DeviceId { get; set; }
        /// <summary>
        /// 变量ID
        /// </summary>
        [ImporterHeader(IsIgnore = true)]
        public long VariableId { get; set; }
        /// <summary>
        /// 变量名称
        /// </summary>
        [ImporterHeader(Name = "变量名称")]
        public string VariableName { get; set; }
        /// <summary>
        /// 设备名称
        /// </summary>
        [ImporterHeader(Name = "上传设备名称")]
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
    /// 变量上传属性导出DTO
    /// </summary>
    [ExcelExporter(Name = "变量上传属性", TableStyle = TableStyles.Light10, AutoFitAllColumn = true)]
    public class VariablePropertyExport
    {
        /// <summary>
        /// 设备ID
        /// </summary>
        [ExporterHeader(IsIgnore = true)]
        public virtual long DeviceId { get; set; }
        /// <summary>
        /// 变量ID
        /// </summary>
        [ExporterHeader(IsIgnore = true)]
        public long VariableId { get; set; }
        /// <summary>
        /// 设备名称
        /// </summary>
        [ExporterHeader(DisplayName = "变量名称")]
        public string VariableName { get; set; }
        /// <summary>
        /// 设备名称
        /// </summary>
        [ExporterHeader(DisplayName = "上传设备名称")]
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
    /// 变量Excel导入表示类
    /// </summary>
    public class CollectDeviceVariableWithPropertyImport
    {
        /// <summary>
        /// 变量基本属性
        /// </summary>
        [ExcelImporter(SheetName = "变量")]
        public CollectDeviceVariableImport CollectDeviceVariableExport { get; set; }
        /// <summary>
        /// 变量上传属性
        /// </summary>
        [ExcelImporter(SheetName = "变量上传属性")]
        public VariablePropertyImport DevicePropertyExcel { get; set; }

    }
    #endregion
}