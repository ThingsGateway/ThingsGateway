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

using System.ComponentModel.DataAnnotations;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 设备表
/// </summary>
[OrmTable("device", TableDescription = "设备表")]
[Tenant(SqlOrmConst.DB_Custom)]
[OrmIndex("unique_device_name", nameof(Device.Name), OrderByType.Asc, true)]
public class Device : BaseDataEntity, IValidatableObject
{
    public override string ToString()
    {
        if (Description.IsNullOrWhiteSpace())
            return Name;
        else
            return $"{Name}[{Description}]";
    }

    /// <summary>
    /// 名称
    /// </summary>
    [OrmColumn(ColumnDescription = "名称", Length = 200)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    [Required]
    [RegularExpression(@"^[^.]*$", ErrorMessage = "The field {0} cannot contain a dot ('.')")]
    [StringNotMemory]
    public virtual string Name { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [OrmColumn(ColumnDescription = "描述", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public string? Description { get; set; }

    /// <summary>
    /// 通道
    /// </summary>
    [OrmColumn(ColumnDescription = "通道")]
    [AutoGenerateColumn(Ignore = true)]
    [IgnoreExcel]
    [MinValue(1)]
    [Required]
    public virtual long ChannelId { get; set; }

    /// <summary>
    /// 默认执行间隔，支持corn表达式
    /// </summary>
    [OrmColumn(ColumnDescription = "默认执行间隔")]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual string IntervalTime { get; set; } = "1000";

    /// <summary>
    /// 设备使能
    /// </summary>
    [OrmColumn(ColumnDescription = "设备使能")]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual bool Enable { get; set; } = true;

    /// <summary>
    /// LogLevel
    /// </summary>
    [OrmColumn(ColumnDescription = "日志等级")]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual TouchSocket.Core.LogLevel LogLevel { get; set; } = TouchSocket.Core.LogLevel.Info;

    /// <summary>
    /// 设备属性Json
    /// </summary>
    [OrmColumn(IsJson = true, ColumnDataType = StaticConfig.CodeFirst_BigString, ColumnDescription = "设备属性Json")]
    [IgnoreExcel]
    [AutoGenerateColumn(Ignore = true)]
    public Dictionary<string, string>? DevicePropertys { get; set; } = new();

    #region 冗余配置

    /// <summary>
    /// 启用冗余
    /// </summary>
    [OrmColumn(ColumnDescription = "启用冗余")]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public bool RedundantEnable { get; set; }

    /// <summary>
    /// 冗余设备Id,只能选择相同驱动
    /// </summary>
    [OrmColumn(ColumnDescription = "冗余设备", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = false, Sortable = false)]
    [IgnoreExcel]
    public long? RedundantDeviceId { get; set; }

    /// <summary>
    /// 冗余模式
    /// </summary>
    [OrmColumn(ColumnDescription = "冗余模式")]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual RedundantSwitchTypeEnum RedundantSwitchType { get; set; }

    /// <summary>
    /// 冗余扫描间隔
    /// </summary>
    [OrmColumn(ColumnDescription = "冗余扫描间隔")]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    [MinValue(10000)]
    public virtual int RedundantScanIntervalTime { get; set; } = 30000;

    /// <summary>
    /// 冗余切换判断脚本，返回true则切换冗余设备
    /// </summary>
    [OrmColumn(ColumnDescription = "冗余切换判断脚本", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual string RedundantScript { get; set; }

    #endregion 冗余配置

    #region 备用字段

    /// <summary>
    /// 自定义
    /// </summary>
    [OrmColumn(ColumnDescription = "自定义1", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string? Remark1 { get; set; }

    /// <summary>
    /// 自定义
    /// </summary>
    [OrmColumn(ColumnDescription = "自定义2", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string? Remark2 { get; set; }

    /// <summary>
    /// 自定义
    /// </summary>
    [OrmColumn(ColumnDescription = "自定义3", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string? Remark3 { get; set; }

    /// <summary>
    /// 自定义
    /// </summary>
    [OrmColumn(ColumnDescription = "自定义4", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string? Remark4 { get; set; }

    /// <summary>
    /// 自定义
    /// </summary>
    [OrmColumn(ColumnDescription = "自定义5", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string? Remark5 { get; set; }

    #endregion 备用字段

    /// <summary>
    /// 导入验证专用
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    internal bool IsUp;

    /// <summary>
    /// 额外属性
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [MapperIgnore]
    public ModelValueValidateForm? ModelValueValidateForm;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (RedundantEnable && RedundantDeviceId == null)
        {
            yield return new ValidationResult("When enable redundancy, you must select a redundant device.", new[] { nameof(RedundantEnable), nameof(RedundantDeviceId) });
        }
    }
}
