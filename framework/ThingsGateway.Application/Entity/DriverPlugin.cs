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

using SqlSugar.DbConvert;

namespace ThingsGateway.Application;
/// <summary>
/// 插件信息表
/// </summary>
[SugarTable("driverPlugin", TableDescription = "插件信息表")]
[Tenant(ThingsGatewayConst.DB_ThingsGateway)]
[SugarIndex("unique_driverplugin_name", nameof(DriverPlugin.AssembleName), OrderByType.Asc, true)]
public class DriverPlugin : BaseEntity
{
    /// <summary>
    /// 文件名称
    /// </summary>
    [SugarColumn(ColumnName = "FileName", ColumnDescription = "文件名称")]
    [DataTable(Order = 1, IsShow = true, Sortable = true)]
    public string FileName { get; set; }

    /// <summary>
    /// 插件类名称
    /// </summary>
    [SugarColumn(ColumnName = "AssembleName", ColumnDescription = "插件类名称")]
    [DataTable(Order = 2, IsShow = true, Sortable = true)]
    public string AssembleName { get; set; }

    /// <summary>
    /// 插件类型
    /// </summary>
    [SugarColumn(ColumnDataType = "varchar(50)", ColumnName = "DriverTypeEnum", ColumnDescription = "插件类型", SqlParameterDbType = typeof(EnumToStringConvert))]
    [DataTable(Order = 3, IsShow = true, Sortable = true)]
    public DriverEnum DriverTypeEnum { get; set; }

    /// <summary>
    /// 插件文件全路径
    /// </summary>
    [SugarColumn(ColumnName = "FilePath", ColumnDescription = "插件文件全路径")]
    [DataTable(Order = 4, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public string FilePath { get; set; }

}
