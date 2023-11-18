#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using System.Collections.Concurrent;

namespace ThingsGateway.Gateway.Core;
/// <summary>
/// 设备变量表
/// </summary>
[SugarTable("deviceVariable", TableDescription = "设备变量表")]
[Tenant(SqlSugarConst.DB_Custom)]
[SugarIndex("index_device", nameof(DeviceVariable.DeviceId), OrderByType.Asc)]
[SugarIndex("unique_deviceVariable_name", nameof(DeviceVariable.Name), OrderByType.Asc, true)]
public class DeviceVariable : BaseEntity
{
    #region mem

    /// <summary>
    /// 变量名称
    /// </summary>
    [SugarColumn(ColumnDescription = "变量名称", IsNullable = true)]
    [DataTable(Order = 1, IsShow = true, Sortable = true)]
    public virtual string Name { get; set; }
    /// <summary>
    /// 描述
    /// </summary>
    [SugarColumn(ColumnDescription = "描述", Length = 200, IsNullable = true)]
    [DataTable(Order = 2, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public string Description { get; set; }

    /// <summary>
    /// 读写权限
    /// </summary>
    [SugarColumn(ColumnDescription = "读写权限", IsNullable = false)]
    [DataTable(Order = 5, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public virtual ProtectTypeEnum ProtectTypeEnum { get; set; }

    /// <summary>
    /// 数据类型
    /// </summary>
    [SugarColumn(ColumnDescription = "数据类型")]
    [DataTable(Order = 6, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public virtual DataTypeEnum DataTypeEnum { get; set; }

    /// <summary>
    /// 读取表达式
    /// </summary>
    [SugarColumn(ColumnDescription = "读取表达式", Length = 200, IsNullable = true)]
    [DataTable(Order = 7, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public string ReadExpressions { get; set; }
    /// <summary>
    /// 写入表达式
    /// </summary>
    [SugarColumn(ColumnDescription = "写入表达式", Length = 200, IsNullable = true)]
    [DataTable(Order = 7, IsShow = true, Sortable = true)]
    public string WriteExpressions { get; set; }
    /// <summary>
    /// 是否允许远程Rpc写入
    /// </summary>
    [SugarColumn(ColumnDescription = "远程写入", IsNullable = true)]
    [DataTable(Order = 4, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public bool RpcWriteEnable { get; set; }
    /// <summary>
    /// 变量额外属性Json，通常使用为上传设备,List属性
    /// </summary>
    [SugarColumn(IsJson = true, ColumnDataType = StaticConfig.CodeFirst_BigString, ColumnDescription = "变量属性Json", IsNullable = true)]
    [IgnoreExcel]
    public ConcurrentDictionary<long, List<DependencyProperty>> VariablePropertys { get; set; } = new();
    /// <summary>
    /// 导入验证专用
    /// </summary>
    [IgnoreExcel]
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public bool IsUp { get; set; }

    #region 报警
    /// <summary>
    /// 布尔开报警使能
    /// </summary>
    [SugarColumn(ColumnDescription = "布尔开报警使能")]
    [DataTable(Order = 99, IsShow = true, Sortable = true, DefaultFilter = true, CellClass = " table-text-truncate ")]
    public bool BoolOpenAlarmEnable { get; set; }
    /// <summary>
    /// 布尔开报警约束
    /// </summary>
    [SugarColumn(ColumnDescription = "布尔开报警约束", IsNullable = true)]
    [DataTable(Order = 99, IsShow = true, Sortable = true, DefaultFilter = true, CellClass = " table-text-truncate ")]
    public string BoolOpenRestrainExpressions { get; set; } = "";
    /// <summary>
    /// 布尔开报警文本
    /// </summary>
    [SugarColumn(ColumnDescription = "布尔开报警文本", IsNullable = true)]
    [DataTable(Order = 99, IsShow = true, Sortable = true, DefaultFilter = true, CellClass = " table-text-truncate ")]
    public string BoolOpenAlarmText { get; set; } = "";

    /// <summary>
    /// 布尔关报警使能
    /// </summary>
    [SugarColumn(ColumnDescription = "布尔关报警使能")]
    [DataTable(Order = 99, IsShow = true, Sortable = true, DefaultFilter = true, CellClass = " table-text-truncate ")]
    public bool BoolCloseAlarmEnable { get; set; }
    /// <summary>
    /// 布尔关报警约束
    /// </summary>
    [SugarColumn(ColumnDescription = "布尔关报警约束", IsNullable = true)]
    [DataTable(Order = 99, IsShow = true, Sortable = true, DefaultFilter = true, CellClass = " table-text-truncate ")]
    public string BoolCloseRestrainExpressions { get; set; } = "";
    /// <summary>
    /// 布尔关报警文本
    /// </summary>
    [SugarColumn(ColumnDescription = "布尔关报警文本", IsNullable = true)]
    [DataTable(Order = 99, IsShow = true, Sortable = true, DefaultFilter = true, CellClass = " table-text-truncate ")]
    public string BoolCloseAlarmText { get; set; } = "";

    /// <summary>
    /// 高报使能
    /// </summary>
    [SugarColumn(ColumnDescription = "高报使能")]
    [DataTable(Order = 99, IsShow = true, Sortable = true, DefaultFilter = true, CellClass = " table-text-truncate ")]
    public bool HAlarmEnable { get; set; }
    /// <summary>
    /// 高报约束
    /// </summary>
    [SugarColumn(ColumnDescription = "高报约束", IsNullable = true)]
    [DataTable(Order = 99, IsShow = true, Sortable = true, DefaultFilter = true, CellClass = " table-text-truncate ")]
    public string HRestrainExpressions { get; set; } = "";
    /// <summary>
    /// 高报文本
    /// </summary>
    [SugarColumn(ColumnDescription = "高报文本", IsNullable = true)]
    [DataTable(Order = 99, IsShow = true, Sortable = true, DefaultFilter = true, CellClass = " table-text-truncate ")]
    public string HAlarmText { get; set; } = "";
    /// <summary>
    /// 高限值
    /// </summary>
    [SugarColumn(ColumnDescription = "高限值", IsNullable = true)]
    [DataTable(Order = 99, IsShow = true, Sortable = true, DefaultFilter = true, CellClass = " table-text-truncate ")]
    public double HAlarmCode { get; set; }

    /// <summary>
    /// 高高报使能
    /// </summary>
    [SugarColumn(ColumnDescription = "高高报使能")]
    [DataTable(Order = 99, IsShow = true, Sortable = true, DefaultFilter = true, CellClass = " table-text-truncate ")]
    public bool HHAlarmEnable { get; set; }
    /// <summary>
    /// 高高报约束
    /// </summary>
    [SugarColumn(ColumnDescription = "高高报约束", IsNullable = true)]
    [DataTable(Order = 99, IsShow = true, Sortable = true, DefaultFilter = true, CellClass = " table-text-truncate ")]
    public string HHRestrainExpressions { get; set; } = "";
    /// <summary>
    /// 高高报文本
    /// </summary>
    [SugarColumn(ColumnDescription = "高高报文本", IsNullable = true)]
    [DataTable(Order = 99, IsShow = true, Sortable = true, DefaultFilter = true, CellClass = " table-text-truncate ")]
    public string HHAlarmText { get; set; } = "";
    /// <summary>
    /// 高高限值
    /// </summary>
    [SugarColumn(ColumnDescription = "高高限值", IsNullable = true)]
    [DataTable(Order = 99, IsShow = true, Sortable = true, DefaultFilter = true, CellClass = " table-text-truncate ")]
    public double HHAlarmCode { get; set; }

    /// <summary>
    /// 低报使能
    /// </summary>
    [SugarColumn(ColumnDescription = "低报使能")]
    [DataTable(Order = 99, IsShow = true, Sortable = true, DefaultFilter = true, CellClass = " table-text-truncate ")]
    public bool LAlarmEnable { get; set; }
    /// <summary>
    /// 低报约束
    /// </summary>
    [SugarColumn(ColumnDescription = "低报约束", IsNullable = true)]
    [DataTable(Order = 99, IsShow = true, Sortable = true, DefaultFilter = true, CellClass = " table-text-truncate ")]
    public string LRestrainExpressions { get; set; } = "";
    /// <summary>
    /// 低报文本
    /// </summary>
    [SugarColumn(ColumnDescription = "低报文本", IsNullable = true)]
    [DataTable(Order = 99, IsShow = true, Sortable = true, DefaultFilter = true, CellClass = " table-text-truncate ")]
    public string LAlarmText { get; set; } = "";
    /// <summary>
    /// 低限值
    /// </summary>
    [SugarColumn(ColumnDescription = "低限值", IsNullable = true)]
    [DataTable(Order = 99, IsShow = true, Sortable = true, DefaultFilter = true, CellClass = " table-text-truncate ")]
    public double LAlarmCode { get; set; }

    /// <summary>
    /// 低低报使能
    /// </summary>
    [SugarColumn(ColumnDescription = "低低报使能")]
    [DataTable(Order = 99, IsShow = true, Sortable = true, DefaultFilter = true, CellClass = " table-text-truncate ")]
    public bool LLAlarmEnable { get; set; }
    /// <summary>
    /// 低低报约束
    /// </summary>
    [SugarColumn(ColumnDescription = "低低报约束", IsNullable = true)]
    [DataTable(Order = 99, IsShow = true, Sortable = true, DefaultFilter = true, CellClass = " table-text-truncate ")]
    public string LLRestrainExpressions { get; set; } = "";
    /// <summary>
    /// 低低报文本
    /// </summary>
    [SugarColumn(ColumnDescription = "低低报文本", IsNullable = true)]
    [DataTable(Order = 99, IsShow = true, Sortable = true, DefaultFilter = true, CellClass = " table-text-truncate ")]
    public string LLAlarmText { get; set; } = "";
    /// <summary>
    /// 低低限值
    /// </summary>
    [SugarColumn(ColumnDescription = "低低限值", IsNullable = true)]
    [DataTable(Order = 99, IsShow = true, Sortable = true, DefaultFilter = true, CellClass = " table-text-truncate ")]
    public double LLAlarmCode { get; set; }
    #endregion



    #endregion


    /// <summary>
    /// 设备
    /// </summary>
    [SugarColumn(ColumnDescription = "设备")]
    [DataTable(Order = 3, IsShow = true, Sortable = true)]
    [IgnoreExcel]
    public virtual long DeviceId { get; set; }

    /// <summary>
    /// 单位
    /// </summary>
    [SugarColumn(ColumnDescription = "单位", Length = 200, IsNullable = true)]
    [DataTable(Order = 3, IsShow = true, Sortable = true)]
    public string Unit { get; set; }

    /// <summary>
    /// 执行间隔
    /// </summary>
    [SugarColumn(ColumnDescription = "执行间隔", IsNullable = true)]
    [DataTable(Order = 3, IsShow = true, Sortable = true)]
    public virtual int? IntervalTime { get; set; }

    /// <summary>
    /// 其他方法，若不为空，此时Address为方法参数
    /// </summary>
    [SugarColumn(ColumnDescription = "特殊方法", Length = 200, IsNullable = true)]
    [DataTable(Order = 7, IsShow = true, Sortable = true)]

    public string OtherMethod { get; set; }

    /// <summary>
    /// 变量地址，可能带有额外的信息，比如<see cref="DataFormat"/> ，以;分割
    /// </summary>
    [SugarColumn(ColumnDescription = "变量地址", Length = 200, IsNullable = true)]
    [DataTable(Order = 3, IsShow = true, Sortable = true)]
    public string Address { get; set; }

}

