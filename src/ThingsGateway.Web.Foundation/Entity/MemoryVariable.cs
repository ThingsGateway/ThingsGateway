#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation;
/// <summary>
/// 内存变量表
/// </summary>
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
    /// 读取表达式
    /// </summary>
    [SugarColumn(ColumnName = "ReadExpressions", ColumnDescription = "读取表达式", Length = 200, IsNullable = true)]
    [OrderTable(Order = 7)]
    [Excel]
    public string ReadExpressions { get; set; }

    /// <summary>
    /// 是否中间变量
    /// </summary>
    [SugarColumn(ColumnName = "IsMemoryVariable", ColumnDescription = "是否中间变量", IsNullable = false)]
    public virtual bool? IsMemoryVariable { get; set; } = true;
    /// <summary>
    /// 是否允许远程Rpc写入，不包含Blazor Web页
    /// </summary>
    [SugarColumn(ColumnName = "RpcWriteEnable", ColumnDescription = "允许远程写入", IsNullable = true)]
    [OrderTable(Order = 4)]
    [Excel]
    public bool RpcWriteEnable { get; set; }
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
    [SugarColumn(ColumnDescription = "布尔开报警使能")]
    public bool BoolOpenAlarmEnable { get; set; }
    /// <summary>
    /// 布尔开报警约束
    /// </summary>
    [Description("布尔开报警约束")]
    [SugarColumn(ColumnDescription = "布尔开报警约束", IsNullable = true)]
    [Excel]
    public string BoolOpenRestrainExpressions { get; set; } = "";
    /// <summary>
    /// 布尔开报警文本
    /// </summary>
    [SugarColumn(ColumnDescription = "布尔开报警文本", IsNullable = true)]
    [Description("布尔开报警文本")]
    [Excel]
    public string BoolOpenAlarmText { get; set; } = "";

    /// <summary>
    /// 布尔关报警使能
    /// </summary>
    [SugarColumn(ColumnDescription = "布尔关报警使能")]
    [Description("布尔关报警使能")]
    [Excel]
    public bool BoolCloseAlarmEnable { get; set; }
    /// <summary>
    /// 布尔关报警约束
    /// </summary>
    [SugarColumn(ColumnDescription = "布尔关报警约束", IsNullable = true)]
    [Description("布尔关报警约束")]
    [Excel]
    public string BoolCloseRestrainExpressions { get; set; } = "";
    /// <summary>
    /// 布尔关报警文本
    /// </summary>
    [SugarColumn(ColumnDescription = "布尔关报警文本", IsNullable = true)]
    [Description("布尔关报警文本")]
    [Excel]
    public string BoolCloseAlarmText { get; set; } = "";

    /// <summary>
    /// 高报使能
    /// </summary>
    [SugarColumn(ColumnDescription = "高报使能")]
    [Description("高报使能")]
    [Excel]
    public bool HAlarmEnable { get; set; }
    /// <summary>
    /// 高报约束
    /// </summary>
    [SugarColumn(ColumnDescription = "高报约束", IsNullable = true)]
    [Description("高报约束")]
    [Excel]
    public string HRestrainExpressions { get; set; } = "";
    /// <summary>
    /// 高报文本
    /// </summary>
    [SugarColumn(ColumnDescription = "高报文本", IsNullable = true)]
    [Description("高报文本")]
    [Excel]
    public string HAlarmText { get; set; } = "";
    /// <summary>
    /// 高限值
    /// </summary>
    [SugarColumn(ColumnDescription = "高限值", IsNullable = true)]
    [Description("高限值")]
    [Excel]
    public double HAlarmCode { get; set; }

    /// <summary>
    /// 高高报使能
    /// </summary>
    [SugarColumn(ColumnDescription = "高高报使能")]
    [Description("高高报使能")]
    [Excel]
    public bool HHAlarmEnable { get; set; }
    /// <summary>
    /// 高高报约束
    /// </summary>
    [SugarColumn(ColumnDescription = "高高报约束", IsNullable = true)]
    [Description("高高报约束")]
    [Excel]
    public string HHRestrainExpressions { get; set; } = "";
    /// <summary>
    /// 高高报文本
    /// </summary>
    [SugarColumn(ColumnDescription = "高高报文本", IsNullable = true)]
    [Description("高高报文本")]
    [Excel]
    public string HHAlarmText { get; set; } = "";
    /// <summary>
    /// 高高限值
    /// </summary>
    [SugarColumn(ColumnDescription = "高高限值", IsNullable = true)]
    [Description("高高限值")]
    [Excel]
    public double HHAlarmCode { get; set; }

    /// <summary>
    /// 低报使能
    /// </summary>
    [SugarColumn(ColumnDescription = "低报使能")]
    [Description("低报使能")]
    [Excel]
    public bool LAlarmEnable { get; set; }
    /// <summary>
    /// 低报约束
    /// </summary>
    [SugarColumn(ColumnDescription = "低报约束", IsNullable = true)]
    [Description("低报约束")]
    [Excel]
    public string LRestrainExpressions { get; set; } = "";
    /// <summary>
    /// 低报文本
    /// </summary>
    [SugarColumn(ColumnDescription = "低报文本", IsNullable = true)]
    [Description("低报文本")]
    [Excel]
    public string LAlarmText { get; set; } = "";
    /// <summary>
    /// 低限值
    /// </summary>
    [SugarColumn(ColumnDescription = "低限值", IsNullable = true)]
    [Description("低限值")]
    [Excel]
    public double LAlarmCode { get; set; }

    /// <summary>
    /// 低低报使能
    /// </summary>
    [SugarColumn(ColumnDescription = "低低报使能")]
    [Description("低低报使能")]
    [Excel]
    public bool LLAlarmEnable { get; set; }
    /// <summary>
    /// 低低报约束
    /// </summary>
    [SugarColumn(ColumnDescription = "低低报约束", IsNullable = true)]
    [Description("低低报约束")]
    [Excel]
    public string LLRestrainExpressions { get; set; } = "";
    /// <summary>
    /// 低低报文本
    /// </summary>
    [SugarColumn(ColumnDescription = "低低报文本", IsNullable = true)]
    [Description("低低报文本")]
    [Excel]
    public string LLAlarmText { get; set; } = "";
    /// <summary>
    /// 低低限值
    /// </summary>
    [SugarColumn(ColumnDescription = "低低限值", IsNullable = true)]
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
    /// 历史使能
    /// </summary>
    [Description("历史使能")]
    [SugarColumn(ColumnName = "HisEnable", ColumnDescription = "历史使能")]
    [Excel]
    public bool HisEnable { get; set; }

    #endregion
}


