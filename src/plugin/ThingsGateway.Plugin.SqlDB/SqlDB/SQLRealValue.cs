//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using SqlSugar;

using ThingsGateway.Core;

namespace ThingsGateway.Plugin.SqlDB;

[SugarTable(TableDescription = "设备采集实时表")]
[SugarIndex("{table}_index_Name", nameof(SQLRealValue.Name), OrderByType.Desc)]
[SugarIndex("{table}_index_DeviceName", nameof(SQLRealValue.DeviceName), OrderByType.Desc)]
[SugarIndex("{table}_index_CollectTime", nameof(SQLRealValue.CollectTime), OrderByType.Desc)]
public class SQLRealValue : IPrimaryIdEntity
{
    [SugarColumn(ColumnDescription = "Id", IsPrimaryKey = true)]
    [DataTable(Order = 1, IsShow = true, Sortable = true, DefaultFilter = true, CellClass = " table-text-truncate ")]
    public long Id { get; set; }

    /// <summary>
    /// 变量名称
    /// </summary>
    [SugarColumn(ColumnDescription = "变量名称")]
    [DataTable(Order = 14, IsShow = true, Sortable = true, DefaultFilter = false, CellClass = " table-text-truncate ")]
    public string Name { get; set; }

    /// <summary>
    /// 设备名称
    /// </summary>
    [SugarColumn(ColumnDescription = "设备名称")]
    [DataTable(Order = 15, IsShow = true, Sortable = true, DefaultFilter = false, CellClass = " table-text-truncate ")]
    public string DeviceName { get; set; }

    ///<summary>
    ///实时值
    ///</summary>
    [SugarColumn(ColumnDescription = "实时值")]
    [DataTable(Order = 21, IsShow = true, Sortable = true, DefaultFilter = false, CellClass = " table-text-truncate ")]
    public string Value { get; set; }

    ///<summary>
    ///是否在线
    ///</summary>
    [SugarColumn(ColumnDescription = "是否在线")]
    [DataTable(Order = 23, IsShow = true, Sortable = true, DefaultFilter = false, CellClass = " table-text-truncate ")]
    public bool IsOnline { get; set; }

    [DataTable(Order = 22, IsShow = true, Sortable = true, DefaultFilter = false, CellClass = " table-text-truncate ")]
    [SugarColumn(ColumnDescription = "采集时间")]
    public DateTime CollectTime { get; set; }
}