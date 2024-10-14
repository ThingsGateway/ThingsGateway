﻿//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using SqlSugar;

using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 设备变量表
/// </summary>
[SugarTable("variable", TableDescription = "设备变量表")]
[Tenant(SqlSugarConst.DB_Custom)]
[SugarIndex("index_device", nameof(Variable.DeviceId), OrderByType.Asc)]
[SugarIndex("unique_variable_name", nameof(Variable.Name), OrderByType.Asc, true)]
public class Variable : PrimaryIdEntity
{
    /// <summary>
    /// 设备
    /// </summary>
    [SugarColumn(ColumnDescription = "设备")]
    [AutoGenerateColumn(Visible = true, Order = 1, Filterable = false, Sortable = false)]
    [IgnoreExcel]
    [Required]
    [NotNull]
    public virtual long? DeviceId { get; set; }

    /// <summary>
    /// 变量名称
    /// </summary>
    [SugarColumn(ColumnDescription = "变量名称", IsNullable = false)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 1)]
    [Required]
    public virtual string Name { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [SugarColumn(ColumnDescription = "描述", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 2)]
    public string? Description { get; set; }

    /// <summary>
    /// 单位
    /// </summary>
    [SugarColumn(ColumnDescription = "单位", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 3)]
    public virtual string? Unit { get; set; }

    /// <summary>
    /// 执行间隔
    /// </summary>
    [SugarColumn(ColumnDescription = "执行间隔", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual int? IntervalTime { get; set; }

    /// <summary>
    /// 变量地址，可能带有额外的信息，比如<see cref="DataFormatEnum"/> ，以;分割
    /// </summary>
    [SugarColumn(ColumnDescription = "变量地址", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public string? RegisterAddress { get; set; }

    /// <summary>
    /// 其他方法，若不为空，此时RegisterAddress为方法参数
    /// </summary>
    [SugarColumn(ColumnDescription = "特殊方法", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public string? OtherMethod { get; set; }

    /// <summary>
    /// 使能
    /// </summary>
    [SugarColumn(ColumnDescription = "使能")]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual bool Enable { get; set; } = true;

    /// <summary>
    /// 读写权限
    /// </summary>
    [SugarColumn(ColumnDescription = "读写权限", IsNullable = false)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual ProtectTypeEnum ProtectType { get; set; } = ProtectTypeEnum.ReadWrite;

    /// <summary>
    /// 数据类型
    /// </summary>
    [SugarColumn(ColumnDescription = "数据类型")]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual DataTypeEnum DataType { get; set; } = DataTypeEnum.Int16;

    /// <summary>
    /// 读取表达式
    /// </summary>
    [SugarColumn(ColumnDescription = "读取表达式", Length = 1000, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual string? ReadExpressions { get; set; }

    /// <summary>
    /// 写入表达式
    /// </summary>
    [SugarColumn(ColumnDescription = "写入表达式", Length = 1000, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual string? WriteExpressions { get; set; }

    /// <summary>
    /// 是否允许远程Rpc写入
    /// </summary>
    [SugarColumn(ColumnDescription = "远程写入", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual bool RpcWriteEnable { get; set; } = true;


    /// <summary>
    /// 保存值
    /// </summary>
    [SugarColumn(ColumnDescription = "保存值", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual bool SaveValue { get; set; } = false;

    /// <summary>
    /// 变量额外属性Json
    /// </summary>
    [SugarColumn(IsJson = true, ColumnDataType = StaticConfig.CodeFirst_BigString, ColumnDescription = "变量属性Json", IsNullable = true)]
    [IgnoreExcel]
    [AutoGenerateColumn(Visible = false)]
    public ConcurrentDictionary<long, Dictionary<string, string>>? VariablePropertys { get; set; }

    #region 报警
    /// <summary>
    /// 报警延时
    /// </summary>
    [SugarColumn(ColumnDescription = "报警延时")]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public int AlarmDelay { get; set; }

    /// <summary>
    /// 布尔开报警使能
    /// </summary>
    [SugarColumn(ColumnDescription = "布尔开报警使能")]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public bool BoolOpenAlarmEnable { get; set; }

    /// <summary>
    /// 布尔开报警约束
    /// </summary>
    [SugarColumn(ColumnDescription = "布尔开报警约束", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string? BoolOpenRestrainExpressions { get; set; }

    /// <summary>
    /// 布尔开报警文本
    /// </summary>
    [SugarColumn(ColumnDescription = "布尔开报警文本", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string? BoolOpenAlarmText { get; set; }

    /// <summary>
    /// 布尔关报警使能
    /// </summary>
    [SugarColumn(ColumnDescription = "布尔关报警使能")]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public bool BoolCloseAlarmEnable { get; set; }

    /// <summary>
    /// 布尔关报警约束
    /// </summary>
    [SugarColumn(ColumnDescription = "布尔关报警约束", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string? BoolCloseRestrainExpressions { get; set; }

    /// <summary>
    /// 布尔关报警文本
    /// </summary>
    [SugarColumn(ColumnDescription = "布尔关报警文本", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string? BoolCloseAlarmText { get; set; }

    /// <summary>
    /// 高报使能
    /// </summary>
    [SugarColumn(ColumnDescription = "高报使能")]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public bool HAlarmEnable { get; set; }

    /// <summary>
    /// 高报约束
    /// </summary>
    [SugarColumn(ColumnDescription = "高报约束", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string? HRestrainExpressions { get; set; }

    /// <summary>
    /// 高报文本
    /// </summary>
    [SugarColumn(ColumnDescription = "高报文本", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string? HAlarmText { get; set; }

    /// <summary>
    /// 高限值
    /// </summary>
    [SugarColumn(ColumnDescription = "高限值", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public double? HAlarmCode { get; set; }

    /// <summary>
    /// 高高报使能
    /// </summary>
    [SugarColumn(ColumnDescription = "高高报使能")]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public bool HHAlarmEnable { get; set; }

    /// <summary>
    /// 高高报约束
    /// </summary>
    [SugarColumn(ColumnDescription = "高高报约束", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string? HHRestrainExpressions { get; set; }

    /// <summary>
    /// 高高报文本
    /// </summary>
    [SugarColumn(ColumnDescription = "高高报文本", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string? HHAlarmText { get; set; }

    /// <summary>
    /// 高高限值
    /// </summary>
    [SugarColumn(ColumnDescription = "高高限值", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public double? HHAlarmCode { get; set; }

    /// <summary>
    /// 低报使能
    /// </summary>
    [SugarColumn(ColumnDescription = "低报使能")]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public bool LAlarmEnable { get; set; }

    /// <summary>
    /// 低报约束
    /// </summary>
    [SugarColumn(ColumnDescription = "低报约束", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string? LRestrainExpressions { get; set; }

    /// <summary>
    /// 低报文本
    /// </summary>
    [SugarColumn(ColumnDescription = "低报文本", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string? LAlarmText { get; set; }

    /// <summary>
    /// 低限值
    /// </summary>
    [SugarColumn(ColumnDescription = "低限值", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public double? LAlarmCode { get; set; }

    /// <summary>
    /// 低低报使能
    /// </summary>
    [SugarColumn(ColumnDescription = "低低报使能")]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public bool LLAlarmEnable { get; set; }

    /// <summary>
    /// 低低报约束
    /// </summary>
    [SugarColumn(ColumnDescription = "低低报约束", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string? LLRestrainExpressions { get; set; }

    /// <summary>
    /// 低低报文本
    /// </summary>
    [SugarColumn(ColumnDescription = "低低报文本", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string? LLAlarmText { get; set; }

    /// <summary>
    /// 低低限值
    /// </summary>
    [SugarColumn(ColumnDescription = "低低限值", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public double? LLAlarmCode { get; set; }

    /// <summary>
    /// 自定义报警使能
    /// </summary>
    [SugarColumn(ColumnDescription = "自定义报警使能")]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public bool CustomAlarmEnable { get; set; }

    /// <summary>
    /// 自定义报警条件约束
    /// </summary>
    [SugarColumn(ColumnDescription = "自定义报警条件约束", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string? CustomRestrainExpressions { get; set; }

    /// <summary>
    /// 自定义文本
    /// </summary>
    [SugarColumn(ColumnDescription = "自定义文本", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string? CustomAlarmText { get; set; }

    /// <summary>
    /// 自定义报警条件
    /// </summary>
    [SugarColumn(ColumnDescription = "自定义报警条件", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string? CustomAlarmCode { get; set; }

    #endregion 报警

    #region 备用字段

    /// <summary>
    /// 自定义
    /// </summary>
    [SugarColumn(ColumnDescription = "自定义1", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string? Remark1 { get; set; }

    /// <summary>
    /// 自定义
    /// </summary>
    [SugarColumn(ColumnDescription = "自定义2", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string? Remark2 { get; set; }

    /// <summary>
    /// 自定义
    /// </summary>
    [SugarColumn(ColumnDescription = "自定义3", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string? Remark3 { get; set; }

    /// <summary>
    /// 自定义
    /// </summary>
    [SugarColumn(ColumnDescription = "自定义4", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string? Remark4 { get; set; }

    /// <summary>
    /// 自定义
    /// </summary>
    [SugarColumn(ColumnDescription = "自定义5", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string? Remark5 { get; set; }

    #endregion 备用字段

    /// <summary>
    /// 导入验证专用
    /// </summary>
    [IgnoreExcel]
    [SugarColumn(IsIgnore = true)]
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [AutoGenerateColumn(Visible = false)]
    internal bool IsUp { get; set; }

    /// <summary>
    /// 导入验证专用
    /// </summary>
    [IgnoreExcel]
    [SugarColumn(IsIgnore = true)]
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [AutoGenerateColumn(Visible = false)]
    internal long Row { get; set; }

    /// <summary>
    /// 变量额外属性Json
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public ConcurrentDictionary<long, ModelValueValidateForm>? VariablePropertyModels;
}

public class ModelValueValidateForm
{
    public object Value { get; set; }
    public ValidateForm ValidateForm { get; set; }
}
