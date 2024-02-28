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

[SplitTable(SplitType.Week)]//按周分表 （自带分表支持 年、季、月、周、日）
[SugarTable("{name}_{year}{month}{day}", TableDescription = "设备采集历史表")]//3个变量必须要有
[SugarIndex("index_Name", nameof(SQLHistoryValue.Name), OrderByType.Desc)]
[SugarIndex("index_DeviceName", nameof(SQLHistoryValue.DeviceName), OrderByType.Desc)]
[SugarIndex("index_CollectTime", nameof(SQLHistoryValue.CollectTime), OrderByType.Desc)]
public class SQLHistoryValue : IPrimaryIdEntity
{
    [SugarColumn(ColumnDescription = "Id", IsPrimaryKey = true)]
    [DataTable(Order = 1, IsShow = true, Sortable = true, DefaultFilter = true, CellClass = " table-text-truncate ")]
    public long Id { get; set; }

    /// <summary>
    /// 变量名称
    /// </summary>
    [SugarColumn(ColumnDescription = "变量名称")]
    [DataTable(Order = 13, IsShow = true, Sortable = true, DefaultFilter = false, CellClass = " table-text-truncate ")]
    public string Name { get; set; }

    /// <summary>
    /// 设备名称
    /// </summary>
    [SugarColumn(ColumnDescription = "设备名称")]
    [DataTable(Order = 21, IsShow = true, Sortable = true, DefaultFilter = false, CellClass = " table-text-truncate ")]
    public string DeviceName { get; set; }

    ///<summary>
    ///实时值
    ///</summary>
    [SugarColumn(ColumnDescription = "实时值")]
    [DataTable(Order = 23, IsShow = true, Sortable = true, DefaultFilter = false, CellClass = " table-text-truncate ")]
    public string Value { get; set; }

    ///<summary>
    ///是否在线
    ///</summary>
    [SugarColumn(ColumnDescription = "是否在线")]
    [DataTable(Order = 24, IsShow = true, Sortable = true, DefaultFilter = false, CellClass = " table-text-truncate ")]
    public bool IsOnline { get; set; }

    [SugarColumn(ColumnDescription = "采集时间")]
    public DateTime CollectTime { get; set; }

    [SplitField] //分表字段 在插入的时候会根据这个字段插入哪个表，在更新删除的时候用这个字段找出相关表
    [DataTable(Order = 11, IsShow = true, Sortable = true, DefaultFilter = false, CellClass = " table-text-truncate ")]
    public DateTime CreateTime { get; set; }
}