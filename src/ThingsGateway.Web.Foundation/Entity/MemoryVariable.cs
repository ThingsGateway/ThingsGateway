using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation;
/// <summary>
/// 内存变量表
/// </summary>
[SugarTable("memory_variable", TableDescription = "内存变量表")]
[Tenant(SqlsugarConst.DB_CustomId)]
public class MemoryVariable : BaseEntity
{
    /// <summary>
    /// 变量名称
    /// </summary>
    [SugarColumn(ColumnName = "Name", ColumnDescription = "变量名称", IsNullable = true)]
    [OrderData(Order = 1)]
    public virtual string Name { get; set; }
    /// <summary>
    /// 描述
    /// </summary>
    [SugarColumn(ColumnName = "Description", ColumnDescription = "描述", Length = 200, IsNullable = true)]
    [OrderData(Order = 2)]
    public string Description { get; set; }

    /// <summary>
    /// 初始值
    /// </summary>
    [SugarColumn(ColumnName = "InitialValue", ColumnDescription = "初始值", IsNullable = true)]
    [OrderData(Order = 4)]
    public string InitialValue { get; set; }
    /// <summary>
    /// 变量组
    /// </summary>
    [SugarColumn(ColumnName = "VariableGroup", ColumnDescription = "变量组", IsNullable = true)]
    [OrderData(Order = 4)]
    public virtual string VariableGroup { get; set; }
    /// <summary>
    /// 读写权限
    /// </summary>
    [SugarColumn(ColumnName = "ProtectTypeEnum", ColumnDescription = "读写权限", IsNullable = false)]
    [OrderData(Order = 5)]
    public ProtectTypeEnum ProtectTypeEnum { get; set; }

    /// <summary>
    /// 数据类型
    /// </summary>
    [SugarColumn(ColumnName = "DataType", ColumnDescription = "数据类型")]
    [OrderData(Order = 6)]
    public DataTypeEnum DataTypeEnum { get; set; }

    /// <summary>
    /// 变量额外属性Json，通常使用为<上传设备，List属性>
    /// </summary>
    [SugarColumn(IsJson = true, ColumnName = "VariablePropertys", ColumnDescription = "变量属性Json", IsNullable = true)]
    public Dictionary<long, List<DependencyProperty>> VariablePropertys { get; set; } = new();



    #region 报警
    ///// <summary>
    ///// 报警组
    ///// </summary>
    //[Description("报警组")]
    //public string AlarmGroup { get; set; }
    ///// <summary>
    ///// 报警死区
    ///// </summary>
    //[Description("报警死区")]
    //public int AlarmDeadZone { get; set; }
    ///// <summary>
    ///// 报警延时
    ///// </summary>
    //[Description("报警延时")]
    //public int AlarmDelayTime { get; set; }
    /// <summary>
    /// 布尔开报警使能
    /// </summary>
    [Description("布尔开报警使能")]
    public bool BoolOpenAlarmEnable { get; set; }
    /// <summary>
    /// 布尔开报警约束
    /// </summary>
    [Description("布尔开报警约束")]
    public string BoolOpenRestrainExpressions { get; set; } = "";
    /// <summary>
    /// 布尔开报警文本
    /// </summary>
    [Description("布尔开报警文本")]
    public string BoolOpenAlarmText { get; set; } = "";

    /// <summary>
    /// 布尔关报警使能
    /// </summary>
    [Description("布尔关报警使能")]
    public bool BoolCloseAlarmEnable { get; set; }
    /// <summary>
    /// 布尔关报警约束
    /// </summary>
    [Description("布尔关报警约束")]
    public string BoolCloseRestrainExpressions { get; set; } = "";
    /// <summary>
    /// 布尔关报警文本
    /// </summary>
    [Description("布尔关报警文本")]
    public string BoolCloseAlarmText { get; set; } = "";

    /// <summary>
    /// 高报使能
    /// </summary>
    [Description("高报使能")]
    public bool HAlarmEnable { get; set; }
    /// <summary>
    /// 高报约束
    /// </summary>
    [Description("高报约束")]
    public string HRestrainExpressions { get; set; } = "";
    /// <summary>
    /// 高报文本
    /// </summary>
    [Description("高报文本")]
    public string HAlarmText { get; set; } = "";
    /// <summary>
    /// 高限值
    /// </summary>
    [Description("高限值")]
    public double HAlarmCode { get; set; }

    /// <summary>
    /// 高高报使能
    /// </summary>
    [Description("高高报使能")]
    public bool HHAlarmEnable { get; set; }
    /// <summary>
    /// 高高报约束
    /// </summary>
    [Description("高高报约束")]
    public string HHRestrainExpressions { get; set; } = "";
    /// <summary>
    /// 高高报文本
    /// </summary>
    [Description("高高报文本")]
    public string HHAlarmText { get; set; } = "";
    /// <summary>
    /// 高高限值
    /// </summary>
    [Description("高高限值")]
    public double HHAlarmCode { get; set; }

    /// <summary>
    /// 低报使能
    /// </summary>
    [Description("低报使能")]
    public bool LAlarmEnable { get; set; }
    /// <summary>
    /// 低报约束
    /// </summary>
    [Description("低报约束")]
    public string LRestrainExpressions { get; set; } = "";
    /// <summary>
    /// 低报文本
    /// </summary>
    [Description("低报文本")]
    public string LAlarmText { get; set; } = "";
    /// <summary>
    /// 低限值
    /// </summary>
    [Description("低限值")]
    public double LAlarmCode { get; set; }

    /// <summary>
    /// 低低报使能
    /// </summary>
    [Description("低低报使能")]
    public bool LLAlarmEnable { get; set; }
    /// <summary>
    /// 低低报约束
    /// </summary>
    [Description("低低报约束")]
    public string LLRestrainExpressions { get; set; } = "";
    /// <summary>
    /// 低低报文本
    /// </summary>
    [Description("低低报文本")]
    public string LLAlarmText { get; set; } = "";
    /// <summary>
    /// 低低限值
    /// </summary>
    [Description("低低限值")]
    public double LLAlarmCode { get; set; }
    #endregion

    #region 历史
    /// <summary>
    /// 存储类型
    /// </summary>
    [SugarColumn(ColumnName = "HisType", ColumnDescription = "存储类型")]
    public HisType HisType { get; set; }

    /// <summary>
    /// 使能
    /// </summary>
    [Description("使能")]
    [SugarColumn(ColumnName = "HisEnable", ColumnDescription = "使能")]
    public bool HisEnable { get; set; }
    #endregion
}


public enum HisType
{
    Change,
    Collect,
}

