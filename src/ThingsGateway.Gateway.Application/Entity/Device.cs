
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------


using BootstrapBlazor.Components;

using SqlSugar;

using System.ComponentModel.DataAnnotations;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 设备表
/// </summary>
[SugarTable("device", TableDescription = "设备表")]
[Tenant(SqlSugarConst.DB_Custom)]
[SugarIndex("unique_device_name", nameof(Device.Name), OrderByType.Asc, true)]
public class Device : PrimaryIdEntity
{
    /// <summary>
    /// 名称
    /// </summary>
    [SugarColumn(ColumnDescription = "名称", Length = 200)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    [Required]
    public virtual string Name { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [SugarColumn(ColumnDescription = "描述", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public string? Description { get; set; }

    /// <summary>
    /// 通道
    /// </summary>
    [SugarColumn(ColumnDescription = "通道", Length = 200)]
    [AutoGenerateColumn(Visible = true, Filterable = false, Sortable = false)]
    [IgnoreExcel]
    [MinValue(1)]
    [Required]
    public virtual long ChannelId { get; set; }

    /// <summary>
    /// 插件类型
    /// </summary>
    [SugarColumn(ColumnDescription = "插件类型")]
    [AutoGenerateColumn(Ignore = true)]
    public virtual PluginTypeEnum PluginType { get; set; }

    /// <summary>
    /// 默认执行间隔
    /// </summary>
    [SugarColumn(ColumnDescription = "默认执行间隔")]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    [MinValue(10)]
    public virtual int IntervalTime { get; set; } = 1000;

    /// <summary>
    /// 插件名称
    /// </summary>
    [SugarColumn(ColumnDescription = "插件名称")]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    [Required]
    public virtual string PluginName { get; set; }

    /// <summary>
    /// 设备使能
    /// </summary>
    [SugarColumn(ColumnDescription = "设备使能")]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public virtual bool Enable { get; set; } = true;

    /// <summary>
    /// 设备属性Json
    /// </summary>
    [SugarColumn(IsJson = true, ColumnDataType = StaticConfig.CodeFirst_BigString, ColumnDescription = "设备属性Json")]
    [IgnoreExcel]
    [AutoGenerateColumn(Ignore = true)]
    public Dictionary<string, string>? DevicePropertys { get; set; } = new();

    #region 冗余配置

    /// <summary>
    /// 启用冗余
    /// </summary>
    [SugarColumn(ColumnDescription = "启用冗余")]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public bool RedundantEnable { get; set; }

    /// <summary>
    /// 冗余设备Id,只能选择相同驱动
    /// </summary>
    [SugarColumn(ColumnDescription = "冗余设备", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = false, Sortable = false)]
    [IgnoreExcel]
    public long? RedundantDeviceId { get; set; }

    #endregion 冗余配置

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
    [AutoGenerateColumn(Ignore = true)]
    internal bool IsUp { get; set; }

    /// <summary>
    /// 插件属性
    /// </summary>
    [IgnoreExcel]
    [SugarColumn(IsIgnore = true)]
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    [AutoGenerateColumn(Ignore = true)]
    public ModelValueValidateForm PluginPropertyModel { get; set; }
}
