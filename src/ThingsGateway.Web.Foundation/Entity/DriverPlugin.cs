using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation;
/// <summary>
/// 插件信息表
/// </summary>
[SugarTable("driver_plugin", TableDescription = "插件信息表")]
[Tenant(SqlsugarConst.DB_CustomId)]
public class DriverPlugin : BaseEntity
{
    /// <summary>
    /// 插件名称
    /// </summary>
    [SugarColumn(ColumnName = "AssembleName", ColumnDescription = "插件名称")]
    public string AssembleName { get; set; }
    /// <summary>
    /// 文件名称
    /// </summary>
    [SugarColumn(ColumnName = "FileName", ColumnDescription = "文件名称")]
    public string FileName { get; set; }
    /// <summary>
    /// 插件类型
    /// </summary>
    [SugarColumn(ColumnName = "DriverTypeEnum", ColumnDescription = "插件类型")]
    public DriverEnum DriverTypeEnum { get; set; }

    /// <summary>
    /// 插件文件全路径
    /// </summary>
    [SugarColumn(ColumnName = "FilePath", ColumnDescription = "插件文件全路径")]
    public string FilePath { get; set; }

}
public enum DriverEnum
{
    Collect,
    Upload,
}
