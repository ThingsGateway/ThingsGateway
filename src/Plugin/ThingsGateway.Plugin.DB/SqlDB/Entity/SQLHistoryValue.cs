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

using SqlSugar;

namespace ThingsGateway.Plugin.SqlDB;

[SplitTable(SplitType.Week)]//按周分表 （自带分表支持 年、季、月、周、日）
[SugarTable("{name}_{year}{month}{day}", TableDescription = "设备采集历史表")]//3个变量必须要有
[SugarIndex("index_Id", nameof(SQLHistoryValue.Id), OrderByType.Desc)]
[SugarIndex("index_Name", nameof(SQLHistoryValue.Name), OrderByType.Desc)]
[SugarIndex("index_DeviceName", nameof(SQLHistoryValue.DeviceName), OrderByType.Desc)]
[SugarIndex("index_CollectTime", nameof(SQLHistoryValue.CollectTime), OrderByType.Desc)]
public class SQLHistoryValue : IPrimaryIdEntity, IDBHistoryValue
{
    [SugarColumn(ColumnDescription = "变量Id")]
    [AutoGenerateColumn(Order = 1, Visible = true, Sortable = true, Filterable = true)]
    public long Id { get; set; }

    /// <summary>
    /// 变量名称
    /// </summary>
    [SugarColumn(ColumnDescription = "变量名称")]
    [AutoGenerateColumn(Order = 13, Visible = true, Sortable = true, Filterable = false)]
    public string Name { get; set; }

    /// <summary>
    /// 设备名称
    /// </summary>
    [SugarColumn(ColumnDescription = "设备名称")]
    [AutoGenerateColumn(Order = 21, Visible = true, Sortable = true, Filterable = false)]
    public string DeviceName { get; set; }

    ///<summary>
    ///实时值
    ///</summary>
    [SugarColumn(ColumnDescription = "实时值")]
    [AutoGenerateColumn(Order = 23, Visible = true, Sortable = true, Filterable = false)]
    public string Value { get; set; }

    ///<summary>
    ///是否在线
    ///</summary>
    [SugarColumn(ColumnDescription = "是否在线")]
    [AutoGenerateColumn(Order = 24, Visible = true, Sortable = true, Filterable = false)]
    public bool IsOnline { get; set; }

    [SugarColumn(ColumnDescription = "采集时间")]
    public DateTime CollectTime { get; set; }

    [SplitField] //分表字段 在插入的时候会根据这个字段插入哪个表，在更新删除的时候用这个字段找出相关表
    [AutoGenerateColumn(Order = 11, Visible = true, Sortable = true, Filterable = false)]
    public DateTime CreateTime { get; set; }
}
