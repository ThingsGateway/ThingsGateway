//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Riok.Mapperly.Abstractions;

using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using ThingsGateway.Foundation.Common.Json.Extension;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 设备变量表
/// </summary>
[OrmTable("variable", TableDescription = "设备变量表")]
[Tenant(SqlOrmConst.DB_Custom)]
[OrmIndex("index_device", nameof(Variable.DeviceId), OrderByType.Asc)]
[OrmIndex("unique_deviceid_variable_name", nameof(Variable.Name), OrderByType.Asc, nameof(Variable.DeviceId), OrderByType.Asc, true)]
public class Variable : PrimaryKeyEntity, IValidatableObject
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [OrmColumn(ColumnDescription = "Id", IsPrimaryKey = true)]
    [AutoGenerateColumn(Visible = false, IsVisibleWhenEdit = false, IsVisibleWhenAdd = false, Sortable = true, DefaultSort = true, DefaultSortOrder = SortOrder.Asc)]
    [System.ComponentModel.DataAnnotations.Key]
    public override long Id { get; set; }

    /// <summary>
    /// 导入验证专用
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public long Row;

    /// <summary>
    /// 导入验证专用
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public bool IsUp;
    public bool DynamicVariable;

    private object _value;

    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [MapperIgnore]
    [AutoGenerateColumn(Ignore = true)]
    [IgnoreExcel]
    [OrmColumn(IsIgnore = true)]
    public virtual ValidateForm? AlarmPropertysValidateForm { get; set; }

    /// <summary>
    /// 变量额外属性Json
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [MapperIgnore]
    [AutoGenerateColumn(Ignore = true)]
    [IgnoreExcel]
    [OrmColumn(IsIgnore = true)]
    public virtual NonBlockingDictionary<long, ModelValueValidateForm>? VariablePropertyModels { get; set; }

    /// <summary>
    /// 设备
    /// </summary>
    [OrmColumn(ColumnDescription = "设备")]
    [AutoGenerateColumn(Visible = true, Order = 1, Filterable = false, Sortable = false)]
    [IgnoreExcel]
    [Required]
    [NotNull]
    [MinValue(1)]
    public virtual long DeviceId { get; set; }

    /// <summary>
    /// 变量名称
    /// </summary>
    [OrmColumn(ColumnDescription = "变量名称", IsNullable = false)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 1)]
    [Required]
    public virtual string Name { get; set; }

    /// <summary>
    /// 采集组
    /// </summary>
    [OrmColumn(ColumnDescription = "采集组", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 1)]
    public virtual string CollectGroup { get; set; } = string.Empty;
    /// <summary>
    /// 分组名称
    /// </summary>
    [OrmColumn(ColumnDescription = "分组名称", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 1)]
    public virtual string BusinessGroup { get; set; }

    /// <summary>
    /// 分组上传触发变量
    /// </summary>
    [OrmColumn(ColumnDescription = "分组上传触发变量", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 1)]
    public virtual bool BusinessGroupUpdateTrigger { get; set; } = true;

    /// <summary>
    /// 写入后再次读取检查值是否一致
    /// </summary>
    [OrmColumn(ColumnDescription = "写入后再次读取检查值是否一致", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 1)]
    public virtual bool RpcWriteCheck { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [OrmColumn(ColumnDescription = "描述", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 2)]
    public string Description { get; set; }

    /// <summary>
    /// 单位
    /// </summary>
    [OrmColumn(ColumnDescription = "单位", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true, Order = 3)]
    public virtual string Unit { get; set; }

    /// <summary>
    /// 间隔时间
    /// </summary>
    [OrmColumn(ColumnDescription = "间隔时间", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual string IntervalTime { get; set; }

    /// <summary>
    /// 变量地址，可能带有额外的信息，比如<see cref="DataFormatEnum"/> ，以;分割
    /// </summary>
    [OrmColumn(ColumnDescription = "变量地址", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual string RegisterAddress { get; set; }

    /// <summary>
    /// 数组长度
    /// </summary>
    [OrmColumn(ColumnDescription = "数组长度", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual int? ArrayLength { get; set; }

    /// <summary>
    /// 其他方法，若不为空，此时RegisterAddress为方法参数
    /// </summary>
    [OrmColumn(ColumnDescription = "特殊方法", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual string OtherMethod { get; set; }

    /// <summary>
    /// 使能
    /// </summary>
    [OrmColumn(ColumnDescription = "使能")]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual bool Enable { get; set; } = true;
    /// <summary>
    /// 读写权限
    /// </summary>
    [OrmColumn(ColumnDescription = "读写权限", IsNullable = false)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual ProtectTypeEnum ProtectType { get; set; } = ProtectTypeEnum.ReadWrite;
    /// <summary>
    /// 数据类型
    /// </summary>
    [OrmColumn(ColumnDescription = "数据类型")]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual DataTypeEnum DataType { get; set; } = DataTypeEnum.Int16;
    /// <summary>
    /// 读取表达式
    /// </summary>
    [OrmColumn(ColumnDescription = "读取表达式", Length = 1000, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual string ReadExpressions { get; set; }

    /// <summary>
    /// 写入表达式
    /// </summary>
    [OrmColumn(ColumnDescription = "写入表达式", Length = 1000, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual string WriteExpressions { get; set; }

    /// <summary>
    /// 是否允许远程Rpc写入
    /// </summary>
    [OrmColumn(ColumnDescription = "远程写入", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual bool RpcWriteEnable { get; set; } = true;
    /// <summary>
    /// 初始值
    /// </summary>
    [OrmColumn(IsJson = true, ColumnDataType = StaticConfig.CodeFirst_BigString, ColumnDescription = "初始值", IsNullable = true)]
    [AutoGenerateColumn(Ignore = true)]
    public object InitValue
    {
        get
        {
            return _value;
        }
        set
        {
            if (value != null)
                _value = value?.ToString()?.GetJsonNodeFromString();
            else
                _value = null;
        }
    }

    /// <summary>
    /// 保存初始值
    /// </summary>
    [OrmColumn(ColumnDescription = "保存初始值", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual bool SaveValue { get; set; } = false;
    /// <summary>
    /// 变量额外属性Json
    /// </summary>
    [OrmColumn(IsJson = true, ColumnDataType = StaticConfig.CodeFirst_BigString, ColumnDescription = "变量属性Json", IsNullable = true)]
    [IgnoreExcel]
    [AutoGenerateColumn(Ignore = true)]
    public Dictionary<long, Dictionary<string, string>>? VariablePropertys { get; set; }


    /// <summary>
    /// 变量报警属性Json
    /// </summary>
    [OrmColumn(IsJson = true, ColumnDataType = StaticConfig.CodeFirst_BigString, ColumnDescription = "报警属性Json", IsNullable = true)]
    [IgnoreExcel]
    [AutoGenerateColumn(Ignore = true)]
    public AlarmPropertys? AlarmPropertys { get; set; }

    #region 异常值过滤

    /// <summary>
    /// 合理范围过滤使能
    /// </summary>
    [OrmColumn(ColumnDescription = "合理范围过滤使能", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public virtual bool RangeFilterEnable { get; set; }

    /// <summary>
    /// 最小值
    /// </summary>
    [OrmColumn(ColumnDescription = "最小值", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public virtual decimal RangeFilterMinValue { get; set; }

    /// <summary>
    /// 最大值
    /// </summary>
    [OrmColumn(ColumnDescription = "最大值", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public virtual decimal RangeFilterMaxValue { get; set; } = 100;

    /// <summary>
    /// 变化幅度过滤使能
    /// </summary>
    [OrmColumn(ColumnDescription = "变化幅度过滤使能", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public virtual bool AmplitudeFilterEnable { get; set; }

    /// <summary>
    /// 最大变化幅度（与上次值的绝对差值上限）
    /// </summary>
    [OrmColumn(ColumnDescription = "最大变化幅度", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public virtual decimal MaxAmplitude { get; set; } = 100;

    /// <summary>
    /// 最大变化幅度百分比（与上次值的变化比例上限，单位%）
    /// </summary>
    [OrmColumn(ColumnDescription = "最大变化幅度%", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public virtual decimal MaxAmplitudePercent { get; set; } = 100;

    /// <summary>
    /// 连续异常过滤使能
    /// </summary>
    [OrmColumn(ColumnDescription = "连续异常过滤使能", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public virtual bool ContinuousFilterEnable { get; set; }

    /// <summary>
    /// 连续异常最大次数，超过此次数后接受该值
    /// </summary>
    [OrmColumn(ColumnDescription = "连续异常最大次数", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public virtual int MaxContinuousCount { get; set; } = 3;

    #endregion 异常值过滤

    #region 备用字段

    /// <summary>
    /// 自定义
    /// </summary>
    [OrmColumn(ColumnDescription = "自定义1", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string Remark1 { get; set; }

    /// <summary>
    /// 自定义
    /// </summary>
    [OrmColumn(ColumnDescription = "自定义2", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string Remark2 { get; set; }

    /// <summary>
    /// 自定义
    /// </summary>
    [OrmColumn(ColumnDescription = "自定义3", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string Remark3 { get; set; }

    /// <summary>
    /// 自定义
    /// </summary>
    [OrmColumn(ColumnDescription = "自定义4", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string Remark4 { get; set; }

    /// <summary>
    /// 自定义
    /// </summary>
    [OrmColumn(ColumnDescription = "自定义5", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string Remark5 { get; set; }

    #endregion 备用字段

    public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(RegisterAddress) && string.IsNullOrEmpty(OtherMethod))
        {
            yield return new ValidationResult("Both RegisterAddress and OtherMethod cannot be empty or null.", new[] { nameof(OtherMethod), nameof(RegisterAddress) });
        }
    }
}
