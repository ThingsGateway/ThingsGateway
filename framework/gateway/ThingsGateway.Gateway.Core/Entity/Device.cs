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

namespace ThingsGateway.Gateway.Core;

/// <summary>
/// 设备表
/// </summary>
[SugarTable("uploadDevice", TableDescription = "上传设备表")]
[Tenant(SqlSugarConst.DB_Custom)]
public class Device : BaseEntity
{
    /// <summary>
    /// 名称
    /// </summary>
    [SugarColumn(ColumnDescription = "名称", Length = 200)]
    [DataTable(Order = 3, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public virtual string Name { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [SugarColumn(ColumnDescription = "描述", Length = 200, IsNullable = true)]
    [DataTable(Order = 3, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public string Description { get; set; }

    /// <summary>
    /// 插件名称
    /// </summary>
    [SugarColumn(ColumnDescription = "插件名称")]
    [DataTable(Order = 3, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public virtual string PluginName { get; set; }

    /// <summary>
    /// 设备使能
    /// </summary>
    [SugarColumn(ColumnDescription = "设备使能")]
    [DataTable(Order = 3, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public virtual bool Enable { get; set; }

    /// <summary>
    /// 设备组
    /// </summary>
    [SugarColumn(ColumnDescription = "设备组", IsNullable = true)]
    [DataTable(Order = 3, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public virtual string DeviceGroup { get; set; }

    /// <summary>
    /// 输出日志
    /// </summary>
    [SugarColumn(ColumnDescription = "输出日志")]
    [DataTable(Order = 3, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public virtual bool IsLogOut { get; set; }

    /// <summary>
    /// 设备属性Json
    /// </summary>
    [SugarColumn(IsJson = true, ColumnDataType = StaticConfig.CodeFirst_BigString, ColumnDescription = "设备属性Json", IsNullable = true)]
    [IgnoreExcel]
    public List<DependencyProperty> DevicePropertys { get; set; }

    #region 冗余配置

    /// <summary>
    /// 是否冗余
    /// </summary>
    [SugarColumn(ColumnDescription = "是否冗余")]
    [DataTable(Order = 9, IsShow = true, Sortable = true)]
    public bool IsRedundant { get; set; }

    /// <summary>
    /// 冗余设备Id,只能选择相同驱动
    /// </summary>
    [SugarColumn(ColumnDescription = "冗余设备")]
    [IgnoreExcel]
    public long RedundantDeviceId { get; set; }

    #endregion

    /// <summary>
    /// 默认执行间隔
    /// </summary>
    [SugarColumn(ColumnDescription = "默认执行间隔")]
    public virtual int IntervalTime { get; set; }

}

