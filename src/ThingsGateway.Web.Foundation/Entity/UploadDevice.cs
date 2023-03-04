using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 设备通用表
/// </summary>
[SugarTable("uploaddevice", TableDescription = "设备通用表")]
[Tenant(SqlsugarConst.DB_CustomId)]
public class UploadDevice : BaseEntity
{
    /// <summary>
    /// 名称
    /// </summary>
    [SugarColumn(ColumnName = "Name", ColumnDescription = "名称", Length = 200)]
    [OrderData(Order = 1)]
    public virtual string Name { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [SugarColumn(ColumnName = "Description", ColumnDescription = "描述", Length = 200, IsNullable = true)]
    [OrderData(Order = 2)]
    public string Description { get; set; }

    /// <summary>
    /// 插件Id
    /// </summary>
    [SugarColumn(ColumnName = "PluginId", ColumnDescription = "插件")]
    public virtual long PluginId { get; set; }

    /// <summary>
    /// 设备使能
    /// </summary>
    [SugarColumn(ColumnName = "Enable", ColumnDescription = "设备使能")]
    [OrderData(Order = 3)]
    public virtual bool Enable { get; set; }

    /// <summary>
    /// 输出日志
    /// </summary>
    [SugarColumn(ColumnName = "IsLogOut", ColumnDescription = "输出日志")]
    [OrderData(Order = 3)]
    public virtual bool IsLogOut { get; set; }

    /// <summary>
    /// 设备属性Json
    /// </summary>
    [SugarColumn(IsJson = true, ColumnName = "DevicePropertys", ColumnDescription = "设备属性Json", IsNullable = true)]
    public List<DependencyProperty> DevicePropertys { get; set; }

}

