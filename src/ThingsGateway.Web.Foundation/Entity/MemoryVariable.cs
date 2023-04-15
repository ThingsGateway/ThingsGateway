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
    [OrderTable(Order = 1)]
    [Excel]
    public virtual string Name { get; set; }
    /// <summary>
    /// 描述
    /// </summary>
    [SugarColumn(ColumnName = "Description", ColumnDescription = "描述", Length = 200, IsNullable = true)]
    [OrderTable(Order = 2)]
    [Excel]
    public string Description { get; set; }

    /// <summary>
    /// 读写权限
    /// </summary>
    [SugarColumn(ColumnName = "ProtectTypeEnum", ColumnDescription = "读写权限", IsNullable = false)]
    [OrderTable(Order = 5)]
    [Excel]
    public ProtectTypeEnum ProtectTypeEnum { get; set; }

    /// <summary>
    /// 数据类型
    /// </summary>
    [SugarColumn(ColumnName = "DataType", ColumnDescription = "数据类型")]
    [OrderTable(Order = 6)]
    [Excel]
    public DataTypeEnum DataTypeEnum { get; set; }

    /// <summary>
    /// 变量额外属性Json，通常使用为上传设备,List属性
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
    [Excel]
    public bool BoolOpenAlarmEnable { get; set; }
    /// <summary>
    /// 布尔开报警约束
    /// </summary>
    [Description("布尔开报警约束")]
    [Excel]
    public string BoolOpenRestrainExpressions { get; set; } = "";
    /// <summary>
    /// 布尔开报警文本
    /// </summary>
    [Description("布尔开报警文本")]
    [Excel]
    public string BoolOpenAlarmText { get; set; } = "";

    /// <summary>
    /// 布尔关报警使能
    /// </summary>
    [Description("布尔关报警使能")]
    [Excel]
    public bool BoolCloseAlarmEnable { get; set; }
    /// <summary>
    /// 布尔关报警约束
    /// </summary>
    [Description("布尔关报警约束")]
    [Excel]
    public string BoolCloseRestrainExpressions { get; set; } = "";
    /// <summary>
    /// 布尔关报警文本
    /// </summary>
    [Description("布尔关报警文本")]
    [Excel]
    public string BoolCloseAlarmText { get; set; } = "";

    /// <summary>
    /// 高报使能
    /// </summary>
    [Description("高报使能")]
    [Excel]
    public bool HAlarmEnable { get; set; }
    /// <summary>
    /// 高报约束
    /// </summary>
    [Description("高报约束")]
    [Excel]
    public string HRestrainExpressions { get; set; } = "";
    /// <summary>
    /// 高报文本
    /// </summary>
    [Description("高报文本")]
    [Excel]
    public string HAlarmText { get; set; } = "";
    /// <summary>
    /// 高限值
    /// </summary>
    [Description("高限值")]
    [Excel]
    public double HAlarmCode { get; set; }

    /// <summary>
    /// 高高报使能
    /// </summary>
    [Description("高高报使能")]
    [Excel]
    public bool HHAlarmEnable { get; set; }
    /// <summary>
    /// 高高报约束
    /// </summary>
    [Description("高高报约束")]
    [Excel]
    public string HHRestrainExpressions { get; set; } = "";
    /// <summary>
    /// 高高报文本
    /// </summary>
    [Description("高高报文本")]
    [Excel]
    public string HHAlarmText { get; set; } = "";
    /// <summary>
    /// 高高限值
    /// </summary>
    [Description("高高限值")]
    [Excel]
    public double HHAlarmCode { get; set; }

    /// <summary>
    /// 低报使能
    /// </summary>
    [Description("低报使能")]
    [Excel]
    public bool LAlarmEnable { get; set; }
    /// <summary>
    /// 低报约束
    /// </summary>
    [Description("低报约束")]
    [Excel]
    public string LRestrainExpressions { get; set; } = "";
    /// <summary>
    /// 低报文本
    /// </summary>
    [Description("低报文本")]
    [Excel]
    public string LAlarmText { get; set; } = "";
    /// <summary>
    /// 低限值
    /// </summary>
    [Description("低限值")]
    [Excel]
    public double LAlarmCode { get; set; }

    /// <summary>
    /// 低低报使能
    /// </summary>
    [Description("低低报使能")]
    [Excel]
    public bool LLAlarmEnable { get; set; }
    /// <summary>
    /// 低低报约束
    /// </summary>
    [Description("低低报约束")]
    [Excel]
    public string LLRestrainExpressions { get; set; } = "";
    /// <summary>
    /// 低低报文本
    /// </summary>
    [Description("低低报文本")]
    [Excel]
    public string LLAlarmText { get; set; } = "";
    /// <summary>
    /// 低低限值
    /// </summary>
    [Description("低低限值")]
    [Excel]
    public double LLAlarmCode { get; set; }
    #endregion

    #region 历史
    /// <summary>
    /// 存储类型
    /// </summary>
    [SugarColumn(ColumnName = "HisType", ColumnDescription = "存储类型")]
    [Excel]
    public HisType HisType { get; set; }

    /// <summary>
    /// 使能
    /// </summary>
    [Description("使能")]
    [SugarColumn(ColumnName = "HisEnable", ColumnDescription = "使能")]
    [Excel]
    public bool HisEnable { get; set; }
    #endregion
}

/// <summary>
/// 历史类型
/// </summary>
public enum HisType
{
    /// <summary>
    /// 改变存储
    /// </summary>
    Change,
    /// <summary>
    /// 采集存储
    /// </summary>
    Collect,
}

