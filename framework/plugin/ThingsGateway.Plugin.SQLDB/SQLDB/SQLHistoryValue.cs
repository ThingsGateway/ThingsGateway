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

using SqlSugar;

namespace ThingsGateway.Plugin.SQLDB;

[SplitTable(SplitType.Month)]//按月分表 （自带分表支持 年、季、月、周、日）
[SugarTable("historyValue_{year}{month}{day}", TableDescription = "设备采集历史表")]//3个变量必须要有
[SugarIndex("index_Name", nameof(SQLHistoryValue.Name), OrderByType.Desc)]
[SugarIndex("index_DeviceName", nameof(SQLHistoryValue.DeviceName), OrderByType.Desc)]
[SugarIndex("index_CollectTime", nameof(SQLHistoryValue.CollectTime), OrderByType.Desc)]
public class SQLHistoryValue
{
    [SugarColumn(ColumnDescription = "Id", IsPrimaryKey = true)]
    public long Id { get; set; }

    /// <summary>
    /// 变量名称
    /// </summary>
    [SugarColumn(ColumnName = "Name", ColumnDescription = "变量名称")]
    public string Name { get; set; }

    /// <summary>
    /// 设备名称
    /// </summary>
    [SugarColumn(ColumnName = "DeviceName", ColumnDescription = "设备名称")]
    public string DeviceName { get; set; }

    ///<summary>
    ///实时值
    ///</summary>
    [SugarColumn(ColumnName = "Value", ColumnDescription = "实时值")]
    public string Value { get; set; }

    ///<summary>
    ///是否在线
    ///</summary>
    [SugarColumn(ColumnName = "IsOnline", ColumnDescription = "是否在线 True=在线;False=离线")]
    public bool IsOnline { get; set; }

    public DateTime CollectTime { get; set; }

    [SplitField] //分表字段 在插入的时候会根据这个字段插入哪个表，在更新删除的时候用这个字段找出相关表
    public DateTime CreateTime { get; set; }
}